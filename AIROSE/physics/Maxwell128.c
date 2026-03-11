/**
 * Program: maxwell_summary.c
 * Description: Summarizes James Clerk Maxwell's contributions to electromagnetism,
 *              using multiple C standard libraries to present the information,
 *              compute related quantities, and provide a timestamped output.
 *
 * Libraries used: stdio.h, stdlib.h, string.h, math.h, time.h
 */

#include <stdio.h>   // Standard I/O
#include <stdlib.h>  // Memory allocation, system commands
#include <string.h>  // String manipulation
#include <math.h>    // Mathematical constants and functions
#include <time.h>    // Time functions

#define MU0 (4.0 * M_PI * 1e-7)      // Permeability of free space (H/m)
#define EPS0 (8.8541878128e-12)      // Permittivity of free space (F/m)

/**
 * Prints a decorative header.
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
 * Prints the date and time of execution.
 */
void print_timestamp() {
    time_t rawtime;
    struct tm *timeinfo;
    char buffer[80];

    time(&rawtime);
    timeinfo = localtime(&rawtime);
    strftime(buffer, sizeof(buffer), "%Y-%m-%d %H:%M:%S", timeinfo);
    printf("\n[Summary generated on: %s]\n", buffer);
}

/**
 * Prints the core contributions: Maxwell's equations and unification.
 */
void print_core_contributions() {
    printf("1. UNIFICATION OF ELECTRICITY AND MAGNETISM\n");
    printf("   - Showed that electric and magnetic fields are two aspects\n");
    printf("     of the same fundamental force: electromagnetism.\n\n");

    printf("2. MAXWELL'S EQUATIONS\n");
    printf("   - Formulated four partial differential equations:\n");
    printf("     a) Gauss's law for electricity: ∇·E = ρ/ε₀\n");
    printf("     b) Gauss's law for magnetism:   ∇·B = 0\n");
    printf("     c) Faraday's law of induction:  ∇×E = –∂B/∂t\n");
    printf("     d) Ampère's law (with Maxwell's addition): ∇×B = μ₀J + μ₀ε₀ ∂E/∂t\n\n");

    printf("3. DISPLACEMENT CURRENT\n");
    printf("   - Introduced the term μ₀ε₀ ∂E/∂t to Ampère's law,\n");
    printf("     ensuring charge conservation and enabling wave solutions.\n\n");
}

/**
 * Computes and displays the speed of light from μ₀ and ε₀,
 * demonstrating Maxwell's prediction.
 */
void compute_speed_of_light() {
    double c = 1.0 / sqrt(MU0 * EPS0);
    printf("4. PREDICTION OF ELECTROMAGNETIC WAVES\n");
    printf("   - Derived the wave equation from his equations.\n");
    printf("   - Calculated the wave speed as c = 1/√(μ₀ε₀)\n");
    printf("     Using μ₀ = 4π×10⁻⁷ H/m and ε₀ ≈ %.4e F/m,\n", EPS0);
    printf("     we obtain c ≈ %.3e m/s.\n", c);
    printf("   - This matched the known speed of light, leading to the\n");
    printf("     conclusion that light is an electromagnetic wave.\n\n");
}

/**
 * Lists other important contributions by Maxwell.
 */
void print_other_contributions() {
    printf("5. OTHER CONTRIBUTIONS\n");
    printf("   - Color vision and the first color photograph.\n");
    printf("   - Kinetic theory of gases (Maxwell–Boltzmann distribution).\n");
    printf("   - Thermodynamics and statistical mechanics.\n");
    printf("   - Work on optics and the theory of Saturn's rings.\n\n");
}

/**
 * Prints a closing statement and a famous quote.
 */
void print_conclusion() {
    printf("6. LEGACY\n");
    printf("   - Maxwell's work laid the foundation for modern physics,\n");
    printf("     including special relativity and quantum electrodynamics.\n");
    printf("   - Albert Einstein said: \"The work of James Clerk Maxwell changed\n");
    printf("     the world forever.\"\n");
}

/**
 * Main function: brings everything together.
 */
int main() {
    // Clear screen (optional, for a cleaner look)
    #ifdef _WIN32
        system("cls");
    #else
        system("clear");
    #endif

    print_header("JAMES CLERK MAXWELL: CONTRIBUTIONS TO ELECTROMAGNETISM");
    print_timestamp();

    print_core_contributions();
    compute_speed_of_light();
    print_other_contributions();
    print_conclusion();

    print_header("END OF SUMMARY");

    // Pause before exiting (useful when run from an IDE)
    printf("\nPress Enter to exit...");
    getchar();

    return 0;
}