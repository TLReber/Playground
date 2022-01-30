namespace TestConsole;

using System.Threading.Tasks;
using StackExchange.Redis;

public static class SimpleRedisClient
{
    private static readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(
        new ConfigurationOptions { EndPoints = { "localhost:6379" } }
    );

    public static async Task Test()
    {
        var db   = _redis.GetDatabase();
        var pong = await db.PingAsync();

        Console.WriteLine(pong);
    }
}