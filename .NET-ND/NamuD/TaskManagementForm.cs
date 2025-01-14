using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NamuD.Library;

namespace NamuD
{
    public partial class TaskManagementForm : Form
    {
        private List<TaskModel> tasks;
        private DataGridView taskGridView;
        private DataGridView checklistGridView;
        private Class1 dataHandler;

        public TaskManagementForm()
        {
            InitializeComponent();
            tasks = new List<TaskModel>();
            dataHandler = new Class1("tasks.json");
            tasks = dataHandler.LoadTasks();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Task Management";
            this.Size = new System.Drawing.Size(1000, 600);

            // Task DataGridView
            taskGridView = new DataGridView
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(500, 500),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                DataSource = tasks
            };
            taskGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TaskName",
                HeaderText = "Task Name",
                Width = 200
            });
            taskGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsCompleted",
                HeaderText = "Completed",
                Width = 100
            });
            taskGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = "Priority",
                Width = 100
            });
            taskGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                Width = 100
            });
            taskGridView.CellValueChanged += TaskGridView_CellValueChanged;
            taskGridView.CurrentCellDirtyStateChanged += TaskGridView_CurrentCellDirtyStateChanged;
            taskGridView.SelectionChanged += TaskGridView_SelectionChanged;
            this.Controls.Add(taskGridView);

            // Checklist DataGridView
            checklistGridView = new DataGridView
            {
                Location = new System.Drawing.Point(550, 20),
                Size = new System.Drawing.Size(400, 500),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false
            };
            checklistGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Checklist Item",
                Width = 250
            });
            checklistGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsCompleted",
                HeaderText = "Completed",
                Width = 100
            });
            checklistGridView.CellValueChanged += ChecklistGridView_CellValueChanged;
            checklistGridView.CurrentCellDirtyStateChanged += ChecklistGridView_CurrentCellDirtyStateChanged;
            this.Controls.Add(checklistGridView);

            // Add Task Button
            var addTaskButton = new Button
            {
                Text = "Add Task",
                Location = new System.Drawing.Point(20, 530),
                Size = new System.Drawing.Size(120, 30)
            };
            addTaskButton.Click += AddTaskButton_Click;
            this.Controls.Add(addTaskButton);

            // Delete Task Button
            var deleteTaskButton = new Button
            {
                Text = "Delete Task",
                Location = new System.Drawing.Point(160, 530),
                Size = new System.Drawing.Size(120, 30)
            };
            deleteTaskButton.Click += DeleteTaskButton_Click;
            this.Controls.Add(deleteTaskButton);
        }

        private void TaskGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && taskGridView.Columns[e.ColumnIndex].DataPropertyName == "IsCompleted")
            {
                var selectedTask = tasks[e.RowIndex];
                selectedTask.IsCompleted = (bool)taskGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                if (selectedTask.IsCompleted)
                {
                    selectedTask.Status = "Completed";
                    foreach (var item in selectedTask.Checklist)
                    {
                        item.IsCompleted = true;
                    }
                }

                dataHandler.SaveTasks(tasks);
                taskGridView.Refresh();
            }
        }

        private void TaskGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (taskGridView.IsCurrentCellDirty)
            {
                taskGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void TaskGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0)
            {
                var selectedTask = tasks[taskGridView.SelectedRows[0].Index];
                checklistGridView.DataSource = null;
                checklistGridView.DataSource = selectedTask.Checklist;
            }
        }

        private void ChecklistGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && checklistGridView.Columns[e.ColumnIndex].DataPropertyName == "IsCompleted")
            {
                var selectedTask = tasks[taskGridView.SelectedRows[0].Index];
                var checklistItem = selectedTask.Checklist[e.RowIndex];
                checklistItem.IsCompleted = (bool)checklistGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                dataHandler.SaveTasks(tasks);
            }
        }

        private void ChecklistGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (checklistGridView.IsCurrentCellDirty)
            {
                checklistGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            var taskName = Prompt.ShowDialog("Enter Task Name:", "Add Task");
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                var newTask = new TaskModel
                {
                    TaskName = taskName,
                    Priority = "Medium",
                    Status = "Not Started",
                    IsCompleted = false
                };
                tasks.Add(newTask);
                taskGridView.DataSource = null;
                taskGridView.DataSource = tasks;
                dataHandler.SaveTasks(tasks);
            }
        }

        private void DeleteTaskButton_Click(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = taskGridView.SelectedRows[0].Index;
                tasks.RemoveAt(selectedIndex);
                taskGridView.DataSource = null;
                taskGridView.DataSource = tasks;
                checklistGridView.DataSource = null;
                dataHandler.SaveTasks(tasks);
            }
        }
    }
}
