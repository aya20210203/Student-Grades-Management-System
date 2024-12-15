open System 
open System.Drawing
open System.Windows.Forms
open Newtonsoft.Json
open System.IO

type Subject = {
    name: string
    grade: float
}

type Student = {
    id: int
    name: string
    grades: Subject list
}

let mutable students = [] : Student list
let filePath = "data.json"

let loadData () =
    if File.Exists(filePath) then
        let json = File.ReadAllText(filePath)
        students <- JsonConvert.DeserializeObject<Student list>(json)
    else
        students <- []

let save () =
    let json = JsonConvert.SerializeObject(students, Formatting.Indented)
    File.WriteAllText(filePath, json)

let addStudent id name subjects =
    let newStudent = { id = id; name = name; grades = subjects }
    students <- newStudent :: students
    save()
    MessageBox.Show(sprintf "Student added: %s" name, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore

let editStudent id updateFn =
    students <-
        students
        |> List.map (fun student ->
            if student.id = id then updateFn student else student)
    save()
    MessageBox.Show(sprintf "Student with ID %d has been updated." id, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore

let removeStudent id =
    students <- students |> List.filter (fun student -> student.id <> id)
    save()
    MessageBox.Show(sprintf "Student with ID %d has been removed." id, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore

// 2. Grade Management
let calculateAverage (grades: Subject list) =
    if grades.IsEmpty then 0.0
    else grades |> List.averageBy (fun g -> g.grade)

let findStudentById students id =
    List.tryFind (fun student -> student.id = id) students

let calculateStudentAverage student =
    calculateAverage student.grades

let calculateClassAverage students =
    let allGrades = students |> List.collect (fun student -> student.grades |> List.map (fun g -> g.grade))
    if allGrades.IsEmpty then None
    else Some (List.average allGrades)

let calculatePassRate students passingGrade =
    let passed = students |> List.filter (fun student -> calculateAverage student.grades >= passingGrade)
    if students.IsEmpty then 0.0
    else (float passed.Length / float students.Length) * 100.0

let calculateExtremeGrades students =
    let allGrades = students |> List.collect (fun student -> student.grades |> List.map (fun g -> g.grade))
    if allGrades.IsEmpty then None
    else Some (List.max allGrades, List.min allGrades)


// GUI Code

let mainForm = new Form(Text = "Student Grades Management", Size = Size(1200, 800), StartPosition = FormStartPosition.CenterScreen)
let tabControl = new TabControl(Dock = DockStyle.Fill)

// Viewer Tab (Views)
let viewerTab = new TabPage(Text = "Viewer")
let viewerList = new ListBox(Dock = DockStyle.Bottom, Size = Size(760, 400))
viewerTab.Controls.Add(viewerList)

let studentAverageLabel = new Label(Text = "Student Average:", Location = Point(20, 50), AutoSize = true)
let studentAverageComboBox = new ComboBox(Location = Point(150, 50), Size = Size(200, 20))
let calculateStudentAverageButton = new Button(Text = "Calculate", Location = Point(360, 50), Size = Size(80, 25))

let classAverageLabel = new Label(Text = "Class Average:", Location = Point(20, 100), AutoSize = true)
let classAverageValueLabel = new Label(Text = "", Location = Point(150, 100), AutoSize = true)
let calculateClassAverageButton = new Button(Text = "Calculate", Location = Point(360, 100), Size = Size(80, 25))

let passRateLabel = new Label(Text = "Pass Rate (50.0):", Location = Point(20, 150), AutoSize = true)
let passRateValueLabel = new Label(Text = "", Location = Point(150, 150), AutoSize = true)
let calculatePassRateButton = new Button(Text = "Calculate", Location = Point(360, 150), Size = Size(80, 25))

let extremeGradesLabel = new Label(Text = "Highest/Lowest Grades:", Location = Point(20, 200), AutoSize = true)
let extremeGradesValueLabel = new Label(Text = "", Location = Point(150, 200), AutoSize = true)
let calculateExtremeGradesButton = new Button(Text = "Calculate Extreme Grades", Location = Point(360, 200), Size = Size(150, 25))


viewerTab.Controls.Add(studentAverageLabel)
viewerTab.Controls.Add(studentAverageComboBox)
viewerTab.Controls.Add(calculateStudentAverageButton)
viewerTab.Controls.Add(classAverageLabel)
viewerTab.Controls.Add(classAverageValueLabel)
viewerTab.Controls.Add(calculateClassAverageButton)
viewerTab.Controls.Add(passRateLabel)
viewerTab.Controls.Add(passRateValueLabel)
viewerTab.Controls.Add(calculatePassRateButton)
viewerTab.Controls.Add(extremeGradesLabel)
viewerTab.Controls.Add(extremeGradesValueLabel)
viewerTab.Controls.Add(calculateExtremeGradesButton)

// Button Events for Viewer (Controller)
calculateStudentAverageButton.Click.Add(fun _ ->
    if studentAverageComboBox.SelectedItem <> null then
        let selectedStudent = studentAverageComboBox.SelectedItem.ToString()
        let id = int (selectedStudent.Split('-').[0].Trim())
        match findStudentById students id with
        | Some student ->
            let avg = calculateStudentAverage student
            MessageBox.Show(sprintf "Student %s has an average grade of %.2f" student.name avg, "Student Average", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
        | None ->
            MessageBox.Show("Student not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
    else
        MessageBox.Show("Please select a student.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
)

calculateClassAverageButton.Click.Add(fun _ ->
    match calculateClassAverage students with
    | Some avg ->
        classAverageValueLabel.Text <- sprintf "%.2f" avg
        MessageBox.Show(sprintf "Class Average: %.2f" avg, "Class Average", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    | None ->
        classAverageValueLabel.Text <- "No grades"
        MessageBox.Show("No grades available.", "Class Average", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
)

calculatePassRateButton.Click.Add(fun _ ->
    let passingGrade = 50.0
    let rate = calculatePassRate students passingGrade
    passRateValueLabel.Text <- sprintf "%.2f%%" rate
    MessageBox.Show(sprintf "Pass Rate: %.2f%%" rate, "Pass Rate", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
)

calculateExtremeGradesButton.Click.Add(fun _ ->
    match calculateExtremeGrades students with
    | Some (maxGrade, minGrade) ->
        extremeGradesValueLabel.Text <- sprintf "Max: %.2f, Min: %.2f" maxGrade minGrade
        MessageBox.Show(sprintf "Highest grade: %.2f\nLowest grade: %.2f" maxGrade minGrade, "Grade Range", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    | None ->
        extremeGradesValueLabel.Text <- "No grades"
        MessageBox.Show("No grades available.", "Grade Range", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
)

// admin Tab (Views)
let adminTab = new TabPage(Text = "Admin")
let adminList = new ListBox(Location = Point(20, 20), Size = Size(600, 400))
let addButton = new Button(Text = "Add student", Location = Point(650, 50), Size = Size(120, 40), BackColor = Color.MediumPurple, ForeColor = Color.White)
let editButton = new Button(Text = "Edit student", Location = Point(650, 120), Size = Size(120, 40), BackColor = Color.MediumPurple, ForeColor = Color.White)
let removeButton = new Button(Text = "Remove student", Location = Point(650, 190), Size = Size(120, 40), BackColor = Color.MediumPurple, ForeColor = Color.White)

adminTab.Controls.Add(adminList)
adminTab.Controls.Add(addButton)
adminTab.Controls.Add(editButton)
adminTab.Controls.Add(removeButton)

let refreshLists () =
    adminList.Items.Clear()
    viewerList.Items.Clear()
    studentAverageComboBox.Items.Clear()
    students |> List.iter (fun student ->
        let studentInfo = sprintf "ID: %d, Name: %s, Grades: [%s]" student.id student.name (String.Join(", ", student.grades |> List.map (fun g -> sprintf "%s: %.2f" g.name g.grade)))
        adminList.Items.Add(studentInfo) |> ignore
        viewerList.Items.Add(studentInfo) |> ignore
        studentAverageComboBox.Items.Add(sprintf "%d - %s" student.id student.name) |> ignore
    )

// Button Events for admin (Controller)
addButton.Click.Add(fun _ ->
    let inputForm = new Form(Text = "Add Student", Size = Size(400, 400))
    let idLabel = new Label(Text = "ID:", Location = Point(10, 20), AutoSize = true)
    let idBox = new TextBox(Location = Point(100, 20), Size = Size(150, 20))
    let nameLabel = new Label(Text = "Name:", Location = Point(10, 60), AutoSize = true)
    let nameBox = new TextBox(Location = Point(100, 60), Size = Size(150, 20))
    let gradesLabel = new Label(Text = "Grades (Format: Subject1=Grade1,Subject2=Grade2):", Location = Point(10, 100), AutoSize = true)
    let gradesBox = new TextBox(Location = Point(10, 140), Size = Size(350, 20))
    let submitButton = new Button(Text = "Submit", Location = Point(100, 200))

    submitButton.Click.Add(fun _ ->
        try
            let id = int idBox.Text
            // Check if the student ID already exists
            if List.exists (fun student -> student.id = id) students then
                MessageBox.Show("Student ID already exists. Please enter a different ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            else
                let name = nameBox.Text
                let grades =
                    gradesBox.Text.Split(',')
                    |> Array.toList
                    |> List.map (fun s ->
                        let parts = s.Split('=')
                        { name = parts.[0]; grade = float parts.[1] })
                addStudent id name grades
                refreshLists()
                inputForm.Close()
        with ex ->
            MessageBox.Show("Invalid input: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
    )

    inputForm.Controls.AddRange([| idLabel; idBox; nameLabel; nameBox; gradesLabel; gradesBox; submitButton |])
    inputForm.ShowDialog() |> ignore
)

editButton.Click.Add(fun _ ->
    if adminList.SelectedItem <> null then
        let selected = adminList.SelectedItem.ToString()
        let parts = selected.Split(',')
        let id = int (parts.[0].Split(':').[1].Trim())
        let inputForm = new Form(Text = "Edit Student", Size = Size(400, 400))
        let nameLabel = new Label(Text = "New Name:", Location = Point(10, 20), AutoSize = true)
        let nameBox = new TextBox(Location = Point(100, 20), Size = Size(150, 20))
        let gradesLabel = new Label(Text = "New Grades (Format: Subject1=Grade1,Subject2=Grade2):", Location = Point(10, 60), AutoSize = true)
        let gradesBox = new TextBox(Location = Point(10, 100), Size = Size(350, 20))
        let submitButton = new Button(Text = "Submit", Location = Point(100, 150))

        submitButton.Click.Add(fun _ ->
            try
                let newName = nameBox.Text
                let newGrades =
                    gradesBox.Text.Split(',')
                    |> Array.toList
                    |> List.map (fun s ->
                        let parts = s.Split('=')
                        { name = parts.[0]; grade = float parts.[1] })
                editStudent id (fun student -> { student with name = newName; grades = newGrades })
                refreshLists()
                inputForm.Close()
            with ex ->
                MessageBox.Show("Invalid input: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        )

        inputForm.Controls.AddRange([| nameLabel; nameBox; gradesLabel; gradesBox; submitButton |])
        inputForm.ShowDialog() |> ignore
    else
        MessageBox.Show("Please select a student to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
)

removeButton.Click.Add(fun _ ->
    if adminList.SelectedItem <> null then
        let selected = adminList.SelectedItem.ToString()
        let parts = selected.Split(',')
        let id = int (parts.[0].Split(':').[1].Trim())
        removeStudent id
        refreshLists()
    else
        MessageBox.Show("Please select a student to remove.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
)


// Tab selection and Password form (Roles separation)
let passwordForm = new Form(Text = "Admin Login", Size = Size(300, 150), StartPosition = FormStartPosition.CenterScreen)
let passwordLabel = new Label(Text = "Enter Password:", Location = Point(20, 20), AutoSize = true)
let passwordBox = new TextBox(Location = Point(20, 50), Size = Size(150, 20), PasswordChar = '*')
let submitButton = new Button(Text = "Submit", Location = Point(180, 50), Size = Size(75, 25))
let errorLabel = new Label(Text = "", Location = Point(20, 80), AutoSize = true, ForeColor = Color.Red)
passwordForm.Controls.Add(passwordLabel)
passwordForm.Controls.Add(passwordBox)
passwordForm.Controls.Add(submitButton)
passwordForm.Controls.Add(errorLabel)

let mutable isAdminAuthenticated = false
submitButton.Click.Add(fun _ ->
    if passwordBox.Text = "123" then
        isAdminAuthenticated <- true
        passwordForm.DialogResult <- DialogResult.OK
        passwordForm.Close()
    else
        errorLabel.Text <- "Incorrect password. Try again."
)

// Tab selection event for Admin
tabControl.SelectedIndexChanged.Add(fun _ ->
    if tabControl.SelectedTab = adminTab && not isAdminAuthenticated then
        tabControl.SelectedTab <- viewerTab
        passwordBox.Text <- ""
        errorLabel.Text <- ""
        let result = passwordForm.ShowDialog()
        if isAdminAuthenticated then
            tabControl.SelectedTab <- adminTab
)

// Adding tabs to tab control
viewerTab.Enter.Add(fun _ -> isAdminAuthenticated <- false) // Reset admin authentication when entering Viewer
tabControl.TabPages.Add(viewerTab)
tabControl.TabPages.Add(adminTab)
mainForm.Controls.Add(tabControl)

[<STAThread>]
do
    loadData()
    refreshLists()
    Application.EnableVisualStyles()
    Application.Run(mainForm)
