using System.Collections.Generic;

namespace NamuD.Library
{
    public class ChecklistItem
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TaskModel
    {
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; }
        public List<ChecklistItem> Checklist { get; set; } = new List<ChecklistItem>();
        public string Priority { get; set; } = "Medium"; // Default priority
        public string Status { get; set; } = "Not Started"; // Default status
    }
}