namespace FTypes.Poker

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
type Hand = Card seq
type ScorableHand = EntireHand of Hand
                    | SplitHand of Hand * Hand
                    | TwoHand of Hand * Hand * Card
                    | NoHand of Hand