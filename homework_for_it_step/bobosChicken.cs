using System;
using System.Windows.Forms;

namespace TaskApp
{
    public partial class Form1 : Form
    {
        private Button button1;
        private Button button2;
        private Label label1;

        public Form1()
        {
            InitializeComponentsManual();
        }

        private void InitializeComponentsManual()
        {
            button1 = new Button { Text = "Button 1", Left = 50, Top = 50 };
            button2 = new Button { Text = "Show Button 1", Left = 150, Top = 50 };
            label1 = new Label { Text = "Видим", Left = 50, Top = 100, AutoSize = true };

            button1.MouseEnter += Button1_MouseEnter;
            button2.Click += Button2_Click;

            this.Controls.Add(button1);
            this.Controls.Add(button2);
            this.Controls.Add(label1);
        }

        private void Button1_MouseEnter(object sender, EventArgs e)
        {
            button1.Visible = false;
            UpdateStatus();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            button1.Visible = true;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            label1.Text = button1.Visible ? "Видим" : "Невидим";
        }
    }
}