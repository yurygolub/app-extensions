using System;
using System.Threading;

namespace AppExtensionsLib;

public static class ProgramExtensions
{
    private static bool isContinueWithoutClosing;
    private static bool isAutoReconnect;

    public static CancellationTokenSource Cts { get; private set; } = new ();

    public static void ResetToken() => Cts = new CancellationTokenSource();

    public static void SetContinueWithoutDisconnectOption() =>
        SetOption("Auto continue without closing websocket[y/n]: ", ref isContinueWithoutClosing);

    public static void SetAutoReconnectOption() =>
        SetOption("Auto reconnect[y/n]: ", ref isAutoReconnect);

    public static bool ContinueWithoutDisconnect()
    {
        if (isContinueWithoutClosing)
        {
            return true;
        }

        Console.WriteLine("Press space to continue without disconnecting");

        return ConsoleExtensions.GetKey() == ConsoleKey.Spacebar;
    }

    public static bool AutoReconnect()
    {
        if (isAutoReconnect)
        {
            return true;
        }

        Console.WriteLine("Press space to test again");

        return ConsoleExtensions.GetKey() == ConsoleKey.Spacebar;
    }

    public static void SubscribeEscapeKeyPress()
    {
        ConsoleExtensions.KeyPressed += (o, e) =>
        {
            if (e.Key == ConsoleKey.Escape)
            {
                Cts?.Cancel();
            }
        };
    }

    private static void SetOption(string message, ref bool option)
    {
        ConsoleKey reply;
        do
        {
            Console.Write(message);
            reply = ConsoleExtensions.GetKey();
            Console.Write(reply);
            if (reply == ConsoleKey.Y)
            {
                option = true;
            }

            Console.WriteLine();
        }
        while (reply != ConsoleKey.Y && reply != ConsoleKey.N);
    }
}
