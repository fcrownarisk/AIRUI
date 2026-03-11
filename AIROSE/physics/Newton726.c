/**
 * Program: newton_contributions_advanced.c
 * Description: Comprehensive summary of Isaac Newton's contributions to physics,
 *              mathematics (calculus and geometry), and optics using advanced C
 *              programming concepts with mathematical implementations.
 *
 * Libraries used: All C standard libraries with advanced math demonstrations
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <time.h>
#include <ctype.h>
#include <assert.h>
#include <errno.h>
#include <float.h>
#include <limits.h>
#include <locale.h>
#include <setjmp.h>
#include <signal.h>
#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <complex.h>
#include <fenv.h>
#include <inttypes.h>
#include <iso646.h>
#include <stdbool.h>
#include <tgmath.h>
#include <wchar.h>
#include <wctype.h>

#define G 6.67430e-11           // Gravitational constant
#define SOLAR_MASS 1.989e30     // Solar mass in kg
#define EARTH_RADIUS 6371000.0  // Earth radius in meters
#define PI M_PI                  // Pi constant

// Structure for mathematical functions
typedef struct {
    double (*function)(double);
    double (*derivative)(double);
    double (*integral)(double, double);
} MathFunction;

// Structure for point in 2D space
typedef struct {
    double x;
    double y;
} Point2D;

// Structure for polynomial terms
typedef struct PolynomialTerm {
    double coefficient;
    int exponent;
    struct PolynomialTerm* next;
} PolynomialTerm;

// Global variables
static jmp_buf calculus_error_buffer;
static PolynomialTerm* newton_series = NULL;

/**
 * Signal handler for floating point exceptions
 */
void fpe_signal_handler(int sig) {
    printf("\n⚠️ Floating Point Exception in Newton's calculations!\n");
    printf("Newton would say: 'Nature is pleased with simplicity'\n");
    exit(EXIT_FAILURE);
}

/**
 * Custom logging with variable arguments
 */
void newton_log(const char* format, ...) {
    va_list args;
    va_start(args, format);
    printf("📜 [Newton's Note]: ");
    vprintf(format, args);
    printf("\n");
    va_end(args);
}

/**
 * Prints decorative headers
 */
void print_newton_header(const char* title) {
    int len = strlen(title);
    printf("\n");
    for (int i = 0; i < len + 12; i++) printf("⚛️");
    printf("\n⚛️⚛️  %s  ⚛️⚛️\n", title);
    for (int i = 0; i < len + 12; i++) printf("⚛️");
    printf("\n");
}

/**
 * ============================================
 * PART 1: CALCULUS DEMONSTRATIONS
 * ============================================
 */

/**
 * Newton's method for finding roots: x_{n+1} = x_n - f(x_n)/f'(x_n)
 */
double newtons_method(double (*f)(double), double (*df)(double), 
                      double initial_guess, double tolerance, int max_iterations) {
    double x = initial_guess;
    printf("\n🔍 Newton's Method Iterations:\n");
    
    for (int i = 0; i < max_iterations; i++) {
        double fx = f(x);
        double dfx = df(x);
        
        if (fabs(dfx) < 1e-15) {
            newton_log("Derivative too small, method fails");
            return x;
        }
        
        double x_next = x - fx / dfx;
        printf("   Iter %d: x = %.10f, f(x) = %.10e\n", i+1, x_next, f(x_next));
        
        if (fabs(x_next - x) < tolerance) {
            printf("   ✅ Converged after %d iterations\n", i+1);
            return x_next;
        }
        x = x_next;
    }
    
    newton_log("Maximum iterations reached");
    return x;
}

/**
 * Example functions for Newton's method
 */
double example_func(double x) {
    return x * x * x - 2 * x - 5;  // x³ - 2x - 5 = 0
}

double example_derivative(double x) {
    return 3 * x * x - 2;  // 3x² - 2
}

/**
 * Numerical differentiation using Newton's forward difference formula
 */
double numerical_derivative(double (*f)(double), double x, double h) {
    // f'(x) ≈ (f(x+h) - f(x-h)) / (2h)
    return (f(x + h) - f(x - h)) / (2 * h);
}

/**
 * Numerical integration using Newton-Cotes formulas
 * Simpson's 1/3 rule
 */
double simpsons_rule(double (*f)(double), double a, double b, int n) {
    if (n % 2 != 0) n++;  // Simpson's rule requires even number of intervals
    
    double h = (b - a) / n;
    double sum = f(a) + f(b);
    
    for (int i = 1; i < n; i++) {
        double x = a + i * h;
        if (i % 2 == 0) {
            sum += 2 * f(x);
        } else {
            sum += 4 * f(x);
        }
    }
    
    return (h / 3) * sum;
}

/**
 * Newton's forward interpolation formula for discrete data points
 */
double newtons_forward_interpolation(Point2D* points, int n, double x) {
    double diff_table[n][n];
    
    // Initialize difference table with y values
    for (int i = 0; i < n; i++) {
        diff_table[i][0] = points[i].y;
    }
    
    // Calculate forward differences
    for (int j = 1; j < n; j++) {
        for (int i = 0; i < n - j; i++) {
            diff_table[i][j] = diff_table[i + 1][j - 1] - diff_table[i][j - 1];
        }
    }
    
    // Calculate interpolation
    double h = points[1].x - points[0].x;
    double p = (x - points[0].x) / h;
    double result = diff_table[0][0];
    double p_term = 1.0;
    
    for (int j = 1; j < n; j++) {
        p_term *= (p - (j - 1)) / j;
        result += p_term * diff_table[0][j];
    }
    
    return result;
}

/**
 * Binomial theorem implementation (Newton's generalization)
 */
double binomial_expansion(double x, double n, int terms) {
    // (1 + x)^n = Σ C(n,k) x^k
    double result = 1.0;
    double term = 1.0;
    
    printf("\n📊 Binomial Expansion (1 + %.2f)^%.2f:\n", x, n);
    
    for (int k = 1; k < terms; k++) {
        term *= (n - k + 1) * x / k;
        result += term;
        printf("   Term %d: %+.6f\n", k, term);
    }
    
    return result;
}

/**
 * ============================================
 * PART 2: GEOMETRY DEMONSTRATIONS
 * ============================================
 */

/**
 * Calculate area under a curve using Newton's method of exhaustion
 */
double method_of_exhaustion(double (*f)(double), double a, double b, int rectangles) {
    double width = (b - a) / rectangles;
    double area = 0.0;
    
    printf("\n📐 Method of Exhaustion (%d rectangles):\n", rectangles);
    
    for (int i = 0; i < rectangles; i++) {
        double x = a + i * width;
        double height = f(x);
        area += height * width;
        
        if (i < 5) {  // Show first few calculations
            printf("   Rectangle %d: height = %.4f, width = %.4f, area = %.4f\n", 
                   i+1, height, width, height * width);
        }
    }
    
    return area;
}

/**
 * Calculate properties of conic sections (Newton's work in Principia)
 */
typedef struct {
    char type[20];
    double eccentricity;
    double semi_major;
    double semi_minor;
    double focus_distance;
} ConicSection;

ConicSection analyze_conic_section(double eccentricity, double semi_major) {
    ConicSection conic;
    conic.eccentricity = eccentricity;
    conic.semi_major = semi_major;
    
    if (fabs(eccentricity) < 1e-10) {
        strcpy(conic.type, "Circle");
        conic.semi_minor = semi_major;
        conic.focus_distance = 0;
    } else if (eccentricity < 1.0) {
        strcpy(conic.type, "Ellipse");
        conic.semi_minor = semi_major * sqrt(1 - eccentricity * eccentricity);
        conic.focus_distance = semi_major * eccentricity;
    } else if (fabs(eccentricity - 1.0) < 1e-10) {
        strcpy(conic.type, "Parabola");
        conic.semi_minor = INFINITY;
        conic.focus_distance = semi_major;
    } else {
        strcpy(conic.type, "Hyperbola");
        conic.semi_minor = semi_major * sqrt(eccentricity * eccentricity - 1);
        conic.focus_distance = semi_major * eccentricity;
    }
    
    return conic;
}

/**
 * Newton's method for finding area of an ellipse
 */
double ellipse_area(double semi_major, double semi_minor) {
    return PI * semi_major * semi_minor;
}

/**
 * ============================================
 * PART 3: PHYSICS DEMONSTRATIONS
 * ============================================
 */

/**
 * Newton's Law of Universal Gravitation: F = G * m1 * m2 / r²
 */
double universal_gravitation(double m1, double m2, double distance) {
    return G * m1 * m2 / (distance * distance);
}

/**
 * Newton's Second Law: F = ma
 */
double newtons_second_law(double mass, double acceleration) {
    return mass * acceleration;
}

/**
 * Newton's Law of Cooling: T(t) = T_env + (T0 - T_env)e^{-kt}
 */
double newtons_cooling(double initial_temp, double env_temp, double k, double t) {
    return env_temp + (initial_temp - env_temp) * exp(-k * t);
}

/**
 * Calculate orbital period using Newton's form of Kepler's Third Law
 * T² = (4π²/GM) * a³
 */
double orbital_period(double semi_major_axis, double central_mass) {
    double period_squared = (4 * PI * PI / (G * central_mass)) * pow(semi_major_axis, 3);
    return sqrt(period_squared);
}

/**
 * Calculate escape velocity: v_escape = √(2GM/R)
 */
double escape_velocity(double mass, double radius) {
    return sqrt(2 * G * mass / radius);
}

/**
 * ============================================
 * PART 4: OPTICS DEMONSTRATIONS
 * ============================================
 */

/**
 * Newton's law of refraction (Snell's law, studied by Newton)
 */
double snells_law(double n1, double n2, double theta1) {
    // n1 sin(θ1) = n2 sin(θ2)
    double sin_theta2 = (n1 / n2) * sin(theta1);
    
    if (fabs(sin_theta2) > 1.0) {
        newton_log("Total internal reflection occurs");
        return NAN;
    }
    
    return asin(sin_theta2);
}

/**
 * Newton's ring pattern calculation
 * Radius of m-th ring: r_m = √(mλR)
 */
double newtons_ring_radius(int m, double lambda, double R) {
    return sqrt(m * lambda * R);
}

/**
 * Calculate dispersion (Newton's prism experiments)
 */
double dispersion_angle(double lambda, double A, double B, double C) {
    // Simplified Cauchy's equation: n(λ) = A + B/λ² + C/λ⁴
    double n = A + B / (lambda * lambda) + C / pow(lambda, 4);
    return n;  // Returns refractive index
}

/**
 * ============================================
 * PART 5: ADVANCED MATHEMATICAL DEMONSTRATIONS
 * ============================================
 */

/**
 * Newton's identities for power sums of roots
 */
void newtons_identities(double* coefficients, int degree) {
    printf("\n🔢 Newton's Identities for Polynomial:\n");
    printf("   P(x) = ");
    for (int i = degree; i >= 0; i--) {
        printf("%+.2fx^%d ", coefficients[i], i);
    }
    printf("\n");
    
    double power_sums[degree + 1];
    power_sums[0] = degree;  // Sum of 0th powers
    
    // Calculate power sums using Newton's identities
    for (int k = 1; k <= degree; k++) {
        power_sums[k] = 0;
        for (int i = 1; i <= k; i++) {
            if (i <= degree) {
                power_sums[k] += coefficients[degree - i] * power_sums[k - i];
            }
        }
        power_sums[k] = -power_sums[k] / coefficients[degree];
        printf("   s%d = %.4f (sum of %dth powers of roots)\n", 
               k, power_sums[k], k);
    }
}

/**
 * Newton's divided differences for interpolation
 */
double newtons_divided_differences(Point2D* points, int n, double x) {
    double diff[n][n];
    
    // Initialize with y values
    for (int i = 0; i < n; i++) {
        diff[i][0] = points[i].y;
    }
    
    // Calculate divided differences
    for (int j = 1; j < n; j++) {
        for (int i = 0; i < n - j; i++) {
            diff[i][j] = (diff[i+1][j-1] - diff[i][j-1]) / 
                         (points[i+j].x - points[i].x);
        }
    }
    
    // Evaluate interpolation polynomial
    double result = diff[0][0];
    double product = 1.0;
    
    for (int j = 1; j < n; j++) {
        product *= (x - points[j-1].x);
        result += diff[0][j] * product;
    }
    
    return result;
}

/**
 * Newton's series expansion (generalized binomial theorem)
 */
double newtons_series(double x, double alpha, int terms) {
    // (1 + x)^α = Σ C(α, k) x^k
    double result = 1.0;
    double term = 1.0;
    
    printf("\n📈 Newton's Generalized Binomial Series:\n");
    printf("   (1 + %.3f)^%.3f = ", x, alpha);
    
    for (int k = 1; k < terms; k++) {
        term *= (alpha - k + 1) * x / k;
        result += term;
        printf("%+.*f ", (int)(k > 3 ? 2 : 4), term);
        if (k < terms - 1) printf("+ ");
    }
    
    printf("\n   ≈ %.6f\n", result);
    return result;
}

/**
 * ============================================
 * PART 6: PRINCIPIA MATHEMATICA DEMONSTRATIONS
 * ============================================
 */

/**
 * Newton's derivation of Kepler's laws from gravitation
 */
void demonstrate_keplers_laws() {
    printf("\n📚 NEWTON'S PRINCIPIA MATHEMATICA (1687):\n");
    printf("   Book I: Motion of Bodies - Derivation of Kepler's Laws\n\n");
    
    // Orbital mechanics calculations
    double earth_orbit_radius = 1.496e11;  // 1 AU in meters
    double sun_mass = 1.989e30;
    double earth_mass = 5.972e24;
    
    double orbital_velocity = sqrt(G * sun_mass / earth_orbit_radius);
    double period = orbital_period(earth_orbit_radius, sun_mass);
    
    printf("   EARTH'S ORBIT (from Newton's laws):\n");
    printf("   • Orbital radius: %.2e m\n", earth_orbit_radius);
    printf("   • Orbital velocity: %.2f km/s\n", orbital_velocity / 1000);
    printf("   • Orbital period: %.2f days\n", period / (24 * 3600));
    
    // Verify Kepler's Third Law
    double t_squared = period * period;
    double a_cubed = pow(earth_orbit_radius, 3);
    double constant = t_squared / a_cubed;
    printf("   • T²/a³ = %.2e s²/m³ (constant for all planets)\n", constant);
}

/**
 * Newton's method for finding minima (optimization)
 */
double newtons_optimization(double (*f)(double), double (*df)(double), 
                            double (*d2f)(double), double initial, int iterations) {
    double x = initial;
    
    printf("\n🎯 Newton's Method for Optimization:\n");
    printf("   Finding minimum of function\n");
    
    for (int i = 0; i < iterations; i++) {
        double grad = df(x);
        double hess = d2f(x);
        
        if (fabs(hess) < 1e-10) {
            newton_log("Hessian too small, method fails");
            return x;
        }
        
        double step = -grad / hess;
        x = x + step;
        
        printf("   Iter %d: x = %.6f, f(x) = %.6f, gradient = %.6f\n", 
               i+1, x, f(x), grad);
        
        if (fabs(step) < 1e-8) {
            printf("   ✅ Converged to minimum\n");
            break;
        }
    }
    
    return x;
}

/**
 * Example quadratic function for optimization
 */
double quadratic_func(double x) {
    return x * x - 4 * x + 5;  // Minimum at x = 2
}

double quadratic_grad(double x) {
    return 2 * x - 4;
}

double quadratic_hess(double x) {
    return 2.0;  // Constant second derivative
}

/**
 * ============================================
 * MAIN FUNCTION: COMPREHENSIVE DEMONSTRATION
 * ============================================
 */

int main() {
    // Setup signal handling for floating point exceptions
    signal(SIGFPE, fpe_signal_handler);
    setlocale(LC_ALL, "");
    
    print_newton_header("SIR ISAAC NEWTON: CONTRIBUTIONS TO PHYSICS & MATHEMATICS");
    
    time_t now;
    time(&now);
    printf("\n⏰ Demonstration at: %s", ctime(&now));
    
    printf("\n\"If I have seen further, it is by standing on the shoulders of giants.\"\n");
    printf("— Isaac Newton (1675)\n");
    
    /* =========================================
       PART 1: CALCULUS DEMONSTRATIONS
       ========================================= */
    
    print_newton_header("CALCULUS: METHOD OF FLUXIONS");
    
    // Newton's method for finding roots
    double root = newtons_method(example_func, example_derivative, 2.0, 1e-10, 10);
    printf("\n✅ Root of x³ - 2x - 5 = 0: x = %.10f\n", root);
    
    // Numerical differentiation
    double deriv = numerical_derivative(sin, PI/4, 1e-6);
    printf("\n📐 Numerical derivative of sin(x) at π/4: %.6f (exact: %.6f)\n", 
           deriv, cos(PI/4));
    
    // Numerical integration
    double integral = simpsons_rule(sin, 0, PI, 100);
    printf("📊 ∫ sin(x) dx from 0 to π ≈ %.6f (exact: 2.0)\n", integral);
    
    // Binomial theorem
    binomial_expansion(0.5, 3, 5);
    
    /* =========================================
       PART 2: GEOMETRY DEMONSTRATIONS
       ========================================= */
    
    print_newton_header("GEOMETRY: CONIC SECTIONS & CURVES");
    
    // Method of exhaustion
    double area = method_of_exhaustion(sin, 0, PI, 1000);
    printf("\n✅ Area under sin(x) from 0 to π: %.6f\n", area);
    
    // Conic sections analysis
    ConicSection orbit = analyze_conic_section(0.967, 2.68e11);  // Halley's comet
    printf("\n🌌 Halley's Comet orbit analysis:\n");
    printf("   Type: %s\n", orbit.type);
    printf("   Eccentricity: %.3f\n", orbit.eccentricity);
    printf("   Semi-major axis: %.2e m\n", orbit.semi_major);
    printf("   Focus distance: %.2e m\n", orbit.focus_distance);
    
    // Interpolation
    Point2D points[] = {{0,1}, {1,2}, {2,4}, {3,8}};
    double interp = newtons_forward_interpolation(points, 4, 1.5);
    printf("\n📈 Newton's interpolation at x=1.5: y = %.4f\n", interp);
    
    /* =========================================
       PART 3: PHYSICS DEMONSTRATIONS
       ========================================= */
    
    print_newton_header("PHYSICS: PHILOSOPHIAE NATURALIS PRINCIPIA MATHEMATICA");
    
    // Universal gravitation
    double force = universal_gravitation(5.972e24, 7.348e22, 3.844e8);
    printf("🌍 Earth-Moon gravitational force: %.2e N\n", force);
    
    // Orbital mechanics
    double v_escape = escape_velocity(5.972e24, EARTH_RADIUS);
    printf("🚀 Earth escape velocity: %.2f km/s\n", v_escape / 1000);
    
    // Newton's laws of motion
    printf("\n⚡ Three Laws of Motion:\n");
    printf("   1. Law of Inertia: Every body persists in its state of rest\n");
    printf("      or uniform motion unless acted upon by a force.\n");
    printf("   2. F = ma: Force equals mass times acceleration.\n");
    printf("   3. Action-Reaction: For every action, there's an equal and opposite reaction.\n");
    
    /* =========================================
       PART 4: OPTICS DEMONSTRATIONS
       ========================================= */
    
    print_newton_header("OPTICS: DISPERSION & COLOR THEORY");
    
    // Refraction
    double theta2 = snells_law(1.0, 1.5, 30 * PI / 180);
    printf("🔆 Refraction angle (air to glass): %.2f°\n", theta2 * 180 / PI);
    
    // Newton's rings
    double ring = newtons_ring_radius(5, 589e-9, 1.0);
    printf("💿 5th Newton's ring radius: %.6f m\n", ring);
    
    // Dispersion
    printf("\n🌈 Newton's prism experiment (white light decomposition):\n");
    double lambda_red = 700e-9, lambda_blue = 400e-9;
    double n_red = dispersion_angle(lambda_red, 1.45, 3.5e-12, 2.1e-21);
    double n_blue = dispersion_angle(lambda_blue, 1.45, 3.5e-12, 2.1e-21);
    printf("   Refractive index for red light: %.4f\n", n_red);
    printf("   Refractive index for blue light: %.4f\n", n_blue);
    printf("   Dispersion (n_blue - n_red): %.4f\n", n_blue - n_red);
    
    /* =========================================
       PART 5: ADVANCED MATHEMATICS
       ========================================= */
    
    print_newton_header("ADVANCED MATHEMATICS: NEWTON'S IDENTITIES & SERIES");
    
    // Newton's identities
    double coeffs[] = {1, -6, 11, -6};  // (x-1)(x-2)(x-3) = x³ - 6x² + 11x - 6
    newtons_identities(coeffs, 3);
    
    // Newton's series
    newtons_series(0.25, 0.5, 6);  // (1 + 0.25)^0.5 = √1.25
    
    // Newton's optimization
    double min_x = newtons_optimization(quadratic_func, quadratic_grad, 
                                        quadratic_hess, 0.0, 10);
    printf("\n✅ Minimum of x² - 4x + 5 at x = %.6f\n", min_x);
    
    /* =========================================
       PART 6: PRINCIPIA DEMONSTRATION
       ========================================= */
    
    demonstrate_keplers_laws();
    
    /* =========================================
       PART 7: LEGACY AND CONCLUSION
       ========================================= */
    
    print_newton_header("NEWTON'S LEGACY");
    
    printf("📜 Major Works:\n");
    printf("• 1687: Philosophiæ Naturalis Principia Mathematica\n");
    printf("• 1704: Opticks\n");
    printf("• Method of Fluxions (calculus) - developed 1665-1666\n\n");
    
    printf("🏛️ Positions Held:\n");
    printf("• Lucasian Professor of Mathematics, Cambridge (1669-1696)\n");
    printf("• President of the Royal Society (1703-1727)\n");
    printf("• Master of the Royal Mint (1699-1727)\n");
    printf("• Knighted by Queen Anne (1705)\n\n");
    
    printf("💡 Key Contributions:\n");
    printf("• Calculus (independently developed with Leibniz)\n");
    printf("• Laws of motion and universal gravitation\n");
    printf("• Reflecting telescope (Newtonian telescope)\n");
    printf("• Theory of color and light dispersion\n");
    printf("• Binomial theorem generalization\n");
    printf("• Method for finding roots of equations\n");
    printf("• Fluid dynamics and acoustics\n");
    
    // Floating point limits
    printf("\n📊 Numerical Limits (from float.h):\n");
    printf("   Maximum finite double: %e\n", DBL_MAX);
    printf("   Smallest positive double: %e\n", DBL_MIN);
    printf("   Double epsilon: %e\n", DBL_EPSILON);
    printf("   Double mantissa digits: %d\n", DBL_MANT_DIG);
    
    print_newton_header("END OF DEMONSTRATION");
    
    printf("\n\"Truth is ever to be found in simplicity, and not in the\n");
    printf("multiplicity and confusion of things.\"\n");
    printf("— Isaac Newton\n\n");
    
    printf("Press Enter to exit...");
    while (getchar() != '\n');
    getchar();
    
    return 0;
}