using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NamuD.Library;

namespace NamuD
{
    public partial class MainForm : Form
    {
        private Class1 dataHandler;
        private List<TaskModel> tasks;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();

            // Initialize data handler and load tasks
            dataHandler = new Class1("tasks.json");
            tasks = dataHandler.LoadTasks();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Task Management System";
            this.Size = new System.Drawing.Size(400, 300);

            // Task Management Button
            var taskManagementButton = new Button
            {
                Text = "Manage Tasks",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 30)
            };
            taskManagementButton.Click += (sender, e) =>
            {
                var taskManagementForm = new TaskManagementForm();
                taskManagementForm.ShowDialog();
            };
            this.Controls.Add(taskManagementButton);

            // Priority & Status Button
            var priorityStatusButton = new Button
            {
                Text = "Priority & Status",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(150, 30)
            };
            priorityStatusButton.Click += (sender, e) =>
            {
                var priorityStatusForm = new PriorityStatusForm(tasks, dataHandler); // Pass tasks and dataHandler
                priorityStatusForm.ShowDialog();
            };
            this.Controls.Add(priorityStatusButton);
        }
    }
}