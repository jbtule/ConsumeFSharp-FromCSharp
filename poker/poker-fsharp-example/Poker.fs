// Copyright (c) 2020 Jay Tuley
// MIT License

module Poker
open System

exception ParseErrorException of string
type Suit = Heart | Diamond | Club | Spade
type Card = {Rank:int; Suit:Suit}
type Hand = Card seq
type ScorableHand = EntireHand of Hand
                    | SplitHand of Hand * Hand
                    | TwoHand of Hand * Hand * Card
                    | NoHand of Hand

let key, groups = fst, snd //aliased for readablity with groupBy
[<Literal>] 
let Ace = 14

let handValue =
    let rank, suit = (fun x->x.Rank), (fun x->x.Suit)
    let ofAKind x = Seq.groupBy rank
                    >> Seq.map groups
                    >> Seq.filter (fun y -> y |> Seq.length = x)
    let ofAKindRem x =  Seq.groupBy rank
                        >> Seq.map groups
                        >> Seq.filter(fun y -> y |> Seq.length <> x)
                        >> Seq.collect id
    let isFlush h = h |> Seq.map suit
                      |> Seq.distinct
                      |> Seq.length = 1
    let ranking = Seq.map rank >> Seq.sortDescending
    let rankValue = ranking >> Seq.head
    let (|Flush|_|) h = if h |> isFlush then Some <| EntireHand h else None
    let (|Straight|_|) h = 
        let highCard = h |> rankValue
        h |> Seq.sortBy rank 
          |> Seq.toList
          |> function | [{Rank=2};{Rank=3};{Rank=4};{Rank=5};{Rank=Ace}] as l-> 
                            l |> Seq.map (function {Rank=Ace} as x-> {x with Rank=1} | y->y)
                              |> EntireHand
                              |> Some
                      | hs when hs |> List.map rank = [highCard-4..highCard] ->
                            Some <| EntireHand hs
                      | _ ->  None
    let (|StraightFlush|_|) =  
        function | Flush _ & Straight h -> Some h 
                 | ____________________ -> None
    let (|OfAKind|_|) x h = (ofAKind x h |> Seq.tryHead, ofAKindRem x h)
                            |> function | Some a, b -> Some <| SplitHand (a,b)
                                        | _________ -> None
    let (|FullHouse|_|) h =
        (ofAKind 3 h |> Seq.tryHead, ofAKind 2 h |> Seq.tryHead)
        |> function | Some a, Some b -> Some <| SplitHand(a,b)
                    | ______________ -> None
    let (|TwoPair|_|) h = 
        ofAKind 2 h 
        |> Seq.sortByDescending rankValue
        |> Seq.toList
        |> function 
            | [p1;p2] -> 
                let c2 = ofAKindRem 2 h |> Seq.exactlyOne
                TwoHand(p1, p2, c2) |> Some
            | _______ -> None  
    let (|HighCard|) h = NoHand h
    let scoreValue =
        function | EntireHand h      -> [h |> rankValue]
                 | SplitHand (h,k)   -> [h |> rankValue; k |> rankValue]
                 | TwoHand (bp,sp,k) -> [bp |> rankValue; sp |> rankValue; k |> rank]
                 | NoHand h          -> h |> ranking |> Seq.toList
    function 
        | StraightFlush h -> 
            8::scoreValue h
        | OfAKind 4 h ->
            7::scoreValue h
        | FullHouse h ->
            6::scoreValue h
        | Flush h->
            5::scoreValue h
        | Straight h ->
            4::scoreValue h
        | OfAKind 3 h ->
            3::scoreValue h
        | TwoPair h ->
            2::scoreValue h
        | OfAKind 2 h ->
            1::scoreValue h
        | HighCard h ->
            0::scoreValue h

let parseSuit =
    function | 'H' -> Heart
             | 'D' -> Diamond
             | 'C' -> Club
             | 'S' -> Spade
             | ___ -> raise <| ParseErrorException "Invalid Suit"

let parseRank =
    let (|NumberCard|_|) (s:string) =
        let success, i = Int32.TryParse(s)
        if success then Some i else None
    function | "J" -> 11
             | "Q" -> 12
             | "K" -> 13
             | "A" -> Ace
             | NumberCard i -> i
             | ___ -> raise <| ParseErrorException "Invalid Rank"

let parseHand (hand:string) : Hand =
    hand.Split([|' '|])
    |> Seq.map (fun h-> { Rank = h.[..^1] |> parseRank
                          Suit = h.[^0] |> parseSuit})

let bestHands =
    List.groupBy (parseHand >> handValue)
    >> List.sortByDescending key
    >> List.map groups
    >> List.head