namespace ParserFSharp


open System.Text.RegularExpressions


module Parser =
    let initsRegex = new Regex("(set ((?<initX>\d+) (?<initY>\d+)))", RegexOptions.Multiline)
    let directionsRegex = new Regex("(?=(?<direction>up|down|left|right) (?<steps>\d+))", RegexOptions.Multiline)
    let repeatition = new Regex("(repeat (?<times>\d+) times: \((?<operations>.*?\w+)\))", RegexOptions.Multiline)
    
    let regexGroupString (groupName: string) (m: Match) = m.Groups.[groupName].Value
    let regexGroupInt    (groupName: string) (m: Match) = regexGroupString groupName m |> int
    
    let parseInitialX    (m: Match) = regexGroupInt "initX" m
    let parseInitialY    (m: Match) = regexGroupInt "initY" m
    let parseInitial     (m: Match) = (parseInitialX m, parseInitialY m)
    let parseRepeatition (m: Match) = regexGroupInt "repeatition" m
    let parseDirection   (m: Match) = regexGroupString "direction" m
    let parseSteps       (m: Match) = regexGroupInt "steps" m
    let parseOperation   (m: Match) = (parseDirection m, parseSteps m)
    
    let parseInits (matches: MatchCollection) =
        matches 
        |> Seq.cast
        |> Seq.map parseInitial
    
    let parseInit (task: string) =
        match initsRegex.Matches task with
        | matches when matches.Count = 0 -> None
        | matches -> Some (parseInits matches)
    
    let parseOperations (matches: MatchCollection) =
        matches 
        |> Seq.cast
        |> Seq.map parseOperation
    
    let parseDirections (task: string) =
        match directionsRegex.Matches task with
        | matches when matches.Count = 0 -> None
        | matches -> Some (parseOperations matches)
    
    let parseRepeatitionTimes (m: Match) = regexGroupInt "times" m
    let parseRepeatitionOperations (m: Match) = regexGroupString "operations" m
    
    let parseRepeatitions (m: Match) = (parseRepeatitionTimes m, parseRepeatitionOperations m)
    
    let parseRepeats (task: string) =
        match repeatition.Matches task with
        | matches when matches.Count = 0 -> None
        | matches -> Some(matches)
    
    let unwrappedRepeatition (task: string) =
        let repeatitionsArr:list<Match> =
            let matchCollection = parseRepeats task
            match matchCollection with
            | matches when matches.IsNone -> []
            | matches when matches.IsSome -> 
                matchCollection
                |> Option.get
                |> Seq.cast
                |> Seq.toList
            | _ -> failwith "Unknown pattern"
    
        let rec replace (task: string) (repeatitionsArr:list<Match>) =
            match repeatitionsArr with
            | [] -> task
            | h::t -> 
                let strHead = h.ToString()
                let repeatTimes, repeatOperations = parseRepeatitions h
    
                let mutable count = 0
                let rec unwrapRepeatition concatedRepeatOperations repeatOperations =
                    count <- count + 1
                    if count < repeatTimes then
                        unwrapRepeatition ([concatedRepeatOperations; repeatOperations] |> String.concat ", ") repeatOperations
                    else concatedRepeatOperations
                        
                replace (task.Replace(strHead, unwrapRepeatition repeatOperations repeatOperations)) t
            
        replace task repeatitionsArr
    
    
    let parseTask (task: string) =
        let initials = parseInit task
        let directions = parseDirections task
    
        (initials, directions)
    
    let applyOperation (x, y) (operations:seq<string * int>) =
        let operationsEnum = operations.GetEnumerator()
        printfn "operationsEnum: %A" operationsEnum
    
        (x, y)
        |> Seq.unfold (fun s -> 
            match operationsEnum.MoveNext() with
            | true ->
                let direction, steps = operationsEnum.Current
                match direction with 
                | "up"    -> Some((fst s, snd s - steps), (fst s, snd s - steps))
                | "down"  -> Some((fst s, snd s + steps), (fst s, snd s + steps))
                | "left"  -> Some((fst s - steps, snd s), (fst s - steps, snd s))
                | "right" -> Some((fst s + steps, snd s), (fst s + steps, snd s))
                | _       -> failwith "Unsupported operator"
            | false -> None
            )
    
    let parseCoordinates (initials:seq<int * int> option, operations:seq<string * int> option) =
        let initial = (Seq.toArray <| (Option.get initials)).[0]
        Option.map (fun ops -> Seq.append [initial] (applyOperation (initial) ops)) operations
    
    let result (task: string) =
        task
        |> unwrappedRepeatition
        |> parseTask
        |> parseCoordinates
    