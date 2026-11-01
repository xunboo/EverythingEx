using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormUI.Forms
{
    public static class InputBoxDialog
    {
        public static string Show(string prompt, string title, string defaultValue = "")
        {
            using (Form form = new Form())
            {
                form.Text = title;
                form.ClientSize = new Size(380, 130);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                Label label = new Label
                {
                    Text = prompt,
                    Location = new Point(16, 16),
                    AutoSize = true
                };

                TextBox textBox = new TextBox
                {
                    Text = defaultValue,
                    Location = new Point(16, 42),
                    Width = 348
                };

                Button buttonOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(208, 85),
                    Width = 75,
                    Height = 25
                };

                Button buttonCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(289, 85),
                    Width = 75,
                    Height = 25
                };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}
