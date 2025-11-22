using EasySave.Core.Model.CryptoSoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasySave.Core.Model
{
    public class DecryptionModel
    {
        private readonly List<string> filesToDecrypt;

        public DecryptionModel(List<string> filesToDecrypt)
        {
            this.filesToDecrypt = filesToDecrypt;
        }

        public string ComputeHash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool ValidateHash(string filePath, string jobName, string hash)
        {
            var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath), ".hash");

            if (!File.Exists(hashFilePath))
            {
                return false;
            }

            var lines = File.ReadAllLines(hashFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 3 && parts[0] == jobName && parts[1] == Path.GetFileName(filePath) && parts[2] == hash)
                {
                    return true;
                }
            }

            return false;
        }

        public string ComputeHashForFile(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }

        public int ExecuteDecryptionProcess(string filePath, string key)
        {
            try
            {
                
                
                CryptoSoftAsker asker = new CryptoSoftAsker();
                int res = asker.Execute(filePath, key, true);
                RemoveHashEntry(filePath);
                return 1;
            }
            catch (Exception ex)
            {
                
                return -3;
            }
        }

        public void RemoveHashEntry(string filePath)
        {
            var hashFilePath = Path.Combine(Path.GetDirectoryName(filePath), ".hash");

            Console.WriteLine($"Suppression du fichier hash : {hashFilePath}");

            if (File.Exists(hashFilePath))
            {
                try
                {
                    File.Delete(hashFilePath);
                    Console.WriteLine("Fichier .hash supprimé avec succès.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la suppression du fichier .hash : {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Fichier .hash introuvable.");
            }
        }
    }
}
