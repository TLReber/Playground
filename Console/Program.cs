namespace Console;

using System;

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
        if (cmd?.ToLower() == "channel")
        {
            Console.WriteLine("Starting simple channel operations.");

            var producerTask = Task.Run(() => SimpleChannel.Produce(token), token);
            var consumerTask = Task.Run(() => SimpleChannel.Consume(token), token);

            tasks = new[] { producerTask, consumerTask };
        }
        else if (cmd?.ToLower() == "redis")
        {
            Console.WriteLine("Starting simple redis operations.");

            var redis = SimpleRedisClient.Test();

            tasks = new[] { redis };
        }

        return tasks;
    }
}