using System.Threading.Channels;

var cts   = new CancellationTokenSource();
var token = cts.Token;

while (!token.IsCancellationRequested)
{
    Console.WriteLine("Please enter a command - Q to quit.\n");

    var input = Console.ReadLine();

    switch (input)
    {
        case "q":
        case "Q":
            cts.Cancel();
            break;
        default:
            Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} - Processing next command in 1000ms.");
            break;
    }

    try
    {
        await Task.Delay(1000, cts.Token);
    }
    catch (TaskCanceledException) { }
}

Console.WriteLine("Goodbye!");