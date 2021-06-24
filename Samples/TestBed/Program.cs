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
using LanguageExt.ClassInstances;
using LanguageExt.TypeClasses;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LanguageExt.Effects;
using LanguageExt.Effects.Traits;
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

        await AsyncTests();
    }

    static async Task<Unit> AsyncTests()
    {
        // Setup
        MkIO.Setup(Runtime.New());
        var tmp1 = Path.GetTempFileName();
        var tmp2 = Path.GetTempFileName();
        var tmp3 = "//**--";
        File.WriteAllLines(tmp1, new[] {"Hello", "World"});
        File.WriteAllLines(tmp2, new[] {"Hello", "World", "Again"});

        // Run with environment
        var res1 = await AddLines<Runtime>(tmp1, tmp2).Run(Runtime.New());
        
        // Run with wrapped environment
        var res2 = await AddLines(tmp1, tmp2);

        // Run with environment
        var fail1 = await AddLines<Runtime>(tmp1, tmp3).Run(Runtime.New());
        
        // Run with wrapped environment
        var fail2 = await AddLines(tmp1, tmp3);

        await OptionAsyncTest();

        return unit;
    }

    static Aff<RT, int> AddLines<RT>(string path1, string path2) where RT : struct, HasFile<RT> =>
        from lines1 in File<RT>.readAllLines(path1)
        from lines2 in File<RT>.readAllLines(path2)
        select lines1.Count + lines2.Count;

    static async Aff<int> AddLines(string path1, string path2)
    {
        var lines1 = await MkIO.readAllLines(path1);
        var lines2 = await MkIO.readAllLines(path2);
        return lines1.Count + lines2.Count;
    }

    static async Aff<int> Add(Aff<int> ma, Aff<int> mb)
    {
        var a = await ma;
        Console.WriteLine("Hello");
        var b = await mb;
        return a + b;
    }

    static async Task OptionAsyncTest()
    {
        var r1 = await Add(SomeAsync(100), SomeAsync(200)).IfNone(-1);
        Console.WriteLine(r1);

        var r2 = await Add(SomeAsync(100), None).IfNone(-1);
        Console.WriteLine(r2);

        var r3 = await Add(SomeAsync<int>(async _ => (await System.IO.File.ReadAllTextAsync("")).Length), SomeAsync(100)).IfNone(-1);
        Console.WriteLine(r2);
    }

    static async OptionAsync<int> Add(OptionAsync<int> ma, OptionAsync<int> mb)
    {
        var a = await ma;
        var b = await mb;
        return a + b;
    }

    static class MkIO
    {
        public static Func<string, Aff<Seq<string>>> readAllLines;

        public static void Setup<RT>(RT runtime) where RT : struct, HasFile<RT>
        {
            readAllLines = path => AffMaybe<Seq<string>>(async () => await File<RT>.readAllLines(path).Run(runtime));
        }
    }
}
