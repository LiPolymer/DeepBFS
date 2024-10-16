using System.Security.Cryptography;
using System.Text.Json;

namespace DeepBFS;

public class Analyzer {
    Dictionary<string, List<string>> _fil = new();

    public void Analyze(string target) {
        if (Directory.Exists(target)) {
            Scanner scanner = new();
            scanner.Scan(target);
            List<string> files = scanner.GetFiles();
            Calculator calculator = new();
            calculator.DoCalc(files);
            _fil = calculator.GetResult();
            JsonSerializerOptions o = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            File.WriteAllText("./dump.json", JsonSerializer.Serialize(_fil ,o));
        }
    }
}

class Scanner {
    readonly List<string> _files = new();

    public List<string> GetFiles() {
        return _files;
    }

    public void Scan(string target) {
        ScanDir(target);
        try {
            var dirs = Directory.GetDirectories(target);
            foreach (var dir in dirs) {
                Scan(dir);
            }
        }
        catch (Exception e) {
            Console.WriteLine($"Unknown Exception: {e.Message}");
        }
    }

    private void ScanDir(string target) {
        var dir = new DirectoryInfo(target);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo fInfo in files) {
            try {
                _files.Add(fInfo.FullName);
                Console.WriteLine($"{fInfo.Name}: {fInfo.FullName}");
            }
            catch (Exception e) {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }
}

class Calculator {
    readonly Dictionary<string, List<string>> _solve = new();

    public void DoCalc(List<string> files) {
        ThreadPool.SetMaxThreads(10, 10);
        long bTaskC = ThreadPool.CompletedWorkItemCount;
        foreach (var t in files) {
            //DoCalcWork(t);
            ThreadPool.QueueUserWorkItem((s)=> { DoCalcWork(t); });
        }
        while (ThreadPool.CompletedWorkItemCount < bTaskC + files.Count) { }
    }
    
    void DoCalcWork(string t) {
        try {
            using SHA256 mySha256 = SHA256.Create();
            FileInfo fInfo = new FileInfo(t);
            using FileStream fileStream = fInfo.Open(FileMode.Open);
            fileStream.Position = 0;
            byte[] hashValue = mySha256.ComputeHash(fileStream);
            Console.Write($"{fInfo.Name}: ");
            Utils.PrintByteArray(hashValue);
            string hexHash = Convert.ToHexString(hashValue);
            if (_solve.TryGetValue(hexHash, out var value)) {
                value.Add(fInfo.FullName);
            }
            else {
                List<string> nl =
                [
                    fInfo.FullName
                ];
                _solve.Add(hexHash, nl);
            }
        }
        catch (IOException e) {
            Console.WriteLine($"I/O Exception: {e.Message}");
        }
        catch (UnauthorizedAccessException e) {
            Console.WriteLine($"Access Exception: {e.Message}");
        }
        catch (Exception e) {
            Console.WriteLine($"Unknown Exception: {e.Message}");
        }
    }

    public Dictionary<string, List<string>> GetResult() {
        return _solve;
    }
}