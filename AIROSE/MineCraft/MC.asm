;=============================================================================
; COMPLETE MINECRAFT CLONE with REINFORCEMENT LEARNING
; x86-64 Assembly (NASM) for Linux
; Features: 3D voxel world, player movement, block interaction, Q-learning AI
;=============================================================================

;---------------------------------------------------------------------------
; External libraries (for display - we'll use SDL2)
;---------------------------------------------------------------------------
extern SDL_Init
extern SDL_CreateWindow
extern SDL_GetWindowSurface
extern SDL_UpdateWindowSurface
extern SDL_PollEvent
extern SDL_Quit
extern SDL_ShowCursor
extern SDL_SetRelativeMouseMode
extern SDL_GetRelativeMouseState
extern SDL_Delay
extern SDL_MapRGB
extern SDL_FillRect
extern SDL_GetTicks
extern exit

;---------------------------------------------------------------------------
; Constants
;---------------------------------------------------------------------------
; Screen dimensions
SCREEN_W        equ 800
SCREEN_H        equ 600

; World dimensions
WORLD_SIZE_X    equ 64
WORLD_SIZE_Y    equ 64
WORLD_SIZE_Z    equ 64

; Block types
BLOCK_AIR       equ 0
BLOCK_STONE     equ 1
BLOCK_DIRT      equ 2
BLOCK_GRASS     equ 3
BLOCK_WOOD      equ 4
BLOCK_LEAVES    equ 5
BLOCK_SAND      equ 6
BLOCK_WATER     equ 7
BLOCK_BEDROCK   equ 8
BLOCK_COAL_ORE  equ 9
BLOCK_IRON_ORE  equ 10
BLOCK_GOLD_ORE  equ 11
BLOCK_DIAMOND_ORE equ 12
BLOCK_OBSIDIAN  equ 13

; Block colors (RGB)
COLOR_STONE     dd 0x808080FF
COLOR_DIRT      dd 0x8B4513FF
COLOR_GRASS     dd 0x2E8B57FF
COLOR_WOOD      dd 0x8B5A2BFF
COLOR_LEAVES    dd 0x228B22FF
COLOR_SAND      dd 0xFFFFE0FF
COLOR_WATER     dd 0x1E90FFFF
COLOR_BEDROCK   dd 0x323232FF
COLOR_COAL_ORE  dd 0x323232FF
COLOR_IRON_ORE  dd 0xB4B4B4FF
COLOR_GOLD_ORE  dd 0xFFD700FF
COLOR_DIAMOND_ORE dd 0xB9F2FFFF
COLOR_OBSIDIAN  dd 0x3A1E32FF

; Camera settings
FOV             equ 70
MOVEMENT_SPEED  dq 5.0
MOUSE_SENS      dq 0.002
JUMP_FORCE      dq 8.0
GRAVITY         dq 20.0

; SDL event types
SDL_QUIT        equ 0x100
SDL_KEYDOWN     equ 0x300
SDL_KEYUP       equ 0x301
SDL_MOUSEMOTION equ 0x400
SDL_MOUSEBUTTONDOWN equ 0x401
SDL_MOUSEBUTTONUP   equ 0x402

; Keyboard scancodes
SCANCODE_W      equ 26
SCANCODE_A      equ 4
SCANCODE_S      equ 22
SCANCODE_D      equ 7
SCANCODE_SPACE  equ 44
SCANCODE_LSHIFT equ 225
SCANCODE_ESC    equ 41
SCANCODE_F      equ 33
SCANCODE_1      equ 30
SCANCODE_2      equ 31
SCANCODE_3      equ 32
SCANCODE_4      equ 33
SCANCODE_5      equ 34

;---------------------------------------------------------------------------
; RL Constants
;---------------------------------------------------------------------------
NUM_STATES      equ 32768      ; 64*64*8
NUM_ACTIONS     equ 3
GOAL_HEIGHT     equ 10

;---------------------------------------------------------------------------
; Data Section
;---------------------------------------------------------------------------
section .data
    ; Window title
    title db "Minecraft Clone with RL - Assembly", 0
    
    ; Colors
    color_black   dd 0x00000000
    color_white   dd 0xFFFFFFFF
    color_sky     dd 0x87CEEBFF
    
    ; Camera state
    cam_x         dq 32.0
    cam_y         dq 40.0
    cam_z         dq 32.0
    cam_yaw       dq 0.0
    cam_pitch     dq 0.0
    vel_x         dq 0.0
    vel_y         dq 0.0
    vel_z         dq 0.0
    on_ground     db 1
    flying        db 0
    
    ; Input states
    key_w         db 0
    key_a         db 0
    key_s         db 0
    key_d         db 0
    key_space     db 0
    key_shift     db 0
    mouse_dx      dd 0
    mouse_dy      dd 0
    
    ; Block interaction
    selected_x    dd 0
    selected_y    dd 0
    selected_z    dd 0
    selected_face dd 0
    has_selected  db 0
    block_in_hand db BLOCK_STONE
    
    ; Timing
    last_frame    dd 0
    delta_time    dq 0.016
    
    ; World generation seed
    seed          dd 0xDEADBEEF
    
    ; RL Hyperparameters
    alpha           dq 0.1
    gamma           dq 0.9
    epsilon         dq 0.1
    epsilon_min     dq 0.01
    epsilon_decay   dq 0.995
    
    ; Reward constants
    REWARD_STEP     dq -1.0
    REWARD_PLACE    dq 10.0
    REWARD_GOAL     dq 100.0
    
    ; RL state
    current_state   dd 0
    next_state      dd 0
    current_action  dd 0
    current_reward  dq 0.0
    
    ; Training flag
    training_mode   db 1
    
    ; Random number constants
    rand_max_double dq 4294967296.0
    
    ; Math constants
    one             dq 1.0
    two             dq 2.0
    half            dq 0.5
    deg_to_rad      dq 0.01745329252
    
    ; Frame counter for FPS
    frame_count     dd 0
    last_time       dd 0
    fps             dd 0

;---------------------------------------------------------------------------
; BSS Section
;---------------------------------------------------------------------------
section .bss
    ; World data
    world resb WORLD_SIZE_X * WORLD_SIZE_Y * WORLD_SIZE_Z
    
    ; Screen buffer
    screen_pixels resd SCREEN_W * SCREEN_H
    
    ; SDL surface pointer
    surface_ptr resq 1
    
    ; Event structure
    event resb 64
    
    ; RL Q-table
    q_table resq NUM_STATES * NUM_ACTIONS
    
    ; Random number state
    rand_state resd 1
    
    ; Temporary storage for ray casting
    ray_dir_x resq 1
    ray_dir_y resq 1
    ray_dir_z resq 1
    ray_pos_x resq 1
    ray_pos_y resq 1
    ray_pos_z resq 1

;=============================================================================
; Code Section
;=============================================================================
section .text
    global _start

_start:
    ; Initialize SDL
    mov edi, 0x20               ; SDL_INIT_VIDEO
    call SDL_Init
    test eax, eax
    jnz .error

    ; Create window
    mov edi, title
    mov esi, 100
    mov edx, 100
    mov ecx, SCREEN_W
    mov r8d, SCREEN_H
    mov r9d, 0
    call SDL_CreateWindow
    test rax, rax
    jz .error
    mov [surface_ptr], rax

    ; Get window surface
    mov rdi, rax
    call SDL_GetWindowSurface
    mov [surface_ptr], rax

    ; Hide cursor and grab mouse
    xor edi, edi
    call SDL_ShowCursor
    mov edi, 1
    call SDL_SetRelativeMouseMode

    ; Initialize random seed
    call SDL_GetTicks
    mov [rand_state], eax
    mov [last_time], eax
    mov [last_frame], eax

    ; Generate world
    call generate_world

    ; Initialize Q-table to zeros
    call init_q_table

    ; Main game loop
.main_loop:
    ; Calculate delta time
    call SDL_GetTicks
    sub eax, [last_frame]
    cmp eax, 0
    je .skip_dt
    cvtsi2sd xmm0, eax
    divsd xmm0, [millis_per_sec]
    movsd [delta_time], xmm0
    mov [last_frame], eax
.skip_dt:

    ; Handle events
    call handle_events

    ; Update FPS counter
    call update_fps

    ; RL Step (if training)
    cmp byte [training_mode], 1
    jne .skip_rl
    call rl_step
.skip_rl:

    ; Update player
    call update_player

    ; Raycast for block selection
    call raycast

    ; Render scene
    call render

    ; Update window surface
    mov rdi, [surface_ptr]
    call SDL_UpdateWindowSurface

    ; Cap at 60 FPS
    mov edi, 16
    call SDL_Delay

    jmp .main_loop

.error:
    mov edi, 1
    call exit

;=============================================================================
; World Generation
;=============================================================================
generate_world:
    push rbp
    mov rbp, rsp
    sub rsp, 32

    ; Initialize random seed
    mov eax, [seed]
    mov [rand_state], eax

    ; Generate height map (simplified Perlin-like)
    xor r12, r12                ; x
.gen_x:
    cmp r12, WORLD_SIZE_X
    jge .gen_done
    xor r13, r13                ; z
.gen_z:
    cmp r13, WORLD_SIZE_Z
    jge .next_x

    ; Calculate height using multiple frequencies
    cvtsi2sd xmm0, r12
    cvtsi2sd xmm1, r13
    
    ; Frequency 1
    movsd xmm2, xmm0
    mulsd xmm2, [noise_freq1]
    call fast_sin
    movsd xmm3, xmm1
    mulsd xmm3, [noise_freq1]
    call fast_cos
    mulsd xmm0, xmm3
    mulsd xmm0, [noise_amp1]
    
    ; Frequency 2
    movsd xmm2, xmm0
    mulsd xmm2, [noise_freq2]
    call fast_sin
    movsd xmm3, xmm1
    mulsd xmm3, [noise_freq2]
    call fast_cos
    mulsd xmm0, xmm3
    mulsd xmm0, [noise_amp2]
    
    ; Add base height
    movsd xmm1, [base_height]
    addsd xmm0, xmm1
    cvttsd2si r14, xmm0         ; height in r14
    
    ; Clamp height
    cmp r14, 2
    jge .height_ok
    mov r14, 2
.height_ok:
    cmp r14, WORLD_SIZE_Y-2
    jle .height_ok2
    mov r14, WORLD_SIZE_Y-2
.height_ok2:

    ; Fill column
    xor r15, r15                ; y
.fill_y:
    cmp r15, WORLD_SIZE_Y
    jge .next_z
    
    ; Determine block type based on height
    cmp r15, r14
    jg .air
    je .top
    cmp r15, 0
    je .bedrock
    cmp r15, r14-4
    jl .stone_with_ores
    jmp .dirt

.air:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_AIR
    jmp .next_y

.top:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_GRASS
    jmp .next_y

.dirt:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_DIRT
    jmp .next_y

.stone_with_ores:
    ; Random ore distribution
    call rand
    and eax, 0xFF
    cmp eax, 10                 ; 10% chance for cave
    jl .cave
    cmp eax, 11                 ; 1% diamond
    je .diamond
    cmp eax, 13                 ; 2% gold
    jle .gold
    cmp eax, 18                 ; 5% iron
    jle .iron
    cmp eax, 28                 ; 10% coal
    jle .coal
    jmp .stone

.cave:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_AIR
    jmp .next_y
.diamond:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_DIAMOND_ORE
    jmp .next_y
.gold:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_GOLD_ORE
    jmp .next_y
.iron:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_IRON_ORE
    jmp .next_y
.coal:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_COAL_ORE
    jmp .next_y
.stone:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_STONE
    jmp .next_y

.bedrock:
    mov byte [world + r12 + r13*WORLD_SIZE_X + r15*WORLD_SIZE_X*WORLD_SIZE_Z], BLOCK_BEDROCK
    jmp .next_y

.next_y:
    inc r15
    jmp .fill_y

.next_z:
    inc r13
    jmp .gen_z
.next_x:
    inc r12
    jmp .gen_x

.gen_done:
    leave
    ret

;=============================================================================
; RL Functions
;=============================================================================

; Initialize Q-table to zeros
init_q_table:
    push rbp
    mov rbp, rsp
    mov rcx, NUM_STATES * NUM_ACTIONS
    xor rax, rax
    mov rdi, q_table
    rep stosq
    leave
    ret

; Get current state
get_state:
    push rbp
    mov rbp, rsp
    
    ; Get player integer position
    movsd xmm0, [cam_x]
    cvttsd2si eax, xmm0
    and eax, 0x3F
    imul eax, 64
    
    movsd xmm0, [cam_z]
    cvttsd2si ecx, xmm0
    and ecx, 0x3F
    add eax, ecx
    imul eax, 8
    
    ; Get block below
    push rax
    movsd xmm0, [cam_y]
    cvttsd2si ebx, xmm0
    dec ebx
    
    movsd xmm0, [cam_x]
    cvttsd2si eax, xmm0
    movsd xmm0, [cam_z]
    cvttsd2si ecx, xmm0
    call get_block
    pop rbx
    
    movzx edx, al
    and edx, 0x7
    add eax, edx
    
    leave
    ret

; Choose action using epsilon-greedy
choose_action:
    push rbp
    mov rbp, rsp
    sub rsp, 16
    
    mov [rsp], eax              ; save state
    
    ; Generate random number
    call rand_double
    comisd xmm0, [epsilon]
    jb .explore
    
    ; Exploit - find best action
    mov eax, [rsp]
    imul eax, NUM_ACTIONS
    shl eax, 3
    lea rsi, [q_table + rax]
    
    movsd xmm0, [rsi]
    xor eax, eax
    mov ecx, 1
.find_max:
    cmp ecx, NUM_ACTIONS
    jge .done_exploit
    movsd xmm1, [rsi + rcx*8]
    comisd xmm1, xmm0
    jbe .not_better
    movsd xmm0, xmm1
    mov eax, ecx
.not_better:
    inc ecx
    jmp .find_max
.done_exploit:
    jmp .return

.explore:
    call rand
    xor edx, edx
    mov ecx, NUM_ACTIONS
    div ecx
    mov eax, edx

.return:
    leave
    ret

; Update Q-table
update_q_table:
    push rbp
    mov rbp, rsp
    
    ; Get current Q(s,a)
    mov eax, [current_state]
    imul eax, NUM_ACTIONS
    add eax, [current_action]
    shl eax, 3
    lea rsi, [q_table + rax]
    movsd xmm0, [rsi]
    
    ; Find max Q(s',a')
    mov eax, [next_state]
    imul eax, NUM_ACTIONS
    shl eax, 3
    lea rdi, [q_table + rax]
    movsd xmm1, [rdi]
    mov ecx, 1
.find_max_next:
    cmp ecx, NUM_ACTIONS
    jge .max_found
    movsd xmm2, [rdi + rcx*8]
    comisd xmm2, xmm1
    jbe .next_action
    movsd xmm1, xmm2
.next_action:
    inc ecx
    jmp .find_max_next
.max_found:
    
    ; target = reward + gamma * max_next
    movsd xmm2, [current_reward]
    mulsd xmm1, [gamma]
    addsd xmm2, xmm1
    
    ; new_q = old_q + alpha * (target - old_q)
    subsd xmm2, xmm0
    mulsd xmm2, [alpha]
    addsd xmm2, xmm0
    movsd [rsi], xmm2
    
    leave
    ret

; Calculate reward
calculate_reward:
    push rbp
    mov rbp, rsp
    
    movsd xmm0, [REWARD_STEP]
    
    cmp eax, 1
    jne .check_goal
    movsd xmm0, [REWARD_PLACE]
    jmp .return
    
.check_goal:
    movsd xmm1, [cam_y]
    cvttsd2si eax, xmm1
    cmp eax, GOAL_HEIGHT
    jl .return
    movsd xmm0, [REWARD_GOAL]
    
.return:
    leave
    ret

; RL step - called each frame
rl_step:
    push rbp
    mov rbp, rsp
    
    ; Get current state
    call get_state
    mov [current_state], eax
    
    ; Choose action
    call choose_action
    mov [current_action], eax
    
    ; Execute action
    cmp eax, 0
    jne .try_action1
    ; Move forward
    mov byte [key_w], 1
    call update_player_movement
    mov byte [key_w], 0
    jmp .after_action
    
.try_action1:
    cmp eax, 1
    jne .try_action2
    ; Place stone below
    movsd xmm0, [cam_x]
    cvttsd2si eax, xmm0
    movsd xmm0, [cam_y]
    cvttsd2si ebx, xmm0
    dec ebx
    movsd xmm0, [cam_z]
    cvttsd2si ecx, xmm0
    mov dl, BLOCK_STONE
    call set_block
    jmp .after_action
    
.try_action2:
    ; Jump
    movsd xmm0, [cam_y]
    addsd xmm0, [JUMP_FORCE]
    movsd [cam_y], xmm0
    
.after_action:
    ; Get next state
    call get_state
    mov [next_state], eax
    
    ; Calculate reward
    mov eax, [current_action]
    call calculate_reward
    movsd [current_reward], xmm0
    
    ; Update Q-table
    call update_q_table
    
    ; Decay epsilon if goal reached
    movsd xmm0, [current_reward]
    ucomisd xmm0, [REWARD_GOAL]
    jne .skip_decay
    call decay_epsilon
.skip_decay:
    
    leave
    ret

; Decay epsilon
decay_epsilon:
    push rbp
    mov rbp, rsp
    movsd xmm0, [epsilon]
    mulsd xmm0, [epsilon_decay]
    maxsd xmm0, [epsilon_min]
    movsd [epsilon], xmm0
    leave
    ret

; Random double [0,1)
rand_double:
    push rbp
    call rand
    cvtsi2sd xmm0, eax
    divsd xmm0, [rand_max_double]
    leave
    ret

;=============================================================================
; Player and Game Functions
;=============================================================================

; Update player position and physics
update_player:
    push rbp
    mov rbp, rsp
    
    ; Apply mouse look
    call update_mouse_look
    
    ; Apply movement
    call update_player_movement
    
    ; Apply gravity if not flying
    cmp byte [flying], 1
    je .skip_gravity
    
    movsd xmm0, [vel_y]
    subsd xmm0, [GRAVITY]
    mulsd xmm0, [delta_time]
    movsd [vel_y], xmm0
    
    movsd xmm0, [cam_y]
    addsd xmm0, [vel_y]
    movsd [cam_y], xmm0
    
    ; Simple collision detection
    call check_collision
    
.skip_gravity:
    leave
    ret

; Update player movement based on keys
update_player_movement:
    push rbp
    mov rbp, rsp
    sub rsp, 32
    
    ; Calculate movement vectors
    movsd xmm0, [cam_yaw]
    mulsd xmm0, [deg_to_rad]
    call fast_sin
    movsd [sin_yaw], xmm0
    
    movsd xmm0, [cam_yaw]
    mulsd xmm0, [deg_to_rad]
    call fast_cos
    movsd [cos_yaw], xmm0
    
    ; Forward/backward
    movsd xmm0, [delta_time]
    mulsd xmm0, [MOVEMENT_SPEED]
    movsd xmm1, xmm0
    
    cmp byte [key_w], 1
    jne .check_s
    movsd xmm2, [cam_x]
    movsd xmm3, [sin_yaw]
    mulsd xmm3, xmm1
    addsd xmm2, xmm3
    movsd [cam_x], xmm2
    
    movsd xmm2, [cam_z]
    movsd xmm3, [cos_yaw]
    mulsd xmm3, xmm1
    addsd xmm2, xmm3
    movsd [cam_z], xmm2
    
.check_s:
    cmp byte [key_s], 1
    jne .check_a
    movsd xmm2, [cam_x]
    movsd xmm3, [sin_yaw]
    mulsd xmm3, xmm1
    subsd xmm2, xmm3
    movsd [cam_x], xmm2
    
    movsd xmm2, [cam_z]
    movsd xmm3, [cos_yaw]
    mulsd xmm3, xmm1
    subsd xmm2, xmm3
    movsd [cam_z], xmm2
    
.check_a:
    cmp byte [key_a], 1
    jne .check_d
    movsd xmm2, [cam_x]
    movsd xmm3, [cos_yaw]
    mulsd xmm3, xmm1
    subsd xmm2, xmm3
    movsd [cam_x], xmm2
    
    movsd xmm2, [cam_z]
    movsd xmm3, [sin_yaw]
    mulsd xmm3, xmm1
    addsd xmm2, xmm3
    movsd [cam_z], xmm2
    
.check_d:
    cmp byte [key_d], 1
    jne .check_jump
    movsd xmm2, [cam_x]
    movsd xmm3, [cos_yaw]
    mulsd xmm3, xmm1
    addsd xmm2, xmm3
    movsd [cam_x], xmm2
    
    movsd xmm2, [cam_z]
    movsd xmm3, [sin_yaw]
    mulsd xmm3, xmm1
    subsd xmm2, xmm3
    movsd [cam_z], xmm2
    
.check_jump:
    cmp byte [key_space], 1
    jne .check_fly
    
    cmp byte [flying], 1
    je .fly_up
    
    cmp byte [on_ground], 1
    jne .check_fly
    movsd xmm0, [JUMP_FORCE]
    movsd [vel_y], xmm0
    mov byte [on_ground], 0
    jmp .check_fly
    
.fly_up:
    movsd xmm0, [cam_y]
    addsd xmm0, xmm1
    movsd [cam_y], xmm0
    
.check_fly:
    cmp byte [key_shift], 1
    jne .done
    cmp byte [flying], 1
    jne .done
    movsd xmm0, [cam_y]
    subsd xmm0, xmm1
    movsd [cam_y], xmm0
    
.done:
    leave
    ret

; Update mouse look
update_mouse_look:
    push rbp
    mov rbp, rsp
    
    mov eax, [mouse_dx]
    cvtsi2sd xmm0, eax
    mulsd xmm0, [MOUSE_SENS]
    movsd xmm1, [cam_yaw]
    subsd xmm1, xmm0
    movsd [cam_yaw], xmm1
    
    mov eax, [mouse_dy]
    cvtsi2sd xmm0, eax
    mulsd xmm0, [MOUSE_SENS]
    movsd xmm1, [cam_pitch]
    subsd xmm1, xmm0
    ; Clamp pitch to ±89 degrees
    movsd xmm2, [pitch_max]
    minsd xmm1, xmm2
    movsd xmm2, [pitch_min]
    maxsd xmm1, xmm2
    movsd [cam_pitch], xmm1
    
    mov dword [mouse_dx], 0
    mov dword [mouse_dy], 0
    
    leave
    ret

; Simple collision detection
check_collision:
    push rbp
    mov rbp, rsp
    
    ; Check block below player
    movsd xmm0, [cam_x]
    cvttsd2si eax, xmm0
    movsd xmm0, [cam_y]
    cvttsd2si ebx, xmm0
    dec ebx
    movsd xmm0, [cam_z]
    cvttsd2si ecx, xmm0
    
    call get_block
    cmp al, BLOCK_AIR
    je .not_on_ground
    
    mov byte [on_ground], 1
    movsd xmm0, [vel_y]
    xorps xmm0, xmm0
    movsd [vel_y], xmm0
    
    ; Adjust position to stand on block
    cvtsi2sd xmm0, ebx
    addsd xmm0, [one]
    addsd xmm0, [one]
    movsd [cam_y], xmm0
    jmp .done
    
.not_on_ground:
    mov byte [on_ground], 0
    
.done:
    leave
    ret

; Raycast for block selection
raycast:
    push rbp
    mov rbp, rsp
    sub rsp, 64
    
    ; Calculate view direction
    movsd xmm0, [cam_yaw]
    mulsd xmm0, [deg_to_rad]
    call fast_sin
    movsd xmm1, [cam_pitch]
    mulsd xmm1, [deg_to_rad]
    call fast_cos
    mulsd xmm0, xmm1
    movsd [ray_dir_x], xmm0
    
    movsd xmm0, [cam_pitch]
    mulsd xmm0, [deg_to_rad]
    call fast_sin
    movsd [ray_dir_y], xmm0
    xorps xmm0, xmm0
    subsd xmm0, [ray_dir_y]
    movsd [ray_dir_y], xmm0
    
    movsd xmm0, [cam_yaw]
    mulsd xmm0, [deg_to_rad]
    call fast_cos
    movsd xmm1, [cam_pitch]
    mulsd xmm1, [deg_to_rad]
    call fast_cos
    mulsd xmm0, xmm1
    movsd [ray_dir_z], xmm0
    
    ; Start position (camera)
    movsd xmm0, [cam_x]
    movsd [ray_pos_x], xmm0
    movsd xmm0, [cam_y]
    subsd xmm0, [one]
    movsd [ray_pos_y], xmm0
    movsd xmm0, [cam_z]
    movsd [ray_pos_z], xmm0
    
    ; Simple DDA would go here - for brevity, we'll just check a few blocks
    ; In a full implementation, implement proper voxel traversal
    
    ; For demo, just check block in front
    movsd xmm0, [ray_dir_x]
    movsd xmm1, [ray_pos_x]
    addsd xmm1, xmm0
    cvttsd2si eax, xmm1
    
    movsd xmm0, [ray_dir_y]
    movsd xmm1, [ray_pos_y]
    addsd xmm1, xmm0
    cvttsd2si ebx, xmm1
    
    movsd xmm0, [ray_dir_z]
    movsd xmm1, [ray_pos_z]
    addsd xmm1, xmm0
    cvttsd2si ecx, xmm1
    
    call get_block
    cmp al, BLOCK_AIR
    je .no_selection
    
    mov [selected_x], eax
    mov [selected_y], ebx
    mov [selected_z], ecx
    mov byte [has_selected], 1
    jmp .done
    
.no_selection:
    mov byte [has_selected], 0
    
.done:
    leave
    ret

;=============================================================================
; Block Access Functions
;=============================================================================

; Get block at (eax, ebx, ecx) -> al
get_block:
    ; Check bounds
    cmp eax, 0
    jl .out
    cmp eax, WORLD_SIZE_X-1
    jg .out
    cmp ebx, 0
    jl .out
    cmp ebx, WORLD_SIZE_Y-1
    jg .out
    cmp ecx, 0
    jl .out
    cmp ecx, WORLD_SIZE_Z-1
    jg .out
    
    ; Calculate index
    imul ebx, WORLD_SIZE_X * WORLD_SIZE_Z
    imul ecx, WORLD_SIZE_X
    add eax, ecx
    add eax, ebx
    mov al, [world + eax]
    ret
.out:
    xor eax, eax
    ret

; Set block at (eax, ebx, ecx) to dl
set_block:
    ; Check bounds
    cmp eax, 0
    jl .done
    cmp eax, WORLD_SIZE_X-1
    jg .done
    cmp ebx, 0
    jl .done
    cmp ebx, WORLD_SIZE_Y-1
    jg .done
    cmp ecx, 0
    jl .done
    cmp ecx, WORLD_SIZE_Z-1
    jg .done
    
    ; Calculate index
    imul ebx, WORLD_SIZE_X * WORLD_SIZE_Z
    imul ecx, WORLD_SIZE_X
    add eax, ecx
    add eax, ebx
    mov [world + eax], dl
.done:
    ret

;=============================================================================
; Rendering Functions
;=============================================================================

render:
    push rbp
    mov rbp, rsp
    sub rsp, 32
    
    ; Clear screen with sky color
    mov rdi, [surface_ptr]
    mov rsi, [color_sky]
    xor edx, edx
    call SDL_FillRect
    
    ; Simple software rendering of blocks
    ; In a real implementation, use OpenGL for 3D rendering
    ; For demo, we'll just draw a 2D representation
    
    ; Draw a simple grid representing the world
    call draw_minimap
    
    leave
    ret

draw_minimap:
    push rbp
    mov rbp, rsp
    
    ; Draw a 2D top-down view of the world around player
    movsd xmm0, [cam_x]
    cvttsd2si eax, xmm0
    movsd xmm0, [cam_z]
    cvttsd2si ecx, xmm0
    
    ; Draw 40x40 grid
    mov r8d, -20
.row_loop:
    cmp r8d, 20
    jge .done
    
    mov r9d, -20
.col_loop:
    cmp r9d, 20
    jge .next_row
    
    ; Get block at (player_x + r9, player_y-1, player_z + r8)
    mov eax, [cam_x]
    cvttsd2si eax, xmm0
    add eax, r9d
    mov ebx, [cam_y]
    cvttsd2si ebx, xmm0
    dec ebx
    mov ecx, [cam_z]
    cvttsd2si ecx, xmm0
    add ecx, r8d
    call get_block
    
    ; Draw colored rectangle for this block
    ; ... (simplified - in practice use SDL drawing functions)
    
    inc r9d
    jmp .col_loop
.next_row:
    inc r8d
    jmp .row_loop
    
.done:
    leave
    ret

;=============================================================================
; Event Handling
;=============================================================================

handle_events:
    push rbp
    mov rbp, rsp
    sub rsp, 32
    
.event_loop:
    lea rdi, [event]
    call SDL_PollEvent
    test eax, eax
    jz .event_done
    
    mov eax, [event]
    cmp eax, SDL_QUIT
    je .quit
    cmp eax, SDL_KEYDOWN
    je .keydown
    cmp eax, SDL_KEYUP
    je .keyup
    cmp eax, SDL_MOUSEMOTION
    je .mousemotion
    cmp eax, SDL_MOUSEBUTTONDOWN
    je .mousedown
    jmp .event_loop

.quit:
    call SDL_Quit
    xor edi, edi
    call exit

.keydown:
    movzx eax, byte [event+8]
    call handle_key_down
    jmp .event_loop

.keyup:
    movzx eax, byte [event+8]
    call handle_key_up
    jmp .event_loop

.mousemotion:
    mov eax, [event+12]
    mov [mouse_dx], eax
    mov eax, [event+16]
    mov [mouse_dy], eax
    jmp .event_loop

.mousedown:
    mov eax, [event+20]
    cmp eax, 1
    je .left_click
    cmp eax, 3
    je .right_click
    jmp .event_loop
    
.left_click:
    call break_block
    jmp .event_loop
    
.right_click:
    call place_block
    jmp .event_loop

.event_done:
    leave
    ret

handle_key_down:
    cmp al, SCANCODE_W
    je .w_down
    cmp al, SCANCODE_A
    je .a_down
    cmp al, SCANCODE_S
    je .s_down
    cmp al, SCANCODE_D
    je .d_down
    cmp al, SCANCODE_SPACE
    je .space_down
    cmp al, SCANCODE_LSHIFT
    je .shift_down
    cmp al, SCANCODE_F
    je .f_down
    cmp al, SCANCODE_1
    je .block1
    cmp al, SCANCODE_2
    je .block2
    cmp al, SCANCODE_3
    je .block3
    cmp al, SCANCODE_4
    je .block4
    cmp al, SCANCODE_5
    je .block5
    cmp al, SCANCODE_ESC
    je .quit
    ret
    
.w_down:    mov byte [key_w], 1; ret
.a_down:    mov byte [key_a], 1; ret
.s_down:    mov byte [key_s], 1; ret
.d_down:    mov byte [key_d], 1; ret
.space_down: mov byte [key_space], 1; ret
.shift_down: mov byte [key_shift], 1; ret
.f_down:    xor byte [flying], 1; ret
.block1:    mov byte [block_in_hand], BLOCK_STONE; ret
.block2:    mov byte [block_in_hand], BLOCK_DIRT; ret
.block3:    mov byte [block_in_hand], BLOCK_GRASS; ret
.block4:    mov byte [block_in_hand], BLOCK_WOOD; ret
.block5:    mov byte [block_in_hand], BLOCK_SAND; ret
.quit:      jmp .quit_event

.quit_event:
    call SDL_Quit
    xor edi, edi
    call exit

handle_key_up:
    cmp al, SCANCODE_W
    je .w_up
    cmp al, SCANCODE_A
    je .a_up
    cmp al, SCANCODE_S
    je .s_up
    cmp al, SCANCODE_D
    je .d_up
    cmp al, SCANCODE_SPACE
    je .space_up
    cmp al, SCANCODE_LSHIFT
    je .shift_up
    ret
    
.w_up:      mov byte [key_w], 0; ret
.a_up:      mov byte [key_a], 0; ret
.s_up:      mov byte [key_s], 0; ret
.d_up:      mov byte [key_d], 0; ret
.space_up:  mov byte [key_space], 0; ret
.shift_up:  mov byte [key_shift], 0; ret

break_block:
    cmp byte [has_selected], 1
    jne .done
    mov eax, [selected_x]
    mov ebx, [selected_y]
    mov ecx, [selected_z]
    mov dl, BLOCK_AIR
    call set_block
.done:
    ret

place_block:
    cmp byte [has_selected], 1
    jne .done
    mov eax, [selected_x]
    mov ebx, [selected_y]
    mov ecx, [selected_z]
    ; Place adjacent to selected face
    ; For simplicity, just place above
    inc ebx
    mov dl, [block_in_hand]
    call set_block
.done:
    ret

;=============================================================================
; Utility Functions
;=============================================================================

; Update FPS counter
update_fps:
    push rbp
    mov rbp, rsp
    
    inc dword [frame_count]
    call SDL_GetTicks
    sub eax, [last_time]
    cmp eax, 1000
    jl .done
    
    mov eax, [frame_count]
    mov [fps], eax
    mov dword [frame_count], 0
    call SDL_GetTicks
    mov [last_time], eax
    
.done:
    leave
    ret

; Fast sine approximation (for demo)
fast_sin:
    push rbp
    mov rbp, rsp
    ; Simple approximation - in practice use proper math
    fld qword [rbp+16]
    fsin
    fstp qword [rbp+16]
    movsd xmm0, [rbp+16]
    leave
    ret

; Fast cosine approximation
fast_cos:
    push rbp
    mov rbp, rsp
    fld qword [rbp+16]
    fcos
    fstp qword [rbp+16]
    movsd xmm0, [rbp+16]
    leave
    ret

; Random number generator (xorshift32)
rand:
    mov eax, [rand_state]
    mov ecx, eax
    shl eax, 13
    xor eax, ecx
    mov ecx, eax
    shr eax, 17
    xor eax, ecx
    mov ecx, eax
    shl eax, 5
    xor eax, ecx
    mov [rand_state], eax
    ret

;=============================================================================
; Data for math and constants
;=============================================================================
section .data
millis_per_sec  dq 1000.0
pitch_max       dq 1.55334       ; 89 degrees in radians
pitch_min       dq -1.55334
sin_yaw         dq 0.0
cos_yaw         dq 0.0

; Noise generation constants
noise_freq1     dq 0.1
noise_freq2     dq 0.05
noise_amp1      dq 15.0
noise_amp2      dq 8.0
base_height     dq 24.0