namespace FTypes.Poker
open System

exception ParseErrorException of string
type Suit = Heart | Diamond | Club | Spade

[<CLIMutable>]
type Card = {Rank: int; Suit:Suit } with
    member private _.EqualityContract = typeof<Card>
    member c.``<Clone>$``() = {Rank = c.Rank; Suit = c.Suit}
    static member op_Equality (r1 : Card, r2: Card ) =
        box r1 = box r2 || ((not (isNull (box r1))) && (r1).Equals(r2));
    static member op_Inequality (r1 : Card, r2: Card ) =
        not (Card.op_Equality(r1, r2))

(*
//Fake Card Record trying to see if I can match shape by adding copy consructor
[<Sealed>][<AllowNullLiteral>]
type Card (rank, suit) = 
    //ideally would be init only setters, don't have the feature in F# yet
    member val Rank = rank : int with get,set
    member val Suit = suit : Suit with get,set
    member private _.EqualityContract = typeof<Card>
    new (c:Card) = Card(c.Rank, c.Suit)
    member c.``<Clone>$``() = Card(c)
    abstract Equals : Card -> bool
    override a.Equals(b:Card) =
            if a.Rank = b.Rank then
                a.Suit = b.Suit
            else 
                false
    interface IEquatable<Card> with
        override a.Equals(b) =
            a.Equals(b)
    static member op_Equality (r1 : Card, r2: Card ) =
        box r1 = box r2 || ((not (isNull (box r1))) && (r1).Equals(r2));
    static member op_Inequality (r1 : Card, r2: Card ) =
        not (Card.op_Equality(r1, r2))
*)

type Hand = Card seq
type ScorableHand = EntireHand of Hand
                    | SplitHand of Hand * Hand
                    | TwoHand of Hand * Hand * Card
                    | NoHand of Hand