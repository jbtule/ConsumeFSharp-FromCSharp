namespace FTypes.Poker

exception ParseErrorException of string
type Suit = Heart | Diamond | Club | Spade
type Card = {Rank:int; Suit:Suit}
type Hand = Card seq
type ScorableHand = EntireHand of Hand
                    | SplitHand of Hand * Hand
                    | TwoHand of Hand * Hand * Card
                    | NoHand of Hand