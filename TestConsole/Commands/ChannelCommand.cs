using System.ComponentModel;
using System.Threading.Channels;
using Spectre.Console.Cli;

namespace TestConsole.Commands;

public class ChannelCommand : AsyncCommand<ChannelCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ChannelCommandSettings settings)
    {
        var token = context.CastData<CancellationToken>();
        
        var channelTasks = SimpleChannel.StartSimpleChannel(settings.NumProducers, token);
        await Task.WhenAll(channelTasks);

        return 0;
    }
}

public class ChannelCommandSettings : CommandSettings
{
    [CommandOption("-p|--producers")]
    [DefaultValue(1)]
    public int NumProducers { get; init; }
}

internal static class SimpleChannel
{
    private static readonly Channel<(int id, int tid, int val)> _channel;

    static SimpleChannel()
    {
        _channel = Channel.CreateUnbounded<(int id, int tid, int val)>();
    }

    public static Task[] StartSimpleChannel(int numProducers, CancellationToken token)
    {
        var producerTasks = Enumerable.Range(1, numProducers).Select(id =>
            Produce(id, token)
        );

        var consumerTask = Consume(token);

        var tasks = producerTasks.Concat(new[] { consumerTask }).ToArray();
        return tasks;
    }

    private static async Task Produce(int id, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var next = Random.Shared.Next(1, 1000);
            var tid  = Environment.CurrentManagedThreadId;
            await _channel.Writer.WriteAsync((id, tid, next), token);

            var delay = Random.Shared.NextDouble() * 2;
            await Task.Delay(TimeSpan.FromSeconds(delay), token);
        }
    }

    private static async Task Consume(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var (producerId, producerTid, next) = await _channel.Reader.ReadAsync(token);
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} - Got {next,3} from {producerId} on {producerTid}");
        }
    }
}