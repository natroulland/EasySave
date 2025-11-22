using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Logger
{
    public class JobstateOpener
    {
        private static readonly Lazy<JobstateOpener> _instance = new(() => new JobstateOpener());
        private readonly string jobstateDirectory = "Jobstate";
        private readonly string jobstateFilePath;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private List<Dictionary<string, string>> _cache = new();
        private bool _isDirty = false;
        private static readonly Timer _saveTimer;

        static JobstateOpener()
        {
            _saveTimer = new Timer(async _ => await Instance.SaveCacheToFileAsync(), null, 50, 50);
        }

        private JobstateOpener()
        {
            if (!Directory.Exists(jobstateDirectory))
                Directory.CreateDirectory(jobstateDirectory);

            jobstateFilePath = Path.Combine(jobstateDirectory, "jobstate.json");

            if (!File.Exists(jobstateFilePath))
                File.WriteAllText(jobstateFilePath, "[]");

            LoadCache();
        }

        public static JobstateOpener Instance => _instance.Value;

        private void LoadCache()
        {
            try
            {
                string content = File.ReadAllText(jobstateFilePath);
                _cache = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(content) ?? new List<Dictionary<string, string>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cache: {ex.Message}");
                _cache = new List<Dictionary<string, string>>();
            }
        }

        public Task<string?> ReadFileAsync()
        {
            string json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
            return Task.FromResult<string?>(json);
        }

        public async Task UpdateJobstateAsync(string newJobJson)
        {
            await _semaphore.WaitAsync();
            try
            {
                Dictionary<string, string> newJobData = JsonSerializer.Deserialize<Dictionary<string, string>>(newJobJson);
                if (newJobData == null || !newJobData.ContainsKey("Name")) return;

                string newJobName = newJobData["Name"];
                bool jobUpdated = false;

                for (int i = 0; i < _cache.Count; i++)
                {
                    if (_cache[i]["Name"] == newJobName)
                    {
                        foreach (string key in newJobData.Keys)
                        {
                            if (newJobData[key] != "NAN")
                            {
                                _cache[i][key] = newJobData[key];
                            }
                        }
                        jobUpdated = true;
                        break;
                    }
                }

                if (!jobUpdated)
                {
                    _cache.Add(newJobData);
                }

                _isDirty = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveCacheToFileAsync()
        {
            if (!_isDirty) return;

            await _semaphore.WaitAsync();
            try
            {
                string updatedJson = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(jobstateFilePath, updatedJson);
                _isDirty = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
