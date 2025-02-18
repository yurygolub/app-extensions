using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppExtensionsLib;

public static class ConsoleExtensions
{
    private static ConsoleKey pressedKey;

    private static bool keyRequested;
    private static bool isPressed;

    public static event EventHandler<KeyPressedEventArgs> KeyPressed;

    public static ConsoleKey GetKey()
    {
        keyRequested = true;

        while (!isPressed)
        {
            Thread.Sleep(1);
        }

        isPressed = false;
        keyRequested = false;
        return pressedKey;
    }

    public static async Task StartAsync(CancellationToken token = default)
    {
        await Task.Run(() => ConsoleReadKey(token), token);

        static void ConsoleReadKey(CancellationToken token)
        {
            while (true)
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                    token.ThrowIfCancellationRequested();
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (keyRequested)
                {
                    pressedKey = key;
                    isPressed = true;
                }
                else
                {
                    KeyPressed?.Invoke(null, new KeyPressedEventArgs { Key = key });
                }
            }
        }
    }
}
