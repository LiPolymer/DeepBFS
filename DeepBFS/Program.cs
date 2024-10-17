namespace DeepBFS;

static class Program {
    static void Main(string[] args) {
        Console.WriteLine("=====DeepBFS=====");
        Debug.WriteLine($"Startup Arguments[{args.Length}]:{Utils.GetStringArray(args)}");
        if (args.Length == 0) {
            Console.WriteLine("Interactive Mode");
            while (true) {
                Console.Write(">");
                MainCross(
                    Utils.ResolveArgs(
                        Console.ReadLine()!));
            }
        }
    }

    static void MainCross(string[] args) {
        switch (args[0]) {
            case "run":
                Analyzer analyzer = new();
                analyzer.Analyze(args[1]);
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }
}