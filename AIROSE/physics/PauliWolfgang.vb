' Program: PauliMatrices.vb
' Description: Comprehensive demonstration of Wolfgang Pauli's contributions to quantum mechanics,
'              focusing on Pauli matrices, spin-1/2 particles, exclusion principle, and neutrino hypothesis.
'              Includes visualization of spin operators, matrix representations, and quantum phenomena.
'
' Pauli Matrices: σ₁ = [0 1; 1 0], σ₂ = [0 -i; i 0], σ₃ = [1 0; 0 -1]
' Pauli Exclusion Principle: No two fermions can occupy the same quantum state
' Neutrino Hypothesis: Prediction of neutrino to explain beta decay energy conservation

Imports System
Imports System.Numerics
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D


''' Main form for Pauli matrices demonstration

Public Class PauliForm
    Inherits Form
    
    ' Pauli matrices
    Private sigma1 As Complex(,)
    Private sigma2 As Complex(,)
    Private sigma3 As Complex(,)
    Private identity As Complex(,)
    
    ' Spin states
    Private spinUp As Complex()
    Private spinDown As Complex()
    Private spinLeft As Complex()
    Private spinRight As Complex()
    Private spinIn As Complex()
    Private spinOut As Complex()
    
    ' Current state
    Private currentState As Complex()
    Private currentStateName As String = "|↑⟩"
    Private measurementResult As String = ""
    Private expectationValue As Double = 0
    
    ' Animation
    Private rotationAngle As Double = 0
    Private WithEvents animationTimer As New Timer()
    Private isAnimating As Boolean = False
    
    ' UI Components
    Private WithEvents upButton As New Button()
    Private WithEvents downButton As New Button()
    Private WithEvents leftButton As New Button()
    Private WithEvents rightButton As New Button()
    Private WithEvents inButton As New Button()
    Private WithEvents outButton As New Button()
    Private WithEvents measureXButton As New Button()
    Private WithEvents measureYButton As New Button()
    Private WithEvents measureZButton As New Button()
    Private WithEvents rotateButton As New Button()
    Private WithEvents pauliDisplay As New Panel()
    Private WithEvents blochSphere As New Panel()
    Private WithEvents resultLabel As New Label()
    Private WithEvents matrixLabel As New Label()
    Private WithEvents propertyGrid As New PropertyGrid()
    
    Public Sub New()
        InitializeComponent()
        InitializePauliMatrices()
        InitializeSpinStates()
        SetupDisplay()
    End Sub
    
    
    ''' Initialize form components
    
    Private Sub InitializeComponent()
        Me.Text = "Wolfgang Pauli - Matrix Mechanics & Spin Physics"
        Me.Size = New Size(1200, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 50)
        Me.DoubleBuffered = True
        
        ' Title
        Dim titleLabel As New Label()
        titleLabel.Text = "Wolfgang Pauli (1900-1958)"
        titleLabel.Font = New Font("Times New Roman", 18, FontStyle.Bold)
        titleLabel.ForeColor = Color.Gold
        titleLabel.Size = New Size(400, 40)
        titleLabel.Location = New Point(400, 10)
        Me.Controls.Add(titleLabel)
        
        ' Subtitle
        Dim subtitleLabel As New Label()
        subtitleLabel.Text = "Pauli Matrices • Exclusion Principle • Neutrino Hypothesis"
        subtitleLabel.Font = New Font("Times New Roman", 12, FontStyle.Italic)
        subtitleLabel.ForeColor = Color.LightBlue
        subtitleLabel.Size = New Size(500, 25)
        subtitleLabel.Location = New Point(350, 50)
        Me.Controls.Add(subtitleLabel)
        
        ' Pauli Display Panel
        pauliDisplay.Location = New Point(20, 100)
        pauliDisplay.Size = New Size(400, 300)
        pauliDisplay.BackColor = Color.FromArgb(20, 20, 35)
        pauliDisplay.BorderStyle = BorderStyle.Fixed3D
        
        ' Bloch Sphere Panel
        blochSphere.Location = New Point(450, 100)
        blochSphere.Size = New Size(300, 300)
        blochSphere.BackColor = Color.FromArgb(20, 20, 35)
        blochSphere.BorderStyle = BorderStyle.Fixed3D
        
        ' Spin State Buttons
        CreateButton(upButton, "|↑⟩ Spin Up", 20, 420, Color.Cyan)
        CreateButton(downButton, "|↓⟩ Spin Down", 150, 420, Color.Cyan)
        CreateButton(leftButton, "|←⟩ Spin Left", 280, 420, Color.LightGreen)
        CreateButton(rightButton, "|→⟩ Spin Right", 410, 420, Color.LightGreen)
        CreateButton(inButton, "|⊙⟩ Spin In", 540, 420, Color.Orange)
        CreateButton(outButton, "|⊗⟩ Spin Out", 670, 420, Color.Orange)
        
        ' Measurement Buttons
        Dim measureLabel As New Label()
        measureLabel.Text = "Measure Spin Along:"
        measureLabel.ForeColor = Color.White
        measureLabel.Location = New Point(20, 480)
        measureLabel.Size = New Size(150, 20)
        Me.Controls.Add(measureLabel)
        
        CreateButton(measureXButton, "σₓ (X-axis)", 20, 510, Color.Magenta)
        CreateButton(measureYButton, "σᵧ (Y-axis)", 150, 510, Color.Magenta)
        CreateButton(measureZButton, "σ₂ (Z-axis)", 280, 510, Color.Magenta)
        
        ' Rotate Button
        CreateButton(rotateButton, "🔄 Rotate State", 410, 510, Color.Yellow)
        
        ' Result Label
        resultLabel.Location = New Point(20, 560)
        resultLabel.Size = New Size(400, 60)
        resultLabel.ForeColor = Color.White
        resultLabel.Font = New Font("Consolas", 10)
        resultLabel.BackColor = Color.FromArgb(40, 40, 60)
        resultLabel.BorderStyle = BorderStyle.FixedSingle
        
        ' Matrix Label
        matrixLabel.Location = New Point(20, 630)
        matrixLabel.Size = New Size(400, 80)
        matrixLabel.ForeColor = Color.LightGreen
        matrixLabel.Font = New Font("Consolas", 9)
        matrixLabel.BackColor = Color.FromArgb(40, 40, 60)
        matrixLabel.BorderStyle = BorderStyle.FixedSingle
        
        ' Property Grid for Pauli's achievements
        propertyGrid.Location = New Point(800, 100)
        propertyGrid.Size = New Size(350, 600)
        propertyGrid.BackColor = Color.FromArgb(40, 40, 60)
        propertyGrid.ForeColor = Color.White
        propertyGrid.HelpVisible = True
        propertyGrid.ToolbarVisible = True
        propertyGrid.SelectedObject = New PauliAchievements()
        
        ' Add controls
        Me.Controls.AddRange(New Control() {pauliDisplay, blochSphere, resultLabel, 
                                            matrixLabel, propertyGrid})
        
        ' Set up timer
        animationTimer.Interval = 50
        
        ' Paint handlers
        AddHandler pauliDisplay.Paint, AddressOf PauliDisplay_Paint
        AddHandler blochSphere.Paint, AddressOf BlochSphere_Paint
        AddHandler Me.Paint, AddressOf PauliForm_Paint
        AddHandler animationTimer.Tick, AddressOf AnimationTimer_Tick
    End Sub
    
    
    ''' Helper to create styled buttons
    
    Private Sub CreateButton(ByRef btn As Button, text As String, x As Integer, 
                            y As Integer, color As Color)
        btn.Text = text
        btn.Location = New Point(x, y)
        btn.Size = New Size(120, 35)
        btn.BackColor = color
        btn.ForeColor = Color.Black
        btn.Font = New Font("Consolas", 9, FontStyle.Bold)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 1
        Me.Controls.Add(btn)
    End Sub
    
    
    ''' Initialize Pauli matrices
    
    Private Sub InitializePauliMatrices()
        ' Identity matrix
        identity = New Complex(1, 1) {{}}
        identity(0, 0) = Complex.One
        identity(1, 1) = Complex.One
        
        ' σ₁ = [0 1; 1 0] (X-axis)
        sigma1 = New Complex(1, 1) {{}}
        sigma1(0, 1) = Complex.One
        sigma1(1, 0) = Complex.One
        
        ' σ₂ = [0 -i; i 0] (Y-axis)
        sigma2 = New Complex(1, 1) {{}}
        sigma2(0, 1) = New Complex(0, -1)
        sigma2(1, 0) = New Complex(0, 1)
        
        ' σ₃ = [1 0; 0 -1] (Z-axis)
        sigma3 = New Complex(1, 1) {{}}
        sigma3(0, 0) = Complex.One
        sigma3(1, 1) = New Complex(-1, 0)
    End Sub
    
    
    ''' Initialize spin eigenstates
    
    Private Sub InitializeSpinStates()
        ' Spin up along Z: |↑⟩ = (1,0)ᵀ
        spinUp = New Complex() {Complex.One, Complex.Zero}
        
        ' Spin down along Z: |↓⟩ = (0,1)ᵀ
        spinDown = New Complex() {Complex.Zero, Complex.One}
        
        ' Spin left along X: |←⟩ = (1/√2)(1, -1)ᵀ
        Dim invSqrt2 As Double = 1.0 / Math.Sqrt(2)
        spinLeft = New Complex() {
            New Complex(invSqrt2, 0),
            New Complex(-invSqrt2, 0)
        }
        
        ' Spin right along X: |→⟩ = (1/√2)(1, 1)ᵀ
        spinRight = New Complex() {
            New Complex(invSqrt2, 0),
            New Complex(invSqrt2, 0)
        }
        
        ' Spin in along Y: |⊙⟩ = (1/√2)(1, -i)ᵀ
        spinIn = New Complex() {
            New Complex(invSqrt2, 0),
            New Complex(0, -invSqrt2)
        }
        
        ' Spin out along Y: |⊗⟩ = (1/√2)(1, i)ᵀ
        spinOut = New Complex() {
            New Complex(invSqrt2, 0),
            New Complex(0, invSqrt2)
        }
        
        ' Set initial state
        currentState = spinUp
    End Sub
    
    
    ''' Setup display
    
    Private Sub SetupDisplay()
        UpdateMatrixDisplay()
    End Sub
    
    
    ''' Update matrix display
    
    Private Sub UpdateMatrixDisplay()
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("Pauli Matrices:")
        sb.AppendLine($"σ₁ = [0 1; 1 0]    σ₂ = [0 -i; i 0]    σ₃ = [1 0; 0 -1]")
        sb.AppendLine()
        sb.AppendLine($"Current State: {currentStateName} = [{currentState(0):F2}, {currentState(1):F2}]ᵀ")
        sb.AppendLine($"⟨σₓ⟩ = {ExpectationValue(sigma1):F3}   ⟨σᵧ⟩ = {ExpectationValue(sigma2):F3}   ⟨σ₂⟩ = {ExpectationValue(sigma3):F3}")
        
        matrixLabel.Text = sb.ToString()
    End Sub
    
    
    ''' Calculate expectation value of Pauli matrix
    
    Private Function ExpectationValue(sigma As Complex(,)) As Double
        ' ⟨ψ|σ|ψ⟩
        Dim psi_dag(1) As Complex
        For i As Integer = 0 To 1
            psi_dag(i) = New Complex(currentState(i).Real, -currentState(i).Imaginary)
        Next
        
        ' σ|ψ⟩
        Dim sigma_psi(1) As Complex
        For i As Integer = 0 To 1
            sigma_psi(i) = Complex.Zero
            For j As Integer = 0 To 1
                sigma_psi(i) += sigma(i, j) * currentState(j)
            Next
        Next
        
        ' ⟨ψ|σ|ψ⟩
        Dim result As Complex = Complex.Zero
        For i As Integer = 0 To 1
            result += psi_dag(i) * sigma_psi(i)
        Next
        
        Return result.Real
    End Function
    
    
    ''' Apply Pauli matrix to current state
    
    Private Function ApplyPauli(sigma As Complex(,)) As Complex()
        Dim result(1) As Complex
        For i As Integer = 0 To 1
            result(i) = Complex.Zero
            For j As Integer = 0 To 1
                result(i) += sigma(i, j) * currentState(j)
            Next
        Next
        Return result
    End Function
    
    
    ''' Calculate measurement probabilities
    
    Private Function MeasureProbability(sigma As Complex(,), eigenvector As Complex()) As Double
        ' |⟨eigen|ψ⟩|²
        Dim dot As Complex = Complex.Zero
        For i As Integer = 0 To 1
            dot += Complex.Conjugate(eigenvector(i)) * currentState(i)
        Next
        Return dot.Magnitude * dot.Magnitude
    End Function
    
    
    ''' Pauli display paint
    
    Private Sub PauliDisplay_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        
        ' Draw Pauli matrices
        DrawMatrix(g, sigma1, 20, 20, "σ₁")
        DrawMatrix(g, sigma2, 150, 20, "σ₂")
        DrawMatrix(g, sigma3, 280, 20, "σ₃")
        
        ' Draw current state
        DrawSpinor(g, currentState, 20, 180, currentStateName)
        
        ' Draw commutation relations
        Dim font As New Font("Consolas", 8)
        g.DrawString("[σᵢ, σⱼ] = 2iεᵢⱼₖσₖ", font, Brushes.Yellow, 20, 270)
        g.DrawString("σᵢ² = I", font, Brushes.Cyan, 20, 285)
        g.DrawString("det(σᵢ) = -1", font, Brushes.LightGreen, 20, 300)
        g.DrawString("Tr(σᵢ) = 0", font, Brushes.Orange, 20, 315)
    End Sub
    
    
    ''' Draw a 2x2 complex matrix
    
    Private Sub DrawMatrix(g As Graphics, matrix As Complex(,), x As Integer, 
                          y As Integer, label As String)
        Dim font As New Font("Consolas", 10)
        Dim brush As New SolidBrush(Color.Cyan)
        
        g.DrawString(label, font, brush, x, y - 15)
        
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                Dim value As String
                If matrix(i, j).Magnitude < 0.001 Then
                    value = "0"
                ElseIf Math.Abs(matrix(i, j).Imaginary) < 0.001 Then
                    value = If(matrix(i, j).Real > 0, "1", "-1")
                Else
                    value = If(matrix(i, j).Imaginary > 0, "i", "-i")
                End If
                
                g.DrawString(value, font, Brushes.White, x + j * 30, y + i * 20)
            Next
        Next
    End Sub
    
    
    ''' Draw spinor components
    
    Private Sub DrawSpinor(g As Graphics, spinor As Complex(), x As Integer, 
                          y As Integer, label As String)
        Dim font As New Font("Consolas", 10)
        
        g.DrawString(label, font, Brushes.Yellow, x, y - 20)
        g.DrawString("ψ = [", font, Brushes.White, x, y)
        g.DrawString($"{spinor(0):F2}", font, Brushes.LightGreen, x + 40, y)
        g.DrawString($"{spinor(1):F2}", font, Brushes.LightGreen, x + 100, y)
        g.DrawString("]ᵀ", font, Brushes.White, x + 160, y)
        
        ' Draw probability bars
        Dim prob0 As Single = CSng(spinor(0).MagnitudeSquared())
        Dim prob1 As Single = CSng(spinor(1).MagnitudeSquared())
        
        g.FillRectangle(Brushes.Cyan, x, y + 30, 100 * prob0, 20)
        g.FillRectangle(Brushes.Magenta, x + 120, y + 30, 100 * prob1, 20)
        
        g.DrawString($"P(↑) = {prob0:F3}", font, Brushes.Cyan, x, y + 55)
        g.DrawString($"P(↓) = {prob1:F3}", font, Brushes.Magenta, x + 120, y + 55)
    End Sub
    
    
    ''' Bloch sphere paint
    
    Private Sub BlochSphere_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        
        Dim center As New Point(150, 150)
        Dim radius As Integer = 120
        
        ' Draw sphere
        Using pen As New Pen(Color.FromArgb(100, 100, 150))
            g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2)
        End Using
        
        ' Draw axes
        Using pen As New Pen(Color.Gray, 1)
            pen.DashStyle = DashStyle.Dash
            g.DrawLine(pen, center.X - radius - 20, center.Y, center.X + radius + 20, center.Y)
            g.DrawLine(pen, center.X, center.Y - radius - 20, center.X, center.Y + radius + 20)
        End Using
        
        ' Draw axis labels
        Dim font As New Font("Consolas", 8)
        g.DrawString("X", font, Brushes.Cyan, center.X + radius + 5, center.Y)
        g.DrawString("Y", font, Brushes.LightGreen, center.X + 5, center.Y - radius - 15)
        g.DrawString("Z", font, Brushes.Orange, center.X - 15, center.Y - radius - 5)
        
        ' Calculate Bloch sphere coordinates
        Dim theta As Double = 2 * Math.Acos(Math.Max(0, Math.Min(1, currentState(0).Magnitude)))
        Dim phi As Double = Math.Atan2(currentState(1).Imaginary, currentState(1).Real) - 
                           Math.Atan2(currentState(0).Imaginary, currentState(0).Real)
        
        ' Add rotation animation
        If isAnimating Then
            phi += rotationAngle
        End If
        
        ' Convert to Cartesian
        Dim x As Double = radius * Math.Sin(theta) * Math.Cos(phi)
        Dim y As Double = radius * Math.Sin(theta) * Math.Sin(phi)
        Dim z As Double = radius * Math.Cos(theta)
        
        Dim statePoint As New Point(
            center.X + CInt(x),
            center.Y - CInt(z)  ' Y axis is vertical in display
        )
        
        ' Draw state vector
        Using pen As New Pen(Color.Yellow, 2)
            pen.EndCap = LineCap.ArrowAnchor
            g.DrawLine(pen, center, statePoint)
        End Using
        
        ' Draw point
        g.FillEllipse(Brushes.Yellow, statePoint.X - 5, statePoint.Y - 5, 10, 10)
        
        ' Draw coordinates
        g.DrawString($"θ = {theta / Math.PI:F2}π", font, Brushes.White, 10, 10)
        g.DrawString($"φ = {phi / Math.PI:F2}π", font, Brushes.White, 10, 25)
    End Sub
    
    
    ''' Main form paint
    
    Private Sub PauliForm_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        
        ' Draw Pauli's achievements
        Dim font As New Font("Times New Roman", 10)
        Dim y As Integer = 700
        
        g.DrawString("🏆 Nobel Prize 1945: Exclusion Principle", font, Brushes.Gold, 20, y)
        g.DrawString("🔮 Predicted neutrino (1930) to save energy conservation", font, Brushes.LightBlue, 20, y + 20)
        g.DrawString("⚛️ Pauli matrices - foundation of spin physics", font, Brushes.Cyan, 20, y + 40)
        g.DrawString("🧮 Pauli effect - legendary ability to break equipment", font, Brushes.Orange, 20, y + 60)
    End Sub
    
    
    ''' Animation timer tick
    
    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
        rotationAngle += 0.1
        If rotationAngle > 2 * Math.PI Then
            rotationAngle -= 2 * Math.PI
        End If
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin up button click
    
    Private Sub UpButton_Click(sender As Object, e As EventArgs) Handles upButton.Click
        currentState = spinUp
        currentStateName = "|↑⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin down button click
    
    Private Sub DownButton_Click(sender As Object, e As EventArgs) Handles downButton.Click
        currentState = spinDown
        currentStateName = "|↓⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin left button click
    
    Private Sub LeftButton_Click(sender As Object, e As EventArgs) Handles leftButton.Click
        currentState = spinLeft
        currentStateName = "|←⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin right button click
    
    Private Sub RightButton_Click(sender As Object, e As EventArgs) Handles rightButton.Click
        currentState = spinRight
        currentStateName = "|→⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin in button click
    
    Private Sub InButton_Click(sender As Object, e As EventArgs) Handles inButton.Click
        currentState = spinIn
        currentStateName = "|⊙⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Spin out button click
    
    Private Sub OutButton_Click(sender As Object, e As EventArgs) Handles outButton.Click
        currentState = spinOut
        currentStateName = "|⊗⟩"
        measurementResult = ""
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Measure X button click
    
    Private Sub MeasureXButton_Click(sender As Object, e As EventArgs) Handles measureXButton.Click
        MeasureSpin(sigma1, "X", spinRight, spinLeft, "→", "←")
    End Sub
    
    
    ''' Measure Y button click
    
    Private Sub MeasureYButton_Click(sender As Object, e As EventArgs) Handles measureYButton.Click
        MeasureSpin(sigma2, "Y", spinOut, spinIn, "⊗", "⊙")
    End Sub
    
    
    ''' Measure Z button click
    
    Private Sub MeasureZButton_Click(sender As Object, e As EventArgs) Handles measureZButton.Click
        MeasureSpin(sigma3, "Z", spinUp, spinDown, "↑", "↓")
    End Sub
    
    
    ''' Generic spin measurement
    
    Private Sub MeasureSpin(sigma As Complex(,), axis As String, 
                           eigenPlus As Complex(), eigenMinus As Complex(),
                           plusName As String, minusName As String)
        Dim probPlus As Double = MeasureProbability(sigma, eigenPlus)
        Dim probMinus As Double = MeasureProbability(sigma, eigenMinus)
        
        ' Simulate measurement outcome
        Dim rand As New Random()
        Dim outcome As String
        Dim collapsed As Complex()
        
        If rand.NextDouble() < probPlus Then
            outcome = $"+ħ/2 ({plusName})"
            collapsed = eigenPlus
        Else
            outcome = $"-ħ/2 ({minusName})"
            collapsed = eigenMinus
        End If
        
        ' Update state (collapse)
        currentState = collapsed
        currentStateName = If(outcome.Contains("+"), $"|{plusName}⟩", $"|{minusName}⟩")
        
        measurementResult = $"Measurement along {axis}-axis: {outcome}" & vbCrLf &
                           $"Probability: P(+) = {probPlus:F3}, P(-) = {probMinus:F3}" & vbCrLf &
                           $"⟨σ{sub script(axis)}⟩ = {ExpectationValue(sigma):F3}"
        
        resultLabel.Text = measurementResult
        UpdateMatrixDisplay()
        blochSphere.Invalidate()
    End Sub
    
    
    ''' Rotate button click
    
    Private Sub RotateButton_Click(sender As Object, e As EventArgs) Handles rotateButton.Click
        isAnimating = Not isAnimating
        If isAnimating Then
            animationTimer.Start()
            rotateButton.Text = "⏸ Stop Rotation"
        Else
            animationTimer.Stop()
            rotateButton.Text = "🔄 Rotate State"
        End If
    End Sub
    
    
    ''' Helper for subscript
    
    Private Function sub script(axis As String) As String
        Select Case axis
            Case "X" : Return "ₓ"
            Case "Y" : Return "ᵧ"
            Case "Z" : Return "₂"
            Case Else : Return ""
        End Select
    End Function
End Class


''' Pauli's achievements class for property grid

Public Class PauliAchievements
    Public Property Name As String = "Wolfgang Pauli"
    Public Property Born As String = "April 25, 1900, Vienna, Austria"
    Public Property Died As String = "December 15, 1958, Zurich, Switzerland"
    Public Property NobelPrize As String = "1945 Physics - Exclusion Principle"
    
    Public ReadOnly Property PauliMatrices As String
        Get
            Return "σ₁ = [0 1; 1 0]" & vbCrLf &
                   "σ₂ = [0 -i; i 0]" & vbCrLf &
                   "σ₃ = [1 0; 0 -1]"
        End Get
    End Property
    
    Public ReadOnly Property MatrixProperties As String
        Get
            Return "• Hermitian: σᵢ† = σᵢ" & vbCrLf &
                   "• Unitary: σᵢ†σᵢ = I" & vbCrLf &
                   "• Trace: Tr(σᵢ) = 0" & vbCrLf &
                   "• Determinant: det(σᵢ) = -1" & vbCrLf &
                   "• σᵢ² = I" & vbCrLf &
                   "• [σᵢ, σⱼ] = 2iεᵢⱼₖσₖ"
        End Get
    End Property
    
    Public ReadOnly Property ExclusionPrinciple As String
        Get
            Return "No two fermions can occupy the same quantum state" & vbCrLf &
                   "Explains electron shell structure" & vbCrLf &
                   "Fundamental to matter stability"
        End Get
    End Property
    
    Public ReadOnly Property NeutrinoHypothesis As String
        Get
            Return "Proposed 1930 to explain beta decay energy spectrum" & vbCrLf &
                   "Predicted neutral, nearly massless particle" & vbCrLf &
                   "Confirmed 1956 (Cowan-Reines experiment)"
        End Get
    End Property
    
    Public ReadOnly Property PauliEffect As String
        Get
            Return "Legendary ability to cause equipment failure" & vbCrLf &
                   "by mere presence" & vbCrLf &
                   "Inside joke in physics community"
        End Get
    End Property
    
    Public ReadOnly Property SpinStatistics As String
        Get
            Return "Fermions: half-integer spin (Pauli principle)" & vbCrLf &
                   "Bosons: integer spin (no exclusion)" & vbCrLf &
                   "Spin-Statistics theorem"
        End Get
    End Property
    
    Public ReadOnly Property FamousQuotes As String
        Get
            Return "'I don't mind if you think slowly, but I do object when you publish more quickly than you think.'" & vbCrLf &
                   "'This isn't right. This isn't even wrong.'" & vbCrLf &
                   "'God made the bulk; surfaces were invented by the devil.'"
        End Get
    End Property
End Class


''' Module containing the main entry point

Module Program
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New PauliForm())
    End Sub
End Module


''' Supplementary class for Pauli algebra

Public Class PauliAlgebra
    
    ''' Calculate commutator [A, B]
    
    Public Shared Function Commutator(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(1, 1) As Complex
        
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                result(i, j) = Complex.Zero
                For k As Integer = 0 To 1
                    result(i, j) += A(i, k) * B(k, j) - B(i, k) * A(k, j)
                Next
            Next
        Next
        
        Return result
    End Function
    
    
    ''' Calculate anticommutator {A, B}
    
    Public Shared Function AntiCommutator(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(1, 1) As Complex
        
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                result(i, j) = Complex.Zero
                For k As Integer = 0 To 1
                    result(i, j) += A(i, k) * B(k, j) + B(i, k) * A(k, j)
                Next
            Next
        Next
        
        Return result
    End Function
    
    
    ''' Verify Pauli algebra properties
    
    Public Shared Function VerifyProperties(sigma1 As Complex(,), sigma2 As Complex(,), 
                                           sigma3 As Complex(,)) As String
        Dim sb As New System.Text.StringBuilder()
        
        ' Check σᵢ² = I
        Dim s1sq = MatrixMultiply(sigma1, sigma1)
        Dim s2sq = MatrixMultiply(sigma2, sigma2)
        Dim s3sq = MatrixMultiply(sigma3, sigma3)
        
        sb.AppendLine("σ₁² = I: " & IsIdentity(s1sq))
        sb.AppendLine("σ₂² = I: " & IsIdentity(s2sq))
        sb.AppendLine("σ₃² = I: " & IsIdentity(s3sq))
        
        ' Check commutation relations
        Dim comm12 = Commutator(sigma1, sigma2)
        Dim shouldBe2is3 = MatrixMultiply(sigma3, New Complex(0, 2))
        
        sb.AppendLine("[σ₁,σ₂] = 2iσ₃: " & AreEqual(comm12, shouldBe2is3))
        
        Return sb.ToString()
    End Function
    
    Private Shared Function MatrixMultiply(A As Complex(,), B As Complex(,)) As Complex(,)
        Dim result(1, 1) As Complex
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                result(i, j) = Complex.Zero
                For k As Integer = 0 To 1
                    result(i, j) += A(i, k) * B(k, j)
                Next
            Next
        Next
        Return result
    End Function
    
    Private Shared Function IsIdentity(M As Complex(,)) As Boolean
        Dim eps As Double = 0.001
        Return (M(0, 0) - Complex.One).Magnitude < eps AndAlso
               M(0, 1).Magnitude < eps AndAlso
               M(1, 0).Magnitude < eps AndAlso
               (M(1, 1) - Complex.One).Magnitude < eps
    End Function
    
    Private Shared Function AreEqual(A As Complex(,), B As Complex(,)) As Boolean
        Dim eps As Double = 0.001
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                If (A(i, j) - B(i, j)).Magnitude > eps Then
                    Return False
                End If
            Next
        Next
        Return True
    End Function
End Class


''' Supplementary class for two-state quantum system

Public Class TwoStateSystem
    Public Property State As Complex()
    
    Public Sub New()
        State = New Complex() {Complex.One, Complex.Zero}
    End Sub
    
    
    ''' Apply rotation around axis
    
    Public Sub Rotate(axis As String, angle As Double)
        ' Rotation operator R = exp(-i θ/2 σ·n)
        Dim cos As Double = Math.Cos(angle / 2)
        Dim sin As Double = Math.Sin(angle / 2)
        
        Select Case axis.ToUpper()
            Case "X"
                ' R_x(θ) = [cos(θ/2)  -i sin(θ/2); -i sin(θ/2)  cos(θ/2)]
                Dim new0 = State(0) * cos + State(1) * New Complex(0, -sin)
                Dim new1 = State(0) * New Complex(0, -sin) + State(1) * cos
                State(0) = new0
                State(1) = new1
                
            Case "Y"
                ' R_y(θ) = [cos(θ/2)  -sin(θ/2); sin(θ/2)  cos(θ/2)]
                Dim new0 = State(0) * cos - State(1) * sin
                Dim new1 = State(0) * sin + State(1) * cos
                State(0) = new0
                State(1) = new1
                
            Case "Z"
                ' R_z(θ) = [e^{-iθ/2} 0; 0 e^{iθ/2}]
                State(0) = State(0) * New Complex(cos, -sin)
                State(1) = State(1) * New Complex(cos, sin)
        End Select
    End Sub
    
    
    ''' Calculate density matrix
    
    Public Function DensityMatrix() As Complex(,)
        Dim rho(1, 1) As Complex
        
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                rho(i, j) = State(i) * Complex.Conjugate(State(j))
            Next
        Next
        
        Return rho
    End Function
    
    
    ''' Calculate purity Tr(ρ²)
    
    Public Function Purity() As Double
        Dim rho = DensityMatrix()
        Dim rho2(1, 1) As Complex
        
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                rho2(i, j) = Complex.Zero
                For k As Integer = 0 To 1
                    rho2(i, j) += rho(i, k) * rho(k, j)
                Next
            Next
        Next
        
        Dim tr As Complex = Complex.Zero
        For i As Integer = 0 To 1
            tr += rho2(i, i)
        Next
        
        Return tr.Real
    End Function
    
    Public Overrides Function ToString() As String
        Return $"[{State(0):F3}, {State(1):F3}]ᵀ"
    End Function
End Class