﻿////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                    //
//                                                                                                    //
//     NOTE: This is just my scratch pad for quickly testing stuff, not for human consumption         //
//                                                                                                    //
//                                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using static LanguageExt.Prelude;
using static LanguageExt.Pipes.Proxy;
using LanguageExt.ClassInstances;
using LanguageExt.TypeClasses;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using LanguageExt.Effects;
using LanguageExt.Effects.Traits;
using LanguageExt.Pipes;
using LanguageExt.Sys;
using LanguageExt.Sys.Live;
using LanguageExt.Sys.Traits;
using LanguageExt.Sys.IO;
using TestBed;

public class Program
{
    static async Task Main(string[] args)
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                                    //
        //                                                                                                    //
        //     NOTE: This is just my scratch pad for quickly testing stuff, not for human consumption         //
        //                                                                                                    //
        //                                                                                                    //
        ///////////////////////////////////////////v////////////////////////////////////////////////////////////

        await PipesTest();

        // await ObsAffTests.Test();
        // await AsyncTests();
    }

    public static async Task PipesTest()
    {
        var clientServer = incrementer | oneTwoThree;
        
        var file1 = File<Runtime>.openRead("i:\\defaults.xml") 
                  | Stream<Runtime>.read(240) 
                  | decodeUtf8 
                  | writeLine;
        
        var file2 = File<Runtime>.openText("i:\\defaults.xml") 
                  | TextRead<Runtime>.readLine 
                  | writeLine;
        
        var effect1 = enumerate(Seq("Paul", "James", "Gavin")) | sayHello | writeLine;

        var time = Observable.Interval(TimeSpan.FromSeconds(1));

        var effect2 = observe2(time) | now | toLongTimeString | writeLine;

        var result = (await file2.RunEffect<Runtime, Unit>()
                                 .Run(Runtime.New()))
                                 .Match(Succ: x => Console.WriteLine($"Success: {x}"), 
                                        Fail: e => Console.WriteLine(e));
    }

    static Pipe<Runtime, DateTime, string, Unit> toLongTimeString =>
        from n in awaiting<DateTime>()       
        from _ in yield(n.ToLongTimeString())
        select unit;

    static Pipe<Runtime, long, DateTime, Unit> now =>
        from t in awaiting<long>()         
        from n in Time<Runtime>.now
        from _ in yield(n)
        select unit;

    static Consumer<Runtime, string, Unit> writeLine =>
        from l in awaiting<string>()
        from _ in Console<Runtime>.writeLine(l)
        select unit;
    
    static Consumer<Runtime, string, Unit> write =>
        from l in awaiting<string>()
        from a in Console<Runtime>.write(l)
        select unit;
    
    static Producer<Runtime, string, Unit> readLine =>
        repeat(from _1 in Console<Runtime>.writeLine("Enter your name")
               from nw in Time<Runtime>.now
               from _2 in yield(nw.ToLongTimeString())
               select unit);

    static Pipe<Runtime, string, string, Unit> sayHello =>
        from l in awaiting<string>()         
        from _ in yield($"Hello {l}")
        select unit;

    static Pipe<Runtime, Seq<byte>, string, Unit> decodeUtf8 =>
        from c in awaiting<Seq<byte>>()         
        from _ in yield(Encoding.UTF8.GetString(c.ToArray()))
        select unit;

    static Pipe<Runtime, A, string, Unit> toString<A>() =>
        from l in awaiting<A>()         
        from _ in yield($"{l} ")
        select unit;

    static Pipe<Runtime, int, string, Unit> times10 =>
        from n in awaiting<int>()         
        from _ in yield($"{n * 10}")
        select unit;
    
    
    
    
    static Producer<Runtime, string, Unit> readLine2 =>
        from w in Producer.lift<Runtime, string, Unit>(Console<Runtime>.writeLine("Enter your name"))
        from l in Producer.lift<Runtime, string, string>(Console<Runtime>.readLine)
        from _ in Producer.yield<Runtime, string>(l)
        from n in readLine2
        select unit;

    static Pipe<Runtime, string, string, Unit> sayHello2 =>
        from l in Pipe.await<Runtime, string, string>()         
        from _ in Pipe.yield<Runtime, string, string>($"Hello {l}")
        from n in sayHello2
        select unit;
    
    static Consumer<Runtime, string, Unit> writeLine2 =>
        from l in Consumer.await<Runtime, string>()
        from a in Consumer.lift<Runtime, string>(Console<Runtime>.writeLine(l))
        from n in writeLine2
        select unit;


    static Server<Runtime, int, int, Unit> incrementer(int question) =>
        from _1 in Server.lift<Runtime, int, int, Unit>(Console<Runtime>.writeLine($"Server received: {question}"))
        from _2 in Server.lift<Runtime, int, int, Unit>(Console<Runtime>.writeLine($"Server responded: {question + 1}"))
        from nq in Server.respond<Runtime, int, int>(question + 1)
        select unit;

    static Client<Runtime, int, int, Unit> oneTwoThree =>
        from qn in Client.enumerate<Runtime, int, int, int>(Seq(1, 2, 3))
        from _1 in Client.lift<Runtime, int, int, Unit>(Console<Runtime>.writeLine($"Client requested: {qn}"))
        from an in Client.request<Runtime, int, int>(qn)
        from _2 in Client.lift<Runtime, int, int, Unit>(Console<Runtime>.writeLine($"Client received: {an}"))
        select unit;

    
    static Pipe<Runtime, string, string, Unit> pipeMap =>
        Pipe.map((string x) => $"Hello {x}");
    
}
