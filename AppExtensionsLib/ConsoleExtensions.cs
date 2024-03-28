using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppExtensionsLib;

public static class ConsoleExtensions
{
    private static CancellationTokenSource cts = new ();

    private static ConsoleKey? pressed;

    private static bool keyRequested;

    public static event EventHandler<KeyPressedEventArgs> KeyPressed;

    public static ConsoleKey GetKey()
    {
        keyRequested = true;

        try
        {
            Task.Delay(Timeout.Infinite, cts.Token).Wait();
        }
        catch (AggregateException)
        {
        }

        keyRequested = false;

        var result = pressed.Value;
        pressed = null;

        cts = new CancellationTokenSource();

        return result;
    }

    public static async Task StartAsync()
    {
        await Task.Run(ConsoleReadKey);

        static void ConsoleReadKey()
        {
            var key = Console.ReadKey(true).Key;

            if (keyRequested)
            {
                pressed = key;
                cts.Cancel();
            }
            else
            {
                KeyPressed?.Invoke(null, new KeyPressedEventArgs { Key = key });
            }
        }
    }
}
