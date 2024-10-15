using System.Security.Cryptography;
using System.Text.Json;

namespace DeepBFS;

public class Analyzer {
    readonly Dictionary<string, List<string>> _fil = new();
    
    public void Analyze(string target) {
        ThreadPool.SetMaxThreads(100,100);
        if (Directory.Exists(target)) {
            AnalyzeCallback(target);
            JsonSerializerOptions o = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            File.WriteAllText("./dump.json", JsonSerializer.Serialize(_fil ,o));
        }
        else if (File.Exists(target)) {
            if (Path.GetExtension(target) == ".zip") {
                
            }
            else {
                
            }
        }
    }

    void AnalyzeCallback(string target) {
        AnalyzeDir(target);
        try {
            var dirs = Directory.GetDirectories(target);
            foreach (var dir in dirs) {
                //ThreadPool.QueueUserWorkItem((d) => { AnalyzeCallback(dir); });
                AnalyzeCallback(dir);
            }
        }
        catch (Exception e) {
            Console.WriteLine($"Unknown Exception: {e.Message}");
        }
    }

    void AnalyzeDir(string target) {
        // Create a DirectoryInfo object representing the specified directory.
        var dir = new DirectoryInfo(target);
        // Get the FileInfo objects for every file in the directory.
        FileInfo[] files = dir.GetFiles();
        // Initialize a SHA256 hash object.
        using SHA256 mySha256 = SHA256.Create();
        // Compute and print the hash values for each file in directory.
        foreach (FileInfo fInfo in files) {
            try {
                using FileStream fileStream = fInfo.Open(FileMode.Open);
                // Create a fileStream for the file.
                // Be sure it's positioned to the beginning of the stream.
                fileStream.Position = 0;
                // Compute the hash of the fileStream.
                byte[] hashValue = mySha256.ComputeHash(fileStream);
                // Write the name and hash value of the file to the console.
                Console.Write($"{fInfo.Name}: ");
                Utils.PrintByteArray(hashValue);
                string hexHash = Convert.ToHexString(hashValue);
                if (_fil.TryGetValue(hexHash, out var value)) {
                    value.Add(fInfo.FullName);
                }
                else {
                    List<string> nl =
                    [
                        fInfo.FullName
                    ];
                    _fil.Add(hexHash, nl);
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
        GC.Collect();
    }
}