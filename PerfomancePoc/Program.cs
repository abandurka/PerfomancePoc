using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;


namespace PerfomancePoc
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AlgorithmBenchmark>();
        }
    }
    
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporter]
    [MemoryDiagnoser]
    public class AlgorithmBenchmark
    {
        [Params(10)]
        public int N;

        public TimeSpan diff => TimeSpan.FromHours(1);

        private IReadOnlyCollection<TestClass> data;

        [GlobalSetup]
        public void Setup()
        {
            var fixture = new Fixture();
            data = fixture.CreateMany<TestClass>(N).OrderBy(x=> x.Time).ToList();
        }

        [Benchmark]
        public bool For()
        {
            for (var i = 1; i < data.Count; i++)
            {
                var previousDate = data.ElementAt(i - 1).Time;
                var currentDate = data.ElementAt(i).Time;

                if (currentDate - previousDate < diff)
                {
                    return true;
                }
            }

            return false;
        }
        
        [Benchmark]
        public bool For_optimized()
        {
            var previousDate = data.First().Time;

            for (var i = 1; i < data.Count; i++)
            {
                var currentDate = data.ElementAt(i).Time;

                if (currentDate - previousDate < diff)
                {
                    return true;
                }

                previousDate = currentDate;
            }

            return false;
        }

        [Benchmark]
        public bool Foreach()
        {
            var previousDate = data.First().Time;
            foreach (var currentDate in data.Skip(1))
            {
                if (currentDate.Time - previousDate < diff)
                {
                    return true;
                }

                previousDate = currentDate.Time;
            }

            return false;
        }

        [Benchmark]
        public bool Linq()
        {
            var offsets = data.Select(op => op.Time);
            return offsets
                .Zip(offsets.Skip(1), (previousDate, currentDate) => currentDate - previousDate < diff)
                .Any(x => x);
        }
    }

    public class TestClass
    {
        public TimeSpan Time { get; set; }
    }
}