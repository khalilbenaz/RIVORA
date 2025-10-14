using BenchmarkDotNet.Running;
using RVR.Framework.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
