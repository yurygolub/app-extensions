using System;

namespace AppExtensionsLib;

public class KeyPressedEventArgs : EventArgs
{
    public ConsoleKey Key { get; init; }
}
