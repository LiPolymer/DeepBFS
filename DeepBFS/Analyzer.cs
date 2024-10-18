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
            JsonSerializerOptions o = new JsonSerializerOptions() {
                WriteIndented = true,
            };
            File.WriteAllText("./dump.json", JsonSerializer.Serialize(_fil ,o));
        }
    }

    public void Copy(string targetDir) {
        if (!Directory.Exists(targetDir)) {
            Directory.CreateDirectory(targetDir);
        }
        Console.WriteLine("Saving index...");
        JsonSerializerOptions o = new JsonSerializerOptions() {
            WriteIndented = true,
        };
        File.WriteAllText(Path.Combine(targetDir,"index.dbf.json"), JsonSerializer.Serialize(_fil ,o));
        Console.WriteLine($"Copying files to {targetDir}");
        var x = 0;
        foreach (var f in _fil) {
            x += 1;
            Console.WriteLine($"[{x}/{_fil.Count}][{f.Value.Count}]{f.Key}[{f.Value[0]}]");
            File.Copy(f.Value[0],Path.Combine(targetDir,f.Key));
        }
        Console.WriteLine("Done!");
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
    long _bTaskC;
    int _fc;
    public void DoCalc(List<string> files) {
        ThreadPool.SetMaxThreads(50, 50);
        _bTaskC = ThreadPool.CompletedWorkItemCount;
        foreach (var t in files) {
            //DoCalcWork(t);
            ThreadPool.QueueUserWorkItem(_=> { DoCalcWork(t); });
        }
        _fc = files.Count;
        while (ThreadPool.CompletedWorkItemCount < _bTaskC + files.Count) { }
    }
    
    void DoCalcWork(string t) {
        try {
            using SHA256 mySha256 = SHA256.Create();
            FileInfo fInfo = new FileInfo(t);
            using FileStream fileStream = fInfo.Open(FileMode.Open);
            fileStream.Position = 0;
            byte[] hashValue = mySha256.ComputeHash(fileStream);
            Console.Write($"[{ThreadPool.CompletedWorkItemCount - _bTaskC}/{_fc}]{fInfo.Name}: ");
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