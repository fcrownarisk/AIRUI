import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Random;

/**
 * Program: SchrodingerWaveMechanics.java
 * Description: Comprehensive illustration of Erwin Schrödinger's Wave Mechanics
 *              and the Schrödinger Equation, incorporating Hamilton's classical
 *              mechanics and the Hamiltonian operator.
 * 
 * This program demonstrates:
 * - The Schrödinger equation and wave function ψ(x,t)
 * - Hamiltonian operator Ĥ = -ħ²/2m ∇² + V(x)
 * - Hamilton-Jacobi theory connection
 * - Wave function interpretation (Born rule)
 * - Time evolution of quantum states
 * - Eigenvalue problems and stationary states
 */

public class SchrodingerWaveMechanics {
    
    // Physical constants
    private static final double HBAR = 1.0545718e-34; // Reduced Planck constant (J·s)
    private static final double ELECTRON_MASS = 9.1093837e-31; // kg
    private static final double ELEMENTARY_CHARGE = 1.60217662e-19; // C
    private static final double PI = Math.PI;
    
    // Simulation parameters
    private static final int SPATIAL_POINTS = 1000; // Number of spatial grid points
    private static final double X_MIN = -10.0e-9; // -10 nm
    private static final double X_MAX = 10.0e-9; // 10 nm
    private static final double DX = (X_MAX - X_MIN) / SPATIAL_POINTS;
    private static final double DT = 1.0e-18; // 1 attosecond time step
    private static final int TIME_STEPS = 100;
    
    // Potential parameters
    private static final double BARRIER_WIDTH = 1.0e-9; // 1 nm
    private static final double BARRIER_HEIGHT = 10.0 * ELEMENTARY_CHARGE; // 10 eV
    
    public static void main(String[] args) {
        System.out.println("=".repeat(100));
        System.out.println("ERWIN SCHRÖDINGER'S WAVE MECHANICS (1926)");
        System.out.println("The Wave Equation of Quantum Mechanics");
        System.out.println("=".repeat(100));
        
        // Explain the fundamental principles
        explainWaveMechanics();
        
        // Create spatial grid
        double[] x = createSpatialGrid();
        
        // Create initial wave packet (Gaussian)
        System.out.println("\n📦 STEP 1: CREATING INITIAL WAVE PACKET");
        Complex[] psi = createGaussianWavePacket(x, 0, 2.0e-9, 1.0e6);
        plotWaveFunction(x, psi, "Initial Wave Packet");
        
        // Demonstrate the Hamiltonian operator
        System.out.println("\n🔷 STEP 2: THE HAMILTONIAN OPERATOR");
        demonstrateHamiltonian(x);
        
        // Solve time-independent Schrödinger equation
        System.out.println("\n📈 STEP 3: TIME-INDEPENDENT SCHRÖDINGER EQUATION");
        solveTimeIndependentSchrodinger(x);
        
        // Time evolution of wave function
        System.out.println("\n⏱️ STEP 4: TIME-DEPENDENT SCHRÖDINGER EQUATION");
        psi = timeEvolution(psi, x);
        
        // Demonstrate Hamilton-Jacobi connection
        System.out.println("\n🔗 STEP 5: HAMILTON-JACOBI CONNECTION");
        demonstrateHamiltonJacobiConnection(psi, x);
        
        // Probability interpretation (Born rule)
        System.out.println("\n🎲 STEP 6: BORN RULE - PROBABILITY INTERPRETATION");
        demonstrateBornRule(psi, x);
        
        // Demonstrate superposition principle
        System.out.println("\n🌀 STEP 7: SUPERPOSITION PRINCIPLE");
        demonstrateSuperposition(x);
        
        // Demonstrate tunneling effect
        System.out.println("\n🚇 STEP 8: QUANTUM TUNNELING");
        demonstrateTunneling(x);
        
        // Demonstrate uncertainty principle
        System.out.println("\n🎯 STEP 9: HEISENBERG UNCERTAINTY FROM WAVE MECHANICS");
        demonstrateUncertaintyFromWaveFunction(psi, x);
        
        // Print Schrödinger's famous quotes
        printSchrodingerQuotes();
    }
    
    /**
     * Explains the fundamental principles of wave mechanics
     */
    private static void explainWaveMechanics() {
        System.out.println("\n🔬 FOUNDATIONS OF WAVE MECHANICS:");
        System.out.println("   \"The idea that material particles are waves originated with me.\"");
        System.out.println("   - Erwin Schrödinger, 1926");
        
        System.out.println("\n📖 THE SCHRÖDINGER EQUATION:");
        System.out.println("   TIME-DEPENDENT FORM:");
        System.out.println("   iħ ∂ψ/∂t = Ĥψ");
        System.out.println("   where Ĥ = -ħ²/2m ∇² + V(x,t)");
        System.out.println();
        System.out.println("   TIME-INDEPENDENT FORM:");
        System.out.println("   Ĥψ = Eψ");
        System.out.println();
        
        System.out.println("   HAMILTONIAN OPERATOR (from Hamilton's classical mechanics):");
        System.out.println("   Classical: H(p,q) = p²/2m + V(q)");
        System.out.println("   Quantum:   Ĥ = -ħ²/2m ∂²/∂x² + V(x)");
        System.out.println();
        
        System.out.println("   WAVE FUNCTION ψ(x,t):");
        System.out.println("   • Complex-valued function of space and time");
        System.out.println("   • Contains all information about the quantum state");
        System.out.println("   • |ψ(x,t)|² = probability density (Born rule)");
        System.out.println("   • ∫|ψ|² dx = 1 (normalization)");
    }
    
    /**
     * Creates spatial grid
     */
    private static double[] createSpatialGrid() {
        double[] x = new double[SPATIAL_POINTS];
        for (int i = 0; i < SPATIAL_POINTS; i++) {
            x[i] = X_MIN + i * DX;
        }
        return x;
    }
    
    /**
     * Creates a Gaussian wave packet
     * ψ(x) = (1/√(2πσ²))^(1/2) * exp(-(x-x₀)²/(4σ²)) * exp(ip₀x/ħ)
     */
    private static Complex[] createGaussianWavePacket(double[] x, double x0, 
                                                      double sigma, double p0) {
        Complex[] psi = new Complex[x.length];
        double norm = 1.0 / Math.sqrt(Math.sqrt(2 * PI * sigma * sigma));
        
        for (int i = 0; i < x.length; i++) {
            double dx = x[i] - x0;
            double gaussian = Math.exp(-dx * dx / (4 * sigma * sigma));
            double phase = p0 * dx / HBAR;
            
            double real = norm * gaussian * Math.cos(phase);
            double imag = norm * gaussian * Math.sin(phase);
            
            psi[i] = new Complex(real, imag);
        }
        
        // Normalize
        return normalize(psi, DX);
    }
    
    /**
     * Normalizes a wave function
     */
    private static Complex[] normalize(Complex[] psi, double dx) {
        double norm = 0;
        for (Complex c : psi) {
            norm += c.modulusSquared() * dx;
        }
        norm = Math.sqrt(norm);
        
        Complex[] normalized = new Complex[psi.length];
        for (int i = 0; i < psi.length; i++) {
            normalized[i] = psi[i].multiply(1.0 / norm);
        }
        
        return normalized;
    }
    
    /**
     * Plots wave function (simplified ASCII visualization)
     */
    private static void plotWaveFunction(double[] x, Complex[] psi, String title) {
        System.out.println("\n   " + title + ":");
        System.out.println("   x (nm)    |ψ|²    Re(ψ)    Im(ψ)");
        System.out.println("   ---------------------------------");
        
        int[] indices = {100, 300, 500, 700, 900}; // Sample points
        
        for (int idx : indices) {
            if (idx < psi.length) {
                System.out.printf("   %6.2f  | %6.4f  %6.4f  %6.4f\n",
                    x[idx] * 1e9,
                    psi[idx].modulusSquared(),
                    psi[idx].real,
                    psi[idx].imag);
            }
        }
    }
    
    /**
     * Demonstrates the Hamiltonian operator and its action on wave functions
     */
    private static void demonstrateHamiltonian(double[] x) {
        System.out.println("\n   HAMILTONIAN OPERATOR: Ĥ = -ħ²/2m ∂²/∂x² + V(x)");
        
        // Create a test wave function (Gaussian)
        Complex[] testPsi = createGaussianWavePacket(x, 0, 1.0e-9, 0);
        
        // Apply kinetic operator
        Complex[] kineticPart = applyKineticOperator(testPsi, x);
        
        // Apply potential operator (infinite square well for demonstration)
        double[] V = createInfiniteSquareWell(x);
        Complex[] potentialPart = applyPotentialOperator(testPsi, V);
        
        // Full Hamiltonian
        Complex[] Hpsi = new Complex[x.length];
        for (int i = 0; i < x.length; i++) {
            Hpsi[i] = kineticPart[i].add(potentialPart[i]);
        }
        
        System.out.println("\n   Action of Hamiltonian on Gaussian wave packet:");
        System.out.println("   Position | ψ(x) | Ĥψ(x)");
        System.out.println("   ---------------------------------");
        
        for (int i = 400; i < 600; i += 50) {
            System.out.printf("   %6.2f nm | %6.4f | %6.4f\n",
                x[i] * 1e9,
                testPsi[i].modulus(),
                Hpsi[i].modulus());
        }
    }
    
    /**
     * Applies the kinetic operator -ħ²/2m ∂²/∂x² to wave function
     */
    private static Complex[] applyKineticOperator(Complex[] psi, double[] x) {
        Complex[] result = new Complex[psi.length];
        double coeff = -HBAR * HBAR / (2 * ELECTRON_MASS * DX * DX);
        
        // Second derivative using finite difference
        for (int i = 1; i < psi.length - 1; i++) {
            Complex d2psi = psi[i+1].add(psi[i-1]).subtract(psi[i].multiply(2));
            result[i] = d2psi.multiply(coeff);
        }
        
        // Boundaries (set to zero)
        result[0] = Complex.ZERO;
        result[psi.length - 1] = Complex.ZERO;
        
        return result;
    }
    
    /**
     * Applies potential operator V(x) to wave function
     */
    private static Complex[] applyPotentialOperator(Complex[] psi, double[] V) {
        Complex[] result = new Complex[psi.length];
        for (int i = 0; i < psi.length; i++) {
            result[i] = psi[i].multiply(V[i]);
        }
        return result;
    }
    
    /**
     * Creates infinite square well potential
     */
    private static double[] createInfiniteSquareWell(double[] x) {
        double[] V = new double[x.length];
        double wellWidth = 5.0e-9; // 5 nm
        
        for (int i = 0; i < x.length; i++) {
            if (Math.abs(x[i]) < wellWidth/2) {
                V[i] = 0; // Inside well
            } else {
                V[i] = 1e10 * ELEMENTARY_CHARGE; // Effectively infinite
            }
        }
        return V;
    }
    
    /**
     * Solves time-independent Schrödinger equation for infinite square well
     */
    private static void solveTimeIndependentSchrodinger(double[] x) {
        System.out.println("\n   TIME-INDEPENDENT SCHRÖDINGER EQUATION: Ĥψ = Eψ");
        System.out.println("   For infinite square well of width L = 5 nm:");
        
        double L = 5.0e-9;
        
        System.out.println("\n   Energy eigenvalues: E_n = n²π²ħ²/(2mL²)");
        System.out.println("   n | Energy (eV) | Wave function ψ_n(x)");
        System.out.println("   ---------------------------------------");
        
        for (int n = 1; n <= 5; n++) {
            double En = n * n * PI * PI * HBAR * HBAR / (2 * ELECTRON_MASS * L * L);
            En /= ELEMENTARY_CHARGE; // Convert to eV
            
            System.out.printf("   %d  |    %.3f    | √(2/L) sin(%dπx/L)\n", n, En, n);
        }
        
        // Display wave function at specific point
        System.out.println("\n   Wave function values at x = L/4:");
        for (int n = 1; n <= 3; n++) {
            double psi = Math.sqrt(2/L) * Math.sin(n * PI / 4);
            System.out.printf("   ψ_%d(L/4) = %.4f\n", n, psi);
        }
    }
    
    /**
     * Time evolution using split-operator method
     */
    private static Complex[] timeEvolution(Complex[] psi, double[] x) {
        System.out.println("\n   TIME-DEPENDENT SCHRÖDINGER EQUATION:");
        System.out.println("   iħ ∂ψ/∂t = Ĥψ");
        
        // Create a copy for time evolution
        Complex[] psi_t = psi.clone();
        
        // Simple time evolution (first order)
        for (int t = 0; t < TIME_STEPS; t++) {
            // Apply kinetic part in Fourier space (simplified)
            Complex[] kinetic = applyKineticOperator(psi_t, x);
            
            // Apply potential part
            double[] V = createInfiniteSquareWell(x);
            
            // Update wave function: ψ(t+dt) = ψ(t) - (i/ħ) Ĥψ dt
            for (int i = 0; i < psi_t.length; i++) {
                Complex Hpsi = kinetic[i].add(psi_t[i].multiply(V[i]));
                Complex change = Hpsi.multiply(new Complex(0, -DT / HBAR));
                psi_t[i] = psi_t[i].add(change);
            }
            
            // Re-normalize
            if (t % 20 == 0) {
                psi_t = normalize(psi_t, DX);
                System.out.printf("   Time t = %.2f as: norm = %.6f\n", 
                    t * DT * 1e18, calculateNorm(psi_t, DX));
            }
        }
        
        return psi_t;
    }
    
    /**
     * Calculates norm of wave function
     */
    private static double calculateNorm(Complex[] psi, double dx) {
        double norm = 0;
        for (Complex c : psi) {
            norm += c.modulusSquared() * dx;
        }
        return Math.sqrt(norm);
    }
    
    /**
     * Demonstrates connection between Schrödinger equation and Hamilton-Jacobi theory
     */
    private static void demonstrateHamiltonJacobiConnection(Complex[] psi, double[] x) {
        System.out.println("\n   HAMILTON-JACOBI CONNECTION:");
        System.out.println("   Writing ψ = R exp(iS/ħ):");
        System.out.println("   • R(x,t) = √ρ (amplitude)");
        System.out.println("   • S(x,t) = phase (action function)");
        
        // Extract amplitude and phase from wave function
        double[] R = new double[psi.length];
        double[] S = new double[psi.length];
        
        for (int i = 0; i < psi.length; i++) {
            R[i] = Math.sqrt(psi[i].modulusSquared());
            S[i] = psi[i].phase() * HBAR;
        }
        
        System.out.println("\n   Substituting into Schrödinger equation gives:");
        System.out.println("   1. CONTINUITY EQUATION (probability conservation):");
        System.out.println("      ∂ρ/∂t + ∇·(ρ ∇S/m) = 0");
        System.out.println();
        System.out.println("   2. QUANTUM HAMILTON-JACOBI EQUATION:");
        System.out.println("      ∂S/∂t + (∇S)²/(2m) + V - (ħ²/2m)(∇²R/R) = 0");
        System.out.println("      ↑              ↑              ↑");
        System.out.println("   classical      classical     quantum potential");
        System.out.println("   Hamilton-      kinetic +");
        System.out.println("   Jacobi eq.     potential");
        
        // Show quantum potential at sample points
        System.out.println("\n   Quantum potential Q = -(ħ²/2m)(∇²R/R) at selected points:");
        for (int i = 400; i < 600; i += 50) {
            double Q = -HBAR * HBAR / (2 * ELECTRON_MASS) * 
                      estimateSecondDerivative(R, i, DX) / (R[i] + 1e-20);
            System.out.printf("   x = %.2f nm: Q = %.4e eV\n", 
                x[i] * 1e9, Q / ELEMENTARY_CHARGE);
        }
    }
    
    /**
     * Estimates second derivative for quantum potential
     */
    private static double estimateSecondDerivative(double[] f, int i, double dx) {
        if (i <= 0 || i >= f.length - 1) return 0;
        return (f[i+1] + f[i-1] - 2*f[i]) / (dx * dx);
    }
    
    /**
     * Demonstrates Born rule and probability interpretation
     */
    private static void demonstrateBornRule(Complex[] psi, double[] x) {
        System.out.println("\n   BORN RULE: P = |ψ|² (probability density)");
        
        // Calculate probability in different regions
        double totalProb = 0;
        double leftProb = 0;
        double centerProb = 0;
        double rightProb = 0;
        
        int center = x.length / 2;
        int regionSize = x.length / 3;
        
        for (int i = 0; i < x.length; i++) {
            double prob = psi[i].modulusSquared() * DX;
            totalProb += prob;
            
            if (i < regionSize) {
                leftProb += prob;
            } else if (i < 2 * regionSize) {
                centerProb += prob;
            } else {
                rightProb += prob;
            }
        }
        
        System.out.printf("\n   Probability distribution:\n");
        System.out.printf("   Left region  (x < %.2f nm): P = %.4f\n", 
            x[regionSize] * 1e9, leftProb);
        System.out.printf("   Center region (%.2f - %.2f nm): P = %.4f\n",
            x[regionSize] * 1e9, x[2*regionSize] * 1e9, centerProb);
        System.out.printf("   Right region (x > %.2f nm): P = %.4f\n",
            x[2*regionSize] * 1e9, rightProb);
        System.out.printf("   Total probability: %.4f (should be 1.0)\n", totalProb);
        
        // Expected value of position
        double expX = 0;
        for (int i = 0; i < x.length; i++) {
            expX += x[i] * psi[i].modulusSquared() * DX;
        }
        System.out.printf("\n   Expected position ⟨x⟩ = %.2f nm\n", expX * 1e9);
    }
    
    /**
     * Demonstrates superposition principle
     */
    private static void demonstrateSuperposition(double[] x) {
        System.out.println("\n   SUPERPOSITION PRINCIPLE:");
        System.out.println("   ψ = c₁ψ₁ + c₂ψ₂ (linear combination)");
        
        // Create two stationary states
        Complex[] psi1 = createStationaryState(x, 1, 5.0e-9);
        Complex[] psi2 = createStationaryState(x, 2, 5.0e-9);
        
        // Create superposition: ψ = (ψ₁ + ψ₂)/√2
        Complex[] psi_super = new Complex[x.length];
        for (int i = 0; i < x.length; i++) {
            psi_super[i] = psi1[i].add(psi2[i]).multiply(1.0 / Math.sqrt(2));
        }
        
        System.out.println("\n   Superposition of n=1 and n=2 states:");
        System.out.println("   Position | |ψ₁|²  | |ψ₂|²  | |ψ_super|²");
        System.out.println("   ----------------------------------------");
        
        for (int i = 400; i < 600; i += 50) {
            System.out.printf("   %6.2f nm | %6.4f | %6.4f | %6.4f\n",
                x[i] * 1e9,
                psi1[i].modulusSquared(),
                psi2[i].modulusSquared(),
                psi_super[i].modulusSquared());
        }
        
        System.out.println("\n   Note: |ψ_super|² ≠ |ψ₁|² + |ψ₂|²");
        System.out.println("   Interference terms appear: 2Re(ψ₁*ψ₂)");
    }
    
    /**
     * Creates a stationary state for infinite square well
     */
    private static Complex[] createStationaryState(double[] x, int n, double L) {
        Complex[] psi = new Complex[x.length];
        
        for (int i = 0; i < x.length; i++) {
            if (Math.abs(x[i]) < L/2) {
                double value = Math.sqrt(2/L) * Math.sin(n * PI * (x[i] + L/2) / L);
                psi[i] = new Complex(value, 0);
            } else {
                psi[i] = Complex.ZERO;
            }
        }
        
        return psi;
    }
    
    /**
     * Demonstrates quantum tunneling
     */
    private static void demonstrateTunneling(double[] x) {
        System.out.println("\n   QUANTUM TUNNELING:");
        System.out.println("   Particle can penetrate classically forbidden regions");
        
        // Create barrier potential
        double[] V = createBarrierPotential(x);
        
        // Create incident wave packet
        Complex[] psi_incident = createGaussianWavePacket(x, -4.0e-9, 0.5e-9, 2.0e6);
        
        // Calculate transmission coefficient (simplified)
        double k = Math.sqrt(2 * ELECTRON_MASS * 5 * ELEMENTARY_CHARGE) / HBAR;
        double kappa = Math.sqrt(2 * ELECTRON_MASS * (BARRIER_HEIGHT - 5 * ELEMENTARY_CHARGE)) / HBAR;
        
        double T = 1.0 / (1.0 + (Math.sinh(kappa * BARRIER_WIDTH) * 
                      Math.sinh(kappa * BARRIER_WIDTH)) / 
                      (4 * (k/kappa - kappa/k) * (k/kappa - kappa/k)));
        
        System.out.printf("\n   Barrier height: %.2f eV\n", BARRIER_HEIGHT / ELEMENTARY_CHARGE);
        System.out.printf("   Barrier width: %.2f nm\n", BARRIER_WIDTH * 1e9);
        System.out.printf("   Particle energy: %.2f eV\n", 5.0);
        System.out.printf("   Transmission probability: %.4f\n", T);
        System.out.printf("   Classical prediction: 0.0\n");
        
        System.out.println("\n   The wave function inside the barrier decays exponentially:");
        System.out.println("   ψ(x) ∝ exp(-κx) where κ = √(2m(V₀-E))/ħ");
    }
    
    /**
     * Creates a rectangular barrier potential
     */
    private static double[] createBarrierPotential(double[] x) {
        double[] V = new double[x.length];
        double barrierCenter = 0;
        
        for (int i = 0; i < x.length; i++) {
            if (Math.abs(x[i] - barrierCenter) < BARRIER_WIDTH/2) {
                V[i] = BARRIER_HEIGHT;
            } else {
                V[i] = 0;
            }
        }
        return V;
    }
    
    /**
     * Demonstrates uncertainty principle from wave mechanics
     */
    private static void demonstrateUncertaintyFromWaveFunction(Complex[] psi, double[] x) {
        System.out.println("\n   HEISENBERG UNCERTAINTY FROM WAVE MECHANICS:");
        
        // Calculate ⟨x⟩, ⟨x²⟩
        double meanX = 0;
        double meanX2 = 0;
        
        for (int i = 0; i < x.length; i++) {
            double prob = psi[i].modulusSquared() * DX;
            meanX += x[i] * prob;
            meanX2 += x[i] * x[i] * prob;
        }
        
        double deltaX = Math.sqrt(meanX2 - meanX * meanX);
        
        // Calculate ⟨p⟩, ⟨p²⟩ from wave function
        Complex[] dpsi = new Complex[psi.length];
        for (int i = 1; i < psi.length - 1; i++) {
            dpsi[i] = psi[i+1].subtract(psi[i-1]).multiply(1.0 / (2 * DX));
        }
        
        double meanP = 0;
        double meanP2 = 0;
        
        for (int i = 0; i < psi.length; i++) {
            // ⟨p⟩ = -iħ ∫ ψ* (dψ/dx) dx
            Complex pPsi = dpsi[i].multiply(new Complex(0, -HBAR));
            meanP += (psi[i].conjugate().multiply(pPsi)).real * DX;
            
            // ⟨p²⟩ = -ħ² ∫ ψ* (d²ψ/dx²) dx
            Complex d2psi = i > 0 && i < psi.length - 1 ? 
                psi[i+1].add(psi[i-1]).subtract(psi[i].multiply(2)).multiply(1.0/(DX*DX)) :
                Complex.ZERO;
            Complex p2Psi = d2psi.multiply(-HBAR * HBAR);
            meanP2 += (psi[i].conjugate().multiply(p2Psi)).real * DX;
        }
        
        double deltaP = Math.sqrt(meanP2 - meanP * meanP);
        double product = deltaX * deltaP;
        double hbarOver2 = HBAR / 2;
        
        System.out.printf("\n   Position uncertainty: Δx = %.4e m\n", deltaX);
        System.out.printf("   Momentum uncertainty: Δp = %.4e kg·m/s\n", deltaP);
        System.out.printf("   Δx·Δp = %.4e J·s\n", product);
        System.out.printf("   ħ/2 = %.4e J·s\n", hbarOver2);
        System.out.printf("\n   ✅ Δx·Δp ≥ ħ/2 is %s: %.2f ≥ 1.0\n",
            product >= hbarOver2 ? "SATISFIED" : "VIOLATED",
            product / hbarOver2);
        
        System.out.println("\n   This uncertainty emerges from the wave nature:");
        System.out.println("   • Localized wave packet (small Δx) needs many Fourier components");
        System.out.println("   • Many Fourier components means large Δp");
        System.out.println("   • Δx·Δp ≥ ħ/2 is a mathematical property of Fourier transforms");
    }
    
    /**
     * Prints Schrödinger's famous quotes
     */
    private static void printSchrodingerQuotes() {
        System.out.println("\n" + "=".repeat(100));
        System.out.println("ERWIN SCHRÖDINGER - FAMOUS QUOTES");
        System.out.println("=".repeat(100));
        
        System.out.println("\n\"The wave function is the means for predicting probability of measurement results.\"");
        System.out.println("\n\"I do not like it, and I am sorry I ever had anything to do with quantum theory.\"");
        System.out.println("(About the probabilistic interpretation, despite creating wave mechanics)");
        System.out.println("\n\"The task is... not to see what no one has yet seen, but to think what nobody has yet thought, about that which everybody sees.\"");
        System.out.println("\n\"The wave function evolves deterministically according to the Schrödinger equation.\"");
    }
}

/**
 * Complex Number class for wave function calculations
 */
class Complex {
    public static final Complex ZERO = new Complex(0, 0);
    public static final Complex ONE = new Complex(1, 0);
    public static final Complex I = new Complex(0, 1);
    
    double real;
    double imag;
    
    public Complex(double real, double imag) {
        this.real = real;
        this.imag = imag;
    }
    
    public Complex add(Complex other) {
        return new Complex(real + other.real, imag + other.imag);
    }
    
    public Complex subtract(Complex other) {
        return new Complex(real - other.real, imag - other.imag);
    }
    
    public Complex multiply(Complex other) {
        return new Complex(
            real * other.real - imag * other.imag,
            real * other.imag + imag * other.real
        );
    }
    
    public Complex multiply(double scalar) {
        return new Complex(real * scalar, imag * scalar);
    }
    
    public Complex conjugate() {
        return new Complex(real, -imag);
    }
    
    public double modulus() {
        return Math.hypot(real, imag);
    }
    
    public double modulusSquared() {
        return real * real + imag * imag;
    }
    
    public double phase() {
        return Math.atan2(imag, real);
    }
    
    @Override
    public String toString() {
        if (Math.abs(imag) < 1e-20) {
            return String.format("%.4e", real);
        } else if (Math.abs(real) < 1e-20) {
            return String.format("%.4ei", imag);
        } else {
            return String.format("%.4e %+.4ei", real, imag);
        }
    }
}

/**
 * Supplementary class: HamiltonJacobiTheory
 * Demonstrates deeper connection between Hamilton and Schrödinger
 */
class HamiltonJacobiTheory {
    
    /**
     * Classical Hamilton-Jacobi equation
     * ∂S/∂t + H(q, ∂S/∂q, t) = 0
     */
    public static void classicalHamiltonJacobi() {
        System.out.println("\n   CLASSICAL HAMILTON-JACOBI THEORY:");
        System.out.println("   Hamilton's principal function S(q,t) satisfies:");
        System.out.println("   ∂S/∂t + H(q, ∂S/∂q, t) = 0");
        System.out.println("   where p = ∂S/∂q");
    }
    
    /**
     * Derivation of Schrödinger equation from Hamilton-Jacobi
     */
    public static void deriveSchrodingerFromHJ() {
        System.out.println("\n   DERIVATION OF SCHRÖDINGER EQUATION:");
        System.out.println("   1. Start with Hamilton-Jacobi: ∂S/∂t + (∇S)²/(2m) + V = 0");
        System.out.println("   2. Introduce wave function: ψ = √ρ exp(iS/ħ)");
        System.out.println("   3. Define probability density: ρ = |ψ|²");
        System.out.println("   4. Quantum potential emerges: Q = -ħ²/(2m) ∇²√ρ/√ρ");
        System.out.println("   5. Modified HJ: ∂S/∂t + (∇S)²/(2m) + V + Q = 0");
        System.out.println("   6. Combine with continuity equation: ∂ρ/∂t + ∇·(ρ∇S/m) = 0");
        System.out.println("   7. These two equations are equivalent to Schrödinger equation!");
    }
    
    /**
     * Hamilton's characteristic function for stationary states
     */
    public static void characteristicFunction() {
        System.out.println("\n   FOR STATIONARY STATES (ψ = u(x) exp(-iEt/ħ)):");
        System.out.println("   S(x,t) = W(x) - Et");
        System.out.println("   where W(x) is Hamilton's characteristic function");
        System.out.println("   Then: (∇W)²/(2m) + V - E = -Q");
    }
    
    /**
     * Optical-mechanical analogy (Hamilton's insight)
     */
    public static void opticalMechanicalAnalogy() {
        System.out.println("\n   HAMILTON'S OPTICAL-MECHANICAL ANALOGY:");
        System.out.println("   • Light rays follow Fermat's principle: δ∫ n ds = 0");
        System.out.println("   • Particles follow Maupertuis' principle: δ∫ p dq = 0");
        System.out.println("   • Wave optics → Geometric optics as λ → 0");
        System.out.println("   • Wave mechanics → Classical mechanics as ħ → 0");
        System.out.println("   • Refractive index n ∝ √(E - V)");
    }
}

/**
 * Supplementary class: QuantumOperators
 * Demonstrates operator formalism
 */
class QuantumOperators {
    
    /**
     * Position operator X
     */
    public static double positionOperator(double x, Complex psi) {
        return x * psi.modulusSquared();
    }
    
    /**
     * Momentum operator P = -iħ ∂/∂x
     */
    public static Complex momentumOperator(Complex[] psi, int i, double dx) {
        if (i <= 0 || i >= psi.length - 1) return Complex.ZERO;
        return psi[i+1].subtract(psi[i-1]).multiply(new Complex(0, -HBAR / (2 * dx)));
    }
    
    /**
     * Hamiltonian operator H = P²/(2m) + V
     */
    public static Complex hamiltonianOperator(Complex[] psi, int i, double[] V, double dx) {
        // Kinetic part: -ħ²/(2m) ∂²/∂x²
        Complex kinetic = Complex.ZERO;
        if (i > 0 && i < psi.length - 1) {
            kinetic = psi[i+1].add(psi[i-1]).subtract(psi[i].multiply(2))
                     .multiply(-HBAR * HBAR / (2 * ELECTRON_MASS * dx * dx));
        }
        
        // Potential part
        Complex potential = psi[i].multiply(V[i]);
        
        return kinetic.add(potential);
    }
    
    /**
     * Commutator [X, P] = iħ
     */
    public static Complex commutatorXP(double x, Complex[] psi, int i, double dx) {
        // X P ψ
        Complex Psi = momentumOperator(psi, i, dx);
        Complex XPpsi = Psi.multiply(x);
        
        // P X ψ
        Complex Xpsi = psi[i].multiply(x);
        // Need derivative of Xψ for P X ψ (simplified)
        Complex PXpsi = Complex.ZERO;
        
        return XPpsi.subtract(PXpsi);
    }
}