using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;
using RentCarX.Tests.Benchmark;

var config = ManualConfig.Create(DefaultConfig.Instance);

BenchmarkRunner.Run<GetCarsBenchmark>(config);
