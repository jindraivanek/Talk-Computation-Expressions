(**
<!-- header: 'Computation Expression | Option | Result | Monad | Result ex. | Async | Overloaded Binds | **Task**' -->

# Task

---

# Task

resumable code

<small>https://github.com/fsharp/fslang-design/blob/main/FSharp-6.0/FS-1087-resumable-code.md</small>

Inside CE - intermediate type `TaskCode` is used

`async` can be used in CE without conversion

`task` is not lazy (hot-start), creating value of `Task<_>` type starts execution.

*)

(** --- *)

type TaskLike<'a> = { Run : unit -> 'a }

open System.Threading.Tasks
type TaskCE() =
    member this.Bind (t: Task<'a>, f: 'a -> TaskLike<'b>) = 
        { Run = fun () -> t.Result |> f |> fun x -> x.Run() }
    member this.Bind (a: Async<'a>, f: 'a -> TaskLike<'b>) = 
        { Run = fun () -> Async.RunSynchronously a |> f |> fun x -> x.Run() }
    member this.Bind (a: TaskLike<'a>, f: 'a -> TaskLike<'b>) = 
        { Run = fun () -> a.Run() |> f |> fun x -> x.Run() }
    member this.Return value = { Run = fun () -> value }
    member this.Run (taskLike: TaskLike<_>) = Task.Factory.StartNew(taskLike.Run)
let task1 = TaskCE()

(** --- *)

let task1Example = task1 {
    printfn "Starting"
    let! x = task { return 1 }
    let! y = async { return 2 }
    printfn "Running"
    return x + y }
task1Example.Result

(*** include-output ***)
(*** include-it ***)

(** --- *)

let task1ErrorExample = task1 {
    printfn "Starting"
    let! x = 1 // error
    let! y = async { return 2 }
    printfn "Running"
    return x + y }

(**
```
No overloads match for method 'Bind'.
Known types of arguments: int * (int -> TaskLike<int>)
Available overloads:
 - member TaskCE.Bind: a: Async<'a> * f: ('a -> TaskLike<'b>) -> TaskLike<'b> // Argument 'a' doesn't match
 - member TaskCE.Bind: a: TaskLike<'a> * f: ('a -> TaskLike<'b>) -> TaskLike<'b> // Argument 'a' doesn't match
 - member TaskCE.Bind: t: Task<'a> * f: ('a -> TaskLike<'b>) -> TaskLike<'b> // Argument 't' doesn't match
```

--- 
*)

let taskErrorExample = task {
    printfn "Starting"
    let! x = 1 // error
    let! y = async { return 2 }
    printfn "Running"
    return x + y }

(**
```
No overloads match for method 'Bind'.
Known types of arguments: int * (int -> TaskCode<int,int>)
Available overloads:
 - member TaskBuilderBase.Bind: computation: Async<'TResult1> * continuation: ('TResult1 -> TaskCode<'TOverall,'TResult2>) 
    -> TaskCode<'TOverall,'TResult2> // Argument 'computation' doesn't match
 - member TaskBuilderBase.Bind: task: Task<'TResult1> * continuation: ('TResult1 -> TaskCode<'TOverall,'TResult2>) 
    -> TaskCode<'TOverall,'TResult2> // Argument 'task' doesn't match
 - member TaskBuilderBase.Bind: task: ^TaskLike * continuation: ('TResult1 -> TaskCode<'TOverall,'TResult2>) 
    -> TaskCode<'TOverall,'TResult2> when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter) 
    and ^Awaiter :> System.Runtime.CompilerServices.ICriticalNotifyCompletion 
    and ^Awaiter: (member get_IsCompleted: unit -> bool) 
    and ^Awaiter: (member GetResult: unit -> 'TResult1) // Argument 'task' doesn't match
```

---

##  Realworld example - TODO list
User has *todo* lists and inside each list *todo* entries
*)

type Guid = System.Guid
type User = { Id: Guid; Username: string }
type TodoList = { Id: Guid; UserId: Guid }
type TodoEntry = { Id: Guid; ListId: Guid }

type ITodoDb = {
  GetUser: Guid -> Task<User>
  GetList: Guid -> Task<TodoList>
  GetEntry: Guid -> Task<TodoEntry> }

(** --- *)

(**
### Test data
*)

let entryId = Guid.NewGuid()
let todoDb =
    let user = { Id = Guid.NewGuid(); Username = "user1" }
    let list = { Id = Guid.NewGuid(); UserId = user.Id }
    let entry = { Id = entryId; ListId = list.Id }
    { GetUser = (fun id -> task { return if id = user.Id then user else failwith "user not found" })
      GetList = (fun id -> task { return if id = list.Id then list else failwith "list not found" })
      GetEntry = (fun id -> task { return if id = entry.Id then entry else failwith "entry not found"}) }

(** --- *)

(*** define: taskGetUserFromEntry_CE ***)
let taskGetUserFromEntry_CE (db: ITodoDb) entryId =
    task1 {
        let! entry = db.GetEntry entryId
        let! list = db.GetList entry.ListId
        let! user = db.GetUser list.UserId
        return user
    }

(*** include: taskGetUserFromEntry_CE ***)

taskGetUserFromEntry_CE todoDb entryId |> fun t -> t.Result

(*** include-it ***)

(** --- *)

(*** include: taskGetUserFromEntry_CE ***)

let taskGetUserFromEntry_CE_Expanded (db: ITodoDb) entryId =
    task1.Run(
        task1.Bind(db.GetEntry entryId, fun entry ->
            task1.Bind(db.GetList entry.ListId, fun list ->
                task1.Bind(db.GetUser list.UserId, fun user ->
                    task1.Return user))))

(** $$$ *)

taskGetUserFromEntry_CE_Expanded todoDb entryId |> fun t -> t.Result

(*** include-it ***)

(** --- *)

(**

# There is LOT more

- CE combinations (`asyncResult`, `taskResult`)
- `seq` CE, `list` CE
- state CE
- DB query like CE
- custom operations
- `and!`
- ...

---

# QUESTIONS ???

---
*)
