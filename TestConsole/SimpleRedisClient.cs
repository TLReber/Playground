namespace TestConsole;

using System.Threading.Tasks;
using StackExchange.Redis;

public static class SimpleRedisClient
{
    private static readonly string _channel = "tick";

    private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(
        new ConfigurationOptions { EndPoints = { "localhost:6379" } }
    );

    public static async Task StartProducer(CancellationToken token)
    {
        var producer = _redis.GetSubscriber();

        var tick = 1;
        while (!token.IsCancellationRequested)
        {
            await producer.PublishAsync(_channel, tick);
            tick++;

            var delay = Random.Shared.NextDouble() * 2;
            await Task.Delay(TimeSpan.FromSeconds(delay), token);
        }
    }

    public static async Task StartConsumer(CancellationToken token)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(_channel, (_, message) => {
            Console.WriteLine($"Got tick: {message}");
        });

        token.Register(() => subscriber.UnsubscribeAllAsync());
    }
}