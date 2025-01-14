using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NamuD.Library
{
    public class Class1
    {
        private readonly string filePath;

        public Class1(string filePath)
        {
            this.filePath = filePath;
        }

        public void SaveTasks(List<TaskModel> tasks)
        {
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public List<TaskModel> LoadTasks()
        {
            if (!File.Exists(filePath)) return new List<TaskModel>();

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<TaskModel>>(json) ?? new List<TaskModel>();
        }
    }
}