using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NamuD.Library;

namespace NamuD
{
    public partial class PriorityStatusForm : Form
    {
        private List<TaskModel> tasks;
        private Class1 dataHandler;

        private ComboBox taskComboBox;
        private ComboBox priorityComboBox;
        private ComboBox statusComboBox;

        public PriorityStatusForm(List<TaskModel> tasks, Class1 dataHandler)
        {
            this.tasks = tasks;
            this.dataHandler = dataHandler;

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Priority & Status Management";
            this.Size = new System.Drawing.Size(400, 300);

            // "Select Task" Label
            var selectTaskLabel = new Label
            {
                Text = "Select Task",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(selectTaskLabel);

            // Task ComboBox
            taskComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(350, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            taskComboBox.Items.AddRange(tasks.Select(t => t.TaskName).ToArray());
            this.Controls.Add(taskComboBox);

            // Priority Label
            var priorityLabel = new Label
            {
                Text = "Select Priority",
                Location = new System.Drawing.Point(20, 90),
                AutoSize = true
            };
            this.Controls.Add(priorityLabel);

            // Priority ComboBox
            priorityComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 120),
                Size = new System.Drawing.Size(350, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            priorityComboBox.Items.AddRange(new[] { "High", "Medium", "Low" });
            this.Controls.Add(priorityComboBox);

            // Status Label
            var statusLabel = new Label
            {
                Text = "Select Status",
                Location = new System.Drawing.Point(20, 160),
                AutoSize = true
            };
            this.Controls.Add(statusLabel);

            // Status ComboBox
            statusComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 190),
                Size = new System.Drawing.Size(350, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusComboBox.Items.AddRange(new[] { "Not Started", "In Progress", "Completed" });
            this.Controls.Add(statusComboBox);

            // Save Changes Button
            var saveButton = new Button
            {
                Text = "Save Changes",
                Location = new System.Drawing.Point(20, 230),
                Size = new System.Drawing.Size(120, 30)
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (taskComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a task to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTask = tasks[taskComboBox.SelectedIndex];
            if (priorityComboBox.SelectedIndex != -1)
            {
                selectedTask.Priority = priorityComboBox.SelectedItem.ToString();
            }
            if (statusComboBox.SelectedIndex != -1)
            {
                selectedTask.Status = statusComboBox.SelectedItem.ToString();

                // If "Completed" is selected, mark task and its checklist as completed
                if (selectedTask.Status == "Completed")
                {
                    selectedTask.IsCompleted = true;
                    foreach (var item in selectedTask.Checklist)
                    {
                        item.IsCompleted = true;
                    }
                }
            }

            dataHandler.SaveTasks(tasks);

            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
