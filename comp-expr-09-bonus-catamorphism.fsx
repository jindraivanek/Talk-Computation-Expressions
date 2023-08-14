(**
<!-- header: '**BONUS LEVEL**' -->

# BONUS LEVEL

## Catamorphism

---

# Seq CE as a backtracking algorithm

Simple example: find all combinations of numbers that sum to the target number.
*)

(** --- *)

let solutions =
    seq {
        for a in [ 1..6 ] do
            for b in [ 1..6 ] do
                for c in [ 1..6 ] do
                    if a + b + c = 10 then
                        yield (a, b, c)
    }


solutions |> Seq.toList

(*** include-it ***)

(** --- *)

(**
We can filter out variants in each step:
*)

let solutionsNoDuplicates =
    seq {
        for a in [ 1..6 ] do
            for b in [ a + 1 .. 6 ] do
                for c in [ b + 1 .. 6 ] do
                    if a + b + c = 10 then
                        yield (a, b, c)
    }

(** $$$ *)

solutionsNoDuplicates |> Seq.toList
(*** include-it ***)

(** --- *)

(**
It can also be recursive!
*)

let rec solutionsAnyLength acc =
    seq {
        let from = List.tryHead acc |> Option.defaultValue 1

        for x in [ from..6 ] do
            if List.sum (x :: acc) < 10 then
                yield! solutionsAnyLength (x :: acc)
            elif List.sum (x :: acc) = 10 then
                yield (x :: acc)
    }

(**
This is actually a catamorphism!
*)

(** $$$ *)

solutionsAnyLength [] |> Seq.toList

(*** include-it ***)

(** --- *)

(**
## Seq CE as sudoku solver

*)

type Sudoku = Map<int * int, int>

let row (i, j) =
    ([ 1 .. j - 1 ] @ [ j + 1 .. 9 ]) |> Seq.map (fun k -> (i, k))

let column (i, j) =
    ([ 1 .. i - 1 ] @ [ i + 1 .. 9 ]) |> Seq.map (fun k -> (k, j))

let square (i, j) =
    let i' = (i - 1) / 3 * 3 + 1
    let j' = (j - 1) / 3 * 3 + 1

    seq {
        for k in [ i' .. i' + 2 ] do
            for l in [ j' .. j' + 2 ] do
                if (i, j) <> (k, l) then
                    yield (k, l)
    }

(** $$$ *)

let filledNumbers (sud: Sudoku) xs =
    xs |> Seq.choose (fun (i, j) -> Map.tryFind (i, j) sud) |> set

let rec solve sud =
    seq {
        let toSolve =
            [ 1..9 ]
            |> Seq.collect (fun i -> [ 1..9 ] |> Seq.map (fun j -> (i, j)))
            |> Seq.filter (fun (i, j) -> Map.containsKey (i, j) sud |> not)

        match toSolve |> Seq.tryHead with
        | Some(i, j) ->
            let invalid =
                [ row; column; square ]
                |> List.map (fun f -> f (i, j) |> filledNumbers sud)
                |> Set.unionMany

            let candidates = set [ 1..9 ] - invalid

            for x in candidates do
                yield! solve (sud |> Map.add (i, j) x)
        | None -> yield sud
    }

(** --- *)
(**
## Example
*)

// parse sudoku from http://sudocue.net/daily.php format
let parseSudoku (s: string) =
    (Map.empty, Seq.indexed s)
    ||> Seq.fold (fun m (i, c) -> 
        let x = int (string c) 
        if x > 0 then Map.add (i / 9 + 1, i % 9 + 1) x m else m)

let printSudoku s =
    for i in [ 1..9 ] do
        for j in [ 1..9 ] do
            printf "%i" (Map.tryFind (i, j) s |> Option.defaultValue 0)

        printfn ""

let solveAndPrint sud =
    solve sud |> Seq.tryHead |> Option.iter printSudoku

(** --- *)

// http://sudocue.net/daily.php, daily nightmare July 24, 2023
let ex =
    parseSudoku "000904702004010006002000500600000204050000900400700001000005000298600000070090000"
(** *)
printSudoku ex
(*** include-output ***)

(** $$$ *)
// http://sudocue.net/daily.php, daily nightmare July 24, 2023
let ex =
    parseSudoku "000904702004010006002000500600000204050000900400700001000005000298600000070090000"
(** *)
solveAndPrint ex
(*** include-output ***)

(** --- *)
