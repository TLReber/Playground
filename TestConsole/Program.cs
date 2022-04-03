global using System;
using Spectre.Console;
using Spectre.Console.Cli;
using TestConsole.Commands;

namespace TestConsole;

public static class Program
{
    private static readonly CancellationTokenSource _cts;
    public static readonly  CancellationToken       Token;

    static Program()
    {
        _cts  = new CancellationTokenSource();
        Token = _cts.Token;
    }

    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config => {
            config.AddCommand<RedisCommand>("redis");

            config.PropagateExceptions();
        });

        var appTask = app.RunAsync(args);

        while (!Token.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (input?.ToLower() == "q")
            {
                _cts.Cancel();
            }

            try
            {
                await Task.Delay(1000, Token);
            }
            catch (TaskCanceledException) { }
        }

        try
        {
            var retCode = await appTask;
            return retCode;
        }
        catch (TaskCanceledException) //  when (ex.CancellationToken == Token)
        {
            AnsiConsole.MarkupLine("[red dim underline]Manual cancellation received.[/]");
            return 0;
        }

        return -1;
    }

    /*
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
    */
}