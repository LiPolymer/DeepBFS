using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

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

    public int Length() {
        return _fil.Count;
    }
    
    public void Copy(string targetDir, bool isMultiThread = false) {
        if (!Directory.Exists(targetDir)) {
            Directory.CreateDirectory(targetDir);
        }
        Console.WriteLine("Saving index...");
        JsonSerializerOptions o = new JsonSerializerOptions() {
            WriteIndented = true,
        };
        File.WriteAllText(Path.Combine(targetDir,"index.dbf.json"), JsonSerializer.Serialize(_fil ,o));
        Console.WriteLine($"Copying files to {targetDir}");
        if (!isMultiThread) {
            var x = 0;
            foreach (var f in _fil) {
                x += 1;
                Console.WriteLine($"[{x}/{_fil.Count}][{f.Value.Count}]{f.Key}[{f.Value[0]}]");
                if (f.Value[0].Contains('|')) {
                    string[] fd = f.Value[0].Split('|');
                    try {
                        using ZipArchive za = ZipFile.OpenRead(fd[0]);
                        ZipArchiveEntry? ze = za.GetEntry(fd[1]);
                        ze?.ExtractToFile(Path.Combine(targetDir,f.Key));
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }
                else {
                    File.Copy(f.Value[0],Path.Combine(targetDir,f.Key));  
                }

            }
        }
        else {
            
        }
        Console.WriteLine("Done!");
    }
    
    void DoCopyWork(string targetDir) {
        
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
        ThreadPool.SetMaxThreads(8, 8);
        _bTaskC = ThreadPool.CompletedWorkItemCount;
        foreach (var t in files) {
            //DoCalcWork(t);
            ThreadPool.QueueUserWorkItem(_=> { DoCalcWork(t); });
        }
        _fc = files.Count;
        while (ThreadPool.CompletedWorkItemCount < _bTaskC + files.Count) { }
    }
    
    void DoCalcWork(string t) {
        if (t.EndsWith(".zip")) {
            try {
                using ZipArchive zf = ZipFile.OpenRead(t);
                CalcZip(zf,t);
            }            
            catch (Exception e) {
                Console.WriteLine($"Unknown Exception: {e.Message}");
            }
        } else {
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
    }

    public Dictionary<string, List<string>> GetResult() {
        return _solve;
    }

    void CalcZip(ZipArchive zf, string path) {
        using SHA256 mySha256 = SHA256.Create();
        foreach (var zae in zf.Entries) {
            Console.WriteLine("[ZIP]" + path + "|" + zae.FullName);
            byte[] hashValue = mySha256.ComputeHash(zae.Open());
            Utils.PrintByteArray(hashValue);
            string hexHash = Convert.ToHexString(hashValue);
            string fi = path + "|" + zae.FullName;
            if (_solve.TryGetValue(hexHash, out var value)) {
                value.Add(fi);
            }
            else {
                List<string> nl =
                [
                    fi
                ];
                _solve.Add(hexHash, nl);
            }
        }
    }
}