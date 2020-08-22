using System;

//Using insanity, but a pretty small price to pay for how clear pattern matching became for DU.
using OptionTupInt = Microsoft.FSharp.Core.FSharpOption<System.Tuple<int>>;
using static Microsoft.FSharp.Core.FSharpOption<System.Tuple<int>>;
using OptionString = Microsoft.FSharp.Core.FSharpOption<string>;
using static Microsoft.FSharp.Core.FSharpOption<string>;
using static Microsoft.FSharp.Core.FSharpOption<int>;
using Option = Microsoft.FSharp.Core.OptionModule;
using MyChoice = Microsoft.FSharp.Core.FSharpChoice<uint, char>;
using static Microsoft.FSharp.Core.FSharpChoice<uint, char>;
using EasierChoice = Microsoft.FSharp.Core.FSharpChoice<int, int>;
using static Microsoft.FSharp.Core.FSharpChoice<int, int>;
using MyResult = Microsoft.FSharp.Core.FSharpResult<(int, int), string>;
using static Microsoft.FSharp.Core.FSharpResult<(int, int), string>;

// There aren't any nullable ref annotiation in f# core
// I thought it would be good to mention/think about in the consuming sense
// Syntactically, thanks to C# pattern matching and the fact the C# is
// not going to understand exhaustive match of an FSharp DU
// it really doesn't matter that non null DU types are treated as nullable.
var one = Some(Tuple.Create(1)); //OptionTupInt? If FSharp isn't going to support c# control flow null checkig, NotNull annotation would be great for `Some`
var two = Option.OfObj("2"); // Is it better than Some? Easier to import for sure.
var three = Some(3); //OptionInt? 
var fourA = Some(Tuple.Create(4)); //OptionInt? 

if(one is OptionTupInt {Value: var (a)}){ //Built in deconstruct for Option would be prefered
    Console.WriteLine(a);
}

if(two is OptionString {Value: var b}){
    Console.WriteLine(b);
}

two = OptionString.None; 

if(two is OptionString {Value: var c}){
    Console.WriteLine(c);
}

//Using the ToNullable and ToObj is nice
var threeN = Option.ToNullable(three);
if(threeN is int d){
    Console.WriteLine(d);
}

var fourN = Option.ToObj(fourA);
if(fourN is var (e)){ //is var x allows null, but is var (x) uses decustruction so not null
   Console.WriteLine(e);
}
fourN = null;
if(fourN is var (_)){ 
   throw new InvalidOperationException();
}

var five = NewChoice1Of2(5u); //MyChoice? 🙄 Ideally would *NOT* be a Nullable Ref
var six = NewChoice2Of2('6'); //MyChoice? 🙄
void DoItRefUnion (MyChoice choice) {
    switch (choice){
        case MyChoice.Choice1Of2 {Item: var i}: //Built in deconstruct for Ref Style DU with values would be prefered
            Console.WriteLine(i);
            break;
        case MyChoice.Choice2Of2 {Item: var j}:
            Console.WriteLine(j);
            break;
    };
}

DoItRefUnion(five);
DoItRefUnion(six);

var seven = NewChoice1Of2(7); //EasierChoice? 🙄
var eight = NewChoice2Of2(8); //EasierChoice? 🙄

int ChooseIt (EasierChoice choice) => 
    choice switch {
        EasierChoice.Choice1Of2 {Item: var m} => m,
        EasierChoice.Choice2Of2 {Item: var n} => n, 
        _ => throw new InvalidOperationException()  //nullable ref or not, c# won't know Types are exhausted
    };

Console.WriteLine(ChooseIt(seven));
Console.WriteLine(ChooseIt(eight));

var nineTen = NewOk((9,10));
var done = NewError("Done!");

void DoItStructUnion (MyResult result) {
    switch (result){
        case {Tag:MyResult.Tags.Ok, ResultValue: var (x,y)}:
            Console.WriteLine(x);
            Console.WriteLine(y);
            break;
        case {Tag:MyResult.Tags.Error, ErrorValue: var z}:
            Console.WriteLine(z);
            break;
    };
}

DoItStructUnion(nineTen);
DoItStructUnion(done);