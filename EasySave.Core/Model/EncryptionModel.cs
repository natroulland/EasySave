using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EasySave.Core.Model.CryptoSoft;
using Newtonsoft.Json.Linq;

namespace EasySave.Core.Model
{
    public class EncryptionModel
    {
        private List<string> _extensionsToEncrypt;

        public EncryptionModel()
        {
            LoadExtensionsFromConfig();
        }
        /// <summary>
        /// Load extensions to encrypt from config.json
        /// </summary>
        private void LoadExtensionsFromConfig()
        {
            string filePath = "config.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);
                _extensionsToEncrypt = jsonObj["Extensions"]?.ToObject<List<string>>() ?? new List<string>();
            }
            else
            {
                _extensionsToEncrypt = new List<string>();
            }
        }
        /// <summary>
        /// Computes the SHA-256 hash of the given input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Validates the hash for the given file path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="identifier"></param>
        /// <param name="hash"></param>
        public void SaveHashAndFileName(string filePath, string jobName, string fileName, string hash)
        {
            Console.WriteLine($"Saving hash for file: {fileName}, Job: {jobName}, Hash: {hash}");

            var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath), ".hash");

            var lines = File.Exists(hashFilePath) ? File.ReadAllLines(hashFilePath) : new string[0];
            var updatedLines = new List<string>();
            bool found = false;

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 3 && parts[0] == jobName && parts[1] == fileName)
                {
                    updatedLines.Add($"{jobName}:{fileName}:{hash}");
                    found = true;
                }
                else
                {
                    updatedLines.Add(line);
                }
            }

            if (!found)
            {
                updatedLines.Add($"{jobName}:{fileName}:{hash}");
            }

            File.WriteAllLines(hashFilePath, updatedLines);
        }

        /// <summary>
        /// Executes the encryption process using the CryptoSoft executable.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="key"></param>
        public void ExecuteEncryptionProcess(string filePath, string key)
        {
            try
            {
                
                CryptoSoftAsker asker = new CryptoSoftAsker();
                int res = asker.Execute(filePath, key, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        /// <summary>
        /// Checks if the file should be encrypted based on its extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool ShouldEncryptFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return _extensionsToEncrypt.Contains(extension);
        }
        /// <summary>
        /// Get the job name from the config file
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public string GetJobNameFromConfig(string directoryPath)
        {
            string configFilePath = "config.json";
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                JObject jsonObj = JObject.Parse(json);
                var jobs = jsonObj["Jobs"]?.ToObject<JArray>();

                foreach (var job in jobs)
                {
                    string targetPath = job["targetPath"]?.ToString();
                    string normalizedTargetPath = Path.GetFullPath(targetPath).TrimEnd(Path.DirectorySeparatorChar);
                    string normalizedDirectoryPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar);

                    if (normalizedTargetPath.Equals(normalizedDirectoryPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return job["name"]?.ToString();
                    }
                }
            }
            return null;
        }

    }
}
