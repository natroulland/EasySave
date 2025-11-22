
﻿using EasySave.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.ViewModel
{
    public class FileDecryptionViewModel
    {
        private DecryptionModel _decryptionModel;

        public FileDecryptionViewModel()
        {
            _decryptionModel = new DecryptionModel(new List<string>());
        }

        public int DecryptDirectory(string path)
        {
            

            if (string.IsNullOrEmpty(path))
            {
                
                return -1;
            }

            if (!Directory.Exists(path))
            {
               
                return -4;
            }

            string jobName = GetJobNameFromConfig(path);
            if (string.IsNullOrEmpty(jobName))
            {
                
                return -5;
            }

            

            List<string> filesToDecrypt = GetFilesToDecryptFromHash(path, jobName);
            List<string> allowedExtensions = GetAllowedExtensionsFromConfig();
            _decryptionModel = new DecryptionModel(filesToDecrypt);

            int overallResult = 0;
            List<string> decryptedFiles = new List<string>();

            List<string> filesToDecryptCopy = new List<string>(filesToDecrypt);
            foreach (var filePath in filesToDecryptCopy)
            {
                string fileExtension = Path.GetExtension(filePath);
                if (!allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    
                    continue;
                }

                int result = DecryptSingleFile(filePath, jobName);
                if (result == 0)
                {
                    decryptedFiles.Add(filePath);
                }
                else
                {
                    overallResult = result;
                }
            }

            if (decryptedFiles.Count == filesToDecrypt.Count)
            {
                RemoveHashEntry(path);
            }

            return overallResult;
        }

        private int DecryptSingleFile(string filePath, string jobName)
        {
            try
            {
                var hash = _decryptionModel.ComputeHash(jobName);
                

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
                            

                            if (!hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase))
                            {
                                
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
                
                return -3;
            }
        }

        private void RemoveHashEntry(string filePath)
        {
            var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath), ".hash");

            

            if (File.Exists(hashFilePath))
            {
                try
                {
                    File.Delete(hashFilePath);
                    Console.WriteLine("Fichier .hash supprimé avec succès.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Erreur lors de la suppression du fichier .hash : {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine(" Fichier .hash introuvable.");
            }
        }

        public string GetJobNameFromConfig(string directoryPath)
        {
            string configFilePath = "config.json";
            if (!File.Exists(configFilePath))
            {
                
                return null;
            }

            string json = File.ReadAllText(configFilePath);
            JObject jsonObj = JObject.Parse(json);
            var jobs = jsonObj["Jobs"]?.ToObject<JArray>();

            if (jobs == null)
            {
                
                return null;
            }

            foreach (var job in jobs)
            {
                string targetPath = job["targetPath"]?.ToString();
                if (string.IsNullOrEmpty(targetPath))
                    continue;

                string normalizedTargetPath = Path.GetFullPath(targetPath).TrimEnd(Path.DirectorySeparatorChar);
                string normalizedDirectoryPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar);

                if (normalizedTargetPath.Equals(normalizedDirectoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    return job["name"]?.ToString();
                }
            }
            return null;
        }

        private List<string> GetAllowedExtensionsFromConfig()
        {
            string configFilePath = "config.json";
            if (!File.Exists(configFilePath))
            {
                
                return new List<string>();
            }

            string json = File.ReadAllText(configFilePath);
            JObject jsonObj = JObject.Parse(json);
            var extensions = jsonObj["Extensions"]?.ToObject<JArray>();

            if (extensions == null)
            {
                
                return new List<string>();
            }

            return extensions.Select(ext => ext.ToString()).ToList();
        }

        private List<string> GetFilesToDecryptFromHash(string directoryPath, string jobName)
        {
            var hashFilePath = Path.Combine(directoryPath, ".hash");
            List<string> filesToDecrypt = new List<string>();

            if (File.Exists(hashFilePath))
            {
                var lines = File.ReadAllLines(hashFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 3 && parts[0] == jobName)
                    {
                        var filePath = Path.Combine(directoryPath, parts[1]);
                        if (File.Exists(filePath))
                        {
                            filesToDecrypt.Add(filePath);
                        }
                    }
                }
            }

            return filesToDecrypt;
        }
    }
}

﻿
