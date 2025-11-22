using EasySave.Model;
using System;

namespace EasySave.ViewModel
{
    public class FileEncryptionViewModel
    {
        private readonly EncryptionModel _encryptionModel;

        public FileEncryptionViewModel()
        {
            _encryptionModel = new EncryptionModel();
        }
        /// <summary>
        /// Encrypt a file with a password, boolean return if the encryption was successful
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool EncryptFile(string filePath, string password)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (!_encryptionModel.ShouldEncryptFile(filePath))
            {
                Console.WriteLine("File extension not supported for encryption.");
                return false;
            }

            try
            {
                var jobName = _encryptionModel.GetJobNameFromConfig(Path.GetDirectoryName(filePath));
                if (string.IsNullOrEmpty(jobName))
                {
                    Console.WriteLine("Job name not found in config file.");
                    return false;
                }

                Console.WriteLine($"Encrypting file: {filePath}, Job: {jobName}, Password: {password}");

                var hash = _encryptionModel.ComputeHash(password);
                _encryptionModel.SaveHashAndFileName(filePath, jobName, Path.GetFileName(filePath), hash);
                _encryptionModel.ExecuteEncryptionProcess(filePath, hash);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }




    }



}

