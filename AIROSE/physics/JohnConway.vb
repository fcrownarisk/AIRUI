' Program: GameOfLife.vb
' Description: Comprehensive simulation of John Conway's Game of Life
'              A cellular automaton where cells live, die, or are born based on simple rules.
'              The simulation demonstrates emergent complexity from simple rules.
'
' Rules of the Game of Life:
' 1. Any live cell with fewer than two live neighbours dies (underpopulation)
' 2. Any live cell with two or three live neighbours lives on (survival)
' 3. Any live cell with more than three live neighbours dies (overpopulation)
' 4. Any dead cell with exactly three live neighbours becomes alive (reproduction)

Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Threading
Imports System.Collections.Generic


''' Main form for the Game of Life simulation

Public Class GameOfLifeForm
    Inherits Form
    
    ' Game grid dimensions
    Private Const GRID_WIDTH As Integer = 100
    Private Const GRID_HEIGHT As Integer = 80
    Private Const CELL_SIZE As Integer = 8
    
    ' Game state
    Private currentGrid As Boolean(,) = New Boolean(GRID_WIDTH - 1, GRID_HEIGHT - 1) {}
    Private nextGrid As Boolean(,) = New Boolean(GRID_WIDTH - 1, GRID_HEIGHT - 1) {}
    Private generation As Integer = 0
    Private isRunning As Boolean = False
    Private simulationSpeed As Integer = 100 ' milliseconds
    
    ' UI Components
    Private WithEvents mainTimer As New Timer()
    Private WithEvents startButton As New Button()
    Private WithEvents stopButton As New Button()
    Private WithEvents clearButton As New Button()
    Private WithEvents randomButton As New Button()
    Private WithEvents speedTrackBar As New TrackBar()
    Private WithEvents generationLabel As New Label()
    Private WithEvents populationLabel As New Label()
    Private WithEvents patternComboBox As New ComboBox()
    Private WithEvents gliderGunButton As New Button()
    Private WithEvents saveButton As New Button()
    Private WithEvents loadButton As New Button()
    Private WithEvents statusStrip As New StatusStrip()
    Private WithEvents toolStripStatusLabel As New ToolStripStatusLabel()
    
    
    ''' Constructor - initializes the form and components
    
    Public Sub New()
        InitializeComponent()
        InitializeGame()
        
        ' Set up the timer
        mainTimer.Interval = simulationSpeed
    End Sub
    
    
    ''' Initialize form components
    
    Private Sub InitializeComponent()
        Me.Text = "John Conway's Game of Life"
        Me.Size = New Size(GRID_WIDTH * CELL_SIZE + 250, GRID_HEIGHT * CELL_SIZE + 100)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.DoubleBuffered = True
        Me.BackColor = Color.FromArgb(240, 240, 255)
        
        ' Start Button
        startButton.Text = "▶ Start"
        startButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 20)
        startButton.Size = New Size(100, 30)
        startButton.BackColor = Color.LightGreen
        startButton.Font = New Font("Arial", 10, FontStyle.Bold)
        
        ' Stop Button
        stopButton.Text = "⏸ Stop"
        stopButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 60)
        stopButton.Size = New Size(100, 30)
        stopButton.BackColor = Color.LightCoral
        stopButton.Font = New Font("Arial", 10, FontStyle.Bold)
        stopButton.Enabled = False
        
        ' Clear Button
        clearButton.Text = "🗑 Clear"
        clearButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 100)
        clearButton.Size = New Size(100, 30)
        clearButton.BackColor = Color.LightGray
        
        ' Random Button
        randomButton.Text = "🎲 Random"
        randomButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 140)
        randomButton.Size = New Size(100, 30)
        randomButton.BackColor = Color.LightBlue
        
        ' Glider Gun Button
        gliderGunButton.Text = "🔫 Glider Gun"
        gliderGunButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 180)
        gliderGunButton.Size = New Size(100, 30)
        gliderGunButton.BackColor = Color.LightSalmon
        
        ' Save Button
        saveButton.Text = "💾 Save"
        saveButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 220)
        saveButton.Size = New Size(48, 30)
        saveButton.BackColor = Color.LightYellow
        
        ' Load Button
        loadButton.Text = "📂 Load"
        loadButton.Location = New Point(GRID_WIDTH * CELL_SIZE + 72, 220)
        loadButton.Size = New Size(48, 30)
        loadButton.BackColor = Color.LightYellow
        
        ' Speed TrackBar
        speedTrackBar.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 260)
        speedTrackBar.Size = New Size(100, 45)
        speedTrackBar.Minimum = 1
        speedTrackBar.Maximum = 20
        speedTrackBar.Value = 10
        speedTrackBar.TickFrequency = 2
        speedTrackBar.Label = "Speed"
        
        ' Generation Label
        generationLabel.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 310)
        generationLabel.Size = New Size(100, 20)
        generationLabel.Text = "Generation: 0"
        generationLabel.Font = New Font("Arial", 9, FontStyle.Bold)
        
        ' Population Label
        populationLabel.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 335)
        populationLabel.Size = New Size(100, 20)
        populationLabel.Text = "Population: 0"
        populationLabel.Font = New Font("Arial", 9, FontStyle.Bold)
        
        ' Pattern ComboBox
        patternComboBox.Location = New Point(GRID_WIDTH * CELL_SIZE + 20, 365)
        patternComboBox.Size = New Size(100, 25)
        patternComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        patternComboBox.Items.AddRange(New String() {"Glider", "Blinker", "Block", "Beehive", 
                                                      "Loaf", "Boat", "Tub", "Pulsar", 
                                                      "Pentadecathlon", "Spaceship"})
        patternComboBox.SelectedIndex = 0
        
        ' Status Strip
        statusStrip.Items.Add(toolStripStatusLabel)
        toolStripStatusLabel.Text = "Ready - Click cells to toggle, or use patterns"
        
        ' Add controls to form
        Me.Controls.AddRange(New Control() {startButton, stopButton, clearButton, randomButton,
                                            gliderGunButton, saveButton, loadButton, speedTrackBar,
                                            generationLabel, populationLabel, patternComboBox,
                                            statusStrip})
        
        ' Set up event handlers
        AddHandler Me.Paint, AddressOf GameOfLifeForm_Paint
        AddHandler Me.MouseClick, AddressOf GameOfLifeForm_MouseClick
        AddHandler Me.MouseMove, AddressOf GameOfLifeForm_MouseMove
        AddHandler mainTimer.Tick, AddressOf Timer_Tick
        AddHandler speedTrackBar.Scroll, AddressOf SpeedTrackBar_Scroll
    End Sub
    
    
    ''' Initialize the game grid
    
    Private Sub InitializeGame()
        Array.Clear(currentGrid, 0, currentGrid.Length)
        Array.Clear(nextGrid, 0, nextGrid.Length)
        generation = 0
        UpdateLabels()
    End Sub
    
    
    ''' Start the simulation
    
    Private Sub StartButton_Click(sender As Object, e As EventArgs) Handles startButton.Click
        isRunning = True
        mainTimer.Start()
        startButton.Enabled = False
        stopButton.Enabled = True
        toolStripStatusLabel.Text = "Simulation running..."
    End Sub
    
    
    ''' Stop the simulation
    
    Private Sub StopButton_Click(sender As Object, e As EventArgs) Handles stopButton.Click
        isRunning = False
        mainTimer.Stop()
        startButton.Enabled = True
        stopButton.Enabled = False
        toolStripStatusLabel.Text = "Simulation paused"
    End Sub
    
    
    ''' Clear the grid
    
    Private Sub ClearButton_Click(sender As Object, e As EventArgs) Handles clearButton.Click
        InitializeGame()
        Me.Invalidate()
        toolStripStatusLabel.Text = "Grid cleared"
    End Sub
    
    
    ''' Randomize the grid
    
    Private Sub RandomButton_Click(sender As Object, e As EventArgs) Handles randomButton.Click
        Dim rand As New Random()
        For x As Integer = 0 To GRID_WIDTH - 1
            For y As Integer = 0 To GRID_HEIGHT - 1
                currentGrid(x, y) = rand.NextDouble() < 0.3 ' 30% chance of life
            Next
        Next
        generation = 0
        Me.Invalidate()
        UpdateLabels()
        toolStripStatusLabel.Text = "Random pattern generated"
    End Sub
    
    
    ''' Place a glider gun (Gosper's glider gun)
    
    Private Sub GliderGunButton_Click(sender As Object, e As EventArgs) Handles gliderGunButton.Click
        InitializeGame()
        
        ' Gosper's glider gun coordinates (shifted to center)
        Dim offsetX As Integer = 20
        Dim offsetY As Integer = 20
        
        ' Left block
        currentGrid(offsetX + 0, offsetY + 4) = True
        currentGrid(offsetX + 1, offsetY + 4) = True
        currentGrid(offsetX + 0, offsetY + 5) = True
        currentGrid(offsetX + 1, offsetY + 5) = True
        
        ' Left structure
        currentGrid(offsetX + 10, offsetY + 4) = True
        currentGrid(offsetX + 10, offsetY + 5) = True
        currentGrid(offsetX + 10, offsetY + 6) = True
        currentGrid(offsetX + 11, offsetY + 3) = True
        currentGrid(offsetX + 11, offsetY + 7) = True
        currentGrid(offsetX + 12, offsetY + 2) = True
        currentGrid(offsetX + 12, offsetY + 8) = True
        currentGrid(offsetX + 13, offsetY + 5) = True
        currentGrid(offsetX + 14, offsetY + 3) = True
        currentGrid(offsetX + 14, offsetY + 7) = True
        currentGrid(offsetX + 15, offsetY + 4) = True
        currentGrid(offsetX + 15, offsetY + 5) = True
        currentGrid(offsetX + 15, offsetY + 6) = True
        currentGrid(offsetX + 16, offsetY + 5) = True
        
        ' Right block
        currentGrid(offsetX + 20, offsetY + 2) = True
        currentGrid(offsetX + 20, offsetY + 3) = True
        currentGrid(offsetX + 21, offsetY + 2) = True
        currentGrid(offsetX + 21, offsetY + 3) = True
        
        ' Right structure
        currentGrid(offsetX + 34, offsetY + 2) = True
        currentGrid(offsetX + 34, offsetY + 3) = True
        currentGrid(offsetX + 35, offsetY + 2) = True
        currentGrid(offsetX + 35, offsetY + 3) = True
        
        Me.Invalidate()
        UpdateLabels()
        toolStripStatusLabel.Text = "Gosper's Glider Gun placed"
    End Sub
    
    
    ''' Place a selected pattern
    
    Private Sub PatternComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles patternComboBox.SelectedIndexChanged
        If Not isRunning Then
            PlacePattern(100, 40, patternComboBox.SelectedIndex)
        End If
    End Sub
    
    
    ''' Place a pattern at specified coordinates
    
    Private Sub PlacePattern(centerX As Integer, centerY As Integer, patternIndex As Integer)
        Select Case patternIndex
            Case 0 ' Glider
                If centerX < GRID_WIDTH - 2 AndAlso centerY < GRID_HEIGHT - 2 Then
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX + 2, centerY + 1) = True
                    currentGrid(centerX, centerY + 2) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                    currentGrid(centerX + 2, centerY + 2) = True
                End If
                
            Case 1 ' Blinker (period 3 oscillator)
                If centerY < GRID_HEIGHT - 2 Then
                    currentGrid(centerX, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX, centerY + 2) = True
                End If
                
            Case 2 ' Block (still life)
                If centerX < GRID_WIDTH - 1 AndAlso centerY < GRID_HEIGHT - 1 Then
                    currentGrid(centerX, centerY) = True
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 1, centerY + 1) = True
                End If
                
            Case 3 ' Beehive (still life)
                If centerX < GRID_WIDTH - 3 AndAlso centerY < GRID_HEIGHT - 2 Then
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX + 2, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 3, centerY + 1) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                    currentGrid(centerX + 2, centerY + 2) = True
                End If
                
            Case 4 ' Loaf (still life)
                If centerX < GRID_WIDTH - 3 AndAlso centerY < GRID_HEIGHT - 3 Then
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX + 2, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 3, centerY + 1) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                    currentGrid(centerX + 3, centerY + 2) = True
                    currentGrid(centerX + 2, centerY + 3) = True
                End If
                
            Case 5 ' Boat (still life)
                If centerX < GRID_WIDTH - 2 AndAlso centerY < GRID_HEIGHT - 2 Then
                    currentGrid(centerX, centerY) = True
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 2, centerY + 1) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                End If
                
            Case 6 ' Tub (still life)
                If centerX < GRID_WIDTH - 2 AndAlso centerY < GRID_HEIGHT - 2 Then
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 2, centerY + 1) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                End If
                
            Case 7 ' Pulsar (period 3 oscillator)
                If centerX < GRID_WIDTH - 14 AndAlso centerY < GRID_HEIGHT - 14 Then
                    Dim pulsarPattern As Integer(,) = {
                        {2,0}, {3,0}, {4,0}, {8,0}, {9,0}, {10,0},
                        {0,2}, {5,2}, {7,2}, {12,2},
                        {0,3}, {5,3}, {7,3}, {12,3},
                        {0,4}, {5,4}, {7,4}, {12,4},
                        {2,5}, {3,5}, {4,5}, {8,5}, {9,5}, {10,5},
                        {2,7}, {3,7}, {4,7}, {8,7}, {9,7}, {10,7},
                        {0,8}, {5,8}, {7,8}, {12,8},
                        {0,9}, {5,9}, {7,9}, {12,9},
                        {0,10}, {5,10}, {7,10}, {12,10},
                        {2,12}, {3,12}, {4,12}, {8,12}, {9,12}, {10,12}
                    }
                    
                    For i As Integer = 0 To pulsarPattern.GetLength(0) - 1
                        Dim px As Integer = centerX + pulsarPattern(i, 0)
                        Dim py As Integer = centerY + pulsarPattern(i, 1)
                        If px < GRID_WIDTH AndAlso py < GRID_HEIGHT Then
                            currentGrid(px, py) = True
                        End If
                    Next
                End If
                
            Case 8 ' Pentadecathlon (period 15 oscillator)
                If centerX < GRID_WIDTH - 10 AndAlso centerY < GRID_HEIGHT - 3 Then
                    For i As Integer = 0 To 9
                        currentGrid(centerX + i, centerY + 1) = True
                    Next
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX + 2, centerY) = True
                    currentGrid(centerX + 7, centerY) = True
                    currentGrid(centerX + 8, centerY) = True
                    currentGrid(centerX + 1, centerY + 2) = True
                    currentGrid(centerX + 2, centerY + 2) = True
                    currentGrid(centerX + 7, centerY + 2) = True
                    currentGrid(centerX + 8, centerY + 2) = True
                End If
                
            Case 9 ' Lightweight Spaceship (LWSS)
                If centerX < GRID_WIDTH - 5 AndAlso centerY < GRID_HEIGHT - 4 Then
                    currentGrid(centerX + 1, centerY) = True
                    currentGrid(centerX + 2, centerY) = True
                    currentGrid(centerX + 3, centerY) = True
                    currentGrid(centerX + 4, centerY) = True
                    currentGrid(centerX, centerY + 1) = True
                    currentGrid(centerX + 4, centerY + 1) = True
                    currentGrid(centerX + 4, centerY + 2) = True
                    currentGrid(centerX, centerY + 3) = True
                    currentGrid(centerX + 3, centerY + 3) = True
                End If
        End Select
        
        Me.Invalidate()
        UpdateLabels()
    End Sub
    
    
    ''' Save the current pattern to file
    
    Private Sub SaveButton_Click(sender As Object, e As EventArgs) Handles saveButton.Click
        Using saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Game of Life files (*.gol)|*.gol|Text files (*.txt)|*.txt"
            saveDialog.DefaultExt = "gol"
            saveDialog.Title = "Save Game of Life Pattern"
            
            If saveDialog.ShowDialog() = DialogResult.OK Then
                Try
                    Using writer As New System.IO.StreamWriter(saveDialog.FileName)
                        writer.WriteLine(GRID_WIDTH)
                        writer.WriteLine(GRID_HEIGHT)
                        
                        For y As Integer = 0 To GRID_HEIGHT - 1
                            Dim line As String = ""
                            For x As Integer = 0 To GRID_WIDTH - 1
                                line += If(currentGrid(x, y), "1", "0")
                            Next
                            writer.WriteLine(line)
                        Next
                    End Using
                    
                    toolStripStatusLabel.Text = "Pattern saved successfully"
                Catch ex As Exception
                    MessageBox.Show("Error saving file: " & ex.Message, "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub
    
    
    ''' Load a pattern from file
    
    Private Sub LoadButton_Click(sender As Object, e As EventArgs) Handles loadButton.Click
        Using openDialog As New OpenFileDialog()
            openDialog.Filter = "Game of Life files (*.gol)|*.gol|Text files (*.txt)|*.txt"
            openDialog.Title = "Load Game of Life Pattern"
            
            If openDialog.ShowDialog() = DialogResult.OK Then
                Try
                    Using reader As New System.IO.StreamReader(openDialog.FileName)
                        Dim width As Integer = Integer.Parse(reader.ReadLine())
                        Dim height As Integer = Integer.Parse(reader.ReadLine())
                        
                        If width > GRID_WIDTH OrElse height > GRID_HEIGHT Then
                            MessageBox.Show("Pattern is too large for current grid", "Warning",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return
                        End If
                        
                        InitializeGame()
                        
                        For y As Integer = 0 To Math.Min(height, GRID_HEIGHT) - 1
                            Dim line As String = reader.ReadLine()
                            For x As Integer = 0 To Math.Min(width, GRID_WIDTH) - 1
                                If x < line.Length Then
                                    currentGrid(x, y) = (line(x) = "1"c)
                                End If
                            Next
                        Next
                    End Using
                    
                    Me.Invalidate()
                    UpdateLabels()
                    toolStripStatusLabel.Text = "Pattern loaded successfully"
                Catch ex As Exception
                    MessageBox.Show("Error loading file: " & ex.Message, "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub
    
    
    ''' Handle mouse clicks to toggle cells
    
    Private Sub GameOfLifeForm_MouseClick(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left AndAlso Not isRunning Then
            Dim gridX As Integer = e.X \ CELL_SIZE
            Dim gridY As Integer = e.Y \ CELL_SIZE
            
            If gridX >= 0 AndAlso gridX < GRID_WIDTH AndAlso 
               gridY >= 0 AndAlso gridY < GRID_HEIGHT Then
                currentGrid(gridX, gridY) = Not currentGrid(gridX, gridY)
                Me.Invalidate()
                UpdateLabels()
                toolStripStatusLabel.Text = $"Cell ({gridX}, {gridY}) toggled"
            End If
        End If
    End Sub
    
    
    ''' Handle mouse move for drawing
    
    Private Sub GameOfLifeForm_MouseMove(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left AndAlso Not isRunning AndAlso Control.ModifierKeys = Keys.Shift Then
            Dim gridX As Integer = e.X \ CELL_SIZE
            Dim gridY As Integer = e.Y \ CELL_SIZE
            
            If gridX >= 0 AndAlso gridX < GRID_WIDTH AndAlso 
               gridY >= 0 AndAlso gridY < GRID_HEIGHT Then
                currentGrid(gridX, gridY) = True
                Me.Invalidate()
                UpdateLabels()
            End If
        End If
    End Sub
    
    
    ''' Timer tick - update the game state
    
    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        ComputeNextGeneration()
        SwapGrids()
        generation += 1
        UpdateLabels()
        Me.Invalidate()
    End Sub
    
    
    ''' Compute the next generation using Conway's rules
    
    Private Sub ComputeNextGeneration()
        For x As Integer = 0 To GRID_WIDTH - 1
            For y As Integer = 0 To GRID_HEIGHT - 1
                Dim neighbors As Integer = CountLiveNeighbors(x, y)
                
                If currentGrid(x, y) Then
                    ' Live cell rules
                    nextGrid(x, y) = (neighbors = 2 OrElse neighbors = 3)
                Else
                    ' Dead cell rule
                    nextGrid(x, y) = (neighbors = 3)
                End If
            Next
        Next
    End Sub
    
    
    ''' Count live neighbors for a cell
    
    Private Function CountLiveNeighbors(x As Integer, y As Integer) As Integer
        Dim count As Integer = 0
        
        For dx As Integer = -1 To 1
            For dy As Integer = -1 To 1
                If dx = 0 AndAlso dy = 0 Then Continue For
                
                Dim nx As Integer = x + dx
                Dim ny As Integer = y + dy
                
                ' Toroidal grid (wrap around)
                If nx < 0 Then nx = GRID_WIDTH - 1
                If nx >= GRID_WIDTH Then nx = 0
                If ny < 0 Then ny = GRID_HEIGHT - 1
                If ny >= GRID_HEIGHT Then ny = 0
                
                If currentGrid(nx, ny) Then count += 1
            Next
        Next
        
        Return count
    End Function
    
    
    ''' Swap the current and next grids
    
    Private Sub SwapGrids()
        Dim temp As Boolean(,) = currentGrid
        currentGrid = nextGrid
        nextGrid = temp
        Array.Clear(nextGrid, 0, nextGrid.Length)
    End Sub
    
    
    ''' Update the generation and population labels
    
    Private Sub UpdateLabels()
        generationLabel.Text = $"Generation: {generation}"
        
        Dim population As Integer = 0
        For x As Integer = 0 To GRID_WIDTH - 1
            For y As Integer = 0 To GRID_HEIGHT - 1
                If currentGrid(x, y) Then population += 1
            Next
        Next
        populationLabel.Text = $"Population: {population}"
    End Sub
    
    
    ''' Handle speed trackbar changes
    
    Private Sub SpeedTrackBar_Scroll(sender As Object, e As EventArgs)
        ' Convert trackbar value (1-20) to milliseconds (200-10)
        simulationSpeed = 210 - (speedTrackBar.Value * 10)
        mainTimer.Interval = simulationSpeed
        toolStripStatusLabel.Text = $"Speed: {Math.Round(1000.0 / simulationSpeed, 1)} generations/sec"
    End Sub
    
    
    ''' Paint the game grid
    
    Private Sub GameOfLifeForm_Paint(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.None
        
        ' Draw grid lines
        Using gridPen As New Pen(Color.FromArgb(200, 200, 255))
            For x As Integer = 0 To GRID_WIDTH
                g.DrawLine(gridPen, x * CELL_SIZE, 0, x * CELL_SIZE, GRID_HEIGHT * CELL_SIZE)
            Next
            For y As Integer = 0 To GRID_HEIGHT
                g.DrawLine(gridPen, 0, y * CELL_SIZE, GRID_WIDTH * CELL_SIZE, y * CELL_SIZE)
            Next
        End Using
        
        ' Draw live cells
        For x As Integer = 0 To GRID_WIDTH - 1
            For y As Integer = 0 To GRID_HEIGHT - 1
                If currentGrid(x, y) Then
                    ' Color based on position for visual interest
                    Dim color As Color = Color.FromArgb(
                        100 + x Mod 155,
                        150 + y Mod 105,
                        200
                    )
                    
                    Using cellBrush As New SolidBrush(color)
                        g.FillRectangle(cellBrush, 
                                      x * CELL_SIZE + 1, 
                                      y * CELL_SIZE + 1, 
                                      CELL_SIZE - 2, 
                                      CELL_SIZE - 2)
                    End Using
                    
                    ' Add a highlight
                    Using highlightPen As New Pen(Color.White)
                        g.DrawRectangle(highlightPen,
                                      x * CELL_SIZE + 1,
                                      y * CELL_SIZE + 1,
                                      CELL_SIZE - 3,
                                      CELL_SIZE - 3)
                    End Using
                End If
            Next
        Next
        
        ' Draw pattern preview
        If Not isRunning AndAlso patternComboBox.DroppedDown Then
            Using previewBrush As New SolidBrush(Color.FromArgb(50, 255, 255, 0))
                Dim mousePos As Point = Me.PointToClient(Cursor.Position)
                Dim gridX As Integer = mousePos.X \ CELL_SIZE
                Dim gridY As Integer = mousePos.Y \ CELL_SIZE
                
                If gridX >= 0 AndAlso gridX < GRID_WIDTH AndAlso 
                   gridY >= 0 AndAlso gridY < GRID_HEIGHT Then
                    ' Draw a preview rectangle
                    g.FillRectangle(previewBrush,
                                  gridX * CELL_SIZE,
                                  gridY * CELL_SIZE,
                                  CELL_SIZE * 5,
                                  CELL_SIZE * 5)
                End If
            End Using
        End If
    End Sub
    
    
    ''' Handle form closing
    
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        mainTimer.Stop()
        MyBase.OnFormClosing(e)
    End Sub
End Class


''' Module containing the main entry point

Module Program
    
    ''' The main entry point for the application
    
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New GameOfLifeForm())
    End Sub
End Module


''' Supplementary class for pattern analysis

Public Class PatternAnalyzer
    
    ''' Detects if a pattern is static (still life)
    
    Public Shared Function IsStillLife(grid As Boolean(,)) As Boolean
        Dim temp As Boolean(,) = DirectCast(grid.Clone(), Boolean(,))
        
        ' Compute one generation
        Dim nextGen As Boolean(,) = ComputeGeneration(temp)
        
        ' Compare
        For x As Integer = 0 To grid.GetLength(0) - 1
            For y As Integer = 0 To grid.GetLength(1) - 1
                If grid(x, y) <> nextGen(x, y) Then
                    Return False
                End If
            Next
        Next
        
        Return True
    End Function
    
    
    ''' Computes one generation for analysis
    
    Private Shared Function ComputeGeneration(grid As Boolean(,)) As Boolean(,)
        Dim width As Integer = grid.GetLength(0)
        Dim height As Integer = grid.GetLength(1)
        Dim result(width - 1, height - 1) As Boolean
        
        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1
                Dim neighbors As Integer = CountNeighbors(grid, x, y, width, height)
                
                If grid(x, y) Then
                    result(x, y) = (neighbors = 2 OrElse neighbors = 3)
                Else
                    result(x, y) = (neighbors = 3)
                End If
            Next
        Next
        
        Return result
    End Function
    
    
    ''' Count neighbors for analysis
    
    Private Shared Function CountNeighbors(grid As Boolean(,), x As Integer, y As Integer, 
                                          width As Integer, height As Integer) As Integer
        Dim count As Integer = 0
        
        For dx As Integer = -1 To 1
            For dy As Integer = -1 To 1
                If dx = 0 AndAlso dy = 0 Then Continue For
                
                Dim nx As Integer = x + dx
                Dim ny As Integer = y + dy
                
                If nx >= 0 AndAlso nx < width AndAlso ny >= 0 AndAlso ny < height Then
                    If grid(nx, ny) Then count += 1
                End If
            Next
        Next
        
        Return count
    End Function
    
    
    ''' Finds oscillators and their periods
    
    Public Shared Function FindPeriod(grid As Boolean(,), maxPeriod As Integer) As Integer
        Dim history As New List(Of String)
        Dim current As Boolean(,) = DirectCast(grid.Clone(), Boolean(,))
        
        For period As Integer = 1 To maxPeriod
            ' Convert grid to string for comparison
            Dim state As String = GridToString(current)
            
            ' Check if we've seen this state before
            Dim index As Integer = history.IndexOf(state)
            If index >= 0 Then
                Return period - index
            End If
            
            history.Add(state)
            current = ComputeGeneration(current)
        Next
        
        Return -1 ' No period found within maxPeriod
    End Function
    
    
    ''' Convert grid to string for comparison
    
    Private Shared Function GridToString(grid As Boolean(,)) As String
        Dim sb As New System.Text.StringBuilder()
        Dim width As Integer = grid.GetLength(0)
        Dim height As Integer = grid.GetLength(1)
        
        For y As Integer = 0 To height - 1
            For x As Integer = 0 To width - 1
                sb.Append(If(grid(x, y), "1", "0"))
            Next
        Next
        
        Return sb.ToString()
    End Function
End Class


''' Supplementary class for statistics

Public Class GameStatistics
    Public Property Generation As Integer
    Public Property Population As Integer
    Public Property Births As Integer
    Public Property Deaths As Integer
    Public Property Density As Double
    
    Public Overrides Function ToString() As String
        Return $"Gen: {Generation}, Pop: {Population}, " &
               $"Births: {Births}, Deaths: {Deaths}, " &
               $"Density: {Density:P2}"
    End Function
End Class