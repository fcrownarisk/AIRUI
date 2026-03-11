/**
 * Program: FeynmanPathIntegralDoubleSlit.java
 * Description: Demonstrates Richard Feynman's Path Integral formulation of quantum mechanics
 *              and the double-slit experiment phenomenon using Java.
 * 
 * The program simulates the double-slit experiment by summing over multiple paths
 * (Feynman's sum over histories) to show how interference patterns emerge.
 */

import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import javax.swing.*;
import java.awt.*;
import java.awt.geom.*;

/**
 * Main class demonstrating Feynman's Path Integral and Double-Slit Interference
 */
public class FeynmanPathIntegralDoubleSlit extends JPanel {
    
    // Physical constants
    private static final double PLANCK_CONSTANT = 6.626e-34; // J·s
    private static final double HBAR = PLANCK_CONSTANT / (2 * Math.PI);
    private static final double ELECTRON_MASS = 9.109e-31; // kg
    private static final double SPEED_OF_LIGHT = 2.998e8; // m/s
    
    // Experiment parameters
    private double slitWidth = 0.5e-6; // 0.5 micrometers
    private double slitSeparation = 2.0e-6; // 2 micrometers
    private double screenDistance = 1.0; // 1 meter
    private double wavelength = 500e-9; // 500 nm (green light)
    private int numPaths = 1000; // Number of paths to sum over
    private int numPhotons = 10000; // Number of photons to simulate
    
    // Visualization parameters
    private int width = 800;
    private int height = 600;
    private double[] intensity;
    private List<PhotonHit> hits;
    
    /**
     * Represents a quantum particle (photon or electron)
     */
    static class QuantumParticle {
        double x, y;           // Position
        double px, py;         // Momentum
        double mass;           // Mass
        double wavelength;     // de Broglie wavelength
        double phase;          // Quantum phase
        
        public QuantumParticle(double mass, double energy) {
            this.mass = mass;
            this.wavelength = PLANCK_CONSTANT / Math.sqrt(2 * mass * energy);
        }
        
        public QuantumParticle(double wavelength) {
            this.mass = 0; // Photon
            this.wavelength = wavelength;
        }
    }
    
    /**
     * Represents a photon hit on the screen
     */
    static class PhotonHit {
        double x;              // x-coordinate on screen
        double y;              // y-coordinate on screen
        double probability;     // Quantum probability
        double phase;          // Phase at hit
        
        public PhotonHit(double x, double y, double prob, double phase) {
            this.x = x;
            this.y = y;
            this.probability = prob;
            this.phase = phase;
        }
    }
    
    /**
     * Represents a path in Feynman's path integral
     */
    static class QuantumPath {
        List<Point2D.Double> points = new ArrayList<>();
        double action;          // Classical action along path
        double amplitude;       // Quantum amplitude (e^(iS/ħ))
        double phase;          // Phase of amplitude
        
        public void addPoint(double x, double y) {
            points.add(new Point2D.Double(x, y));
        }
        
        public double calculateLength() {
            double length = 0;
            for (int i = 0; i < points.size() - 1; i++) {
                Point2D.Double p1 = points.get(i);
                Point2D.Double p2 = points.get(i + 1);
                length += Math.hypot(p2.x - p1.x, p2.y - p1.y);
            }
            return length;
        }
    }
    
    public FeynmanPathIntegralDoubleSlit() {
        setPreferredSize(new Dimension(width, height));
        setBackground(Color.BLACK);
        hits = new ArrayList<>();
        intensity = new double[width];
        simulateDoubleSlit();
    }
    
    /**
     * Simulates the double-slit experiment using Feynman's path integral
     */
    private void simulateDoubleSlit() {
        System.out.println("=".repeat(80));
        System.out.println("RICHARD FEYNMAN'S PATH INTEGRAL FORMULATION");
        System.out.println("DOUBLE-SLIT INTERFERENCE SIMULATION");
        System.out.println("=".repeat(80));
        
        // Print experimental setup
        System.out.println("\n📐 EXPERIMENTAL SETUP:");
        System.out.printf("   Slit width: %.2f μm\n", slitWidth * 1e6);
        System.out.printf("   Slit separation: %.2f μm\n", slitSeparation * 1e6);
        System.out.printf("   Screen distance: %.2f m\n", screenDistance);
        System.out.printf("   Wavelength: %.1f nm\n", wavelength * 1e9);
        System.out.printf("   Number of paths: %d\n", numPaths);
        System.out.printf("   Number of photons: %d\n", numPhotons);
        
        // Explain Feynman's principle
        explainFeynmanPathIntegral();
        
        // Perform the path integral summation
        performPathIntegral();
        
        // Simulate photon hits
        simulatePhotonDetections();
        
        // Analyze results
        analyzeInterferencePattern();
    }
    
    /**
     * Explains the basic principle behind Feynman's path integral
     */
    private void explainFeynmanPathIntegral() {
        System.out.println("\n🔬 FEYNMAN'S PATH INTEGRAL PRINCIPLE:");
        System.out.println("   \"A particle takes every possible path from source to screen.\"");
        System.out.println("   - Richard Feynman, QED: The Strange Theory of Light and Matter");
        
        System.out.println("\n📖 BASIC PRINCIPLES:");
        System.out.println("   1. SUPERPOSITION: A quantum particle exists in all possible paths");
        System.out.println("   2. PHASE: Each path contributes with phase e^(iS/ħ)");
        System.out.println("   3. ACTION: S = ∫(KE - PE)dt (classical action)");
        System.out.println("   4. AMPLITUDE: Total amplitude = Σ e^(iS/ħ) over ALL paths");
        System.out.println("   5. PROBABILITY: P = |Total Amplitude|²");
        
        System.out.println("\n⚛️ KEY INSIGHTS:");
        System.out.println("   • Near classical path: action nearly stationary → constructive");
        System.out.println("   • Far from classical path: rapid phase variation → destructive");
        System.out.println("   • The classical path emerges from quantum interference");
    }
    
    /**
     * Performs the actual path integral calculation
     */
    private void performPathIntegral() {
        System.out.println("\n🧮 PATH INTEGRAL CALCULATION:");
        
        double totalAmplitudeReal = 0;
        double totalAmplitudeImag = 0;
        
        Random rand = new Random(42); // Fixed seed for reproducibility
        
        for (int pathNum = 0; pathNum < numPaths; pathNum++) {
            // Generate a random path through one of the slits
            QuantumPath path = generateRandomPath(rand);
            
            // Calculate action along path
            double action = calculateAction(path);
            
            // Calculate quantum amplitude: exp(i * action / ħ)
            double phase = action / HBAR;
            double amplitudeReal = Math.cos(phase);
            double amplitudeImag = Math.sin(phase);
            
            // Sum amplitudes (Feynman's sum over histories)
            totalAmplitudeReal += amplitudeReal;
            totalAmplitudeImag += amplitudeImag;
            
            // Store path contribution (for visualization)
            if (pathNum < 10) { // Show first 10 paths
                System.out.printf("   Path %3d: length = %.6f m, phase = %.2f rad\n", 
                    pathNum + 1, path.calculateLength(), phase % (2*Math.PI));
            }
        }
        
        // Calculate final probability
        double totalAmplitude = Math.hypot(totalAmplitudeReal, totalAmplitudeImag);
        double probability = totalAmplitude * totalAmplitude / (numPaths * numPaths);
        
        System.out.printf("\n📊 PATH INTEGRAL RESULT:\n");
        System.out.printf("   Total amplitude = %.4f\n", totalAmplitude / numPaths);
        System.out.printf("   Probability = %.6f\n", probability);
        System.out.printf("   Phase = %.2f rad\n", Math.atan2(totalAmplitudeImag, totalAmplitudeReal));
    }
    
    /**
     * Generates a random path through one of the slits
     */
    private QuantumPath generateRandomPath(Random rand) {
        QuantumPath path = new QuantumPath();
        
        // Source point
        double sourceX = -0.5;
        double sourceY = 0;
        path.addPoint(sourceX, sourceY);
        
        // Random intermediate points (Feynman's "jiggling" paths)
        int numIntermediate = 5;
        for (int i = 1; i <= numIntermediate; i++) {
            double t = (double)i / (numIntermediate + 1);
            double x = -0.5 + t * (screenDistance + 0.5);
            
            // Choose slit (left or right)
            boolean leftSlit = rand.nextBoolean();
            double slitCenter = leftSlit ? -slitSeparation/2 : slitSeparation/2;
            
            // Add random deviation (quantum fluctuations)
            double y = slitCenter + (rand.nextDouble() - 0.5) * slitWidth;
            
            // Add quantum uncertainty (Heisenberg)
            y += (rand.nextGaussian() * wavelength / (2 * Math.PI));
            
            path.addPoint(x, y);
        }
        
        // Screen point
        double screenX = screenDistance;
        double screenY = (rand.nextDouble() - 0.5) * 0.1; // Small screen region
        path.addPoint(screenX, screenY);
        
        return path;
    }
    
    /**
     * Calculates the classical action along a path
     */
    private double calculateAction(QuantumPath path) {
        double action = 0;
        
        // For photons: action is proportional to path length
        for (int i = 0; i < path.points.size() - 1; i++) {
            Point2D.Double p1 = path.points.get(i);
            Point2D.Double p2 = path.points.get(i + 1);
            
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double ds = Math.hypot(dx, dy);
            
            // Action = ∫ p·dq = (h/λ) * path length
            action += (PLANCK_CONSTANT / wavelength) * ds;
        }
        
        return action;
    }
    
    /**
     * Simulates individual photon detections on the screen
     */
    private void simulatePhotonDetections() {
        System.out.println("\n📸 SIMULATING PHOTON DETECTIONS:");
        
        Random rand = new Random();
        hits.clear();
        
        for (int i = 0; i < numPhotons; i++) {
            // Calculate probability for this position
            double x = (rand.nextDouble() - 0.5) * 0.01; // Small screen region
            double probability = calculateInterferenceProbability(x);
            
            // Determine if photon is detected
            if (rand.nextDouble() < probability * 1000) { // Scale for visibility
                double y = rand.nextDouble() * height/10 - height/20;
                double phase = 2 * Math.PI * rand.nextDouble();
                hits.add(new PhotonHit(x * 10000 + width/2, y + height/2, probability, phase));
                
                // Update intensity array for display
                int pixelX = (int)(x * 10000 + width/2);
                if (pixelX >= 0 && pixelX < width) {
                    intensity[pixelX] += 0.01;
                }
            }
            
            // Progress indicator
            if ((i + 1) % (numPhotons/10) == 0) {
                System.out.printf("   %d%% complete\n", (i + 1) * 100 / numPhotons);
            }
        }
        
        System.out.printf("   Total detections: %d\n", hits.size());
    }
    
    /**
     * Calculates interference probability at position x on screen
     * Using Feynman's rule: probability = |ψ₁ + ψ₂|²
     */
    private double calculateInterferenceProbability(double x) {
        // Distance to left slit
        double r1 = Math.hypot(screenDistance, x + slitSeparation/2);
        // Distance to right slit  
        double r2 = Math.hypot(screenDistance, x - slitSeparation/2);
        
        // Phase difference
        double k = 2 * Math.PI / wavelength;
        double phase1 = k * r1;
        double phase2 = k * r2;
        double deltaPhase = phase2 - phase1;
        
        // Single slit diffraction envelope
        double beta = k * slitWidth * x / screenDistance / 2;
        double diffractionEnvelope = Math.sin(beta) / (beta + 1e-10);
        
        // Double slit interference
        double interference = Math.cos(deltaPhase / 2);
        interference = interference * interference;
        
        // Combined probability
        return diffractionEnvelope * diffractionEnvelope * interference;
    }
    
    /**
     * Analyzes the interference pattern
     */
    private void analyzeInterferencePattern() {
        System.out.println("\n📈 INTERFERENCE PATTERN ANALYSIS:");
        
        // Find maxima and minima
        double maxIntensity = 0;
        double minIntensity = Double.MAX_VALUE;
        int maxPos = 0;
        
        for (int i = 0; i < width; i++) {
            if (intensity[i] > maxIntensity) {
                maxIntensity = intensity[i];
                maxPos = i;
            }
            if (intensity[i] < minIntensity && intensity[i] > 0) {
                minIntensity = intensity[i];
            }
        }
        
        // Calculate fringe spacing
        double xMax = (maxPos - width/2) / 10000.0; // Convert back to meters
        double fringeSpacing = wavelength * screenDistance / slitSeparation;
        
        System.out.printf("   Maximum intensity at x = %.2e m\n", xMax);
        System.out.printf("   Peak-to-peak intensity ratio: %.2f\n", maxIntensity / (minIntensity + 1e-10));
        System.out.printf("   Theoretical fringe spacing: %.2e m\n", fringeSpacing);
        System.out.printf("   Observed fringes: %.0f\n", maxIntensity / (minIntensity + 1e-10));
        
        // Quantum interpretation
        System.out.println("\n🔮 QUANTUM INTERPRETATION:");
        System.out.println("   • Each photon interferes with itself (Feynman)");
        System.out.println("   • Path integral sums over ALL possible paths");
        System.out.println("   • Which-slit information destroys interference");
        System.out.println("   • Complementarity principle: wave and particle aspects");
    }
    
    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);
        Graphics2D g2d = (Graphics2D) g;
        g2d.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        
        // Draw experimental setup
        drawExperimentalSetup(g2d);
        
        // Draw photon hits
        drawPhotonDetections(g2d);
        
        // Draw intensity profile
        drawIntensityProfile(g2d);
        
        // Draw explanatory text
        drawExplanation(g2d);
    }
    
    /**
     * Draws the double-slit experimental setup
     */
    private void drawExperimentalSetup(Graphics2D g2d) {
        // Source
        g2d.setColor(Color.YELLOW);
        g2d.fillOval(50, height/2 - 10, 20, 20);
        g2d.setColor(Color.WHITE);
        g2d.drawString("Source", 30, height/2 - 20);
        
        // Barrier with double slits
        g2d.setColor(Color.GRAY);
        g2d.fillRect(200, 100, 20, height - 200);
        
        // Slits
        g2d.setColor(Color.BLACK);
        int slit1Y = height/2 - (int)(slitSeparation * 1e6) - (int)(slitWidth * 1e6);
        int slit2Y = height/2 + (int)(slitSeparation * 1e6);
        int slitHeight = (int)(slitWidth * 2e6);
        
        g2d.setColor(Color.YELLOW);
        g2d.fillRect(200, slit1Y, 20, slitHeight);
        g2d.fillRect(200, slit2Y, 20, slitHeight);
        
        // Screen
        g2d.setColor(Color.DARK_GRAY);
        g2d.fillRect(600, 50, 10, height - 100);
        g2d.setColor(Color.WHITE);
        g2d.drawString("Screen", 620, height/2);
        
        // Draw some sample paths
        g2d.setColor(new Color(255, 255, 0, 30));
        for (int i = 0; i < 10; i++) {
            int y1 = slit1Y + slitHeight/2;
            int y2 = slit2Y + slitHeight/2;
            
            g2d.drawLine(60, height/2, 200, y1);
            g2d.drawLine(200, y1, 600, height/2 - 100 + i * 20);
            
            g2d.drawLine(60, height/2, 200, y2);
            g2d.drawLine(200, y2, 600, height/2 - 100 + i * 20);
        }
    }
    
    /**
     * Draws individual photon detection events
     */
    private void drawPhotonDetections(Graphics2D g2d) {
        for (PhotonHit hit : hits) {
            // Color based on phase
            float hue = (float)((hit.phase / (2 * Math.PI)) % 1.0);
            g2d.setColor(Color.getHSBColor(hue, 1.0f, 1.0f));
            
            // Draw photon hit
            int alpha = (int)(Math.min(hit.probability * 255, 255));
            g2d.setColor(new Color(255, 255, 255, alpha));
            g2d.fillOval((int)hit.x - 2, (int)hit.y - 2, 4, 4);
        }
    }
    
    /**
     * Draws the interference intensity profile
     */
    private void drawIntensityProfile(Graphics2D g2d) {
        g2d.setColor(Color.CYAN);
        g2d.setStroke(new BasicStroke(2));
        
        int prevX = 600;
        int prevY = height - 50;
        
        for (int x = 600; x < width; x++) {
            int index = x - 600;
            if (index >= 0 && index < intensity.length) {
                int y = height - 50 - (int)(intensity[index] * 100);
                y = Math.max(50, Math.min(height - 50, y));
                
                if (index > 0) {
                    g2d.drawLine(prevX, prevY, x, y);
                }
                
                prevX = x;
                prevY = y;
            }
        }
        
        // Label
        g2d.setColor(Color.WHITE);
        g2d.drawString("Interference Pattern", 650, 30);
    }
    
    /**
     * Draws explanatory text on the panel
     */
    private void drawExplanation(Graphics2D g2d) {
        g2d.setColor(Color.WHITE);
        g2d.setFont(new Font("Monospaced", Font.PLAIN, 12));
        
        int y = 20;
        g2d.drawString("Richard Feynman's Path Integral Formulation", 10, y); y += 15;
        g2d.drawString("Double-Slit Interference", 10, y); y += 15;
        g2d.drawString("----------------------------------------", 10, y); y += 15;
        g2d.drawString("Each photon takes ALL possible paths", 10, y); y += 15;
        g2d.drawString("Amplitude = Σ e^(iS/ħ) over all paths", 10, y); y += 15;
        g2d.drawString("Probability = |Amplitude|²", 10, y); y += 15;
        g2d.drawString("The classical path emerges from interference", 10, y); y += 15;
        
        // Feynman quote
        g2d.setColor(Color.YELLOW);
        g2d.drawString("\"I think I can safely say that nobody", 10, height - 60);
        g2d.drawString("understands quantum mechanics.\"", 10, height - 45);
        g2d.drawString("- Richard Feynman", 10, height - 30);
    }
    
    /**
     * Main method to run the simulation
     */
    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            JFrame frame = new JFrame("Feynman's Path Integral - Double Slit Experiment");
            frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
            
            FeynmanPathIntegralDoubleSlit simulation = new FeynmanPathIntegralDoubleSlit();
            frame.add(simulation);
            frame.pack();
            frame.setLocationRelativeTo(null);
            frame.setVisible(true);
            
            // Print mathematical formulation
            printMathematicalFormulation();
        });
    }
    
    /**
     * Prints the complete mathematical formulation
     */
    private static void printMathematicalFormulation() {
        System.out.println("\n📐 MATHEMATICAL FORMULATION:");
        System.out.println("   FEYNMAN'S PATH INTEGRAL:");
        System.out.println();
        System.out.println("   ⟨x_f|e^(-iHt/ħ)|x_i⟩ = ∫ 𝒟[x(t)] e^(iS[x(t)]/ħ)");
        System.out.println();
        System.out.println("   Where:");
        System.out.println("   • 𝒟[x(t)] represents integration over all paths");
        System.out.println("   • S[x(t)] = ∫ L(x,ẋ,t) dt is the classical action");
        System.out.println("   • ħ is the reduced Planck constant");
        System.out.println();
        System.out.println("   For double-slit experiment:");
        System.out.println("   ψ(x) = ψ₁(x) + ψ₂(x)");
        System.out.println("   ψⱼ(x) = ∫_slit j K(x,t; x',0) ψ₀(x') dx'");
        System.out.println("   K(x,t; x',0) = ∑_{paths} exp(iS_path/ħ)");
        System.out.println();
        System.out.println("   Probability distribution:");
        System.out.println("   P(x) = |ψ₁(x) + ψ₂(x)|²");
        System.out.println("        = |ψ₁|² + |ψ₂|² + 2|ψ₁||ψ₂|cos(Δφ)");
        System.out.println();
        System.out.println("   Phase difference:");
        System.out.println("   Δφ = (2π/λ)(r₂ - r₁)");
        System.out.println("   rⱼ = √(L² + (x ∓ d/2)²)");
        System.out.println();
        System.out.println("   Single slit diffraction envelope:");
        System.out.println("   I(θ) = I₀ [sin(β)/β]², where β = (πa sin θ)/λ");
        System.out.println();
        System.out.println("   Complete double-slit pattern:");
        System.out.println("   I(θ) = I₀ [sin(β)/β]² cos²(α), where α = (πd sin θ)/λ");
    }
}

/**
 * SUPPLEMENTARY CLASS: QuantumPathIntegralSimulator
 * More advanced path integral calculations
 */
class QuantumPathIntegralSimulator {
    
    /**
     * Calculates the propagator using path integral
     */
    public static ComplexNumber calculatePropagator(double x1, double t1, 
                                                     double x2, double t2, 
                                                     int numPaths) {
        ComplexNumber total = new ComplexNumber(0, 0);
        double dt = (t2 - t1) / numPaths;
        
        for (int i = 0; i < numPaths; i++) {
            // Generate path
            double[] path = generatePath(x1, x2, t1, t2, numPaths);
            
            // Calculate action
            double action = calculateAction(path, dt);
            
            // Add amplitude
            total = total.add(ComplexNumber.fromPolar(1, action / HBAR));
        }
        
        return total.multiply(1.0 / numPaths);
    }
    
    private static double[] generatePath(double x1, double x2, double t1, double t2, int steps) {
        double[] path = new double[steps + 1];
        path[0] = x1;
        path[steps] = x2;
        
        Random rand = new Random();
        for (int i = 1; i < steps; i++) {
            // Random midpoint displacement (fractal path)
            double t = (double)i / steps;
            double classical = x1 + (x2 - x1) * t;
            double fluctuation = rand.nextGaussian() * Math.sqrt(t * (1 - t));
            path[i] = classical + fluctuation;
        }
        
        return path;
    }
    
    private static double calculateAction(double[] path, double dt) {
        double action = 0;
        double mass = ELECTRON_MASS;
        
        for (int i = 0; i < path.length - 1; i++) {
            double dx = path[i + 1] - path[i];
            double velocity = dx / dt;
            double kineticEnergy = 0.5 * mass * velocity * velocity;
            action += kineticEnergy * dt;
        }
        
        return action;
    }
    
    /**
     * Complex number class for quantum amplitude calculations
     */
    static class ComplexNumber {
        double real;
        double imag;
        
        public ComplexNumber(double r, double i) {
            real = r;
            imag = i;
        }
        
        public static ComplexNumber fromPolar(double r, double theta) {
            return new ComplexNumber(r * Math.cos(theta), r * Math.sin(theta));
        }
        
        public ComplexNumber add(ComplexNumber other) {
            return new ComplexNumber(real + other.real, imag + other.imag);
        }
        
        public ComplexNumber multiply(double scalar) {
            return new ComplexNumber(real * scalar, imag * scalar);
        }
        
        public double modulus() {
            return Math.hypot(real, imag);
        }
        
        public double modulusSquared() {
            return real * real + imag * imag;
        }
    }
}