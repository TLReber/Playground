namespace TestConsole;

using System.Threading.Channels;

public static class SimpleChannel
{
    private static readonly Channel<(int id, int tid, int val)> _channel;

    static SimpleChannel()
    {
        _channel = Channel.CreateUnbounded<(int id, int tid, int val)>();
    }

    public static async Task Produce(int id, CancellationToken token)
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

    public static async Task Consume(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var (producerId, producerTid, next) = await _channel.Reader.ReadAsync(token);
            Console.WriteLine($"{DateTime.Now:mm:ss.fff} - Got {next,3} from {producerId} on {producerTid}");
        }
    }
}