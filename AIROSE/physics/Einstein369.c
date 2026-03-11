/**
 * Program: einstein_lifetime_relativity.c
 * Description: Comprehensive summary of Albert Einstein's lifelong contributions
 *              to physics, focusing on relativity, using ALL C standard libraries.
 *
 * Libraries used: ALL ISO C standard libraries (stdio.h to wctype.h)
 *                - stdio.h, stdlib.h, string.h, math.h, time.h, ctype.h
 *                - assert.h, errno.h, float.h, limits.h, locale.h, setjmp.h
 *                - signal.h, stdarg.h, stddef.h, stdint.h, complex.h, fenv.h
 *                - inttypes.h, iso646.h, stdbool.h, tgmath.h, wchar.h, wctype.h
 */

// Include ALL C standard libraries
#include <stdio.h>      // Standard I/O
#include <stdlib.h>     // Memory allocation, system commands
#include <string.h>     // String manipulation
#include <math.h>       // Mathematical functions
#include <time.h>       // Time functions
#include <ctype.h>      // Character handling
#include <assert.h>     // Assertions
#include <errno.h>      // Error handling
#include <float.h>      // Floating point limits
#include <limits.h>     // Integer limits
#include <locale.h>     // Localization
#include <setjmp.h>     // Non-local jumps
#include <signal.h>     // Signal handling
#include <stdarg.h>     // Variable arguments
#include <stddef.h>     // Standard definitions
#include <stdint.h>     // Fixed-width integer types
#include <complex.h>    // Complex numbers
#include <fenv.h>       // Floating-point environment
#include <inttypes.h>   // Format macros for inttypes
#include <iso646.h>     // Alternative operator spellings
#include <stdbool.h>    // Boolean type
#include <tgmath.h>     // Type-generic math
#include <wchar.h>      // Wide character handling
#include <wctype.h>     // Wide character classification

#define C 299792458.0           // Speed of light in m/s
#define G 6.67430e-11           // Gravitational constant
#define SOLAR_MASS 1.989e30     // Solar mass in kg

// Global variables for program state
static bool interactive_mode = true;
static jmp_buf error_jump_buffer;
static volatile sig_atomic_t signal_received = 0;

/**
 * Signal handler for graceful exit
 */
void signal_handler(int signum) {
    signal_received = signum;
}

/**
 * Custom error handler using variable arguments
 */
void log_error(const char *format, ...) {
    va_list args;
    va_start(args, format);
    fprintf(stderr, "ERROR: ");
    vfprintf(stderr, format, args);
    fprintf(stderr, "\n");
    va_end(args);
}

/**
 * Prints a decorative header with dynamic width
 */
void print_header(const char *title) {
    int len = strlen(title);
    printf("\n");
    for (int i = 0; i < len + 8; i++) printf("=");
    printf("\n=== %s ===\n", title);
    for (int i = 0; i < len + 8; i++) printf("=");
    printf("\n");
}

/**
 * Prints the date and time using wide characters
 */
void print_timestamp() {
    time_t rawtime;
    struct tm *timeinfo;
    wchar_t buffer[100];

    time(&rawtime);
    timeinfo = localtime(&rawtime);
    wcsftime(buffer, sizeof(buffer)/sizeof(wchar_t), L"%Y-%m-%d %H:%M:%S", timeinfo);
    
    printf("\n[Summary generated on: ");
    printf("%ls", buffer);
    printf("]\n");
}

/**
 * Prints Einstein's early life and education
 */
void print_early_life() {
    printf("\n=== EARLY LIFE AND EDUCATION ===\n");
    printf("• Born: March 14, 1879, Ulm, Germany\n");
    printf("• Education: Swiss Federal Polytechnic (ETH Zurich) - 1896-1900\n");
    printf("• Swiss Patent Office (1902-1909) - 'Miracle Year' discoveries\n");
    printf("• 1905: Annus Mirabilis papers - 4 groundbreaking publications\n");
}

/**
 * Prints the Annus Mirabilis papers (1905)
 */
void print_annus_mirabilis() {
    printf("\n=== 1905: ANNUS MIRABILIS (MIRACLE YEAR) ===\n");
    printf("Four revolutionary papers published in Annalen der Physik:\n\n");
    
    printf("1. PHOTOELECTRIC EFFECT (Nobel Prize 1921)\n");
    printf("   - Light consists of discrete quanta (photons)\n");
    printf("   - E = hf - φ (work function)\n");
    printf("   - Laid foundation for quantum mechanics\n\n");
    
    printf("2. BROWNIAN MOTION\n");
    printf("   - Provided empirical evidence for atoms\n");
    printf("   - Diffusion equation: ∂ρ/∂t = D∇²ρ\n");
    printf("   - Confirmed atomic theory of matter\n\n");
    
    printf("3. SPECIAL RELATIVITY\n");
    printf("   - \"On the Electrodynamics of Moving Bodies\"\n");
    printf("   - Two postulates:\n");
    printf("       1. Laws of physics same in all inertial frames\n");
    printf("       2. Speed of light constant for all observers\n");
    printf("   - Lorentz transformations replaced Galilean\n\n");
    
    printf("4. MASS-ENERGY EQUIVALENCE\n");
    printf("   - E = mc² (most famous equation in physics)\n");
    printf("   - Derived from relativity principles\n");
}

/**
 * Prints special relativity in detail
 */
void print_special_relativity_details() {
    printf("\n=== SPECIAL RELATIVITY (1905) - DETAILED ===\n");
    
    // Time dilation demonstration
    double gamma_99 = 1.0 / sqrt(1.0 - 0.99 * 0.99);
    printf("• TIME DILATION: γ = 1/√(1-v²/c²)\n");
    printf("  At 0.99c: γ = %.3f (time slows by factor %.3f)\n", gamma_99, gamma_99);
    
    // Length contraction
    printf("• LENGTH CONTRACTION: L = L₀√(1-v²/c²)\n");
    printf("• RELATIVISTIC DOPPLER EFFECT: f = f₀√((1+β)/(1-β))\n");
    printf("• RELATIVISTIC MOMENTUM: p = γm₀v\n");
    printf("• RELATIVISTIC ENERGY: E² = (pc)² + (m₀c²)²\n");
}

/**
 * Prints general relativity in detail
 */
void print_general_relativity_details() {
    printf("\n=== GENERAL RELATIVITY (1915) - DETAILED ===\n");
    
    // Einstein field equations in symbolic form
    printf("• EINSTEIN FIELD EQUATIONS:\n");
    printf("  G_μν = R_μν - ½Rg_μν = (8πG/c⁴)T_μν\n\n");
    
    printf("• KEY PRINCIPLES:\n");
    printf("  1. Equivalence Principle: Gravity ≡ Acceleration\n");
    printf("  2. Mach's Principle influence\n");
    printf("  3. General covariance\n\n");
    
    printf("• PREDICTIONS (ALL CONFIRMED):\n");
    printf("  1. Mercury's perihelion precession (43″/century)\n");
    printf("  2. Gravitational lensing (1919 Eddington)\n");
    printf("  3. Gravitational redshift\n");
    printf("  4. Gravitational waves (2016 LIGO)\n");
    printf("  5. Black holes (2019 EHT image)\n");
}

/**
 * Interactive demonstration with complex numbers and math
 */
void interactive_relativity_demo() {
    if (!interactive_mode) return;
    
    char choice;
    printf("\n=== INTERACTIVE RELATIVITY CALCULATIONS ===\n");
    printf("Would you like to explore relativistic effects? (y/n): ");
    
    choice = getchar();
    while (getchar() != '\n'); // Clear buffer
    
    if (tolower(choice) != 'y') {
        printf("Skipping interactive demonstration.\n");
        return;
    }
    
    // Menu using complex numbers and various math functions
    int option;
    printf("\nSelect calculation:\n");
    printf("1. Lorentz factor γ for given velocity\n");
    printf("2. Energy equivalent of mass (E=mc²)\n");
    printf("3. Relativistic Doppler shift\n");
    printf("4. Schwarzschild radius (black hole)\n");
    printf("5. All calculations\n");
    printf("Choice: ");
    
    scanf("%d", &option);
    assert(option >= 1 && option <= 5); // Using assert
    
    double v, mass, beta;
    complex double z; // Complex number example
    
    switch(option) {
        case 1:

            printf("\nEnter velocity as fraction of c (0-1): ");
            scanf("%lf", &v);
            if (v >= 1.0) {
                log_error("Velocity cannot exceed c"); // Variable args usage
                break;
            }
            double gamma = 1.0 / sqrt(1.0 - v*v);
            printf("γ = %.6f\n", gamma);
            printf("Time dilation: Δt' = Δt / γ\n");
            printf("Length contraction: L' = L / γ\n");
            printf("Relativistic momentum: p = γm₀v\n");
            if (option != 5) break;
            
        case 2:

            printf("\nEnter mass in kg: ");
            scanf("%lf", &mass);
            double energy = mass * C * C;
            printf("E = %.6e J\n", energy);
            printf("   = %.6f tons TNT equivalent\n", energy / 4.184e9);
            if (option != 5) break;
            
        case 3:

            printf("\nEnter relative velocity β = v/c (0-1): ");
            scanf("%lf", &beta);
            if (beta >= 1.0) {
                log_error("Invalid velocity fraction");
                break;
            }
            double doppler = sqrt((1.0 + beta) / (1.0 - beta));
            printf("Relativistic Doppler factor = %.6f\n", doppler);
            printf("f_obs = f_src × %.6f (approaching)\n", doppler);
            printf("f_obs = f_src / %.6f (receding)\n", doppler);
            
            // Using complex numbers for demonstration
            z = CMPLX(beta, sqrt(1.0 - beta*beta));
            printf("Complex representation: β + i√(1-β²) = %.2f + %.2fi\n", 
                   creal(z), cimag(z));
            if (option != 5) break;
            
        case 4:

            printf("\nEnter mass in solar masses: ");
            double solar_masses;
            scanf("%lf", &solar_masses);
            double mass_kg = solar_masses * SOLAR_MASS;
            double rs = 2.0 * G * mass_kg / (C * C);
            printf("Schwarzschild radius = %.3e m\n", rs);
            printf("                     = %.3f km\n", rs/1000.0);
            break;
    }
    
    // Using floating point environment
    #pragma STDC FENV_ACCESS ON
    if (fetestexcept(FE_DIVBYZERO)) {
        printf("Warning: Division by zero occurred\n");
        feclearexcept(FE_DIVBYZERO);
    }
}

/**
 * Prints Einstein's later years and legacy
 */
void print_later_years() {
    printf("\n=== LATER YEARS AND LEGACY ===\n");
    printf("• 1919: Divorce from Mileva Marić, marriage to Elsa Löwenthal\n");
    printf("• 1921: Nobel Prize in Physics (Photoelectric effect)\n");
    printf("• 1933: Emigrates to USA (Princeton, Institute for Advanced Study)\n");
    printf("• 1939: Letter to Roosevelt about nuclear fission\n");
    printf("• 1940: Becomes US citizen\n");
    printf("• 1950-1955: Unified field theory attempts\n");
    printf("• 1955: Death on April 18, age 76\n\n");
    
    printf("UNFINISHED WORK:\n");
    printf("• Unified Field Theory\n");
    printf("• EPR Paradox and quantum entanglement\n");
    printf("• Cosmological constant (Λ) - later revived as dark energy\n");
}

/**
 * Prints cultural impact and honors
 */
void print_honors() {
    printf("\n=== HONORS AND CULTURAL IMPACT ===\n");
    printf("• Time Magazine: Person of the Century (1999)\n");
    printf("• Element 99: Einsteinium (Es)\n");
    printf("• Einstein Papers Project\n");
    printf("• Numerous statues, museums, and awards\n");
    printf("• His brain preserved for scientific study\n");
    printf("• Last words: (spoken in German, untranslated to nurse)\n");
}

/**
 * Main function - coordinates entire program
 */
int main(int argc, char *argv[]) {
    // Setup signal handling
    signal(SIGINT, signal_handler);
    
    // Set locale for internationalization
    setlocale(LC_ALL, "");
    
    // Clear screen
    #ifdef _WIN32
        system("cls");
    #else
        system("clear");
    #endif
    
    // Check arguments
    if (argc > 1 && strcmp(argv[1], "--no-interactive") == 0) {
        interactive_mode = false;
    }
    
    // Main content
    print_header("ALBERT EINSTEIN: LIFETIME CONTRIBUTIONS TO RELATIVITY");
    print_timestamp();
    
    print_early_life();
    print_annus_mirabilis();
    print_special_relativity_details();
    print_general_relativity_details();
    
    interactive_relativity_demo();
    
    print_later_years();
    print_honors();
    
    // Using limits.h and float.h
    printf("\n=== NUMERICAL LIMITS ===\n");
    printf("Maximum double: %e\n", DBL_MAX);
    printf("Minimum double: %e\n", DBL_MIN);
    printf("Double precision: %d decimal digits\n", DBL_DIG);
    printf("Maximum int: %d\n", INT_MAX);
    
    // Using stdint.h types
    uint64_t universe_age = 13800000000ULL;
    printf("Universe age: %" PRIu64 " years\n", universe_age);
    
    // Check for interrupt
    if (signal_received) {
        printf("\nProgram interrupted by signal %d\n", signal_received);
    }
    
    print_header("END OF SUMMARY - E = mc²");
    
    printf("\n\"The important thing is not to stop questioning.\"\n");
    printf("- Albert Einstein\n\n");
    
    printf("Press Enter to exit...");
    while (getchar() != '\n');
    getchar();
    
    return 0;
}