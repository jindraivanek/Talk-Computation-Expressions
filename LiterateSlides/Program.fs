open System.Collections
open System.IO
open System.Security.Cryptography
open FSharp.Formatting.Literate
open FSharp.Formatting.Literate.Evaluation

let memoizeBy (g: 'a -> 'c) (f: 'a -> 'b) =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)
    fun x ->
        cache.GetOrAdd(Some (g x), lazy (f x)).Force()

let inline memoize f = memoizeBy id f

let regexReplace (pattern: string) (replacement: string) (input: string) =
    let regex = System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline)
    regex.Replace(input, replacement)

"""
```
A
B
```
"""
|> regexReplace $"```\n([^`]*)```" "<div class=\"out\">\n\n```\n$1```\n\n</div>"


let sourceDir = __SOURCE_DIRECTORY__ + "/.."
let getFiles() = System.IO.Directory.EnumerateFiles(sourceDir, "comp-expr*.fsx") |> Seq.sort

let changeOutput f filename =
    let output = System.IO.File.ReadAllText(filename)
    let x = f output
    System.IO.File.WriteAllText(filename, x)
let fsi = FsiEvaluator()
let nl = System.Environment.NewLine
let divider = "-----------------------"

type Slide = Normal of string | MultiColumn of string * string

let mkMultiColumn (input: string) =
    let cols x y = nl + """<div class="colwrap"><div class="left">""" + nl + x + nl + """</div><div class="right">""" + nl + y + nl + """</div></div>""" + nl
    input.Split(divider)
    |> Array.map (fun x -> x.Split("$$$") |> function
        | [|x|] -> x
        | [|x;y|] -> cols x y
        | [|z;x;y|] -> z + cols x y
        | _ -> failwith "Invalid input")
    |> String.concat "---"

let preBlockChanger (input: string) =
    let lines = input.Split(nl) |> Seq.toList
    let rec loop insideBlock lines = [
        match lines, insideBlock with
        | [], _ -> yield! []
        | "```" :: xs, None ->
            $"<div class=\"out\">"
            ""
            "```"
            yield! loop (Some "```") xs
        | "" :: "```" :: xs, Some "```"
        | "```" :: xs, Some "```" ->
            "```"
            ""
            "</div>"
            yield! loop None xs
        | x :: xs, None when x.StartsWith "```" ->
            x
            yield! loop (Some x) xs
        | "" :: "```" :: xs, Some _
        | "```" :: xs, Some _ ->
            "```"
            yield! loop None xs
        | x :: xs, _ ->
            x
            yield! loop insideBlock xs
        ]
    loop None lines |> String.concat nl

let sha = System.Security.Cryptography.SHA256.Create()

let convertToMd =
    memoizeBy (fun (x: string) -> x, System.IO.File.GetLastWriteTime(x))
        (fun x ->
            let tempFile = Path.GetTempFileName()
            Literate.ConvertScriptFile(x, output = tempFile, outputKind = OutputKind.Markdown, fsiEvaluator = fsi)
            let x = System.IO.File.ReadAllText(tempFile)
            System.IO.File.Delete(tempFile)
            x)
let convert () =
    printfn "Converting: %A" (getFiles() |> Seq.toList)
    let sources = getFiles() |> Seq.toList
    let output =
        sources |> List.map convertToMd |> List.reduce (+)
    let header = """---
title: "Computation Expression"
marp: true
//class: invert
paginate: true
theme: gaia
header: ''
---
<style>
div.colwrap {
  background-color: inherit;
  color: inherit;
  width: 100%;
  height: 100%;
}
div.colwrap div h1:first-child, div.colwrap div h2:first-child {
  margin-top: 0px !important;
}
div.colwrap div.left, div.colwrap div.right {
  position: absolute;
  top: 0;
  bottom: 0;
  padding: 70px 35px 70px 70px;
}
div.colwrap div.left {
  right: 50%;
  left: 0;
}
div.colwrap div.right {
  left: 50%;
  right: 0;
}
div.out {
--color-foreground: #000;
}
div.it {
--color-foreground: #006;
}
</style>
"""

    let result =
        header +
            (mkMultiColumn output |> preBlockChanger)
                .Replace(divider, "---")
                .Replace($">{nl}#", $">{nl}{nl}#")
                .Replace($">{nl}---", $">{nl}{nl}---")
                .Replace($">{nl}```", $">{nl}{nl}```")

    System.IO.File.WriteAllText(sourceDir + "/comp-expr.md", result)
    printfn "Done"

let myWatcher files f =
    let rec loop timestamp = async {
        let newTimestamp = files() |> List.map (fun (x: string) -> System.IO.File.GetLastWriteTime(x)) |> List.max
        if newTimestamp > timestamp then
            printfn "Changed"
            f()
            return! loop newTimestamp
        else
            printfn "No change"
            do! Async.Sleep(1000) |> Async.Ignore
            return! loop timestamp
    }
    loop System.DateTime.Now

let w = myWatcher (fun () -> getFiles() |> Seq.toList) convert |> Async.Start

// let w = System.IO.FileSystemWatcher(DirectoryInfo(sourceDir).FullName, "*.fsx")
// w.Changed.Add(fun e -> printfn $"Changed {e.FullPath}")
// w.Error.Add(fun e -> printfn $"Error {e.GetException()}")
// w.NotifyFilter <- System.IO.NotifyFilters.LastWrite ||| NotifyFilters.LastAccess
// w.IncludeSubdirectories <- true
// w.EnableRaisingEvents <- true
// w.BeginInit()

convert()

System.Console.ReadLine() |> ignore

let s = Seq.initInfinite id |> Seq.map (fun x -> async { printfn "Hi"; return x }) |> Seq.map Async.RunSynchronously |> Seq.chunkBySize 3 |> Seq.take 3 |> Seq.toList