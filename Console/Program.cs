namespace Console;

using System;
using System.Threading.Channels;

public static class Program
{
    private static readonly Channel<int> _channel = Channel.CreateUnbounded<int>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        }
    );

    private static async Task Produce(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var next = Random.Shared.Next(1, 5001);
            // Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Writing {next,-5}");
            await _channel.Writer.WriteAsync(next, token);

            var delay = Random.Shared.NextDouble() * 2;
            await Task.Delay(TimeSpan.FromSeconds(delay), token);
        }
    }

    private static async Task Consume(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var next = await _channel.Reader.ReadAsync(token);
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} - Got {next,5}");
        }
    }

    public static async Task Main()
    {
        var cts   = new CancellationTokenSource();
        var token = cts.Token;

        var producer = Task.Run(() => Produce(token), token);
        var consumer = Task.Run(() => Consume(token), token);

        while (!token.IsCancellationRequested)
        {
            var input = Console.ReadLine();

            switch (input)
            {
                case "q":
                case "Q":
                    cts.Cancel();
                    break;
            }

            try
            {
                await Task.Delay(1000, cts.Token);
            }
            catch (TaskCanceledException) { }
        }

        try
        {
            await producer;
            await consumer;
        }
        catch (TaskCanceledException) { }

        Console.WriteLine("Goodbye!");
    }
}