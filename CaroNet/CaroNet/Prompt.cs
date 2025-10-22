using System.Windows.Forms;

namespace CaroNetClient
{
    public static class Prompt
    {
        public static string Show(string text, string caption, string defaultValue)
        {
            Form prompt = new Form();
            prompt.Width = 360;
            prompt.Height = 160;
            prompt.Text = caption;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.StartPosition = FormStartPosition.CenterParent;
            prompt.MinimizeBox = false;
            prompt.MaximizeBox = false;

            Label lbl = new Label() { Left = 12, Top = 12, Width = 320, Text = text };
            TextBox input = new TextBox() { Left = 12, Top = 40, Width = 320, Text = defaultValue };
            Button ok = new Button() { Text = "OK", Left = 176, Width = 75, Top = 72, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 257, Width = 75, Top = 72, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(input);
            prompt.Controls.Add(ok);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = ok;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? input.Text : null;
        }
    }
}
