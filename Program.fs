module Program.fs

open System
open System.IO
open Newtonsoft.Json

// 1. Student Database:
// Store student data (ID, name, and grades) in F# Record or List structures.

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
//2. Support adding, editing, and removing student records.
let modifyStudentsList modifier =
    students <- modifier students
let addStudent id name subjects =
    let newStudent = { id = id; name = name; grades = subjects }
    modifyStudentsList (fun currentList -> newStudent :: currentList)
    save() 
    printfn "Student added: %s" name 
let editStudent id updateFn =
    modifyStudentsList (fun currentList ->
        currentList
        |> List.map (fun student ->
            if student.id = id then updateFn student else student)
    )
    save() 
    printfn "Student with ID %d has been updated." id
let removeStudent id =
    modifyStudentsList (fun currentList ->
        currentList |> List.filter (fun student -> student.id <> id)
    )
    save() 
    printfn "Student with ID %d has been removed." id

//loadData()
//addStudent 1 "Shouneez" [ { name = "CS"; grade = 90.0 }; { name = "PL3"; grade = 85.0 } ]
//addStudent 2 "Aya" [ { name = "CS"; grade = 88.0 }; { name = "PL3"; grade = 85.0 }]
// editStudent 1 (fun student -> { student with name = "Shouneez Alaa" })
// removeStudent 2

// 2. Grade Management
let calculateAverage (grades: Subject list) =
    if grades.IsEmpty then 0.0
    else grades |> List.averageBy (fun g -> g.grade)

let studentAverage id =
    match List.tryFind (fun student -> student.id = id) students with
    | Some student ->
        let avg = calculateAverage student.grades
        printfn "Student %s has an average grade of %.2f" student.name avg
        avg
    | None ->
        printfn "Student with ID %d not found." id
        -1.0

let classAverage () =
    let allGrades = students |> List.collect (fun student -> student.grades |> List.map (fun g -> g.grade))
    if allGrades.IsEmpty then 0.0
    else List.average allGrades

let passRate passingGrade =
    let passed = students |> List.filter (fun student -> calculateAverage student.grades >= passingGrade)
    let rate = (float passed.Length / float students.Length) * 100.0
    printfn "Pass rate: %.2f%%" rate
    rate

let highestAndLowestGrades () =
    let allGrades = students |> List.collect (fun student -> student.grades |> List.map (fun g -> g.grade))
    if allGrades.IsEmpty then
        printfn "No grades available."
        None
    else
        let maxGrade = List.max allGrades
        let minGrade = List.min allGrades
        printfn "Highest grade: %.2f, Lowest grade: %.2f" maxGrade minGrade
        Some (maxGrade, minGrade)

// Main Program
[<EntryPoint>]
let main argv =
    loadData ()
    printfn "Welcome to Student Grades Management System!"

    // Example usage
    printfn "Class average: %.2f" (classAverage ())
    passRate 50.0
    ignore (highestAndLowestGrades ())

    0
