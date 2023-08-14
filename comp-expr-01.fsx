(**
<!-- header: '**Computation Expression** | Option | Result | Monad | Result ex. | Async | Overloaded Binds | Task' -->

# Computation Expression
*)

task {
    let! entry = db.GetEntry entryId
    let! list = db.GetList entry.ListId
    let! user = db.GetUser list.UserId
    return user
}

(**
---

# Computation Expression
*)

task {
    let! entry = db.GetEntry entryId
    let! list = db.GetList entry.ListId
    let! user = db.GetUser list.UserId
    return user
}

(**

What exactly is `let!`?

---

# Computation Expression - In this talk

- Lots of code examples
- `let!` - `Bind` explained with examples
- Simple computation expressions examples (Option, Result)
- Monad :)
- Some practical examples using Result
- Async
- Task

---

*)

(**
## Code blocks
*)

let x = 1
let y = 2
x + y

(**
  Every `let` introduces code block - from `let` to end of the scope.
$$$
  Can be rewritten as
*)

1 |> (fun x ->
  2 |> (fun y ->
      x + y
  )
)
(**
---

### What if we can insert function call for each block?
*)

let bind f = fun x ->
  printfn "log: %A" x
  f x
(** *)
1
|> bind (fun x ->
  2 |> bind (fun y ->
      x + y))
(*** include-output ***)
(*** include-it ***)

(**
---

### Computation Expression
*)
type LoggerCE() =
    member __.Bind(x, f) =
        printfn "log: %A" x
        f x
    member __.Return x = x
let logger = LoggerCE()
(**
$$$
*)
let logIt() =
    logger {
      let! x = 1
      let! y = 2
      return x + y
    }
logIt()
(*** include-output ***)
(*** include-it ***)

(** --- *)

let logIt_Expanded() =
    logger.Bind(1, fun x ->
        logger.Bind(2, fun y ->
            logger.Return(x + y)))
logIt_Expanded()
(*** include-output ***)
(*** include-it ***)

(**
Note: scope is explicit.

---
*)
