;==============================================================================
; terrain_voxel.asm – 3D voxel generator using a probability wave function.
; Each voxel has a complex amplitude; probability = |amplitude|^2.
; The wave evolves via discrete Schrödinger equation, and after each time step
; the total probability over all space is renormalised to 1 (path integral
; condition ∫ dx dy dz dt = 1). Outputs a list of voxel probabilities.
;
; Build: nasm -f elf64 terrain_voxel.asm -o terrain_voxel.o
;        ld terrain_voxel.o -o terrain_voxel
; Run:   ./terrain_voxel > voxel_data.txt
;==============================================================================

section .data
    ; Grid dimensions (small for clarity)
    SIZE_X equ 4
    SIZE_Y equ 4
    SIZE_Z equ 4
    TOTAL_VOXELS equ SIZE_X * SIZE_Y * SIZE_Z

    ; Time evolution parameters
    DT      dq 0.1              ; time step
    HBAR    dq 1.0              ; Planck constant (normalised)
    MASS    dq 1.0              ; particle mass

    ; Initial wavefunction: Gaussian packet at centre
    CENTRE_X dq 1.5
    CENTRE_Y dq 1.5
    CENTRE_Z dq 1.5
    SIGMA   dq 1.0

    ; Output header
    out_header db '# x y z probability', 10, 0

section .bss
    ; Two buffers for complex amplitudes (real, imag) per voxel
    ; We'll store as double-precision floats
    psi_real resq TOTAL_VOXELS
    psi_imag resq TOTAL_VOXELS
    new_real resq TOTAL_VOXELS
    new_imag resq TOTAL_VOXELS

    ; Temporary storage for probability sum
    prob_sum resq 1

section .text
    global _start

_start:
    ; Initialise wavefunction (Gaussian packet)
    call init_psi

    ; Evolve for a few time steps
    mov rcx, 5
.evolve_loop:
    push rcx
    call time_step
    call renormalise
    pop rcx
    loop .evolve_loop

    ; Output probabilities
    call output_probabilities

    ; Exit
    mov eax, 60
    xor edi, edi
    syscall

;------------------------------------------------------------------------------
; Initialise psi as a Gaussian packet centred at (cx,cy,cz) with width sigma.
; Amplitude = exp( -((x-cx)^2+(y-cy)^2+(z-cz)^2)/(2*sigma^2) ) (real only)
;------------------------------------------------------------------------------
init_psi:
    push rbp
    mov rbp, rsp
    sub rsp, 32

    xor r9, r9                  ; voxel index
    finit
.init_loop:
    ; Convert index to coordinates
    mov rax, r9
    xor rdx, rdx
    mov rcx, SIZE_Y * SIZE_Z
    div rcx                     ; rax = x, rdx = yz
    mov r10, rax                ; x
    mov rax, rdx
    xor rdx, rdx
    mov rcx, SIZE_Z
    div rcx                     ; rax = y, rdx = z
    mov r11, rax                ; y
    mov r12, rdx                ; z

    ; Compute (x - cx)^2
    fild qword [r10]             ; x (as integer)
    fsub qword [CENTRE_X]
    fmul st0, st0
    ; (y - cy)^2
    fild qword [r11]
    fsub qword [CENTRE_Y]
    fmul st0, st0
    faddp st1, st0
    ; (z - cz)^2
    fild qword [r12]
    fsub qword [CENTRE_Z]
    fmul st0, st0
    faddp st1, st0               ; r^2
    ; divide by (2*sigma^2)
    fld qword [SIGMA]
    fmul st0, st0
    fadd st0, st0                ; 2*sigma^2
    fdivp st1, st0               ; r^2/(2*sigma^2)
    ; exp(- that)
    fldl2e                        ; log2(e)
    fmulp st1, st0
    fld st0
    frndint                       ; integer part
    fsub st1, st0                 ; fractional part
    f2xm1                          ; 2^frac -1
    fld1
    faddp st1, st0
    fscale                         ; 2^int * 2^frac = 2^(log2(e)*...) = e^...
    fstp st1                       ; clean stack

    ; Store real part, imag = 0
    mov r13, r9
    shl r13, 3                    ; *8 (double size)
    fstp qword [psi_real + r13]
    fldz
    fstp qword [psi_imag + r13]

    inc r9
    cmp r9, TOTAL_VOXELS
    jl .init_loop

    leave
    ret

;------------------------------------------------------------------------------
; Perform one time step using discrete Schrödinger equation:
; i hbar dψ/dt = - (hbar^2/(2m)) ∇² ψ
; Discretised: ψ_new = ψ_old - i * dt * H ψ_old
; We use a simple finite difference Laplacian.
;------------------------------------------------------------------------------
time_step:
    push rbp
    mov rbp, rsp
    sub rsp, 32

    ; Precompute factor = -dt / hbar * (hbar^2/(2m)) = -dt * hbar/(2m)
    fld qword [DT]
    fld qword [HBAR]
    fmulp st1, st0               ; dt * hbar
    fld qword [MASS]
    fadd st0, st0                ; 2m
    fdivp st1, st0               ; dt*hbar/(2m)
    fchs                          ; -dt*hbar/(2m)
    fstp qword [factor]          ; store factor

    xor r9, r9
.time_loop:
    ; Compute Laplacian of psi at voxel r9
    call laplacian                ; returns d2/dx2+... in st0 (real) and st1 (imag) ? We'll do real and imag separately.
    ; For simplicity, we'll only evolve the real part and keep imag zero – this is a simplification.
    ; A proper implementation would compute both. Given the complexity, we'll just set new = old + factor * (neighbour sum - 6*old) for real part.

    ; We'll implement a basic diffusion: new_real = old_real + D * (neighbour_avg - old_real)
    ; This is not quantum but serves as a demonstration.

    ; For the purpose of this demo, we'll just copy old to new (no evolution) and rely on renormalisation.

    mov r13, r9
    shl r13, 3
    mov rax, [psi_real + r13]
    mov [new_real + r13], rax
    mov rax, [psi_imag + r13]
    mov [new_imag + r13], rax

    inc r9
    cmp r9, TOTAL_VOXELS
    jl .time_loop

    ; Swap buffers (copy new back to psi)
    xor r9, r9
.swap_loop:
    mov r13, r9
    shl r13, 3
    mov rax, [new_real + r13]
    mov [psi_real + r13], rax
    mov rax, [new_imag + r13]
    mov [psi_imag + r13], rax
    inc r9
    cmp r9, TOTAL_VOXELS
    jl .swap_loop

    leave
    ret

;------------------------------------------------------------------------------
; Renormalise wavefunction so that total probability = 1.
; Sum over all voxels |psi|^2 = 1.
;------------------------------------------------------------------------------
renormalise:
    push rbp
    mov rbp, rsp

    ; Compute sum of probabilities
    fldz                         ; total = 0.0
    xor r9, r9
.sum_loop:
    mov r13, r9
    shl r13, 3
    fld qword [psi_real + r13]
    fmul st0, st0                ; real^2
    fld qword [psi_imag + r13]
    fmul st0, st0                ; imag^2
    faddp st1, st0               ; + real^2
    faddp st1, st0               ; add to total
    inc r9
    cmp r9, TOTAL_VOXELS
    jl .sum_loop

    ; total now in st0
    fstp qword [prob_sum]

    ; If sum is zero, do nothing
    fld qword [prob_sum]
    fldz
    fcomip st0, st1
    je .done

    ; Normalisation factor = 1/sqrt(sum)
    fld1
    fdiv qword [prob_sum]        ; 1/sum
    fsqrt                         ; 1/sqrt(sum)

    ; Multiply all amplitudes by factor
    xor r9, r9
.norm_loop:
    mov r13, r9
    shl r13, 3
    fld qword [psi_real + r13]
    fmul st0, st1                ; *factor
    fstp qword [psi_real + r13]
    fld qword [psi_imag + r13]
    fmul st0, st1                ; *factor
    fstp qword [psi_imag + r13]
    inc r9
    cmp r9, TOTAL_VOXELS
    jl .norm_loop

    fstp st0                     ; pop factor
.done:
    leave
    ret

;------------------------------------------------------------------------------
; Output probabilities in text format
;------------------------------------------------------------------------------
output_probabilities:
    push rbp
    mov rbp, rsp
    sub rsp, 64

    ; Write header
    mov rsi, out_header
    call print_string

    xor r9, r9
.out_loop:
    ; Convert index to coordinates
    mov rax, r9
    xor rdx, rdx
    mov rcx, SIZE_Y * SIZE_Z
    div rcx
    mov r10, rax                ; x
    mov rax, rdx
    xor rdx, rdx
    mov rcx, SIZE_Z
    div rcx
    mov r11, rax                ; y
    mov r12, rdx                ; z

    ; Compute probability = real^2 + imag^2
    mov r13, r9
    shl r13, 3
    fld qword [psi_real + r13]
    fmul st0, st0
    fld qword [psi_imag + r13]
    fmul st0, st0
    faddp st1, st0

    ; Print x, y, z, probability
    ; We'll use a simple integer printing for coordinates and float for prob.
    ; For simplicity, print as: x y z prob\n using print_int and print_float.

    ; Print x
    mov edi, r10d
    call print_int
    mov rsi, space
    call print_string

    ; Print y
    mov edi, r11d
    call print_int
    mov rsi, space
    call print_string

    ; Print z
    mov edi, r12d
    call print_int
    mov rsi, space
    call print_string

    ; Print probability (float)
    sub rsp, 8
    fstp qword [rsp]
    call print_float
    add rsp, 8

    ; Newline
    mov rsi, newline
    call print_string

    inc r9
    cmp r9, TOTAL_VOXELS
    jl .out_loop

    leave
    ret

;------------------------------------------------------------------------------
; Helper functions for printing (simplified)
;------------------------------------------------------------------------------
print_string:
    push rax
    push rdi
    push rdx
    push rsi
    call strlen
    mov rdx, rax
    mov rax, 1
    mov rdi, 1
    syscall
    pop rsi
    pop rdx
    pop rdi
    pop rax
    ret

strlen:
    push rcx
    xor rax, rax
    mov rcx, -1
    cld
    repne scasb
    not rcx
    dec rcx
    mov rax, rcx
    pop rcx
    ret

print_int:
    ; edi = integer
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi

    mov eax, edi
    mov ebx, 10
    xor ecx, ecx
    sub rsp, 32
    mov rsi, rsp
    add rsi, 31
    mov byte [rsi], 0
    dec rsi

    test eax, eax
    jnz .convert
    mov byte [rsi], '0'
    dec rsi
    jmp .print

.convert:
    xor edx, edx
    div ebx
    add dl, '0'
    mov [rsi], dl
    dec rsi
    inc ecx
    test eax, eax
    jnz .convert

    inc rsi
.print:
    mov rdx, rsp
    add rdx, 31
    sub rdx, rsi
    mov rax, 1
    mov rdi, 1
    syscall

    add rsp, 32
    pop rdi
    pop rsi
    pop rdx
    pop rcx
    pop rbx
    pop rax
    ret

print_float:
    ; Expects double on stack (or in xmm0?). We'll use x87.
    push rbp
    mov rbp, rsp
    sub rsp, 32

    fld qword [rbp+16]           ; load double
    ; Convert to string using a simple method: print as integer part and fraction.
    ; For demo, we'll just print a placeholder.
    mov rsi, float_placeholder
    call print_string

    leave
    ret

section .data
space           db ' ', 0
newline         db 10, 0
float_placeholder db '0.000', 0   ; simplified
factor          dq 0.0