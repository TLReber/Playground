using System.ComponentModel;
using Spectre.Console.Cli;

namespace TestConsole.Commands;

public class LoopCommand : AsyncCommand<LoopCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LoopCommandSettings settings)
    {
        var token = context.CastData<CancellationToken>();
        
        var channelTasks = SimpleLoop.StartTaskLoop(settings.NumTasks, token);
        await Task.WhenAll(channelTasks);

        return 0;
    }
}

public class LoopCommandSettings : CommandSettings
{
    [CommandOption("-t|--tasks")]
    [DefaultValue(30)]
    public int NumTasks { get; init; }
}

internal static class SimpleLoop
{
    public static Task[] StartTaskLoop(int numTasks, CancellationToken token)
    {
        var completed  = 0;
        var updateFreq = 5;

        Console.WriteLine($"{DateTime.Now:mm:ss.fff} - Starting now!");

        var taskList = Enumerable.Range(1, numTasks).Select(taskNum => {
            var task = Task.Run(async () => {
                await Task.Delay(taskNum * 1000, token);
            }, token).ContinueWith(_ => {
                completed++;

                if (completed % updateFreq == 0)
                {
                    Console.WriteLine($"{DateTime.Now:mm:ss.fff} - {completed,2} of {numTasks,2} done!");
                }
            }, token);

            return task;
        }).ToArray();

        Task.WhenAll(taskList).ContinueWith(_ => {
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} - All done!");
        }, token);

        return taskList;
    }
}