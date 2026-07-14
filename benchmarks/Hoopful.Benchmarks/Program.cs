using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Hoopful.Benchmarks.DecompressionBenchmarks).Assembly)
    .Run(args);
