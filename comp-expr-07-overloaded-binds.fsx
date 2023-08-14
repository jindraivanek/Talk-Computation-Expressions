(**
<!-- header: 'Computation Expression | Option | Result | Monad | Result ex. | Async | **Overloaded Binds** | Task' -->

# Task Prelude - Overloaded Bind

---

# Overloaded Bind - Result
extended with auto-convert from `option`
*)

module Result =
    let ofOption option =
        match option with
        | Some value -> Ok value
        | None -> Error "None"

(*** define: resultce ***)
type ResultCE() =
    member this.Bind (result, f) = Result.bind f result
    member this.Bind (option, f) = Result.bind f (Result.ofOption option)
    member this.Return value = Ok value
let result = ResultCE()

(*** include: resultce ***)

(** --- *)

(*** include: resultce ***)

result {
    let! a = Ok 1
    let! b = Some 2
    return a + b
}

(*** include-it ***)

(** --- *)


