using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.Logger
{
    public  class JobstateWriter : Observer
    {
        public override async void Update(Dictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("subject") || data["subject"] != "Jobstate")
                return;

            // Key list
            string[] expectedKeys = {
        "Name", "FileSource", "FileTarget", "State",
        "TotalFileToCopy", "FileTransferTime", "NbFileToDo", "Progression"
    };

            // replace no key by nan
            Dictionary<string, string> completedData = expectedKeys.ToDictionary(
                key => key,
                key => data.GetValueOrDefault(key, "NAN")
            );


            completedData["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string jsonJobstate = JsonSerializer.Serialize(completedData, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await JobstateOpener.Instance.UpdateJobstateAsync(jsonJobstate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing Jobstate: {ex.Message}");
            }
        }

    }
}
