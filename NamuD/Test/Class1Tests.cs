using NUnit.Framework;
using NamuD.Library;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NamuD.Tests
{
    [TestFixture]
    public class Class1Tests
    {
        private Class1 dataHandler;
        private const string TestFilePath = "test_tasks.json";

        [SetUp]
        public void Setup()
        {
            dataHandler = new Class1(TestFilePath);
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(TestFilePath))
                File.Delete(TestFilePath);
        }

        [Test]
        public void SaveAndLoadTasks_ShouldWorkCorrectly()
        {
            // Arrange
            var tasks = new List<TaskModel>
            {
                new TaskModel { TaskName = "Task 1", Priority = "High", Status = "Not Started" },
                new TaskModel { TaskName = "Task 2", Priority = "Low", Status = "Completed" }
            };

            // Act
            dataHandler.SaveTasks(tasks);
            var loadedTasks = dataHandler.LoadTasks();

            // Assert
            Assert.That(loadedTasks.Count, Is.EqualTo(2));
            Assert.That(loadedTasks[0].TaskName, Is.EqualTo("Task 1"));
            Assert.That(loadedTasks[0].Priority, Is.EqualTo("High"));
            Assert.That(loadedTasks[0].Status, Is.EqualTo("Not Started"));
        }

        [Test]
        public async Task FetchTimeFromApi_ShouldReturnValidTime()
        {
            // Arrange
            var httpClient = new HttpClient();
            var apiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Vilnius";

            // Act
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Parse response to ensure valid time format
            var timeData = JsonSerializer.Deserialize<TimeApiResponse>(responseBody);

            // Assert
            Assert.That(timeData, Is.Not.Null, "API response should not be null.");
            Assert.That(timeData?.dateTime, Is.Not.Null.And.Not.Empty, "dateTime should not be null or empty.");
            Assert.That(timeData?.timeZone, Is.EqualTo("Europe/Vilnius"), "timeZone should match the expected value.");
        }

        // Helper class for deserializing the API response
        private class TimeApiResponse
        {
            public string dateTime { get; set; } = string.Empty; // Initialized to avoid nullable warnings
            public string timeZone { get; set; } = string.Empty; // Initialized to avoid nullable warnings
        }
    }
}
