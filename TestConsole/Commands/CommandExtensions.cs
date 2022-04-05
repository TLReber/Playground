using Spectre.Console.Cli;

namespace TestConsole.Commands;

public static class CommandExtensions
{
    public static T CastData<T>(this CommandContext? context)
    {
        if (context?.Data?.GetType() != typeof(T))
        {
            throw new ArgumentException($"Exepected type {typeof(T)}, got {context?.Data?.GetType()}");
        }

        var ret = (T) context.Data;
        return ret;
    }
}