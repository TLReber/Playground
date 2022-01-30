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
                tasks = StartSimpleChannel(5, token);
                break;
            }
            case "redis":
            {
                Console.WriteLine($"Starting simple redis operations.");
                tasks = StartSimpleRedis(args, token);

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

    private static Task[] StartSimpleRedis(string[] args, CancellationToken token)
    {
        Task[] tasks;
        var    kind = args.Length == 2 ? args[1] : null;

        Console.WriteLine($"\tStarting redis {kind ?? "producer and consumer"}.");

        tasks = kind?.ToLower() switch
        {
            "producer" => new[] { SimpleRedisClient.StartProducer(token) },
            "consumer" => new[] { SimpleRedisClient.StartConsumer(token) },
            _          => new[] { SimpleRedisClient.StartProducer(token), SimpleRedisClient.StartConsumer(token) }
        };
        return tasks;
    }

    private static Task[] StartSimpleChannel(int numProducers, CancellationToken token)
    {
        var producerTasks = Enumerable.Range(1, numProducers).Select(id =>
            SimpleChannel.Produce(id, token)
        );

        var consumerTask = SimpleChannel.Consume(token);

        var tasks = producerTasks.Concat(new[] { consumerTask }).ToArray();
        return tasks;
    }
}