(**
<!-- header: 'Computation Expression | Option | Result | Monad | Result ex. | **Async** | Overloaded Binds | Task' -->

## Async

---

## Async - CE

naive implementation

*)

type AsyncCE() =
    member this.Bind (a, f) = Async.RunSynchronously a |> f
    member this.Return value = Async.FromContinuations(fun (s, e, c) -> s value)
let async1 = AsyncCE()

(** --- *)

(** Problem: async1 is not lazy *)

let async1Example = async1 {
    printfn "Starting"
    let! x = async1 { return 1 }
    printfn "Running"
    return x + 1 }

(*** include-output ***)

(** $$$ *)

let asyncExample = async {
    printfn "Starting"
    let! x = async { return 1 }
    printfn "Running"
    return x + 1 }

(*** include-output ***)

(** --- *)

(**
## Async - CE

add laziness
*)

type AsyncCE2() =
    member this.Bind (a, f) = Async.RunSynchronously a |> f
    member this.Return value = value
    member this.Delay f = // (unit -> 'a) -> Async<'a>
        Async.FromContinuations(fun (s, e, c) -> s (f ()))
let async2 = AsyncCE2()

(** --- *)

(*** define: async2Example ***)
let async2Example = async2 {
    printfn "Starting"
    let! x = async2 { return 1 }
    printfn "Running"
    return x + 1 }

(*** include: async2Example ***)

(** $$$ *)

async2Example
(*** include-output ***)

async2Example |> Async.RunSynchronously
(*** include-output ***)

(** --- *)

(** ## How it works:
(from Computation Expressions docs)
> The compiler, when it parses a computation expression, converts the expression into a series of nested function calls ... :
> `builder.Run(builder.Delay(fun () -> {| cexpr |}))`
> In the above code, the calls to Run and Delay are omitted if they are not defined in the computation expression builder class. ...

*)

(** --- *)

(**
## Async - CE - syntax sugar
*)

(*** include: async2Example ***)

let a_Expanded =
    async2.Delay(fun () ->
        printfn "Starting"
        async2.Bind(async2.Delay(fun () -> async2.Return(1)), fun x ->
            printfn "Running"
            async2.Return (x + 1)))

a_Expanded |> Async.RunSynchronously

(*** include-output ***)

(** --- *)

(**
##  Realworld example - TODO list
User has *todo* lists and inside each list *todo* entries
*)

type Guid = System.Guid
type User = { Id: Guid; Username: string }
type TodoList = { Id: Guid; UserId: Guid }
type TodoEntry = { Id: Guid; ListId: Guid }

type ITodoDb = {
  GetUser: Guid -> Async<User>
  GetList: Guid -> Async<TodoList>
  GetEntry: Guid -> Async<TodoEntry> }

(** --- *)

(**
### Test data
*)

let entryId = Guid.NewGuid()
let todoDb =
    let user = { Id = Guid.NewGuid(); Username = "user1" }
    let list = { Id = Guid.NewGuid(); UserId = user.Id }
    let entry = { Id = entryId; ListId = list.Id }
    { GetUser = (fun id -> async { return if id = user.Id then user else failwith "user not found" })
      GetList = (fun id -> async { return if id = list.Id then list else failwith "list not found" })
      GetEntry = (fun id -> async { return if id = entry.Id then entry else failwith "entry not found"}) }

(** --- *)

(*** define: asyncGetUserFromEntry_CE ***)
let asyncGetUserFromEntry_CE (db: ITodoDb) entryId =
    async2 {
        let! entry = db.GetEntry entryId
        let! list = db.GetList entry.ListId
        let! user = db.GetUser list.UserId
        return user
    }

(*** include: asyncGetUserFromEntry_CE ***)

asyncGetUserFromEntry_CE todoDb entryId |> Async.RunSynchronously

(*** include-it ***)

(** --- *)

(*** include: asyncGetUserFromEntry_CE ***)

let asyncGetUserFromEntry_CE_Expanded (db: ITodoDb) entryId =
    async2.Delay(fun () ->
        async2.Bind(db.GetEntry entryId, fun entry ->
            async2.Bind(db.GetList entry.ListId, fun list ->
                async2.Bind(db.GetUser list.UserId, fun user ->
                    async2.Return user))))

(** $$$ *)

asyncGetUserFromEntry_CE_Expanded todoDb entryId |> Async.RunSynchronously

(*** include-it ***)

(** --- *)
