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

    static Analyzer _analyzer = new Analyzer();
    
    static void MainCross(string[] args) {
        switch (args[0]) {
            case "run":
                _analyzer = new();
                _analyzer.Analyze(args[1]);
                break;
            case "copy":
                _analyzer.Copy(args[1]);
                break;
            case "len":
                Console.WriteLine(_analyzer.Length());
                break;
            case "exit":
                Environment.Exit(0);
                break;
        }
    }
}