using System.Threading.Channels;

namespace Console;

public static class SimpleChannel
{
    private static readonly Channel<int> _channel = Channel.CreateUnbounded<int>(
        new() { SingleReader = true, SingleWriter = true }
    );

    public static async Task Produce(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var next = Random.Shared.Next(1, 5001);
            await _channel.Writer.WriteAsync(next, token);

            var delay = Random.Shared.NextDouble() * 2;
            await Task.Delay(TimeSpan.FromSeconds(delay), token);
        }
    }

    public static async Task Consume(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var next = await _channel.Reader.ReadAsync(token);
            System.Console.WriteLine($"{DateTime.Now:mm:ss.fff} - Got {next,5}");
        }
    }
}