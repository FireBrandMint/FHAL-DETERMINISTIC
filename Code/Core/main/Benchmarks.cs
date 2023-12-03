using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using GJP2;


public class Benchmarks
{
    static Action[] BenchmarkTargets = new Action[]
    {
        ShapeColBench.TestCol1
    };

    public static void Main(string[] args)
    {
        string line;
        int times = 0;
        while(true)
        {
            line = Console.ReadLine();
            if(line == "end") break;
            int fodder;
            if(int.TryParse(line, out fodder)) times = fodder;
            else if (line != "r") times = 6000;
            
            for(int i = 0; i < BenchmarkTargets.Length; ++i)
                DoBenchmark(BenchmarkTargets[i], times);
        }
    }
    [MethodImpl(MethodImplOptions.NoOptimization)]
    static void DoBenchmark(Action func, int quantity)
    {
        long elapsed = 0;

        for(int i = 0; i < quantity; ++i)
        {
            var past = Stopwatch.GetTimestamp();
            func.Invoke();
            var now = Stopwatch.GetTimestamp();
            elapsed += now-past;
        }

        var final = ((elapsed / (double)quantity) * (1d / Stopwatch.Frequency)) * 1000000d;
        var whole = ((elapsed) * (1d / (Stopwatch.Frequency /60.0)));

        Console.WriteLine($"Average: {final.ToString()}us, Total: {whole}frames");
    }
}