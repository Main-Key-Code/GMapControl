using System;
using System.Windows.Forms;

namespace MarkerTest
{
    public class MarkerNameInputForm : Form
    {
        public string MarkerName => textBox.Text;

        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;

        public MarkerNameInputForm()
        {
            this.Text = "마커 입력";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 300;
            this.Height = 130;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label label = new Label() { Text = "마커 명칭:", Left = 10, Top = 15, Width = 70 };
            textBox = new TextBox() { Left = 80, Top = 12, Width = 190 };

            okButton = new Button() { Text = "확인", Left = 80, Width = 80, Top = 50, DialogResult = DialogResult.OK };
            cancelButton = new Button() { Text = "취소", Left = 190, Width = 80, Top = 50, DialogResult = DialogResult.Cancel };

            okButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(label);
            this.Controls.Add(textBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}