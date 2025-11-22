using System;
using System.IO;
using EasySave.Model;


namespace EasySave.ViewModel
{
    public class JobManagerViewModel
    {
        private JobManagerModel jobManagerModel;

        public JobManagerViewModel()
        {
            jobManagerModel = new JobManagerModel();
        }
        /// <summary>
        /// Calls the model to delete a job from the config file
        /// </summary>
        public bool DeleteJob(Job job, string configFilePath)
        {
            if (job == null) return false;

            jobManagerModel.DeleteJobFromConfigFile(configFilePath, job);
            return true;
        }
        /// <summary>
        /// Calls the model to update a job in the config file
        /// </summary>
        public bool UpdateJob(Job updatedJob, string configFilePath, string oldJobName)
        {
            if (ValidateJobOptions(updatedJob))
            {
                jobManagerModel.UpdateJobInConfigFile(configFilePath, updatedJob, oldJobName); // Update the job
                return true;
            }

            return false;
        }
        /// <summary>
        /// Calls the model to add a job to the config file
        /// </summary>

        public bool ValidateAndSaveJob(Job? job, string configFilePath)
        {
            if (ValidateJobOptions(job))
            {
                jobManagerModel.WriteJobToConfigFile(configFilePath, new List<Job> { job }); // Save the job

                return true;
            }

            return false;
        }
        /// <summary>
        /// Validates the job options before saving it
        /// </summary>
        private bool ValidateJobOptions(Job options)
        {
            if (options == null) // Check if options is not null
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.name)) // Check if name is not null or empty
                return false;

            if (options.sourcePath == null || !Directory.Exists(options.sourcePath)) // Check if source path exists
                throw new ArgumentException();

            return true;
        }
    }
}
