using System.Text.RegularExpressions;

namespace DeepBFS;

public static class Utils{
    public static string[] ResolveArgs(string st) {
        return Regex.Matches(st, @"""([^""]*)""|(\S+)")
            .Select(i => i.Value)
            .ToArray();
    }

    public static string GetStringArray(string[] args) {
        string buffer = string.Empty;
        foreach (var child in args)
        {
            buffer += child;
        }
        return buffer;
    }
    
    // Display the byte array in a readable format.
    public static void PrintByteArray(byte[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Console.Write($"{array[i]:X2}");
            if ((i % 4) == 3) Console.Write(" ");
        }
        Console.WriteLine();
    }
}

public static class Debug {
    public static void WriteLine(string msg) {
        #if DEBUG
            Console.WriteLine(msg);
        #endif
    }
}