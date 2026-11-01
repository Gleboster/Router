using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace Router.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Interactive mode: select benchmark to run
        if (args.Length == 0)
        {
            Console.WriteLine("=== Router Benchmarks ===");
            Console.WriteLine();
            Console.WriteLine("Select benchmark to run:");
            Console.WriteLine("  1. SingleThread - Basic routing overhead (4 runs)");
            Console.WriteLine("  2. ConcurrentFast - Parallel routing with fast handlers (10 runs)");
            Console.WriteLine("  3. ConcurrentSlow - Parallel routing with slow handlers (40 runs)");
            Console.WriteLine("  4. HighLoad - High load Sync scenarios (1 runs)");
            Console.WriteLine("  5. HighLoad - High load Async scenarios (6 runs)");
            Console.WriteLine("  6. All - Run all benchmarks (60 runs)");
            Console.WriteLine();
            Console.Write("Enter choice (1-6): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    BenchmarkRunner.Run<SingleThreadBenchmarks>();
                    break;
                case "2":
                    BenchmarkRunner.Run<ConcurrentFastHandlersBenchmarks>();
                    break;
                case "3":
                    BenchmarkRunner.Run<ConcurrentSlowHandlersBenchmarks>();
                    break;
                case "4":
                    BenchmarkRunner.Run<HighLoadSyncBenchmarks>();
                    break;
                case "5":
                    BenchmarkRunner.Run<HighLoadAsyncBenchmarks>();
                    break;
                case "6":
                    RunAllBenchmarks();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Running all benchmarks...");
                    RunAllBenchmarks();
                    break;
            }
        }
        else
        {
            // Command line mode: use BenchmarkSwitcher for flexibility
            var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
            switcher.Run(args);
        }
    }

    private static void RunAllBenchmarks()
    {
        var config = DefaultConfig.Instance;

        Console.WriteLine("Running SingleThreadBenchmarks...");
        BenchmarkRunner.Run<SingleThreadBenchmarks>(config);

        Console.WriteLine("\nRunning ConcurrentFastHandlersBenchmarks...");
        BenchmarkRunner.Run<ConcurrentFastHandlersBenchmarks>(config);

        Console.WriteLine("\nRunning ConcurrentSlowHandlersBenchmarks...");
        BenchmarkRunner.Run<ConcurrentSlowHandlersBenchmarks>(config);

        Console.WriteLine("\nRunning HighLoadSyncBenchmarks...");
        BenchmarkRunner.Run<HighLoadSyncBenchmarks>(config);

        Console.WriteLine("\nRunning HighLoadAsyncBenchmarks...");
        BenchmarkRunner.Run<HighLoadAsyncBenchmarks>(config);

        Console.WriteLine("\n=== All benchmarks completed ===");
    }
}
