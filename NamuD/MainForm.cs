using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamuD.Library;

namespace NamuD
{
    public partial class MainForm : Form
    {
        private Class1 dataHandler;
        private List<TaskModel> tasks;

        private static readonly HttpClient httpClient = new HttpClient(); 
        private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Vilnius";

        // UI elements
        private Label apiTimeLabel;
        private Panel clockPanel;
        private System.Windows.Forms.Timer apiTimeTimer;
        private System.Windows.Forms.Timer clockUpdateTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDirectory = Path.Combine(documentsPath, "TaskManager");

            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            string jsonFilePath = Path.Combine(appDirectory, "tasks.json");

            dataHandler = new Class1(jsonFilePath);
            tasks = dataHandler.LoadTasks();

            StartApiTimeTimer();
            StartClockTimer();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Task Management System";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            var leftTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var taskManagementButton = new Button
            {
                Text = "Manage Tasks",
                AutoSize = true,
                Anchor = AnchorStyles.None
            };
            taskManagementButton.Click += (sender, e) =>
            {
                var taskManagementForm = new TaskManagementForm(dataHandler);
                taskManagementForm.ShowDialog();
            };

            var priorityStatusButton = new Button
            {
                Text = "Priority Status",
                AutoSize = true,
                Anchor = AnchorStyles.None
            };
            priorityStatusButton.Click += (sender, e) =>
            {
                var priorityStatusForm = new PriorityStatusForm(dataHandler);
                priorityStatusForm.ShowDialog();
            };

            leftTable.Controls.Add(taskManagementButton, 0, 1);
            leftTable.Controls.Add(priorityStatusButton, 0, 2);

            var rightTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            clockPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            clockPanel.Paint += ClockPanel_Paint;

            apiTimeLabel = new Label
            {
                Text = "Fetching time...",
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            rightTable.Controls.Add(clockPanel, 0, 0);
            rightTable.Controls.Add(apiTimeLabel, 0, 1);

            mainTable.Controls.Add(leftTable, 0, 0);
            mainTable.Controls.Add(rightTable, 1, 0);

            this.Controls.Add(mainTable);
        }

        private async void StartApiTimeTimer()
        {
            apiTimeTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            apiTimeTimer.Tick += async (sender, e) => await UpdateApiTimeLabel();
            apiTimeTimer.Start();
        }

        private async Task UpdateApiTimeLabel()
        {
            try
            {
                var response = await httpClient.GetStringAsync(TimeApiUrl);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty("dateTime", out var dateTimeProp))
                {
                    var dateTimeString = dateTimeProp.GetString();
                    if (DateTime.TryParse(dateTimeString, out var apiTime))
                    {
                        apiTimeLabel.Text = "Current Time (Vilnius): " + apiTime.ToString("u");
                    }
                }
            }
            catch
            {
                apiTimeLabel.Text = "Failed to fetch time from API.";
            }
        }

        private void StartClockTimer()
        {
            clockUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            clockUpdateTimer.Tick += (sender, e) => clockPanel.Invalidate();
            clockUpdateTimer.Start();
        }

        private void ClockPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = clockPanel.ClientRectangle;
            int size = Math.Min(rect.Width, rect.Height);
            var center = new Point(rect.Width / 2, rect.Height / 2);

            using (var faceBrush = new SolidBrush(Color.White))
            using (var facePen = new Pen(Color.Black, 2))
            {
                e.Graphics.FillEllipse(faceBrush, center.X - size / 2, center.Y - size / 2, size, size);
                e.Graphics.DrawEllipse(facePen, center.X - size / 2, center.Y - size / 2, size, size);
            }

            DateTime now = DateTime.Now;
            double hourAngle = (now.Hour % 12 + now.Minute / 60.0) * Math.PI * 2 / 12;
            double minuteAngle = (now.Minute + now.Second / 60.0) * Math.PI * 2 / 60;
            double secondAngle = now.Second * Math.PI * 2 / 60;

            int hourLength = size / 3;
            int hourX = center.X + (int)(hourLength * Math.Sin(hourAngle));
            int hourY = center.Y - (int)(hourLength * Math.Cos(hourAngle));

            int minuteLength = size / 2 - 20;
            int minuteX = center.X + (int)(minuteLength * Math.Sin(minuteAngle));
            int minuteY = center.Y - (int)(minuteLength * Math.Cos(minuteAngle));

            int secondLength = size / 2 - 15;
            int secondX = center.X + (int)(secondLength * Math.Sin(secondAngle));
            int secondY = center.Y - (int)(secondLength * Math.Cos(secondAngle));

            using (var hourPen = new Pen(Color.Black, 4)) //size: 4px
            {
                e.Graphics.DrawLine(hourPen, center.X, center.Y, hourX, hourY);
            }

            using (var minutePen = new Pen(Color.Black, 2))//size: 2px
            {
                e.Graphics.DrawLine(minutePen, center.X, center.Y, minuteX, minuteY);
            }

            using (var secondPen = new Pen(Color.Red, 1))//size: 1px
            {
                e.Graphics.DrawLine(secondPen, center.X, center.Y, secondX, secondY);
            }
        }
    }
}
