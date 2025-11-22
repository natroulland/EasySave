using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Core.Model.Enums;
using EasySave.Core.Model.Entities;

namespace EasySave.Core.Model
{
    public class JobManagerModel
    {
        /// <summary>
        /// Writes a job to the config file (create, delete or update)
        /// </summary>

        public void WriteJobToConfigFile(string filePath, List<Job> jobs, bool isDeleteOperation = false) // isDeleteOperation is used to delete all jobs and replace them with the new ones
        {

            Dictionary<string, JsonElement> jsonData = new();

            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingJson) ?? new Dictionary<string, JsonElement>();
            }


            List<Job> allJobs = new();
            if (jsonData.ContainsKey("Jobs"))
            {
                allJobs = JsonSerializer.Deserialize<List<Job>>(jsonData["Jobs"].GetRawText()) ?? new List<Job>();
            }

            if (isDeleteOperation)
            {
                allJobs = jobs;
            }
            else
            {
                foreach (Job job in jobs)
                {
                    int existingJobIndex = allJobs.FindIndex(j => j.name == job.name); // Check if the job already exists

                    if (existingJobIndex >= 0)
                    {
                        allJobs[existingJobIndex] = job;
                    }
                    else
                    {
                        allJobs.Add(job);
                    }
                }
            }


            jsonData["Jobs"] = JsonSerializer.SerializeToElement(allJobs);


            JsonSerializerOptions options = new() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(jsonData, options);
            File.WriteAllText(filePath, jsonString);
        }
        /// <summary>
        /// Deletes a job from the config file
        /// </summary>

        public void DeleteJobFromConfigFile(string filePath, Job jobToDelete)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            Dictionary<string, JsonElement> jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString) ?? new Dictionary<string, JsonElement>();

            if (jsonData.ContainsKey("Jobs"))
            {
                List<Job> jobs = JsonSerializer.Deserialize<List<Job>>(jsonData["Jobs"].GetRawText()) ?? new List<Job>();

                jobs.RemoveAll(j => j.name == jobToDelete.name); // Remove the job


                jsonData["Jobs"] = JsonSerializer.SerializeToElement(jobs);


                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                string updatedJsonString = JsonSerializer.Serialize(jsonData, options);
                File.WriteAllText(filePath, updatedJsonString);
            }
        }
        /// <summary>
        /// Updates a job in the config file (deletes the old job and adds the updated job)
        /// </summary>

        public void UpdateJobInConfigFile(string filePath, Job updatedJob, string oldJobName)
        {
            if (!File.Exists(filePath)) return;

            string jsonString = File.ReadAllText(filePath);
            Dictionary<string, JsonElement> jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString) ?? new Dictionary<string, JsonElement>(); // Deserialize the JSON file

            if (jsonData.ContainsKey("Jobs"))
            {
                List<Job> jobs = JsonSerializer.Deserialize<List<Job>>(jsonData["Jobs"].GetRawText()) ?? new List<Job>(); // Deserialize the jobs

                jobs.RemoveAll(j => j.name == oldJobName);


                if (oldJobName != updatedJob.name && jobs.Exists(j => j.name == updatedJob.name)) // Check if a job with the same name already exists
                {
                    throw new InvalidOperationException($"Un job nommé {updatedJob.name} existe déjà.");
                }

                jobs.Add(updatedJob);

                jsonData["Jobs"] = JsonSerializer.SerializeToElement(jobs);

                JsonSerializerOptions options = new() { WriteIndented = true };
                string updatedJsonString = JsonSerializer.Serialize(jsonData, options);
                File.WriteAllText(filePath, updatedJsonString);
            }
        }
    }
}
