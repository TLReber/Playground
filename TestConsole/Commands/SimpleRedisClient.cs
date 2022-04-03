using StackExchange.Redis;

namespace TestConsole.Commands;

public static class SimpleRedisClient
{
    private static readonly string _channel = "tick";

    private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(
        new ConfigurationOptions { EndPoints = { "localhost:6379" } }
    );

    public static Task[] StartSimpleRedis(string[] args, CancellationToken token)
    {
        var kind = args.Length == 2 ? args[1] : null;

        Console.WriteLine($"Starting redis {kind ?? "producer and consumer"}.");

        var tasks = kind?.ToLower() switch
        {
            "producer" => new[] { StartProducer(token) },
            "consumer" => new[] { StartConsumer(token) },
            _          => new[] { StartProducer(token), StartConsumer(token) }
        };
        return tasks;
    }

    private static async Task StartProducer(CancellationToken token)
    {
        var producer = _redis.GetSubscriber();

        var tick = 1;
        while (!token.IsCancellationRequested)
        {
            await producer.PublishAsync(_channel, tick);
            Console.WriteLine($"Wrote {tick}");
            tick++;

            var delay = Random.Shared.NextDouble() * 2;
            await Task.Delay(TimeSpan.FromSeconds(delay), token);
        }
    }

    private static async Task StartConsumer(CancellationToken token)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(_channel, (_, message) => {
            Console.WriteLine($"Got tick: {message}");
        });

        token.Register(() => subscriber.UnsubscribeAllAsync());
    }
}