;=============================================================================
; DREAM CORE PROJECTING CHORD FUNCTION
; Generates ethereal chord frequencies for dreamlike ambiance
;=============================================================================

section .data
    ; Note frequencies for equal temperament (A4 = 440 Hz)
    note_freqs:
        ; C, C#, D, D#, E, F, F#, G, G#, A, A#, B for octave 4
        dq 261.63, 277.18, 293.66, 311.13, 329.63, 349.23, 369.99, 392.00, 415.30, 440.00, 466.16, 493.88
        ; Octave 5 (multiply by 2)
        dq 523.25, 554.37, 587.33, 622.25, 659.25, 698.46, 739.99, 783.99, 830.61, 880.00, 932.33, 987.77

    ; Chord types (intervals relative to root)
    chord_major:    db 0, 4, 7      ; root, major third, perfect fifth
    chord_minor:    db 0, 3, 7      ; root, minor third, perfect fifth
    chord_dim:      db 0, 3, 6      ; diminished
    chord_aug:      db 0, 4, 8      ; augmented
    chord_sus4:     db 0, 5, 7      ; suspended fourth
    chord_maj7:     db 0, 4, 7, 11  ; major seventh
    chord_min7:     db 0, 3, 7, 10  ; minor seventh
    chord_dom7:     db 0, 4, 7, 10  ; dominant seventh
    chord_dream:    db 0, 7, 4, 11, 14  ; custom dreamy chord (root, fifth, major third, major seventh, ninth)

    ; Current chord being played
    current_chord_freqs times 8 dq 0.0
    current_chord_size dd 0

    ; Dream state
    dream_intensity dq 0.5
    dream_phase     dd 0

;=============================================================================
; Dream Core Functions
;=============================================================================

;----------------------------------------------------------------------------
; Initialize dream core (call once)
;----------------------------------------------------------------------------
init_dream_core:
    push rbp
    mov rbp, rsp
    ; Set initial dream intensity
    movsd xmm0, [dream_intensity]
    ; Could load from config
    leave
    ret

;----------------------------------------------------------------------------
; Generate a dream chord based on game state
; Input: eax = root note index (0-11 for C to B), ebx = octave (4 or 5),
;        ecx = chord type (0=major,1=minor,2=dim,3=aug,4=sus4,5=maj7,6=min7,7=dom7,8=dream)
; Output: fills current_chord_freqs array, returns size in eax
;----------------------------------------------------------------------------
generate_dream_chord:
    push rbp
    mov rbp, rsp
    sub rsp, 32

    ; Save root and octave
    mov [rsp], eax
    mov [rsp+4], ebx

    ; Get chord intervals based on type
    cmp ecx, 0
    je .major
    cmp ecx, 1
    je .minor
    cmp ecx, 2
    je .dim
    cmp ecx, 3
    je .aug
    cmp ecx, 4
    je .sus4
    cmp ecx, 5
    je .maj7
    cmp ecx, 6
    je .min7
    cmp ecx, 7
    je .dom7
    cmp ecx, 8
    je .dream
    jmp .major  ; default

.major:
    lea rsi, [chord_major]
    mov ecx, 3
    jmp .load_intervals
.minor:
    lea rsi, [chord_minor]
    mov ecx, 3
    jmp .load_intervals
.dim:
    lea rsi, [chord_dim]
    mov ecx, 3
    jmp .load_intervals
.aug:
    lea rsi, [chord_aug]
    mov ecx, 3
    jmp .load_intervals
.sus4:
    lea rsi, [chord_sus4]
    mov ecx, 3
    jmp .load_intervals
.maj7:
    lea rsi, [chord_maj7]
    mov ecx, 4
    jmp .load_intervals
.min7:
    lea rsi, [chord_min7]
    mov ecx, 4
    jmp .load_intervals
.dom7:
    lea rsi, [chord_dom7]
    mov ecx, 4
    jmp .load_intervals
.dream:
    lea rsi, [chord_dream]
    mov ecx, 5
    jmp .load_intervals

.load_intervals:
    mov [current_chord_size], ecx
    xor r8, r8
.load_loop:
    cmp r8, rcx
    jge .done_load

    ; Get interval
    movzx edx, byte [rsi + r8]
    ; Calculate note index = root + interval
    mov eax, [rsp]              ; root
    add eax, edx
    ; Handle octave wrap (if beyond 11, increase octave)
    mov ebx, [rsp+4]            ; octave
    cmp eax, 11
    jle .same_octave
    sub eax, 12
    inc ebx
.same_octave:
    ; Get frequency from note_freqs
    ; Note: note_freqs has 24 entries (octave 4 then 5)
    ; Index = eax + (ebx-4)*12
    sub ebx, 4
    imul ebx, 12
    add eax, ebx
    ; Load frequency
    lea rdi, [note_freqs]
    movsd xmm0, [rdi + rax*8]

    ; Apply dream modulation: multiply by dream_intensity and add slight detune
    movsd xmm1, [dream_intensity]
    mulsd xmm0, xmm1
    ; Add small random detune (use rand)
    call rand
    cvtsi2sd xmm1, eax
    divsd xmm1, [rand_max_double]
    subsd xmm1, [half]
    mulsd xmm1, [detune_amount]
    addsd xmm0, xmm1

    ; Store in current_chord_freqs
    lea rdi, [current_chord_freqs]
    movsd [rdi + r8*8], xmm0

    inc r8
    jmp .load_loop

.done_load:
    mov eax, [current_chord_size]
    leave
    ret

;----------------------------------------------------------------------------
; Play dream chord (simulated - would output to audio device)
; For now, just print frequencies (if we had print_float)
;----------------------------------------------------------------------------
play_dream_chord:
    push rbp
    mov rbp, rsp

    ; In a real implementation, you would send these frequencies to an audio buffer
    ; For demonstration, we'll just print them (requires print_float)
    ; Since we don't have a full print_float in this snippet, we'll skip.

    ; Simulate playing by updating dream_phase
    inc dword [dream_phase]

    leave
    ret

;----------------------------------------------------------------------------
; Update dream intensity based on game state
; Input: xmm0 = new intensity (0.0 to 1.0)
;----------------------------------------------------------------------------
set_dream_intensity:
    push rbp
    mov rbp, rsp
    ; Clamp between 0 and 1
    xorps xmm1, xmm1
    maxsd xmm0, xmm1
    movsd xmm1, [one]
    minsd xmm0, xmm1
    movsd [dream_intensity], xmm0
    leave
    ret

;----------------------------------------------------------------------------
; Data for detune
;----------------------------------------------------------------------------
section .data
detune_amount dq 2.0            ; max detune in Hz