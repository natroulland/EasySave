using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace EasySave.Core.Model.CryptoSoft
{
    public class CryptoSoftExecutor
    {
        private readonly string _cryptoSoftPath = @"../../../../CryptoSoft/CryptoSoft.exe";
        private static readonly Lazy<CryptoSoftExecutor> _instance =
            new(() => new CryptoSoftExecutor());
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public async Task<int> Execute(string filePath, string key, bool isEncryption)
        {
            string expectedHash = "40AB1A90ADD1DDCAF57E5008E72C76DB07D356C2CABB1332D48486B180F5C4E5";
            string actualHash = ComputeHashForFile(_cryptoSoftPath);
            if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("Erreur : CryptoSoft.exe a été modifié ou corrompu.");
                return -3;
            }

            await semaphore.WaitAsync();
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _cryptoSoftPath,
                        Arguments = isEncryption ? $"\"{filePath}\" {key}" : $"\"{filePath}\" \"{key}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error: {error}");
                }

                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return -1;
            }
            finally
            {
                semaphore.Release();
            }
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
        public static CryptoSoftExecutor Instance => _instance.Value;

    }

}