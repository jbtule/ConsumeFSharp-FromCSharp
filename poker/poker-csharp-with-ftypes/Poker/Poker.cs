// Copyright (c) 2020 Jay Tuley
// MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using FTypes.Poker;
using Hand = System.Collections.Generic.IEnumerable<FTypes.Poker.Card>;
using ScoringList = Microsoft.FSharp.Collections.FSharpList<int>;
using Seq = Microsoft.FSharp.Collections.SeqModule;
namespace CSharpPoker
{
    public static class Poker
    {
        public const int Ace = 14;

        public static ScoringList HandValue(Hand hand){

            static int Rank (Card h) => h.Rank;
            static Suit Suit (Card h) => h.Suit;

            static IEnumerable<int> Ranking(Hand h) =>
                h.Select(Rank)
                 .OrderByDescending(x=>x);

            static int RankValue(Hand h) =>
                Ranking(h).FirstOrDefault();

            static IEnumerable<Hand>OfAKind(int x, Hand h) =>
                h.GroupBy(Rank)
                 .Where(y=>y.Count() == x)
                 .Cast<Hand>();

            static Hand OfAKindRem(int x, Hand h) =>
                h.GroupBy(Rank)
                 .Where(y => y.Count() != x)
                 .SelectMany(z=>z);

            static bool IsFlush(Hand h) =>
                h.Select(Suit).Distinct().Count() == 1;

            static ScorableHand? FlushPattern(Hand h) =>
                IsFlush(h) 
                    ? ScorableHand.NewEntireHand(h)
                    : null;

            static ScorableHand? StraightPattern(Hand h) {
                var high = RankValue(h);
                var lowHypothetical = high - 4;
                var lowStraightRanking = new [] { Ace , 5, 4, 3, 2};
                var typicalRanking = Enumerable.Range(lowHypothetical, 5).Reverse();
                return h switch {
                    Hand hl when lowStraightRanking.SequenceEqual(Ranking(hl)) =>
                        ScorableHand.NewEntireHand(hl.Select(c=>c.Rank == Ace ? new (1,c.Suit): c)),
                    Hand hx when typicalRanking.SequenceEqual(Ranking(hx)) =>
                        ScorableHand.NewEntireHand(hx),
                    _ => null
                };
            }

            static ScorableHand? StraightFlushPattern(Hand h) =>
                (FlushPattern(h), StraightPattern(h)) switch {
                    (ScorableHand _, ScorableHand s) => s,
                    _ => null
                };

            static ScorableHand? OfAKindPattern(int x, Hand h) =>
                (OfAKind(x, h).FirstOrDefault(), OfAKindRem(x, h))
                    switch {
                        (Hand kind, Hand rem) => ScorableHand.NewSplitHand(kind, rem),
                        _ => null
                    };

            static ScorableHand? FullHousePattern(Hand h)=>
                 (OfAKind(3, h).FirstOrDefault(), OfAKind(2, h).FirstOrDefault())
                    switch {
                        (Hand three, Hand two) => ScorableHand.NewSplitHand(three, two),
                        _ => null
                    };

            static ScorableHand? TwoPairPattern(Hand h) =>
                OfAKind(2, h).OrderByDescending(RankValue)
                             .ToArray()
                    switch {
                        Hand[] x when x.Length == 2 => 
                            ScorableHand.NewTwoHand(x[0],x[1], OfAKindRem(2, h).Single()),
                        _ => null
                    };

            static ScorableHand HighCardPattern(Hand h) =>
                ScorableHand.NewNoHand(h);

            static ScoringList ScoreValue(ScorableHand s) =>
                s switch {
                    ScorableHand.EntireHand { Item: var h} => 
                        Seq.ToList(new [] {RankValue(h)}),
                    ScorableHand.SplitHand { Item1: var h, Item2: var k} => 
                        Seq.ToList(new [] {RankValue(h), RankValue(k)}),
                    ScorableHand.TwoHand {Item1: var bp, Item2: var sp, Item3: var k} =>
                        Seq.ToList(new [] {RankValue(bp), RankValue(sp), Rank(k)}),
                    ScorableHand.NoHand {Item: var h} => Seq.ToList(Ranking(h)),
                    _ => throw new InvalidOperationException()
                };

            static ScoringList PrefixScore(int value, ScorableHand ScorableHand) =>
                new ScoringList(value, ScoreValue(ScorableHand));
            
            return hand switch {
                Hand h when StraightFlushPattern(h) is ScorableHand s =>
                    PrefixScore(8, s),
                Hand h when OfAKindPattern(4,h) is ScorableHand s =>
                    PrefixScore(7, s),
                Hand h when FullHousePattern(h) is ScorableHand s =>
                    PrefixScore(6, s),
                Hand h when FlushPattern(h) is ScorableHand s =>
                    PrefixScore(5, s),
                Hand h when StraightPattern(h) is ScorableHand s =>
                    PrefixScore(4, s),
                Hand h when OfAKindPattern(3, h) is ScorableHand s =>
                    PrefixScore(3, s),
                Hand h when TwoPairPattern(h) is ScorableHand s =>
                    PrefixScore(2, s),
                Hand h when OfAKindPattern(2, h) is ScorableHand s =>
                    PrefixScore(1, s),
                Hand h =>
                    PrefixScore(0, HighCardPattern(h))
            };
        }

        public static int ParseRank(string rank) => 
            int.TryParse(rank, out var i)
                ? i
                : rank switch
                    {
                        "J" => 11,
                        "Q" => 12,
                        "K" => 13,
                        "A" => Ace,
                        _ => throw new ParseErrorException("Invalid Rank")
                    };

        public static Suit ParseSuit(char suit) =>
            suit switch {
                'H' => Suit.Heart,
                'D' => Suit.Diamond,
                'C' => Suit.Club,
                'S' => Suit.Spade,
                 _  => throw new ParseErrorException("Invalid Suit")
            };

        public static Hand ParseHand(string hand) =>
            hand.Split(' ')
                .Select(h => new Card(ParseRank(h[..^1]), ParseSuit(h[^1])));

        public static string[] BestHands (string[] hands) =>
            hands.GroupBy(h=>HandValue(ParseHand(h)))
                 .OrderByDescending(g=>g.Key)
                 .First()
                 .ToArray();
    }
}