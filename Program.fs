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