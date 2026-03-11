;==============================================================================
; AIRDESK – Apple's Abandoned Wireless Charging Desk Simulator
; A self-contained assembly program for Linux (x86-64, NASM)
;==============================================================================

section .data
    ; ANSI color codes for better readability
    green   db 27, '[32m', 0
    yellow  db 27, '[33m', 0
    red     db 27, '[31m', 0
    cyan    db 27, '[36m', 0
    reset   db 27, '[0m', 0

    ; Header
    header  db '╔════════════════════════════════════════════════════════╗', 10
            db '║             AIRDESK – Apple''s Lost Dream              ║', 10
            db '║        Wireless Charging Desk (Abandoned 2017)         ║', 10
            db '╚════════════════════════════════════════════════════════╝', 10, 0

    ; Status messages
    power_on    db '[PWR] AirDesk surface activated. 150W capacity.', 10, 0
    scanning    db '[SCN] Scanning for compatible devices...', 10, 0

    device_iphone   db '  → iPhone 12 detected – requesting 15W', 10, 0
    device_airpods  db '  → AirPods Pro detected – requesting 5W', 10, 0
    device_watch    db '  → Apple Watch Series 6 – requesting 2.5W', 10, 0

    power_alloc db '[PWR] Allocating power: %dW total', 10, 0
    temp_report db '[TEMP] Current temperature: %d°C', 10, 0
    overheat    db '[WARN] Thermal threshold exceeded! Throttling...', 10, 0
    failure     db '[FAIL] Project AIRDESK terminated – heat dissipation failure.', 10, 0
    shutdown    db '[SYS] System halted. The desk grows cold.', 10, 0

    ; Prompts
    prompt      db 10, 'Press [Enter] to continue, [Q] to quit: ', 0
    invalid     db 'Invalid option.', 10, 0

    ; Format strings for printf-like macro (we'll implement simple print)
    fmt_int     db '%d', 0

section .bss
    ; Device status
    iphone_present  resb 1
    airpods_present resb 1
    watch_present   resb 1

    ; Power and thermal variables
    total_power     resd 1      ; in watts
    temperature     resd 1      ; in °C
    step_counter    resd 1

    ; Input buffer
    input_buf       resb 4

section .text
    global _start

_start:
    ; Print header and power on
    mov rsi, header
    call print_string
    mov rsi, power_on
    call print_string

    ; Initial state: no devices
    mov byte [iphone_present], 0
    mov byte [airpods_present], 0
    mov byte [watch_present], 0
    mov dword [temperature], 20        ; start at 20°C
    mov dword [step_counter], 0

main_loop:
    inc dword [step_counter]

    ; Simulate device detection (random presence)
    call random_byte
    and al, 1
    mov [iphone_present], al

    call random_byte
    and al, 1
    mov [airpods_present], al

    call random_byte
    and al, 1
    mov [watch_present], al

    ; Print scanning message
    mov rsi, scanning
    call print_string

    ; Calculate total power and update temperature
    call update_power_and_temp

    ; Display which devices are present
    cmp byte [iphone_present], 1
    jne .no_iphone
    mov rsi, device_iphone
    call print_string
.no_iphone:
    cmp byte [airpods_present], 1
    jne .no_airpods
    mov rsi, device_airpods
    call print_string
.no_airpods:
    cmp byte [watch_present], 1
    jne .no_watch
    mov rsi, device_watch
    call print_string
.no_watch:

    ; Show allocated power
    mov rsi, power_alloc
    mov edi, [total_power]
    call print_int_string

    ; Show temperature
    mov rsi, temp_report
    mov edi, [temperature]
    call print_int_string

    ; Check for overheating
    cmp dword [temperature], 45
    jle .temp_ok
    mov rsi, overheat
    call print_string
    ; Throttle: reduce power by cutting devices? For simplicity, just warn.
.temp_ok:

    ; If temperature exceeds 60, project fails
    cmp dword [temperature], 60
    jl .continue
    mov rsi, failure
    call print_string
    jmp shutdown_system

.continue:
    ; Ask user to continue or quit
    mov rsi, prompt
    call print_string
    call read_char

    cmp al, 'q'
    je shutdown_system
    cmp al, 'Q'
    je shutdown_system
    cmp al, 10                  ; Enter key
    je main_loop
    ; If invalid, show message and loop again (without advancing step)
    mov rsi, invalid
    call print_string
    jmp .continue

shutdown_system:
    mov rsi, shutdown
    call print_string
    ; Exit
    mov eax, 60                 ; sys_exit
    xor edi, edi
    syscall

;------------------------------------------------------------------------------
; Updates total power and temperature based on present devices.
; Simple model: each device adds its wattage and generates heat.
;------------------------------------------------------------------------------
update_power_and_temp:
    push rax
    push rbx

    xor eax, eax
    ; iPhone 15W
    cmp byte [iphone_present], 1
    jne .check_airpods
    add eax, 15
.check_airpods:
    cmp byte [airpods_present], 1
    jne .check_watch
    add eax, 5
.check_watch:
    cmp byte [watch_present], 1
    jne .store_power
    add eax, 2                  ; 2.5W rounded to 2 for simplicity
.store_power:
    mov [total_power], eax

    ; Temperature rises with total power and ambient, plus random fluctuation
    ; temp += (power/10) + random(-2..+2)
    xor edx, edx
    mov ecx, 10
    div ecx                     ; eax = power/10
    add [temperature], eax

    ; Add random variation
    call random_byte
    and eax, 3                  ; 0..3
    sub eax, 1                  ; -1..2
    add [temperature], eax

    ; Ensure temperature doesn't go below ambient (20°C)
    cmp dword [temperature], 20
    jge .temp_ok2
    mov dword [temperature], 20
.temp_ok2:

    pop rbx
    pop rax
    ret

;------------------------------------------------------------------------------
; Simple pseudo-random number generator (xorshift32)
; Returns a random byte in al.
;------------------------------------------------------------------------------
random_byte:
    push rcx
    push rdx
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
    ; return low byte
    and eax, 0xFF
    pop rdx
    pop rcx
    ret

;------------------------------------------------------------------------------
; Print a null-terminated string pointed to by rsi.
;------------------------------------------------------------------------------
print_string:
    push rax
    push rdi
    push rdx
    push rsi
    call strlen
    mov rdx, rax                ; length
    mov rax, 1                  ; sys_write
    mov rdi, 1                  ; stdout
    syscall
    pop rsi
    pop rdx
    pop rdi
    pop rax
    ret

;------------------------------------------------------------------------------
; Compute length of null-terminated string at rsi. Returns length in rax.
;------------------------------------------------------------------------------
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

;------------------------------------------------------------------------------
; Print a string with one integer argument.
; rsi = format string containing a '%d' placeholder, edi = integer.
; We implement a very simple version: we replace '%d' with the decimal string.
; For simplicity, we just print the format string and then the integer separately.
; But that's messy. Better to have a dedicated print_int function.
; We'll keep it simple: we'll call print_string for the part before %d, then print_int, then the rest.
; However, to avoid complexity, we'll just print the number after the format string.
; So this function expects rsi = message prefix, edi = integer, and prints both.
; We'll redesign: We'll have separate print_int and then combine.
; For simplicity in this demo, we'll just print the integer with a label using print_int_string.
; Actually we already have temp_report and power_alloc as format strings with %d.
; We'll implement a simple printf-like routine that only handles %d at the end.
;------------------------------------------------------------------------------
print_int_string:
    ; rsi points to format string ending with %d\n
    ; We'll print the format up to '%', then print the integer, then newline.
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi

    ; Find the '%' character
    mov rbx, rsi
.find_percent:
    mov al, [rbx]
    cmp al, 0
    je .done_find
    cmp al, '%'
    je .found
    inc rbx
    jmp .find_percent
.found:
    ; Print the part before '%'
    mov rdx, rbx
    sub rdx, rsi
    mov rax, 1
    mov rdi, 1
    syscall

    ; Print the integer (edi)
    mov rdi, [rsp]              ; original edi
    call print_int

    ; Print the remainder after '%d' (usually just newline)
    add rbx, 2                  ; skip '%d'
    mov rsi, rbx
    call print_string

    jmp .return
.done_find:
    ; No % found, just print whole string
    mov rsi, [rsp+8]
    call print_string
.return:
    pop rdi
    pop rsi
    pop rdx
    pop rcx
    pop rbx
    pop rax
    ret

;------------------------------------------------------------------------------
; Print integer in edi as decimal.
;------------------------------------------------------------------------------
print_int:
    push rax
    push rbx
    push rcx
    push rdx
    push rsi
    push rdi

    mov eax, edi
    mov ebx, 10
    xor ecx, ecx                ; digit count
    ; Convert to string in reverse order on stack
    ; We'll use a small buffer on stack
    sub rsp, 32
    mov rsi, rsp
    add rsi, 31
    mov byte [rsi], 0           ; null terminator
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

    ; rsi points to last digit -1, so increment by 1 to point to first digit
    inc rsi
.print:
    mov rdx, rsp
    add rdx, 31
    sub rdx, rsi                ; length
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

;------------------------------------------------------------------------------
; Read a single character from stdin (blocking, no echo).
; Returns character in al.
;------------------------------------------------------------------------------
read_char:
    push rdi
    push rsi
    push rdx
    push rax

    mov rax, 0                  ; sys_read
    mov rdi, 0                  ; stdin
    mov rsi, input_buf
    mov rdx, 1
    syscall

    mov al, [input_buf]
    pop rdx
    pop rsi
    pop rdi
    ret

;------------------------------------------------------------------------------
; Data
;------------------------------------------------------------------------------
section .data
rand_state dd 123456789         ; seed for RNG