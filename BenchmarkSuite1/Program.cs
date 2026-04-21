using BenchmarkDotNet.Running;
using System;
using BenchmarkDotNet.Running;
using BenchmarkSuite1.Benchmarks;

namespace BenchmarkSuite1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
