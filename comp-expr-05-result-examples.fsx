(**
<!-- header: 'Computation Expression | Option | Result | Monad | **Result ex.** | Async | Overloaded Binds | Task' -->

# Result examples

---

# Result examples
*)

(**
## Result - CE
*)

type ResultCE() =
    member this.Bind (result, f) = Result.bind f result
    member this.Return value = Ok value
    member this.ReturnFrom value = value
let result = ResultCE()

(** --- *)

(**
## recursive result CE - going through list

*)
(*** hide ***)
let listResult (xs : list<Result<'a, string>>) : Result<list<'a>, string> = failwith "todo"


(*** hide ***)
// inefficient, as it goes through the list twice
let listResultMap (xs : list<Result<'a, string>>) : Result<list<'a>, string> =
    match xs |> List.tryPick (function | Error e -> Some e | _ -> None) with
    | Some e -> Error e
    | None -> Ok (xs |> List.map (function | Ok x -> x | _ -> failwith "impossible"))

(*** ***)
// get list of values if no Error case, otherwise return first Error
// recursive loop, hard to read
let listResultAcc (xs : list<Result<'a, string>>) : Result<list<'a>, string> =
    let rec loop xs acc =
        match xs with
        | [] -> Ok (List.rev acc)
        | Ok x :: xs -> loop xs (x :: acc)
        | Error e :: _ -> Error e
    loop xs []

(*** hide ***)
// with fold
let listResultFold (xs : list<Result<'a, string>>) : Result<list<'a>, string> =
    (Ok [], xs) ||> List.fold (fun acc x ->
        match acc, x with
        | Ok acc, Ok x -> Ok (x :: acc)
        | Error e, _
        | _, Error e -> Error e
    ) |> Result.map List.rev

(** --- *)

// with result CE - no need to handle Error case, no List.rev
let rec listResultCE (xs : list<Result<'a, string>>) 
    : Result<list<'a>, string> = 
    result {
        match xs with
        | [] -> return []
        | hd :: tl ->
            let! y = hd
            let! rest = listResultCE tl
            return y :: rest }

(**
<sub><sup><gray>(but not tail recursive)</gray></sup></sub>
*)

(** $$$ *)

listResultCE [Ok 1; Ok 2; Ok 3]
(*** include-it ***)

listResultCE [Ok 1; Error "boom"; Ok 3]
(*** include-it ***)

(**
```fsharp
listResultCE ([1 .. 10000] |> List.map Ok)
```

<div class="out">


```
Stack overflow. 
```

</div>

*)

(** --- *)

(**
## Result - error by condition
*)

(*** hide ***)
let condition = true

(***  ***)

result {
    let! x = Ok 1
    let! _ = if condition then Error "boom" else Ok ()
    return x }

(*** include-it ***)

(** --- *)

