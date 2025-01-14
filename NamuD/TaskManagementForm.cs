using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamuD.Library;

namespace NamuD
{
    public partial class TaskManagementForm : Form
    {
        private List<TaskModel> tasks;
        private Class1 dataHandler;
        private BindingSource taskBindingSource;
        private BindingSource checklistBindingSource;

        private DataGridView taskGridView;
        private DataGridView checklistGridView;

        private static readonly HttpClient httpClient = new HttpClient();
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Vilnius";

        public TaskManagementForm(Class1 dataHandler)
        {
            //TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            InitializeComponent();
            this.dataHandler = dataHandler;

            // Load tasks initially
            tasks = dataHandler.LoadTasks();

            // Initialize binding sources
            taskBindingSource = new BindingSource();
            checklistBindingSource = new BindingSource();

            taskBindingSource.DataSource = tasks;

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
                DataSource = taskBindingSource,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
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

            taskGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CreatedAt",
                HeaderText = "Created Date",
                Width = 150,
                DefaultCellStyle = { Format = "g" }
            });

            taskGridView.SelectionChanged += TaskGridView_SelectionChanged;
            taskGridView.CurrentCellDirtyStateChanged += TaskGridView_CurrentCellDirtyStateChanged;
            taskGridView.CellValueChanged += TaskGridView_CellValueChanged;

            this.Controls.Add(taskGridView);

            // Checklist DataGridView
            checklistGridView = new DataGridView
            {
                Location = new System.Drawing.Point(550, 20),
                Size = new System.Drawing.Size(400, 500),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                DataSource = checklistBindingSource,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
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

            checklistGridView.CurrentCellDirtyStateChanged += ChecklistGridView_CurrentCellDirtyStateChanged;
            checklistGridView.CellValueChanged += ChecklistGridView_CellValueChanged;

            this.Controls.Add(checklistGridView);

            // Add Task Button
            var addTaskButton = new Button
            {
                Text = "Add Task",
                Location = new System.Drawing.Point(20, 530),
                Size = new System.Drawing.Size(120, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            addTaskButton.Click += async (sender, e) => await AddTaskButton_Click(sender, e);
            this.Controls.Add(addTaskButton);

            // Delete Task Button
            var deleteTaskButton = new Button
            {
                Text = "Delete Task",
                Location = new System.Drawing.Point(160, 530),
                Size = new System.Drawing.Size(120, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            deleteTaskButton.Click += DeleteTaskButton_Click;
            this.Controls.Add(deleteTaskButton);

            // Add Checklist Item Button
            var addChecklistButton = new Button
            {
                Text = "Add Checklist Item",
                Location = new System.Drawing.Point(550, 530),
                Size = new System.Drawing.Size(160, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            addChecklistButton.Click += AddChecklistButton_Click;
            this.Controls.Add(addChecklistButton);

            // Delete Checklist Item Button
            var deleteChecklistButton = new Button
            {
                Text = "Delete Checklist Item",
                Location = new System.Drawing.Point(720, 530),
                Size = new System.Drawing.Size(160, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            deleteChecklistButton.Click += DeleteChecklistButton_Click;
            this.Controls.Add(deleteChecklistButton);

            // Sort by Newest Button
            var sortByNewestButton = new Button
            {
                Text = "Sort by Newest",
                Location = new System.Drawing.Point(300, 530),
                Size = new System.Drawing.Size(120, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            sortByNewestButton.Click += (sender, e) =>
            {
                tasks.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
                RefreshTasksGrid();
            };
            this.Controls.Add(sortByNewestButton);

            // Sort by Oldest Button
            var sortByOldestButton = new Button
            {
                Text = "Sort by Oldest",
                Location = new System.Drawing.Point(440, 530),
                Size = new System.Drawing.Size(120, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            sortByOldestButton.Click += (sender, e) =>
            {
                tasks.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
                RefreshTasksGrid();
            };
            this.Controls.Add(sortByOldestButton);
        }

        private void TaskGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = taskGridView.SelectedRows[0].Index;

                if (selectedIndex >= 0 && selectedIndex < tasks.Count)
                {
                    var selectedTask = tasks[selectedIndex];
                    RefreshChecklistGrid(selectedTask);
                }
                else
                {
                    checklistBindingSource.DataSource = null;
                }
            }
            else
            {
                checklistBindingSource.DataSource = null;
            }
        }

        private async Task AddTaskButton_Click(object sender, EventArgs e)
        {
            var taskName = Prompt.ShowDialog("Enter Task Name:", "Add Task");
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                var createdAt = await GetCurrentDateTimeFromApi();
                var newTask = new TaskModel
                {
                    TaskName = taskName,
                    Priority = "Medium",
                    Status = "Not Started",
                    IsCompleted = false,
                    Checklist = new List<ChecklistItem>(),
                    CreatedAt = createdAt
                };

                tasks.Add(newTask);
                dataHandler.SaveTasks(tasks);
                RefreshTasksGrid();
            }
        }

        private void DeleteTaskButton_Click(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = taskGridView.SelectedRows[0].Index;
                if (selectedIndex >= 0 && selectedIndex < tasks.Count)
                {
                    tasks.RemoveAt(selectedIndex);
                    dataHandler.SaveTasks(tasks);
                    RefreshTasksGrid();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddChecklistButton_Click(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = taskGridView.SelectedRows[0].Index;

                if (selectedIndex >= 0 && selectedIndex < tasks.Count)
                {
                    var selectedTask = tasks[selectedIndex];
                    var itemDescription = Prompt.ShowDialog("Enter Checklist Item Description:", "Add Checklist Item");
                    if (!string.IsNullOrWhiteSpace(itemDescription))
                    {
                        var newItem = new ChecklistItem { Description = itemDescription, IsCompleted = false };
                        selectedTask.Checklist.Add(newItem);

                        dataHandler.SaveTasks(tasks);
                        RefreshChecklistGrid(selectedTask);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteChecklistButton_Click(object sender, EventArgs e)
        {
            if (taskGridView.SelectedRows.Count > 0 && checklistGridView.SelectedRows.Count > 0)
            {
                var selectedTaskIndex = taskGridView.SelectedRows[0].Index;
                var checklistIndex = checklistGridView.SelectedRows[0].Index;

                if (selectedTaskIndex >= 0 && selectedTaskIndex < tasks.Count &&
                    checklistIndex >= 0 && checklistIndex < tasks[selectedTaskIndex].Checklist.Count)
                {
                    tasks[selectedTaskIndex].Checklist.RemoveAt(checklistIndex);
                    dataHandler.SaveTasks(tasks);

                    var selectedTask = tasks[selectedTaskIndex];
                    RefreshChecklistGrid(selectedTask);
                }
            }
            else
            {
                MessageBox.Show("Please select a checklist item to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshTasksGrid()
        {
            taskBindingSource.ResetBindings(false);
            taskGridView.ClearSelection();
        }

        private void RefreshChecklistGrid(TaskModel task)
        {
            checklistBindingSource.DataSource = task.Checklist;
            checklistBindingSource.ResetBindings(false);
            checklistGridView.ClearSelection();
        }

        private void TaskGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (taskGridView.IsCurrentCellDirty && taskGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                taskGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void TaskGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0 && e.RowIndex < tasks.Count)
            {
                dataHandler.SaveTasks(tasks);
            }
        }

        private void ChecklistGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (checklistGridView.IsCurrentCellDirty && checklistGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                checklistGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void ChecklistGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && taskGridView.SelectedRows.Count > 0)
            {
                dataHandler.SaveTasks(tasks);
            }
        }

        private async Task<DateTime> GetCurrentDateTimeFromApi()
        {
            try
            {
                var response = await httpClient.GetStringAsync(TimeApiUrl);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                // "dateTime": "2024-12-25T15:30:45.1234567"
                if (root.TryGetProperty("dateTime", out var dateTimeProp))
                {
                    var dateTimeString = dateTimeProp.GetString();
                    if (DateTime.TryParse(dateTimeString, out var apiTime))
                    {
                        return apiTime;
                    }
                    else
                    {
                        MessageBox.Show("Failed to parse time from API. Using local UTC time.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return DateTime.UtcNow;
                    }
                }
                else
                {
                    MessageBox.Show("API did not return expected 'dateTime' field. Using local UTC time.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching time from API: {ex.Message}\nUsing local UTC time.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return DateTime.UtcNow;
            }
        }
    }
}
