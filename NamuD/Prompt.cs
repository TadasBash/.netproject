﻿using System.Windows.Forms;

public static class Prompt
{
    public static string ShowDialog(string text, string caption)
    {
        Form prompt = new Form
        {
            Width = 400,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };

        Label label = new Label { Left = 20, Top = 20, Text = text, AutoSize = true };
        TextBox textBox = new TextBox { Left = 20, Top = 50, Width = 350 };
        Button confirmation = new Button
        {
            Text = "OK",
            Left = 270,
            Width = 100,
            Top = 80,
            DialogResult = DialogResult.OK
        };

        confirmation.Click += (sender, e) => { prompt.Close(); };

        prompt.Controls.Add(label);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
    }
}