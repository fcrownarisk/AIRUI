' Program: DiracEquation.vb
' Description: Comprehensive decomposition and simulation of Paul Dirac's relativistic
'              quantum mechanical equation for spin-1/2 particles (electrons, quarks).
'              The Dirac equation unifies quantum mechanics with special relativity
'              and predicts antimatter.
'
' The Dirac Equation: (iγᵘ∂ᵤ - m)ψ = 0
' Where:
' - γᵘ are 4×4 Dirac gamma matrices
' - ψ is a 4-component spinor wavefunction
' - m is the particle mass
' - ∂ᵤ is the spacetime partial derivative

Imports System
Imports System.Numerics
Imports System.Windows.Forms
Imports System.Drawing


''' Main form for Dirac equation visualization

Public Class DiracEquationForm
    Inherits Form
    
    ' Dirac matrices (4x4)
    Private gamma0 As Complex(,)
    Private gamma1 As Complex(,)
    Private gamma2 As Complex(,)
    Private gamma3 As Complex(,)
    Private gamma5 As Complex(,)
    
    ' Spinor components
    Private spinorUp As Complex()
    Private spinorDown As Complex()
    Private spinorUpAnti As Complex()
    Private spinorDownAnti As Complex()
    
    ' Physical constants
    Private Const HBAR As Double = 1.0545718e-34 ' Reduced Planck constant
    Private Const C As Double = 299792458 ' Speed of light
    Private Const ELECTRON_MASS As Double = 9.1093837e-31 ' kg
    Private Const FINE_STRUCTURE As Double = 1 / 137.036 ' Fine structure constant
    
    ' Simulation parameters
    Private time As Double = 0
    Private dt As Double = 1e-20
    Private isRunning As Boolean = False
    Private particleEnergy As Double = 1e6 ' 1 MeV in eV
    Private magneticField As Double = 1 ' Tesla
    
    ' UI Components
    Private WithEvents mainTimer As New Timer()
    Private WithEvents startButton As New Button()
    Private WithEvents stopButton As New Button()
    Private WithEvents resetButton As New Button()
    Private WithEvents energyTrackBar As New TrackBar()
    Private WithEvents fieldTrackBar As New TrackBar()
    Private WithEvents componentCombo As New ComboBox()
    Private WithEvents probabilityLabel As New Label()
    Private WithEvents energyLabel As New Label()
    Private WithEvents helicityLabel As New Label()
    Private WithEvents displayPanel As New Panel()
    
    Public Sub New()
        InitializeComponent()
        InitializeDiracMatrices()
        InitializeSpinors()
        SetupDisplay()
    End Sub
    
    
    ''' Initialize form components
    
    Private Sub InitializeComponent()
        Me.Text = "Paul Dirac's Relativistic Wave Equation"
        Me.Size = New Size(1000, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(20, 20, 40)
        Me.DoubleBuffered = True
        
        ' Start Button
        startButton.Text = "▶ Start"
        startButton.Location = New Point(20, 20)
        startButton.Size = New Size(100, 30)
        startButton.BackColor = Color.LightGreen
        startButton.Font = New Font("Consolas", 10)
        
        ' Stop Button
        stopButton.Text = "⏸ Stop"
        stopButton.Location = New Point(130, 20)
        stopButton.Size = New Size(100, 30)
        stopButton.BackColor = Color.LightCoral
        stopButton.Font = New Font("Consolas", 10)
        stopButton.Enabled = False
        
        ' Reset Button
        resetButton.Text = "↺ Reset"
        resetButton.Location = New Point(240, 20)
        resetButton.Size = New Size(100, 30)
        resetButton.BackColor = Color.LightYellow
        resetButton.Font = New Font("Consolas", 10)
        
        ' Energy TrackBar
        Dim energyLabelCtrl As New Label()
        energyLabelCtrl.Text = "Particle Energy (MeV):"
        energyLabelCtrl.Location = New Point(20, 70)
        energyLabelCtrl.Size = New Size(150, 20)
        energyLabelCtrl.ForeColor = Color.White
        energyLabelCtrl.Font = New Font("Consolas", 9)
        
        energyTrackBar.Location = New Point(20, 95)
        energyTrackBar.Size = New Size(320, 45)
        energyTrackBar.Minimum = 1
        energyTrackBar.Maximum = 100
        energyTrackBar.Value = 10
        energyTrackBar.TickFrequency = 10
        
        ' Magnetic Field TrackBar
        Dim fieldLabelCtrl As New Label()
        fieldLabelCtrl.Text = "Magnetic Field (Tesla):"
        fieldLabelCtrl.Location = New Point(20, 140)
        fieldLabelCtrl.Size = New Size(150, 20)
        fieldLabelCtrl.ForeColor = Color.White
        fieldLabelCtrl.Font = New Font("Consolas", 9)
        
        fieldTrackBar.Location = New Point(20, 165)
        fieldTrackBar.Size = New Size(320, 45)
        fieldTrackBar.Minimum = 0
        fieldTrackBar.Maximum = 20
        fieldTrackBar.Value = 5
        fieldTrackBar.TickFrequency = 2
        
        ' Component Selector
        Dim compLabelCtrl As New Label()
        compLabelCtrl.Text = "Spinor Component:"
        compLabelCtrl.Location = New Point(20, 220)
        compLabelCtrl.Size = New Size(150, 20)
        compLabelCtrl.ForeColor = Color.White
        compLabelCtrl.Font = New Font("Consolas", 9)
        
        componentCombo.Location = New Point(20, 245)
        componentCombo.Size = New Size(150, 25)
        componentCombo.DropDownStyle = ComboBoxStyle.DropDownList
        componentCombo.Items.AddRange(New String() {"ψ₁ (Spin Up)", "ψ₂ (Spin Down)", 
                                                     "ψ₃ (Anti Up)", "ψ₄ (Anti Down)", 
                                                     "Probability Density"})
        componentCombo.SelectedIndex = 4
        componentCombo.BackColor = Color.White
        
        ' Display Panel
        displayPanel.Location = New Point(400, 20)
        displayPanel.Size = New Size(550, 550)
        displayPanel.BackColor = Color.Black
        displayPanel.BorderStyle = BorderStyle.Fixed3D
        
        ' Labels
        probabilityLabel.Location = New Point(20, 300)
        probabilityLabel.Size = New Size(350, 25)
        probabilityLabel.ForeColor = Color.Cyan
        probabilityLabel.Font = New Font("Consolas", 10)
        
        energyLabel.Location = New Point(20, 330)
        energyLabel.Size = New Size(350, 25)
        energyLabel.ForeColor = Color.LightGreen
        energyLabel.Font = New Font("Consolas", 10)
        
        helicityLabel.Location = New Point(20, 360)
        helicityLabel.Size = New Size(350, 25)
        helicityLabel.ForeColor = Color.Yellow
        helicityLabel.Font = New Font("Consolas", 10)
        
        ' Add controls
        Me.Controls.AddRange(New Control() {startButton, stopButton, resetButton,
                                            energyLabelCtrl, energyTrackBar,
                                            fieldLabelCtrl, fieldTrackBar,
                                            compLabelCtrl, componentCombo,
                                            probabilityLabel, energyLabel, helicityLabel,
                                            displayPanel})
        
        ' Set up timer
        mainTimer.Interval = 50 ' 50ms refresh
        
        ' Add event handlers
        AddHandler Me.Paint, AddressOf DiracForm_Paint
        AddHandler mainTimer.Tick, AddressOf Timer_Tick
        AddHandler energyTrackBar.Scroll, AddressOf EnergyTrackBar_Scroll
        AddHandler fieldTrackBar.Scroll, AddressOf FieldTrackBar_Scroll
    End Sub
    
    
    ''' Initialize Dirac gamma matrices
    
    Private Sub InitializeDiracMatrices()
        ' Gamma matrices in Dirac representation
        ' γ⁰ = [I  0; 0 -I]
        gamma0 = New Complex(3, 3) {{}}
        gamma0(0, 0) = New Complex(1, 0)
        gamma0(1, 1) = New Complex(1, 0)
        gamma0(2, 2) = New Complex(-1, 0)
        gamma0(3, 3) = New Complex(-1, 0)
        
        ' γ¹ = [0  σ¹; -σ¹ 0]
        gamma1 = New Complex(3, 3) {{}}
        gamma1(0, 2) = New Complex(0, 0) ' σ¹ = [0 1; 1 0]
        gamma1(1, 3) = New Complex(1, 0)
        gamma1(2, 0) = New Complex(0, 0)
        gamma1(3, 1) = New Complex(-1, 0)
        
        ' γ² = [0  σ²; -σ² 0]
        gamma2 = New Complex(3, 3) {{}}
        gamma2(0, 3) = New Complex(0, -1) ' σ² = [0 -i; i 0]
        gamma2(1, 2) = New Complex(0, 1)
        gamma2(2, 1) = New Complex(0, -1)
        gamma2(3, 0) = New Complex(0, 1)
        
        ' γ³ = [0  σ³; -σ³ 0]
        gamma3 = New Complex(3, 3) {{}}
        gamma3(0, 2) = New Complex(1, 0) ' σ³ = [1 0; 0 -1]
        gamma3(1, 3) = New Complex(-1, 0)
        gamma3(2, 0) = New Complex(-1, 0)
        gamma3(3, 1) = New Complex(1, 0)
        
        ' γ⁵ = iγ⁰γ¹γ²γ³ (chirality operator)
        gamma5 = MatrixMultiply(gamma0, gamma1)
        gamma5 = MatrixMultiply(gamma5, gamma2)
        gamma5 = MatrixMultiply(gamma5, gamma3)
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                gamma5(i, j) = gamma5(i, j) * New Complex(0, 1)
            Next
        Next
    End Sub
    
    
    ''' Initialize spinor wavefunctions
    
    Private Sub InitializeSpinors()
        ' Dirac spinor ψ = (ψ₁, ψ₂, ψ₃, ψ₄)ᵀ
        ' ψ₁: Spin up particle
        ' ψ₂: Spin down particle
        ' ψ₃: Spin up antiparticle
        ' ψ₄: Spin down antiparticle
        
        spinorUp = New Complex(3) {}
        spinorDown = New Complex(3) {}
        spinorUpAnti = New Complex(3) {}
        spinorDownAnti = New Complex(3) {}
        
        ResetSpinors()
    End Sub
    
    
    ''' Reset spinors to initial state
    
    Private Sub ResetSpinors()
        ' Initialize with a Gaussian wave packet
        Dim momentum As Double = Math.Sqrt(2 * ELECTRON_MASS * particleEnergy * 1.6e-19)
        
        For i As Integer = 0 To 3
            Dim phase As Double = momentum * i * 1e-10 / HBAR
            Dim amplitude As Double = Math.Exp(-(i - 2) * (i - 2) / 4.0)
            
            spinorUp(i) = New Complex(amplitude * Math.Cos(phase), amplitude * Math.Sin(phase))
            spinorDown(i) = New Complex(amplitude * Math.Sin(phase), amplitude * Math.Cos(phase))
            spinorUpAnti(i) = New Complex(amplitude * Math.Cos(phase), -amplitude * Math.Sin(phase))
            spinorDownAnti(i) = New Complex(-amplitude * Math.Sin(phase), amplitude * Math.Cos(phase))
        Next
        
        time = 0
    End Sub
    
    
    ''' Setup display surface
    
    Private Sub SetupDisplay()
        Dim bmp As New Bitmap(displayPanel.Width, displayPanel.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Black)
            DrawCoordinateSystem(g)
            DrawDiracEquation(g)
        End Using
        displayPanel.BackgroundImage = bmp
    End Sub
    
    
    ''' Matrix multiplication for 4x4 complex matrices
    
    Private Function MatrixMultiply(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(3, 3) As Complex
        
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                result(i, j) = Complex.Zero
                For k As Integer = 0 To 3
                    result(i, j) += A(i, k) * B(k, j)
                Next
            Next
        Next
        
        Return result
    End Function
    
    
    ''' Apply Dirac operator to spinor
    
    Private Function DiracOperator(spinor As Complex()) As Complex()
        Dim result(3) As Complex
        
        ' (iγᵘ∂ᵤ - m)ψ = 0
        ' For plane wave solution: (γ⁰E - γ·p - m)ψ = 0
        
        Dim E As Double = particleEnergy * 1.6e-19 ' Convert eV to Joules
        Dim p As Double = Math.Sqrt(E * E - (ELECTRON_MASS * C * C) * (ELECTRON_MASS * C * C)) / C
        
        ' Apply Hamiltonian: H = α·p + βm
        ' where αⁱ = γ⁰γⁱ, β = γ⁰
        
        For i As Integer = 0 To 3
            result(i) = Complex.Zero
            
            ' βm term
            result(i) += gamma0(i, i) * spinor(i) * ELECTRON_MASS * C * C
            
            ' α·p terms
            For j As Integer = 0 To 3
                ' α¹ = γ⁰γ¹
                Dim alpha1 As Complex = Complex.Zero
                For k As Integer = 0 To 3
                    alpha1 += gamma0(i, k) * gamma1(k, j)
                Next
                result(i) += alpha1 * spinor(j) * p * C / 3
                
                ' α² = γ⁰γ²
                Dim alpha2 As Complex = Complex.Zero
                For k As Integer = 0 To 3
                    alpha2 += gamma0(i, k) * gamma2(k, j)
                Next
                result(i) += alpha2 * spinor(j) * p * C / 3
                
                ' α³ = γ⁰γ³
                Dim alpha3 As Complex = Complex.Zero
                For k As Integer = 0 To 3
                    alpha3 += gamma0(i, k) * gamma3(k, j)
                Next
                result(i) += alpha3 * spinor(j) * p * C / 3
            Next
        Next
        
        Return result
    End Function
    
    
    ''' Time evolution of spinors
    
    Private Sub EvolveSpinors()
        ' Solve iℏ ∂ψ/∂t = Hψ
        
        Dim hbarOverI As Complex = New Complex(0, -HBAR)
        
        ' Evolve each spinor component
        Dim newUp As Complex() = DiracOperator(spinorUp)
        Dim newDown As Complex() = DiracOperator(spinorDown)
        Dim newUpAnti As Complex() = DiracOperator(spinorUpAnti)
        Dim newDownAnti As Complex() = DiracOperator(spinorDownAnti)
        
        For i As Integer = 0 To 3
            spinorUp(i) += newUp(i) * dt / hbarOverI
            spinorDown(i) += newDown(i) * dt / hbarOverI
            spinorUpAnti(i) += newUpAnti(i) * dt / hbarOverI
            spinorDownAnti(i) += newDownAnti(i) * dt / hbarOverI
        Next
        
        time += dt
    End Sub
    
    
    ''' Calculate probability density
    
    Private Function ProbabilityDensity() As Double
        Dim prob As Double = 0
        
        For i As Integer = 0 To 3
            prob += spinorUp(i).MagnitudeSquared()
            prob += spinorDown(i).MagnitudeSquared()
            prob += spinorUpAnti(i).MagnitudeSquared()
            prob += spinorDownAnti(i).MagnitudeSquared()
        Next
        
        Return prob / 16 ' Normalize
    End Function
    
    
    ''' Calculate helicity (spin projection along momentum)
    
    Private Function CalculateHelicity() As Double
        ' h = Σ·p̂ where Σ = [σ 0; 0 σ]
        Dim helicity As Double = 0
        
        For i As Integer = 0 To 1
            helicity += spinorUp(i).MagnitudeSquared()
            helicity -= spinorDown(i).MagnitudeSquared()
            helicity += spinorUpAnti(i + 2).MagnitudeSquared()
            helicity -= spinorDownAnti(i + 2).MagnitudeSquared()
        Next
        
        Return helicity / ProbabilityDensity()
    End Function
    
    
    ''' Draw coordinate system
    
    Private Sub DrawCoordinateSystem(g As Graphics)
        Dim pen As New Pen(Color.FromArgb(50, 50, 80))
        
        ' Draw grid
        For x As Integer = 50 To displayPanel.Width - 50 Step 50
            g.DrawLine(pen, x, 50, x, displayPanel.Height - 50)
        Next
        
        For y As Integer = 50 To displayPanel.Height - 50 Step 50
            g.DrawLine(pen, 50, y, displayPanel.Width - 50, y)
        Next
        
        ' Draw axes
        Dim axisPen As New Pen(Color.White, 2)
        g.DrawLine(axisPen, 50, displayPanel.Height / 2, displayPanel.Width - 50, displayPanel.Height / 2)
        g.DrawLine(axisPen, displayPanel.Width / 2, 50, displayPanel.Width / 2, displayPanel.Height - 50)
        
        ' Labels
        Dim font As New Font("Consolas", 8)
        g.DrawString("Space (x)", font, Brushes.White, displayPanel.Width - 100, displayPanel.Height / 2 + 10)
        g.DrawString("|ψ|²", font, Brushes.White, displayPanel.Width / 2 + 10, 30)
    End Sub
    
    
    ''' Draw Dirac equation visualization
    
    Private Sub DrawDiracEquation(g As Graphics)
        Dim centerX As Integer = displayPanel.Width / 2
        Dim centerY As Integer = displayPanel.Height / 2
        
        ' Draw spinor components
        Dim selectedComponent As Integer = componentCombo.SelectedIndex
        Dim spinors As Complex()() = {spinorUp, spinorDown, spinorUpAnti, spinorDownAnti}
        
        If selectedComponent >= 0 AndAlso selectedComponent < 4 Then
            DrawSpinorComponent(g, spinors(selectedComponent), centerX, centerY, Color.Cyan)
        Else
            ' Draw probability density
            DrawProbabilityDensity(g, centerX, centerY)
        End If
        
        ' Draw Dirac matrices representation
        DrawGammaMatrices(g, 50, 100)
    End Sub
    
    
    ''' Draw individual spinor component
    
    Private Sub DrawSpinorComponent(g As Graphics, spinor As Complex(), 
                                   centerX As Integer, centerY As Integer, color As Color)
        Dim points(3) As PointF
        Dim scale As Single = 100
        
        For i As Integer = 0 To 3
            Dim x As Single = centerX + CSng(spinor(i).Real * scale)
            Dim y As Single = centerY + CSng(spinor(i).Imaginary * scale)
            points(i) = New PointF(x, y)
            
            ' Draw point
            g.FillEllipse(New SolidBrush(color), x - 3, y - 3, 6, 6)
            
            ' Draw label
            g.DrawString($"ψ{i + 1}", New Font("Consolas", 8), 
                        Brushes.White, x + 5, y - 10)
        Next
        
        ' Draw connecting lines
        If points.Length > 1 Then
            Using pen As New Pen(color)
                For i As Integer = 0 To points.Length - 2
                    g.DrawLine(pen, points(i), points(i + 1))
                Next
            End Using
        End If
    End Sub
    
    
    ''' Draw probability density
    
    Private Sub DrawProbabilityDensity(g As Graphics, centerX As Integer, centerY As Integer)
        Dim prob As Double() = New Double(3) {}
        
        For i As Integer = 0 To 3
            prob(i) = spinorUp(i).MagnitudeSquared() +
                     spinorDown(i).MagnitudeSquared() +
                     spinorUpAnti(i).MagnitudeSquared() +
                     spinorDownAnti(i).MagnitudeSquared()
            prob(i) /= 4
        Next
        
        Dim colors() As Color = {Color.Cyan, Color.LightGreen, Color.Yellow, Color.Magenta}
        
        For i As Integer = 0 To 3
            Dim x As Integer = centerX - 150 + i * 100
            Dim height As Integer = CInt(prob(i) * 200)
            
            ' Draw bar
            Using brush As New SolidBrush(colors(i))
                g.FillRectangle(brush, x, centerY - height, 50, height)
            End Using
            
            ' Draw label
            g.DrawString($"P{i + 1}", New Font("Consolas", 8), 
                        Brushes.White, x + 10, centerY + 10)
            g.DrawString($"{prob(i):F3}", New Font("Consolas", 7), 
                        Brushes.White, x + 10, centerY - height - 15)
        Next
    End Sub
    
    
    ''' Draw gamma matrix representation
    
    Private Sub DrawGammaMatrices(g As Graphics, x As Integer, y As Integer)
        Dim font As New Font("Consolas", 7)
        Dim matrixSize As Integer = 40
        
        g.DrawString("Dirac γ-matrices:", font, Brushes.Yellow, x, y - 15)
        
        ' Draw γ⁰
        DrawMatrix(g, gamma0, x, y, "γ⁰", Color.Cyan)
        
        ' Draw γ¹
        DrawMatrix(g, gamma1, x + 120, y, "γ¹", Color.LightGreen)
        
        ' Draw γ²
        DrawMatrix(g, gamma2, x + 240, y, "γ²", Color.Orange)
        
        ' Draw γ³
        DrawMatrix(g, gamma3, x + 360, y, "γ³", Color.Magenta)
        
        ' Draw γ⁵
        DrawMatrix(g, gamma5, x + 480, y, "γ⁵", Color.Yellow)
    End Sub
    
    
    ''' Draw a single gamma matrix
    
    Private Sub DrawMatrix(g As Graphics, matrix As Complex(,), 
                          x As Integer, y As Integer, label As String, color As Color)
        Dim cellSize As Integer = 12
        
        ' Draw matrix bracket
        Using pen As New Pen(color)
            g.DrawRectangle(pen, x - 2, y - 2, cellSize * 4 + 4, cellSize * 4 + 4)
        End Using
        
        ' Draw matrix elements
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                Dim value As String
                If matrix(i, j).Magnitude < 0.1 Then
                    value = "0"
                Else
                    value = If(matrix(i, j).Real > 0, "+", "-")
                End If
                
                Using brush As New SolidBrush(If(matrix(i, j).Magnitude > 0.1, color, Color.Gray))
                    g.DrawString(value, New Font("Consolas", 6), brush,
                                x + j * cellSize, y + i * cellSize)
                End Using
            Next
        Next
        
        ' Draw label
        g.DrawString(label, New Font("Consolas", 8), Brushes.White, x + 10, y + 50)
    End Sub
    
    
    ''' Update display
    
    Private Sub UpdateDisplay()
        Dim bmp As New Bitmap(displayPanel.Width, displayPanel.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Black)
            DrawCoordinateSystem(g)
            DrawDiracEquation(g)
        End Using
        displayPanel.BackgroundImage = bmp
        displayPanel.Invalidate()
        
        ' Update labels
        probabilityLabel.Text = $"Probability Density: {ProbabilityDensity():F6}"
        
        Dim energyEV As Double = particleEnergy / 1.6e-19
        energyLabel.Text = $"Energy: {energyEV:F2} eV"
        
        Dim helicity As Double = CalculateHelicity()
        helicityLabel.Text = $"Helicity: {helicity:F3} (spin ∥ momentum)"
    End Sub
    
    
    ''' Timer tick event
    
    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        For i As Integer = 1 To 10
            EvolveSpinors()
        Next
        UpdateDisplay()
    End Sub
    
    
    ''' Start button click
    
    Private Sub StartButton_Click(sender As Object, e As EventArgs) Handles startButton.Click
        isRunning = True
        mainTimer.Start()
        startButton.Enabled = False
        stopButton.Enabled = True
    End Sub
    
    
    ''' Stop button click
    
    Private Sub StopButton_Click(sender As Object, e As EventArgs) Handles stopButton.Click
        isRunning = False
        mainTimer.Stop()
        startButton.Enabled = True
        stopButton.Enabled = False
    End Sub
    
    
    ''' Reset button click
    
    Private Sub ResetButton_Click(sender As Object, e As EventArgs) Handles resetButton.Click
        ResetSpinors()
        UpdateDisplay()
    End Sub
    
    
    ''' Energy trackbar scroll
    
    Private Sub EnergyTrackBar_Scroll(sender As Object, e As EventArgs)
        particleEnergy = energyTrackBar.Value * 1.6e-19 ' Convert to Joules
        ResetSpinors()
        UpdateDisplay()
    End Sub
    
    
    ''' Field trackbar scroll
    
    Private Sub FieldTrackBar_Scroll(sender As Object, e As EventArgs)
        magneticField = fieldTrackBar.Value
        ' Update Hamiltonian with magnetic field (minimal coupling)
        ' p → p - eA
    End Sub
    
    
    ''' Form paint event
    
    Private Sub DiracForm_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        
        ' Draw the Dirac equation
        Dim font As New Font("Consolas", 12)
        Dim titleFont As New Font("Consolas", 14, FontStyle.Bold)
        
        g.DrawString("The Dirac Equation:", titleFont, Brushes.Yellow, 20, 400)
        g.DrawString("(iγᵘ∂ᵤ - m)ψ = 0", font, Brushes.Cyan, 40, 430)
        g.DrawString("γ⁰ = [I 0; 0 -I]", font, Brushes.LightGreen, 40, 460)
        g.DrawString("γⁱ = [0 σⁱ; -σⁱ 0]", font, Brushes.LightGreen, 40, 480)
        g.DrawString("ψ = 4-component Dirac spinor", font, Brushes.Orange, 40, 510)
        g.DrawString("Predicts antimatter (E = ±√(p²c² + m²c⁴))", font, Brushes.Magenta, 40, 540)
    End Sub
    
    
    ''' Form closing event
    
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        mainTimer.Stop()
        MyBase.OnFormClosing(e)
    End Sub
End Class


''' Module containing the main entry point

Module Program
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New DiracEquationForm())
    End Sub
End Module


''' Supplementary class for Dirac algebra calculations

Public Class DiracAlgebra
    ' Pauli matrices
    Public Shared ReadOnly Property Sigma1 As Complex(,) = New Complex(,) {
        {Complex.Zero, Complex.One},
        {Complex.One, Complex.Zero}
    }
    
    Public Shared ReadOnly Property Sigma2 As Complex(,) = New Complex(,) {
        {Complex.Zero, New Complex(0, -1)},
        {New Complex(0, 1), Complex.Zero}
    }
    
    Public Shared ReadOnly Property Sigma3 As Complex(,) = New Complex(,) {
        {Complex.One, Complex.Zero},
        {Complex.Zero, New Complex(-1, 0)}
    }
    
    
    ''' Calculate commutator [A, B] = AB - BA
    
    Public Shared Function Commutator(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim AB = MatrixMultiply(A, B)
        Dim BA = MatrixMultiply(B, A)
        Return MatrixSubtract(AB, BA)
    End Function
    
    
    ''' Calculate anticommutator {A, B} = AB + BA
    
    Public Shared Function AntiCommutator(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim AB = MatrixMultiply(A, B)
        Dim BA = MatrixMultiply(B, A)
        Return MatrixAdd(AB, BA)
    End Function
    
    
    ''' Check Clifford algebra {γᵘ, γᵛ} = 2gᵘᵛ
    
    Public Shared Function VerifyCliffordAlgebra(gammaMatrices As List(Of Complex(,))) As Boolean
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                Dim anticommutator = AntiCommutator(gammaMatrices(i), gammaMatrices(j))
                
                ' Should be 2gᵘᵛ where g = diag(1, -1, -1, -1)
                Dim expected As Double = 0
                If i = j Then
                    expected = If(i = 0, 2, -2)
                End If
                
                For m As Integer = 0 To 3
                    For n As Integer = 0 To 3
                        If m = n AndAlso Math.Abs(anticommutator(m, n).Real - expected) > 0.001 Then
                            Return False
                        End If
                    Next
                Next
            Next
        Next
        
        Return True
    End Function
    
    Private Shared Function MatrixMultiply(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(3, 3) As Complex
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                result(i, j) = Complex.Zero
                For k As Integer = 0 To 3
                    result(i, j) += A(i, k) * B(k, j)
                Next
            Next
        Next
        Return result
    End Function
    
    Private Shared Function MatrixAdd(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(3, 3) As Complex
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                result(i, j) = A(i, j) + B(i, j)
            Next
        Next
        Return result
    End Function
    
    Private Shared Function MatrixSubtract(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(3, 3) As Complex
        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                result(i, j) = A(i, j) - B(i, j)
            Next
        Next
        Return result
    End Function
End Class


''' Supplementary class for plane wave solutions

Public Class DiracPlaneWave
    Public Property Momentum As Vector3
    Public Property Energy As Double
    Public Property Spin As Integer ' ±1
    Public Property Particle As Boolean ' True for particle, False for antiparticle
    
    
    ''' Generate positive energy solution (particle)
    
    Public Function PositiveEnergySolution() As Complex()
        Dim spinor(3) As Complex
        
        Dim p As Double = Momentum.Magnitude
        Dim E As Double = Energy
        Dim m As Double = 9.1093837e-31
        
        ' u(p,s) = [√(E+m) χ^s; √(E-m) (σ·p̂) χ^s] / √(2m)
        Dim norm As Double = 1.0 / Math.Sqrt(2 * m)
        
        ' χ^+ = (1,0) for spin up, χ^- = (0,1) for spin down
        If Spin = 1 Then
            spinor(0) = New Complex(Math.Sqrt(E + m) * norm, 0)
            spinor(1) = Complex.Zero
            spinor(2) = New Complex(Math.Sqrt(E - m) * norm * (Momentum.Z / p), 0)
            spinor(3) = New Complex(Math.Sqrt(E - m) * norm * ((Momentum.X + New Complex(0, 1) * Momentum.Y) / p), 0)
        Else
            spinor(0) = Complex.Zero
            spinor(1) = New Complex(Math.Sqrt(E + m) * norm, 0)
            spinor(2) = New Complex(Math.Sqrt(E - m) * norm * ((Momentum.X - New Complex(0, 1) * Momentum.Y) / p), 0)
            spinor(3) = New Complex(Math.Sqrt(E - m) * norm * (-Momentum.Z / p), 0)
        End If
        
        Return spinor
    End Function
    
    
    ''' Generate negative energy solution (antiparticle)
    
    Public Function NegativeEnergySolution() As Complex()
        Dim spinor(3) As Complex
        
        Dim p As Double = Momentum.Magnitude
        Dim E As Double = Energy
        Dim m As Double = 9.1093837e-31
        
        ' v(p,s) = [√(E-m) (σ·p̂) χ^{-s}; √(E+m) χ^{-s}] / √(2m)
        Dim norm As Double = 1.0 / Math.Sqrt(2 * m)
        
        If Spin = 1 Then
            spinor(0) = New Complex(Math.Sqrt(E - m) * norm * ((Momentum.X - New Complex(0, 1) * Momentum.Y) / p), 0)
            spinor(1) = New Complex(Math.Sqrt(E - m) * norm * (-Momentum.Z / p), 0)
            spinor(2) = New Complex(Math.Sqrt(E + m) * norm, 0)
            spinor(3) = Complex.Zero
        Else
            spinor(0) = New Complex(Math.Sqrt(E - m) * norm * (Momentum.Z / p), 0)
            spinor(1) = New Complex(Math.Sqrt(E - m) * norm * ((Momentum.X + New Complex(0, 1) * Momentum.Y) / p), 0)
            spinor(2) = Complex.Zero
            spinor(3) = New Complex(Math.Sqrt(E + m) * norm, 0)
        End If
        
        Return spinor
    End Function
End Class


''' 3D Vector class

Public Class Vector3
    Public Property X As Double
    Public Property Y As Double
    Public Property Z As Double
    
    Public Sub New(x As Double, y As Double, z As Double)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub
    
    Public ReadOnly Property Magnitude As Double
        Get
            Return Math.Sqrt(X * X + Y * Y + Z * Z)
        End Get
    End Property
    
    Public Function Dot(other As Vector3) As Double
        Return X * other.X + Y * other.Y + Z * other.Z
    End Function
    
    Public Function Cross(other As Vector3) As Vector3
        Return New Vector3(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        )
    End Function
    
    Public Function Normalize() As Vector3
        Dim mag = Magnitude
        If mag > 0 Then
            Return New Vector3(X / mag, Y / mag, Z / mag)
        End If
        Return New Vector3(0, 0, 0)
    End Function
End Class