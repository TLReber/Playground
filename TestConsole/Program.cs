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
            config.AddCommand<ChannelCommand>("channel");
            config.AddCommand<LoopCommand>("loop");

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
        catch (TaskCanceledException ex) when (ex.CancellationToken == Token)
        {
            AnsiConsole.MarkupLine("[red]Manual cancellation received.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed for unknown reason.\n{ex}");

            return -1;
        }
    }
}