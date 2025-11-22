using EasySave.Model;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Windows;
namespace EasySave.ViewModel
{
    public class FileDecryptionViewModel
    {
        private readonly DecryptionModel _decryptionModel;
        
        public FileDecryptionViewModel()
        {
           
            _decryptionModel = new DecryptionModel();
        }
        /// <summary>
        /// Decrypts a file or a directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int DecryptFile(string path, string password = null)
        {
            Console.WriteLine($"DecryptFile called with path: {path}, password: {password}");

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Invalid Path. Example : C:/FolderA/ or C:/FolderA/example.txt");
                return -1;
            }

            if (Directory.Exists(path))
            {
                string jobName = GetJobNameFromConfig(path);
                if (string.IsNullOrEmpty(jobName))
                {
                    Console.WriteLine("Job name not found in config file");
                    return -5;
                }

                Console.WriteLine($"Decrypting directory with job name: {jobName}");
                return DecryptDirectory(path, jobName, jobName);
            }
            else if (File.Exists(path))
            {
                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("No password string to use");
                    return -1;
                }

                var fileName = Path.GetFileName(path);
                var hashFilePath = Path.Combine(Path.GetDirectoryName(path), ".hash");

                if (File.Exists(hashFilePath))
                {
                    var lines = File.ReadAllLines(hashFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 3 && parts[1] == fileName)
                        {
                            string jobName = parts[0];
                            string hash = parts[2];
                            Console.WriteLine($"Decrypting file with job name: {jobName}, hash: {hash}");
                            return DecryptSingleFile(path, jobName, password);
                        }
                    }
                }

                Console.WriteLine("File not found in hash file, no decryption needed.");
                return 0; // No decryption needed
            }
            else
            {
                Console.WriteLine("Invalid Path. Example : C:/FolderA/ or C:/FolderA/example.txt");
                return -4;
            }
        }



        /// <summary>
        /// Decrypts a single file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="jobName"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        private int DecryptSingleFile(string filePath, string jobName, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Incorrect Path. Example : C:/FolderA/ or C:/FolderA/example.txt");
                    return -1;
                }

                if (Path.GetFileName(filePath) == ".hash")
                {
                    return 0;
                }

                if (string.IsNullOrEmpty(jobName))
                {
                    Console.WriteLine("Incorrect job name");
                    return -1;
                }

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Incorrect password");
                    return -1;
                }

                var hash = _decryptionModel.ComputeHash(password);
                Console.WriteLine($"Hash calculé : {hash}");

                var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, ".hash");
                if (File.Exists(hashFilePath))
                {
                    var lines = File.ReadAllLines(hashFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 3 && parts[0] == jobName && parts[1] == Path.GetFileName(filePath))
                        {
                            string storedHash = parts[2];
                            Console.WriteLine($"Hash stocké : {storedHash}");

                            if (!hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"Incorrect hash for file: {filePath}");
                                return -2;
                            }

                            break;
                        }
                    }
                }

                return _decryptionModel.ExecuteDecryptionProcess(filePath, hash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting file : {ex.Message}");
                return -3;
            }
        }



        /// <summary>
        /// Decrypts all files in the given directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="jobName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private int DecryptDirectory(string directoryPath, string jobName, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryPath))
                {
                    Console.WriteLine("Incorrect Path. Example : C:/FolderA/ or C:/FolderA/example.txt");
                    return -1;
                }

                if (string.IsNullOrEmpty(jobName))
                {
                    Console.WriteLine("Job name incorrect");
                    return -1;
                }

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Incorrect password");
                    return -1;
                }

                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                int overallResult = 0;

                foreach (var filePath in files)
                {
                    if (Path.GetFileName(filePath) == ".hash")
                    {
                        continue;
                    }

                    var fileName = Path.GetFileName(filePath);
                    var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath), ".hash");

                    if (File.Exists(hashFilePath))
                    {
                        var lines = File.ReadAllLines(hashFilePath);
                        foreach (var line in lines)
                        {
                            var parts = line.Split(':');
                            if (parts.Length == 3 && parts[0] == jobName && parts[1] == fileName)
                            {
                                int result = DecryptSingleFile(filePath, jobName, password);
                                if (result != 0)
                                {
                                    overallResult = result;
                                }
                                break;
                            }
                        }
                    }
                }

                return overallResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting file : {ex.Message}");
                return -3;
            }
        }



        /// <summary>
        /// Gets the job name from the config file.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public string GetJobNameFromConfig(string directoryPath)
        {
            string configFilePath = "config.json";
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine("Configuration file not found, try to reset to default settings");
                return null;
            }

            string json = File.ReadAllText(configFilePath);
            Console.WriteLine($"Config file content: {json}");

            JObject jsonObj = JObject.Parse(json);
            var jobs = jsonObj["Jobs"]?.ToObject<JArray>();

            if (jobs == null)
            {
                Console.WriteLine("No job found in config file");
                return null;
            }

            foreach (var job in jobs)
            {
                string targetPath = job["targetPath"]?.ToString();
                if (string.IsNullOrEmpty(targetPath))
                {
                    continue;
                }

                string normalizedTargetPath = Path.GetFullPath(targetPath).TrimEnd(Path.DirectorySeparatorChar);
                string normalizedDirectoryPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar);

                Console.WriteLine($"Comparing {normalizedTargetPath} with {normalizedDirectoryPath}");

                if (normalizedTargetPath.Equals(normalizedDirectoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    string jobName = job["name"]?.ToString();
                    Console.WriteLine($"Job name found: {jobName}");
                    return jobName;
                }
            }
            return null;
        }

    }



}

