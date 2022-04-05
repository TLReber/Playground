global using System;
using Spectre.Console;
using Spectre.Console.Cli;
using TestConsole.Commands;

namespace TestConsole;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var cts   = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var token = cts.Token;

        var app = new CommandApp();
        app.Configure(config => {
            config.AddCommand<RedisCommand>("redis")
                  .WithData(token)
                  .WithExample(new[] { "redis", "-p" });

            config.AddCommand<ChannelCommand>("channel")
                  .WithData(token)
                  .WithExample(new[] { "redis", "-p", "4" });

            config.AddCommand<LoopCommand>("loop")
                  .WithData(token)
                  .WithExample(new[] { "loop", "-t", "15" });

            config.PropagateExceptions();
        });

        try
        {
            var retCode = await app.RunAsync(args);
            return retCode;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[red bold]Timed out.[/]");
            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }
}