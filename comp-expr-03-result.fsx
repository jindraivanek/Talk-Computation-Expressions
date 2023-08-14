(**
<!-- header: 'Computation Expression | Option | **Result** | Monad | Result ex. | Async | Overloaded Binds | Task' -->

# Result

---

# Result

##  Realworld example - TODO list
User has *todo* lists and inside each list *todo* entries
*)

type Guid = System.Guid
type User = { Id: Guid; Username: string }
type TodoList = { Id: Guid; UserId: Guid }
type TodoEntry = { Id: Guid; ListId: Guid }

type ITodoDb = {
  GetUser: Guid -> Result<User, string>
  GetList: Guid -> Result<TodoList, string>
  GetEntry: Guid -> Result<TodoEntry, string> }

(** --- *)

(**
### Test data
*)

let entryId = Guid.NewGuid()
let todoDb =
    let user = { Id = Guid.NewGuid(); Username = "user1" }
    let list = { Id = Guid.NewGuid(); UserId = user.Id }
    let entry = { Id = entryId; ListId = list.Id }
    { GetUser = (fun id -> if id = user.Id then Ok user else Error "user not found")
      GetList = (fun id -> if id = list.Id then Ok list else Error "list not found")
      GetEntry = (fun id -> if id = entry.Id then Ok entry else Error "entry not found") }

(** --- *)

(**
## Result - match
*)

let resultGetUserFromEntry_match (db: ITodoDb) entryId =
    match db.GetEntry entryId with
    | Ok entry ->
        match db.GetList entry.ListId with
        | Ok list ->
            match db.GetUser list.UserId with
            | Ok user -> Ok user
            | Error msg -> Error msg
        | Error msg -> Error msg
    | Error msg -> Error msg

(** $$$ *)

resultGetUserFromEntry_match todoDb entryId

(*** include-it ***)

(** --- *)

(**
## Result - CE
*)

module Result =
    let bind f = function
        | Ok value -> f value
        | Error msg -> Error msg

(** *)

type ResultCE() =
    member this.Bind (result, f) =
        Result.bind f result
    member this.Return value = Ok value
    member this.ReturnFrom value = value
let result = ResultCE()

(** $$$ *)

(*** define: resultGetUserFromEntry_CE ***)
let resultGetUserFromEntry_CE (db: ITodoDb) entryId =
    result {
        let! entry = db.GetEntry entryId
        let! list = db.GetList entry.ListId
        let! user = db.GetUser list.UserId
        return user
    }

(*** include: resultGetUserFromEntry_CE ***)

resultGetUserFromEntry_CE todoDb entryId

(*** include-it ***)

(** --- *)

(**
## Result - CE - syntax sugar
*)

(*** include: resultGetUserFromEntry_CE ***)

let resultGetUserFromEntry_CE_Expanded (db: ITodoDb) entryId =
    result.Bind (db.GetEntry entryId, fun entry ->
        result.Bind (db.GetList entry.ListId, fun list ->
            result.Bind (db.GetUser list.UserId, fun user ->
                result.Return user)))

(** $$$ *)

resultGetUserFromEntry_CE_Expanded todoDb entryId

(*** include-it ***)

(** --- *)

