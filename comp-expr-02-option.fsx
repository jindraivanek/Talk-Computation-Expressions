(**
<!-- header: 'Computation Expression | **Option** | Result | Monad | Result ex. | Async | Overloaded Binds | Task' -->

# Option

---

## Pyramid of doom

![](comp-expr/img/1_YoTPCR_l1ApgGGfMp6ZzmQ-1955884515.png)

---

# Option
## Pyramid of doom
*)

let optionGetUserFromEntry (db: ITodoDb) entryId =
    match db.GetEntry entryId with
    | Some entry ->
        match db.GetList entry.ListId with
        | Some list ->
            match db.GetUser list.UserId with
            | Some user -> Some user
            | None -> None
        | None -> None
    | None -> None

(**
---

*)

(**
# Option

##  Realworld example - TODO list
User has *todo* lists and inside each list *todo* entries
*)

type Guid = System.Guid
type User = { Id: Guid; Username: string }
type TodoList = { Id: Guid; UserId: Guid }
type TodoEntry = { Id: Guid; ListId: Guid }

type ITodoDb = {
  GetUser: Guid -> User option
  GetList: Guid -> TodoList option
  GetEntry: Guid -> TodoEntry option }

(** --- *)

(**
### Test data
*)

let entryId = Guid.NewGuid()
let todoDb =
    let user = { Id = Guid.NewGuid(); Username = "user1" }
    let list = { Id = Guid.NewGuid(); UserId = user.Id }
    let entry = { Id = entryId; ListId = list.Id }
    { GetUser = (fun id -> if id = user.Id then Some user else None)
      GetList = (fun id -> if id = list.Id then Some list else None)
      GetEntry = (fun id -> if id = entry.Id then Some entry else None) }

(** --- *)

(**
## Option - match
*)

let optionGetUserFromEntry (db: ITodoDb) entryId =
    match db.GetEntry entryId with
    | Some entry ->
        match db.GetList entry.ListId with
        | Some list ->
            match db.GetUser list.UserId with
            | Some user -> Some user
            | None -> None
        | None -> None
    | None -> None

(** $$$ *)

optionGetUserFromEntry todoDb entryId

(*** include-it ***)

(** --- *)

(**
## Option - CE
*)

module Option =
    let bind f x =
        match x with
        | Some x -> f x
        | None -> None

(** *)

type OptionCE() =
    // let!
    member __.Bind(x, f) =
        Option.bind f x
    // return
    member __.Return x = Some x
    // return!
    member __.ReturnFrom x = x
let maybe = OptionCE()

(** $$$ *)

(*** define: optionGetUserFromEntry_CE ***)
let optionGetUserFromEntry_CE (db: ITodoDb) entryId =
    maybe {
        let! entry = db.GetEntry entryId
        let! list = db.GetList entry.ListId
        let! user = db.GetUser list.UserId
        return user
    }

(*** include: optionGetUserFromEntry_CE ***)

optionGetUserFromEntry_CE todoDb entryId

(*** include-it ***)

(** --- *)

(**
## Option - CE - syntax sugar
*)

(*** include: optionGetUserFromEntry_CE ***)

let optionGetUserFromEntry_CE_Expanded (db: ITodoDb) entryId =
    maybe.Bind(db.GetEntry entryId, fun entry ->
        maybe.Bind(db.GetList entry.ListId, fun list ->
            maybe.Bind(db.GetUser list.UserId, fun user ->
                maybe.Return user)))

(** $$$ *)

optionGetUserFromEntry_CE_Expanded todoDb entryId

(*** include-it ***)

(** --- *)


