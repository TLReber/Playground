global using System;

namespace TestConsole;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var cts   = new CancellationTokenSource();
        var token = cts.Token;

        var tasks = StartTasks(args, token);

        while (!token.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (input?.ToLower() == "q")
            {
                cts.Cancel();
            }

            try
            {
                await Task.Delay(1000, cts.Token);
            }
            catch (TaskCanceledException) { }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (TaskCanceledException) { }

        Console.WriteLine("Goodbye!");
    }

    private static Task[] StartTasks(string[] args, CancellationToken token)
    {
        var tasks = Array.Empty<Task>();

        var cmd = args.FirstOrDefault();
        switch (cmd?.ToLower())
        {
            case "channel":
            {
                Console.WriteLine("Starting simple channel operations.");

                tasks = new[]
                        {
                            // Multiple producers.
                            Task.Run(() => SimpleChannel.Produce(1, token), token),
                            Task.Run(() => SimpleChannel.Produce(2, token), token),
                            Task.Run(() => SimpleChannel.Produce(3, token), token),
                            // Single consumer.
                            Task.Run(() => SimpleChannel.Consume(token), token)
                        };
                break;
            }
            case "redis":
            {
                Console.WriteLine("Starting simple redis operations.");

                var redis = SimpleRedisClient.Test();

                tasks = new[] { redis };
                break;
            }
            default:
            {
                Console.WriteLine("No task selected.");
                break;
            }
        }

        return tasks;
    }
}