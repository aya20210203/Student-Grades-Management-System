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


// gui code

let mainForm = new Form(Text = "Student Grades Management", Size = Size(1200, 800))
let tabControl = new TabControl(Dock = DockStyle.Fill)

// admin Tab (view)
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
    students |> List.iter (fun student ->
        let studentInfo = sprintf "ID: %d, Name: %s, Grades: [%s]" student.id student.name (String.Join(", ", student.grades |> List.map (fun g -> sprintf "%s: %.2f" g.name g.grade)))
        adminList.Items.Add(studentInfo) |> ignore
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

// adding tabs to tab control (will add viwer later)
tabControl.TabPages.Add(adminTab)
mainForm.Controls.Add(tabControl)

[<STAThread>]
do
    loadData()
    refreshLists()
    Application.EnableVisualStyles()
    Application.Run(mainForm)
