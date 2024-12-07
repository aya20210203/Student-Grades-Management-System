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

//addStudent 1 "Shouneez" [ { name = "CS"; grade = 90.0 }; { name = "PL3"; grade = 85.0 } ]
//addStudent 2 "Aya" [ { name = "CS"; grade = 88.0 }; { name = "PL3"; grade = 85.0 }]
// editStudent 1 (fun student -> { student with name = "Shouneez Alaa" })
// removeStudent 2