global using System;
using TestConsole.Commands;

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
        Task[] tasks;
        var    cmd = args.FirstOrDefault();

        switch (cmd?.ToLower())
        {
            case "channel":
            {
                Console.WriteLine("Starting simple channel operations.");
                tasks = SimpleChannel.StartSimpleChannel(5, token);
                break;
            }
            case "redis":
            {
                Console.WriteLine($"Starting simple redis operations.");
                tasks = SimpleRedisClient.StartSimpleRedis(args, token);
                break;
            }
            case "tasks":
            {
                Console.WriteLine($"Starting task operations.");
                tasks = SimpleLoop.StartTaskLoop(30, token);
                break;
            }
            default:
            {
                Console.Error.WriteLine("No task selected.");
                tasks = new[] { Task.CompletedTask };
                break;
            }
        }

        return tasks;
    }
}