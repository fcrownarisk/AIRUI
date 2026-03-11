;=============================================================================
; REINFORCEMENT LEARNING EXTENSIONS for Minecraft Clone (x86-64, NASM)
; Add this to the previous Minecraft clone code.
;=============================================================================

;---------------------------------------------------------------------------
; New Data Section (RL parameters)
;---------------------------------------------------------------------------
section .data
    ; Q-learning hyperparameters
    alpha           dq 0.1          ; learning rate
    gamma           dq 0.9          ; discount factor
    epsilon         dq 0.1          ; exploration rate
    epsilon_min     dq 0.01
    epsilon_decay   dq 0.995

    ; State space dimensions: 
    ; We use player's X (0..63), Z (0..63), and block below type (0..7)
    ; So total states = 64 * 64 * 8 = 32768
    ; Actions: 0 = move forward, 1 = place stone, 2 = jump
    NUM_STATES      equ 32768
    NUM_ACTIONS     equ 3

    ; Reward constants
    REWARD_STEP     dq -1.0
    REWARD_PLACE    dq 10.0
    REWARD_GOAL     dq 100.0
    GOAL_HEIGHT     equ 10         ; build to y=10

    ; Current RL state
    current_state   dd 0
    next_state      dd 0
    current_action  dd 0
    current_reward  dq 0.0

    ; Training flag (set to 1 to enable learning)
    training_mode   db 1

;---------------------------------------------------------------------------
; BSS Section (Q-table)
;---------------------------------------------------------------------------
section .bss
    ; Q-table: float values for each state-action pair
    ; We store as double-precision floats (8 bytes each)
    q_table resq NUM_STATES * NUM_ACTIONS

;---------------------------------------------------------------------------
; New Functions for RL
;---------------------------------------------------------------------------

;----------------------------------------------------------------------------
; Get current state index from player position and block below
; Returns state index in eax
;----------------------------------------------------------------------------
get_state:
    push rbp
    mov rbp, rsp

    ; State = (player_x * 64 + player_z) * 8 + block_below_type
    mov rax, [player_x]          ; player_x is double, we need integer part
    cvttsd2si eax, [cam_x]       ; truncate to integer (floor)
    and eax, 0x3F                ; modulo 64 (just in case)
    imul eax, 64
    mov ecx, [cam_z]
    cvttsd2si ecx, [cam_z]
    and ecx, 0x3F
    add eax, ecx                 ; eax = x*64 + z
    imul eax, 8                  ; *8 for block types

    ; Get block below player (y-1)
    movsd xmm0, [cam_y]
    cvttsd2si ebx, xmm0
    dec ebx                      ; y-1
    ; get block at (player_x_int, y-1, player_z_int)
    push rax
    mov eax, [cam_x]
    cvttsd2si eax, [cam_x]
    mov ecx, [cam_z]
    cvttsd2si ecx, [cam_z]
    call get_block               ; returns al = block type
    pop rbx
    movzx edx, al
    and edx, 0x7                 ; only 8 types (0-7)
    add eax, edx                 ; final state index

    leave
    ret

;----------------------------------------------------------------------------
; Choose action using ε-greedy policy
; Input: state index in eax
; Output: action in eax (0..2)
;----------------------------------------------------------------------------
choose_action:
    push rbp
    mov rbp, rsp
    sub rsp, 16

    ; Generate random number in [0,1)
    call rand_double             ; returns xmm0 = random double
    comisd xmm0, [epsilon]
    jb .explore                  ; if random < epsilon: explore

    ; Exploit: pick action with highest Q-value for this state
    mov ecx, eax                 ; state index in ecx
    ; Compute base address: q_table + state*NUM_ACTIONS*8
    imul ecx, NUM_ACTIONS
    shl ecx, 3                   ; *8 (size of double)
    lea rsi, [q_table + rcx]     ; rsi points to Q[state][0]

    ; Find max over 3 actions
    movsd xmm0, [rsi]            ; max = Q[0]
    xor eax, eax                 ; best_action = 0
    mov edx, 1
.check_next:
    cmp edx, NUM_ACTIONS
    jge .done_exploit
    movsd xmm1, [rsi + rdx*8]
    comisd xmm1, xmm0
    jbe .not_better
    movsd xmm0, xmm1
    mov eax, edx
.not_better:
    inc edx
    jmp .check_next
.done_exploit:
    jmp .return

.explore:
    ; Random action 0..2
    call rand
    xor edx, edx
    mov ecx, NUM_ACTIONS
    div ecx                      ; edx = remainder
    mov eax, edx

.return:
    leave
    ret

;----------------------------------------------------------------------------
; Q-learning update
; Input: state s, action a, reward r, next_state s'
;----------------------------------------------------------------------------
update_q_table:
    push rbp
    mov rbp, rsp
    sub rsp, 32

    ; Load current Q(s,a)
    mov eax, [current_state]
    imul eax, NUM_ACTIONS
    add eax, [current_action]
    shl eax, 3                   ; *8
    lea rsi, [q_table + rax]
    movsd xmm0, [rsi]            ; old_q

    ; Find max Q(s',a')
    mov eax, [next_state]
    imul eax, NUM_ACTIONS
    shl eax, 3
    lea rdi, [q_table + rax]
    movsd xmm1, [rdi]            ; max = Q[s'][0]
    mov ecx, 1
.find_max:
    cmp ecx, NUM_ACTIONS
    jge .max_found
    movsd xmm2, [rdi + rcx*8]
    comisd xmm2, xmm1
    jbe .next
    movsd xmm1, xmm2
.next:
    inc ecx
    jmp .find_max
.max_found:

    ; target = reward + gamma * max_Q(s')
    movsd xmm2, [current_reward]
    mulsd xmm1, [gamma]
    addsd xmm2, xmm1

    ; new_q = old_q + alpha * (target - old_q)
    movsd xmm1, xmm2
    subsd xmm1, xmm0             ; (target - old_q)
    mulsd xmm1, [alpha]
    addsd xmm1, xmm0
    movsd [rsi], xmm1            ; store new Q

    leave
    ret

;----------------------------------------------------------------------------
; Calculate reward based on current state and action
; Input: action taken (eax), old state (optional)
; Output: reward in xmm0
;----------------------------------------------------------------------------
calculate_reward:
    push rbp
    mov rbp, rsp

    ; Default step penalty
    movsd xmm0, [REWARD_STEP]

    ; Check if we placed a block (action == 1)
    cmp eax, 1
    jne .check_goal
    ; Reward for placing
    movsd xmm0, [REWARD_PLACE]
    jmp .return

.check_goal:
    ; Check if we reached goal height (y >= GOAL_HEIGHT)
    movsd xmm1, [cam_y]
    cvttsd2si eax, xmm1
    cmp eax, GOAL_HEIGHT
    jl .return
    movsd xmm0, [REWARD_GOAL]

.return:
    leave
    ret

;----------------------------------------------------------------------------
; Simple random double generator (0..1)
; Uses existing rand() and converts to double
;----------------------------------------------------------------------------
rand_double:
    push rbp
    call rand
    cvtsi2sd xmm0, eax
    divsd xmm0, [rand_max_double]
    ret

;----------------------------------------------------------------------------
; Decay epsilon (call after each episode)
;----------------------------------------------------------------------------
decay_epsilon:
    movsd xmm0, [epsilon]
    mulsd xmm0, [epsilon_decay]
    maxsd xmm0, [epsilon_min]
    movsd [epsilon], xmm0
    ret

;---------------------------------------------------------------------------
; Constants for random
;---------------------------------------------------------------------------
section .data
rand_max_double dq 4294967296.0   ; 2^32

;=============================================================================
; Modifications to main loop (integrate RL)
;=============================================================================

; In the main loop, after processing events and before rendering,
; we add the RL decision and action execution.

main_loop:
    ; ... existing event handling ...

    ; If training mode is on, perform RL step
    cmp byte [training_mode], 1
    jne .skip_rl

    ; Get current state
    call get_state
    mov [current_state], eax

    ; Choose action
    call choose_action
    mov [current_action], eax

    ; Execute action (mapping to game controls)
    ; Action 0: move forward
    cmp eax, 0
    jne .try_action1
    ; Simulate pressing 'W' key
    mov byte [key_w], 1
    call update_camera          ; immediate movement
    mov byte [key_w], 0
    jmp .after_action
.try_action1:
    cmp eax, 1
    jne .try_action2
    ; Action 1: place stone block below
    ; We need to set selected block to block below player
    ; For simplicity, we'll directly call place_block_at(player_x, player_y-1, player_z)
    mov eax, [cam_x]
    cvttsd2si eax, [cam_x]
    mov ebx, [cam_y]
    cvttsd2si ebx, [cam_y]
    dec ebx
    mov ecx, [cam_z]
    cvttsd2si ecx, [cam_z]
    mov dl, BLOCK_STONE
    call set_block
    jmp .after_action
.try_action2:
    ; Action 2: jump (move up)
    movsd xmm0, [cam_y]
    addsd xmm0, [move_speed]
    movsd [cam_y], xmm0
.after_action:

    ; Get new state after action
    call get_state
    mov [next_state], eax

    ; Calculate reward
    mov eax, [current_action]
    call calculate_reward
    movsd [current_reward], xmm0

    ; Update Q-table
    call update_q_table

    ; (Optional) decay epsilon after each episode (e.g., when goal reached)
    ; We'll check if reward is goal reward
    movsd xmm0, [current_reward]
    ucomisd xmm0, [REWARD_GOAL]
    jne .skip_decay
    call decay_epsilon
.skip_decay:

.skip_rl:
    ; ... continue with rendering ...