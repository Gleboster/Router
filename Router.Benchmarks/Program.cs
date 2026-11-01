using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

var config = DefaultConfig.Instance;

BenchmarkRunner.Run<Router.Benchmarks.RouterConcurrentBenchmarks>(config);
