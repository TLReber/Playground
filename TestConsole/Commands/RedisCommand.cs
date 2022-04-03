using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;
using StackExchange.Redis;
using ValidationResult = Spectre.Console.ValidationResult;

namespace TestConsole.Commands;

public class RedisCommand : AsyncCommand<RedisCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RedisCommandSettings settings)
    {
        var tasks = new List<Task>(2);

        if (settings.Consumer.HasValue)
        {
            Console.WriteLine($"Starting redis producer.");

            var producerTask = SimpleRedisClient.StartProducer(Program.Token);
            tasks.Add(producerTask);
        }

        if (settings.Producer.HasValue)
        {
            Console.WriteLine($"Starting redis consumer.");

            var consumerTask = SimpleRedisClient.StartConsumer(Program.Token);
            tasks.Add(consumerTask);
        }

        await Task.WhenAll(tasks);
        return 0;
    }
}

public class RedisCommandSettings : CommandSettings
{
    [CommandOption("-p")] public bool? Producer { get; init; }

    [CommandOption("-c")] public bool? Consumer { get; init; }

    public override ValidationResult Validate()
    {
        if (!Producer.HasValue && !Consumer.HasValue)
        {
            throw new ValidationException("Must be consumer, producer, or both.");
        }

        return base.Validate();
    }
}

internal static class SimpleRedisClient
{
    private static readonly string _channel = "tick";

    private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(
        new ConfigurationOptions { EndPoints = { "localhost:6379" } }
    );

    internal static async Task StartProducer(CancellationToken token)
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

    internal static async Task StartConsumer(CancellationToken token)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(_channel, (_, message) => {
            Console.WriteLine($"Got tick: {message}");
        });

        token.Register(() => subscriber.UnsubscribeAllAsync());
    }
}