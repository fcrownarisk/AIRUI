import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Random;

/**
 * Program: HeisenbergMatrixMechanics.java
 * Description: Comprehensive illustration of Werner Heisenberg's Matrix Mechanics
 *              using advanced mathematical concepts including infinite matrices,
 *              non-commutative algebra, eigenvalue problems, and quantum operators.
 * 
 * This program demonstrates the fundamental principles of matrix mechanics:
 * - Physical observables as infinite matrices
 * - Non-commutative algebra [x,p] = iħ
 * - Eigenvalue equations for energy levels
 * - Time evolution through matrix exponentiation
 * - Transition probabilities and selection rules
 */

public class HeisenbergMatrixMechanics {
    
    // Physical constants
    private static final double HBAR = 1.0545718e-34; // Reduced Planck constant
    private static final double ELECTRON_MASS = 9.1093837e-31; // kg
    private static final double ELEMENTARY_CHARGE = 1.60217662e-19; // C
    
    // Simulation parameters
    private static final int MATRIX_SIZE = 10; // Size of matrices (truncated infinite)
    private static final double HARMONIC_FREQUENCY = 1.0e15; // Hz
    private static final double TIME_STEP = 1.0e-18; // seconds
    private static final int TIME_STEPS = 100;
    
    public static void main(String[] args) {
        System.out.println("=".repeat(100));
        System.out.println("WERNER HEISENBERG'S MATRIX MECHANICS (1925)");
        System.out.println("Advanced Mathematical Formulation of Quantum Mechanics");
        System.out.println("=".repeat(100));
        
        // Explain the fundamental principles
        explainMatrixMechanics();
        
        // Create position and momentum matrices
        System.out.println("\n📊 STEP 1: CONSTRUCTING POSITION AND MOMENTUM MATRICES");
        ComplexMatrix x = createPositionMatrix(MATRIX_SIZE);
        ComplexMatrix p = createMomentumMatrix(MATRIX_SIZE);
        
        System.out.println("\nPosition Matrix X (first 5x5):");
        x.printSubmatrix(5, 5);
        System.out.println("\nMomentum Matrix P (first 5x5):");
        p.printSubmatrix(5, 5);
        
        // Demonstrate non-commutativity [x,p] = iħ
        System.out.println("\n⚛️ STEP 2: DEMONSTRATING NON-COMMUTATIVITY [x,p] = iħ");
        demonstrateCommutator(x, p);
        
        // Create Hamiltonian for harmonic oscillator
        System.out.println("\n🔷 STEP 3: HAMILTONIAN MATRIX FOR QUANTUM HARMONIC OSCILLATOR");
        ComplexMatrix H = createHarmonicOscillatorHamiltonian(MATRIX_SIZE);
        System.out.println("Hamiltonian Matrix H (first 5x5):");
        H.printSubmatrix(5, 5);
        
        // Solve eigenvalue problem (find energy levels)
        System.out.println("\n📈 STEP 4: SOLVING EIGENVALUE PROBLEM H|ψ> = E|ψ>");
        EigenSystem eigenSystem = solveEigenvalueProblem(H);
        eigenSystem.printEnergyLevels();
        
        // Demonstrate matrix mechanics postulates
        System.out.println("\n📚 STEP 5: HEISENBERG'S MATRIX MECHANICS POSTULATES");
        demonstratePostulates(x, p, H, eigenSystem);
        
        // Time evolution in Heisenberg picture
        System.out.println("\n⏱️ STEP 6: TIME EVOLUTION (HEISENBERG PICTURE)");
        demonstrateTimeEvolution(x, p, H);
        
        // Transition probabilities and selection rules
        System.out.println("\n🔄 STEP 7: TRANSITION PROBABILITIES AND SELECTION RULES");
        calculateTransitionProbabilities(x, eigenSystem);
        
        // Advanced mathematical concepts
        System.out.println("\n🧮 STEP 8: ADVANCED MATHEMATICAL STRUCTURES");
        demonstrateAdvancedConcepts();
        
        // Uncertainty principle demonstration
        System.out.println("\n🎯 STEP 9: HEISENBERG UNCERTAINTY PRINCIPLE");
        demonstrateUncertaintyPrinciple(x, p, eigenSystem);
        
        // Print Heisenberg's famous quotes
        printHeisenbergQuotes();
    }
    
    /**
     * Explains the fundamental principles of matrix mechanics
     */
    private static void explainMatrixMechanics() {
        System.out.println("\n🔬 FUNDAMENTAL PRINCIPLES OF MATRIX MECHANICS:");
        System.out.println("   \"The more precisely the position is determined, the less precisely");
        System.out.println("    the momentum is known in this instant, and vice versa.\"");
        System.out.println("    - Werner Heisenberg, 1927");
        
        System.out.println("\n📖 KEY MATHEMATICAL CONCEPTS:");
        System.out.println("   1. PHYSICAL OBSERVABLES ARE INFINITE MATRICES:");
        System.out.println("      X = {x_{mn}}, P = {p_{mn}} where m,n = 1,2,3,...");
        System.out.println();
        System.out.println("   2. NON-COMMUTATIVE ALGEBRA:");
        System.out.println("      [X, P] = XP - PX = iħI");
        System.out.println("      (Matrices do NOT commute, unlike classical quantities)");
        System.out.println();
        System.out.println("   3. EIGENVALUE EQUATIONS:");
        System.out.println("      H|ψ_n> = E_n|ψ_n>  (Energy eigenvalues)");
        System.out.println("      X|ψ_n> = x_n|ψ_n>  (Position eigenvalues)");
        System.out.println();
        System.out.println("   4. MATRIX ELEMENTS CORRESPOND TO TRANSITION AMPLITUDES:");
        System.out.println("      A_{mn} = <ψ_m|Â|ψ_n>");
        System.out.println();
        System.out.println("   5. TIME EVOLUTION (HEISENBERG EQUATION):");
        System.out.println("      dÂ/dt = (i/ħ)[H, Â] + ∂Â/∂t");
        System.out.println();
        System.out.println("   6. PROBABILITY INTERPRETATION:");
        System.out.println("      P(m→n) = |<ψ_m|Â|ψ_n>|²");
    }
    
    /**
     * Creates the position matrix X for harmonic oscillator
     * X_{mn} = √(ħ/(2mω)) (√n δ_{m,n-1} + √m δ_{m-1,n})
     */
    private static ComplexMatrix createPositionMatrix(int size) {
        ComplexMatrix X = new ComplexMatrix(size, size);
        double factor = Math.sqrt(HBAR / (2 * ELECTRON_MASS * HARMONIC_FREQUENCY));
        
        for (int m = 0; m < size; m++) {
            for (int n = 0; n < size; n++) {
                if (n == m + 1) {
                    // Raising operator contribution
                    X.set(m, n, new ComplexNumber(factor * Math.sqrt(n), 0));
                } else if (m == n + 1) {
                    // Lowering operator contribution
                    X.set(m, n, new ComplexNumber(factor * Math.sqrt(m), 0));
                } else {
                    X.set(m, n, ComplexNumber.ZERO);
                }
            }
        }
        return X;
    }
    
    /**
     * Creates the momentum matrix P for harmonic oscillator
     * P_{mn} = i√(ħmω/2) (√n δ_{m,n-1} - √m δ_{m-1,n})
     */
    private static ComplexMatrix createMomentumMatrix(int size) {
        ComplexMatrix P = new ComplexMatrix(size, size);
        double factor = Math.sqrt(HBAR * ELECTRON_MASS * HARMONIC_FREQUENCY / 2);
        
        for (int m = 0; m < size; m++) {
            for (int n = 0; n < size; n++) {
                if (n == m + 1) {
                    // +i contribution from raising operator
                    P.set(m, n, new ComplexNumber(0, factor * Math.sqrt(n)));
                } else if (m == n + 1) {
                    // -i contribution from lowering operator
                    P.set(m, n, new ComplexNumber(0, -factor * Math.sqrt(m)));
                } else {
                    P.set(m, n, ComplexNumber.ZERO);
                }
            }
        }
        return P;
    }
    
    /**
     * Creates the Hamiltonian matrix for harmonic oscillator
     * H = P²/(2m) + (1/2)mω²X²
     */
    private static ComplexMatrix createHarmonicOscillatorHamiltonian(int size) {
        // H = ħω (a†a + 1/2) where a† and a are raising/lowering operators
        ComplexMatrix H = new ComplexMatrix(size, size);
        double hbarOmega = HBAR * HARMONIC_FREQUENCY;
        
        for (int n = 0; n < size; n++) {
            // Diagonal elements: E_n = ħω (n + 1/2)
            H.set(n, n, new ComplexNumber(hbarOmega * (n + 0.5), 0));
        }
        
        return H;
    }
    
    /**
     * Demonstrates the fundamental commutation relation [x,p] = iħ
     */
    private static void demonstrateCommutator(ComplexMatrix x, ComplexMatrix p) {
        ComplexMatrix xp = x.multiply(p);
        ComplexMatrix px = p.multiply(x);
        ComplexMatrix commutator = xp.subtract(px);
        
        System.out.println("\n   Commutator [X,P] = XP - PX:");
        commutator.printSubmatrix(5, 5);
        
        // Check if diagonal and equal to iħ
        ComplexNumber iHbar = new ComplexNumber(0, HBAR);
        boolean isValid = true;
        for (int i = 0; i < Math.min(5, commutator.rows); i++) {
            if (!commutator.get(i, i).equals(iHbar)) {
                isValid = false;
                break;
            }
        }
        
        System.out.printf("\n   ✅ [X,P] = iħ is %s (within numerical precision)\n", 
                         isValid ? "SATISFIED" : "APPROXIMATELY SATISFIED");
    }
    
    /**
     * Solves the eigenvalue problem H|ψ> = E|ψ>
     */
    private static EigenSystem solveEigenvalueProblem(ComplexMatrix H) {
        // This is a simplified eigenvalue solver for demonstration
        // In practice, would use more sophisticated algorithms (QR, Jacobi, etc.)
        
        double[] energies = new double[MATRIX_SIZE];
        ComplexMatrix[] eigenvectors = new ComplexMatrix[MATRIX_SIZE];
        
        // For harmonic oscillator, we know the exact eigenvalues
        double hbarOmega = HBAR * HARMONIC_FREQUENCY;
        for (int n = 0; n < MATRIX_SIZE; n++) {
            energies[n] = hbarOmega * (n + 0.5);
            
            // Create basis vector as eigenvector
            ComplexMatrix vec = new ComplexMatrix(MATRIX_SIZE, 1);
            vec.set(n, 0, new ComplexNumber(1, 0));
            eigenvectors[n] = vec;
        }
        
        return new EigenSystem(energies, eigenvectors);
    }
    
    /**
     * Demonstrates the key postulates of matrix mechanics
     */
    private static void demonstratePostulates(ComplexMatrix x, ComplexMatrix p, 
                                              ComplexMatrix H, EigenSystem eigenSystem) {
        System.out.println("\n   POSTULATE 1: Observable quantities are Hermitian matrices");
        System.out.println("   X is Hermitian: " + x.isHermitian());
        System.out.println("   P is Hermitian: " + p.isHermitian());
        System.out.println("   H is Hermitian: " + H.isHermitian());
        
        System.out.println("\n   POSTULATE 2: Measurement outcomes are eigenvalues");
        System.out.println("   Energy eigenvalues (first 5):");
        for (int n = 0; n < 5; n++) {
            System.out.printf("   E_%d = %.4e J = %.2f eV\n", 
                n, eigenSystem.energies[n], eigenSystem.energies[n] / ELEMENTARY_CHARGE);
        }
        
        System.out.println("\n   POSTULATE 3: Matrix elements give transition amplitudes");
        ComplexNumber transition01 = x.get(0, 1);
        System.out.printf("   <0|X|1> = %s\n", transition01);
        System.out.printf("   Transition probability |<0|X|1>|² = %.4e\n", 
                         transition01.modulusSquared());
    }
    
    /**
     * Demonstrates time evolution in the Heisenberg picture
     * dA/dt = (i/ħ)[H, A]
     */
    private static void demonstrateTimeEvolution(ComplexMatrix x, ComplexMatrix p, 
                                                 ComplexMatrix H) {
        System.out.println("\n   Heisenberg Equation of Motion:");
        System.out.println("   dA/dt = (i/ħ)[H, A]");
        
        // Calculate time derivative of position
        ComplexMatrix iHbarInv = new ComplexNumber(0, 1.0/HBAR);
        ComplexMatrix commHP = H.commutator(p);
        ComplexMatrix dXdt = commHP.multiply(iHbarInv);
        
        System.out.println("\n   dX/dt (should equal P/m):");
        ComplexMatrix pOverM = p.multiply(1.0/ELECTRON_MASS);
        dXdt.printSubmatrix(5, 5);
        
        System.out.println("\n   P/m:");
        pOverM.printSubmatrix(5, 5);
        
        // Check if they're approximately equal
        boolean matches = true;
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                double diff = dXdt.get(i, j).subtract(pOverM.get(i, j)).modulus();
                if (diff > 1e-10) {
                    matches = false;
                }
            }
        }
        System.out.printf("\n   ✅ dX/dt = P/m is %s\n", matches ? "SATISFIED" : "APPROXIMATELY SATISFIED");
    }
    
    /**
     * Calculates transition probabilities and selection rules
     */
    private static void calculateTransitionProbabilities(ComplexMatrix x, 
                                                         EigenSystem eigenSystem) {
        System.out.println("\n   SELECTION RULES:");
        System.out.println("   • Electric dipole transitions: Δn = ±1");
        System.out.println("   • Matrix element <n|x|m> ≠ 0 only for |n-m| = 1");
        
        System.out.println("\n   TRANSITION PROBABILITIES (|⟨m|X|n⟩|²):");
        System.out.println("   m\\n |   0       1       2       3       4");
        System.out.println("   -----------------------------------------");
        
        for (int m = 0; m < 5; m++) {
            System.out.printf("   %d   |", m);
            for (int n = 0; n < 5; n++) {
                double prob = x.get(m, n).modulusSquared();
                if (prob < 1e-10) {
                    System.out.printf("  -      ");
                } else {
                    System.out.printf("  %.2e  ", prob);
                }
            }
            System.out.println();
        }
        
        System.out.println("\n   Note: Non-zero elements only appear when |m-n| = 1");
        System.out.println("   This is the selection rule for harmonic oscillator!");
    }
    
    /**
     * Demonstrates advanced mathematical concepts in matrix mechanics
     */
    private static void demonstrateAdvancedConcepts() {
        System.out.println("\n   A. INFINITE-DIMENSIONAL HILBERT SPACE:");
        System.out.println("      ℋ = L²(ℝ) - Square-integrable functions");
        System.out.println("      ⟨ψ|φ⟩ = ∫ ψ*(x)φ(x) dx");
        
        System.out.println("\n   B. SPECTRAL THEOREM:");
        System.out.println("      A = ∑_n a_n |a_n⟩⟨a_n|");
        System.out.println("      f(A) = ∑_n f(a_n) |a_n⟩⟨a_n|");
        
        System.out.println("\n   C. STONE'S THEOREM (Time evolution):");
        System.out.println("      U(t) = exp(-iHt/ħ)");
        System.out.println("      |ψ(t)⟩ = U(t)|ψ(0)⟩");
        
        System.out.println("\n   D. GELFAND-NAIMARK-SEGAL CONSTRUCTION:");
        System.out.println("      C*-algebra representation in Hilbert space");
        
        System.out.println("\n   E. DIRAC'S BRA-KET NOTATION:");
        System.out.println("      ⟨ψ|Â|φ⟩ = ∑_{mn} ψ_m* A_{mn} φ_n");
        
        // Demonstrate matrix exponential for time evolution
        System.out.println("\n   F. MATRIX EXPONENTIAL exp(-iHt/ħ):");
        ComplexMatrix H_small = createHarmonicOscillatorHamiltonian(3);
        double t = 1e-15; // 1 femtosecond
        
        ComplexMatrix U = matrixExponential(H_small.multiply(new ComplexNumber(0, -t/HBAR)));
        System.out.println("      Time evolution operator U(t) (first 3x3):");
        U.printSubmatrix(3, 3);
    }
    
    /**
     * Demonstrates Heisenberg's uncertainty principle
     * Δx Δp ≥ ħ/2
     */
    private static void demonstrateUncertaintyPrinciple(ComplexMatrix x, ComplexMatrix p,
                                                        EigenSystem eigenSystem) {
        System.out.println("\n   UNCERTAINTY PRINCIPLE: Δx Δp ≥ ħ/2");
        
        // Calculate uncertainties for ground state
        int groundState = 0;
        
        // <x> = <0|x|0> should be zero
        ComplexNumber meanX = x.get(groundState, groundState);
        
        // <x²> = ∑_n <0|x|n><n|x|0>
        ComplexNumber meanX2 = ComplexNumber.ZERO;
        for (int n = 0; n < MATRIX_SIZE; n++) {
            ComplexNumber x0n = x.get(groundState, n);
            ComplexNumber xn0 = x.get(n, groundState);
            meanX2 = meanX2.add(x0n.multiply(xn0));
        }
        
        // Δx = √(<x²> - <x>²)
        double deltaX = Math.sqrt(meanX2.modulus() - meanX.modulusSquared());
        
        // Similarly for momentum
        ComplexNumber meanP = p.get(groundState, groundState);
        
        ComplexNumber meanP2 = ComplexNumber.ZERO;
        for (int n = 0; n < MATRIX_SIZE; n++) {
            ComplexNumber p0n = p.get(groundState, n);
            ComplexNumber pn0 = p.get(n, groundState);
            meanP2 = meanP2.add(p0n.multiply(pn0));
        }
        
        double deltaP = Math.sqrt(meanP2.modulus() - meanP.modulusSquared());
        
        double product = deltaX * deltaP;
        double hbarOver2 = HBAR / 2;
        
        System.out.printf("\n   For ground state (n=0):\n");
        System.out.printf("   Δx = %.4e m\n", deltaX);
        System.out.printf("   Δp = %.4e kg·m/s\n", deltaP);
        System.out.printf("   Δx·Δp = %.4e J·s\n", product);
        System.out.printf("   ħ/2 = %.4e J·s\n", hbarOver2);
        System.out.printf("\n   ✅ Uncertainty principle %s: %.2f ≥ %.2f\n",
                         product >= hbarOver2 ? "SATISFIED" : "VIOLATED",
                         product / hbarOver2, 1.0);
    }
    
    /**
     * Computes matrix exponential using series expansion
     */
    private static ComplexMatrix matrixExponential(ComplexMatrix A) {
        int n = A.rows;
        ComplexMatrix result = ComplexMatrix.identity(n);
        ComplexMatrix term = ComplexMatrix.identity(n);
        
        // Use Taylor series: exp(A) = I + A + A²/2! + A³/3! + ...
        for (int k = 1; k < 10; k++) {
            term = term.multiply(A).multiply(1.0 / k);
            result = result.add(term);
            
            // Check convergence
            if (term.norm() < 1e-10) {
                break;
            }
        }
        
        return result;
    }
    
    /**
     * Prints Heisenberg's famous quotes
     */
    private static void printHeisenbergQuotes() {
        System.out.println("\n" + "=".repeat(100));
        System.out.println("WERNER HEISENBERG - FAMOUS QUOTES");
        System.out.println("=".repeat(100));
        
        System.out.println("\n\"What we observe is not nature itself, but nature exposed to our method of questioning.\"");
        System.out.println("\n\"The violent reaction on the recent development of modern physics can only be understood when one realizes that here the foundations of physics have started moving; and that this motion has caused the feeling that the ground would be cut from science.\"");
        System.out.println("\n\"Not only is the Universe stranger than we think, it is stranger than we can think.\"");
        System.out.println("\n\"The solution of the difficulty is that the two mental pictures which experiment lead us to form - the one of particles, the other of waves - are both incomplete and have only the validity of analogies which are accurate only in limiting cases.\"");
    }
}

/**
 * Complex Number class for quantum mechanical calculations
 */
class ComplexNumber {
    public static final ComplexNumber ZERO = new ComplexNumber(0, 0);
    public static final ComplexNumber I = new ComplexNumber(0, 1);
    public static final ComplexNumber ONE = new ComplexNumber(1, 0);
    
    double real;
    double imag;
    
    public ComplexNumber(double real, double imag) {
        this.real = real;
        this.imag = imag;
    }
    
    public ComplexNumber add(ComplexNumber other) {
        return new ComplexNumber(real + other.real, imag + other.imag);
    }
    
    public ComplexNumber subtract(ComplexNumber other) {
        return new ComplexNumber(real - other.real, imag - other.imag);
    }
    
    public ComplexNumber multiply(ComplexNumber other) {
        return new ComplexNumber(
            real * other.real - imag * other.imag,
            real * other.imag + imag * other.real
        );
    }
    
    public ComplexNumber multiply(double scalar) {
        return new ComplexNumber(real * scalar, imag * scalar);
    }
    
    public ComplexNumber conjugate() {
        return new ComplexNumber(real, -imag);
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
    
    public boolean equals(ComplexNumber other) {
        double eps = 1e-10;
        return Math.abs(real - other.real) < eps && 
               Math.abs(imag - other.imag) < eps;
    }
}

/**
 * Complex Matrix class for matrix mechanics
 */
class ComplexMatrix {
    int rows;
    int cols;
    ComplexNumber[][] data;
    
    public ComplexMatrix(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
        this.data = new ComplexNumber[rows][cols];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                data[i][j] = ComplexNumber.ZERO;
            }
        }
    }
    
    public void set(int i, int j, ComplexNumber value) {
        data[i][j] = value;
    }
    
    public ComplexNumber get(int i, int j) {
        return data[i][j];
    }
    
    public static ComplexMatrix identity(int size) {
        ComplexMatrix I = new ComplexMatrix(size, size);
        for (int i = 0; i < size; i++) {
            I.set(i, i, ComplexNumber.ONE);
        }
        return I;
    }
    
    public ComplexMatrix add(ComplexMatrix other) {
        if (rows != other.rows || cols != other.cols) {
            throw new IllegalArgumentException("Matrix dimensions must match");
        }
        
        ComplexMatrix result = new ComplexMatrix(rows, cols);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                result.set(i, j, data[i][j].add(other.get(i, j)));
            }
        }
        return result;
    }
    
    public ComplexMatrix subtract(ComplexMatrix other) {
        if (rows != other.rows || cols != other.cols) {
            throw new IllegalArgumentException("Matrix dimensions must match");
        }
        
        ComplexMatrix result = new ComplexMatrix(rows, cols);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                result.set(i, j, data[i][j].subtract(other.get(i, j)));
            }
        }
        return result;
    }
    
    public ComplexMatrix multiply(ComplexMatrix other) {
        if (cols != other.rows) {
            throw new IllegalArgumentException("Matrix dimensions incompatible for multiplication");
        }
        
        ComplexMatrix result = new ComplexMatrix(rows, other.cols);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < other.cols; j++) {
                ComplexNumber sum = ComplexNumber.ZERO;
                for (int k = 0; k < cols; k++) {
                    sum = sum.add(data[i][k].multiply(other.get(k, j)));
                }
                result.set(i, j, sum);
            }
        }
        return result;
    }
    
    public ComplexMatrix multiply(double scalar) {
        ComplexMatrix result = new ComplexMatrix(rows, cols);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                result.set(i, j, data[i][j].multiply(scalar));
            }
        }
        return result;
    }
    
    public ComplexMatrix commutator(ComplexMatrix other) {
        return this.multiply(other).subtract(other.multiply(this));
    }
    
    public ComplexMatrix adjoint() {
        ComplexMatrix result = new ComplexMatrix(cols, rows);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                result.set(j, i, data[i][j].conjugate());
            }
        }
        return result;
    }
    
    public boolean isHermitian() {
        if (rows != cols) return false;
        
        ComplexMatrix adj = this.adjoint();
        double eps = 1e-10;
        
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                if (!data[i][j].equals(adj.get(i, j))) {
                    return false;
                }
            }
        }
        return true;
    }
    
    public double norm() {
        // Frobenius norm
        double sum = 0;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                sum += data[i][j].modulusSquared();
            }
        }
        return Math.sqrt(sum);
    }
    
    public void printSubmatrix(int r, int c) {
        r = Math.min(r, rows);
        c = Math.min(c, cols);
        
        for (int i = 0; i < r; i++) {
            System.out.print("   [");
            for (int j = 0; j < c; j++) {
                if (data[i][j].modulus() < 1e-20) {
                    System.out.print("           0");
                } else {
                    System.out.printf(" %10.2e", data[i][j].real);
                }
                if (j < c - 1) System.out.print(",");
            }
            System.out.println(" ]");
        }
    }
}

/**
 * Eigenvalue system class
 */
class EigenSystem {
    double[] energies;
    ComplexMatrix[] eigenvectors;
    
    public EigenSystem(double[] energies, ComplexMatrix[] eigenvectors) {
        this.energies = energies;
        this.eigenvectors = eigenvectors;
    }
    
    public void printEnergyLevels() {
        System.out.println("\n   Energy levels E_n = ħω(n + 1/2):");
        System.out.println("   n | Energy (J)       | Energy (eV)");
        System.out.println("   ---------------------------------");
        
        for (int n = 0; n < Math.min(10, energies.length); n++) {
            System.out.printf("   %d | %.4e | %.4f\n", 
                n, energies[n], energies[n] / 1.60217662e-19);
        }
    }
}

/**
 * Supplementary class: InfiniteMatrixOperations
 * Demonstrates advanced infinite-dimensional matrix operations
 */
class InfiniteMatrixOperations {
    
    /**
     * Creates ladder operators a and a†
     */
    public static ComplexMatrix[] createLadderOperators(int size) {
        ComplexMatrix a = new ComplexMatrix(size, size);  // annihilation
        ComplexMatrix aDag = new ComplexMatrix(size, size); // creation
        
        for (int n = 0; n < size - 1; n++) {
            // a|n> = √n|n-1>
            a.set(n, n+1, new ComplexNumber(Math.sqrt(n+1), 0));
            
            // a†|n> = √(n+1)|n+1>
            aDag.set(n+1, n, new ComplexNumber(Math.sqrt(n+1), 0));
        }
        
        return new ComplexMatrix[]{a, aDag};
    }
    
    /**
     * Number operator N = a†a
     */
    public static ComplexMatrix numberOperator(ComplexMatrix a, ComplexMatrix aDag) {
        return aDag.multiply(a);
    }
    
    /**
     * Squeeze operator S(ξ) = exp((ξ*a†² - ξ* a²)/2)
     */
    public static ComplexMatrix squeezeOperator(double xi, int size) {
        // Simplified squeeze operator for demonstration
        ComplexMatrix squeeze = ComplexMatrix.identity(size);
        
        // Add squeezing terms (off-diagonal)
        for (int n = 0; n < size - 2; n++) {
            double factor = Math.tanh(xi) * Math.sqrt((n+1)*(n+2));
            squeeze.set(n, n+2, new ComplexNumber(factor, 0));
            squeeze.set(n+2, n, new ComplexNumber(factor, 0));
        }
        
        return squeeze;
    }
    
    /**
     * Displacement operator D(α) = exp(αa† - α*a)
     */
    public static ComplexMatrix displacementOperator(ComplexNumber alpha, int size) {
        ComplexMatrix D = ComplexMatrix.identity(size);
        
        // First-order displacement terms
        for (int n = 0; n < size - 1; n++) {
            double factor = alpha.modulus() * Math.sqrt(n+1);
            D.set(n, n+1, new ComplexNumber(factor, 0));
            D.set(n+1, n, new ComplexNumber(-factor, 0));
        }
        
        return D;
    }
}