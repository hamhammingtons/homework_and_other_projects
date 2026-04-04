using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormTask
{
    public partial class Form1 : Form
    {
        private TextBox txtColor;
        private TextBox txtTitle;
        private TextBox txtWidth;
        private TextBox txtHeight;
        private Button btnChange;

        public Form1()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Initial Title";
            this.Size = new Size(400, 400);

            txtColor = new TextBox { Left = 50, Top = 50, Width = 150, Text = "Yellow" };
            txtTitle = new TextBox { Left = 50, Top = 100, Width = 150, Text = "New Title" };
            txtWidth = new TextBox { Left = 50, Top = 150, Width = 150, Text = "500" };
            txtHeight = new TextBox { Left = 50, Top = 200, Width = 150, Text = "500" };

            btnChange = new Button { Left = 50, Top = 250, Text = "Изменить", Width = 100 };
            btnChange.Click += BtnChange_Click;

            this.Controls.Add(new Label { Text = "Цвет фона", Left = 50, Top = 35 });
            this.Controls.Add(txtColor);
            this.Controls.Add(new Label { Text = "Заголовок", Left = 50, Top = 85 });
            this.Controls.Add(txtTitle);
            this.Controls.Add(new Label { Text = "Ширина", Left = 50, Top = 135 });
            this.Controls.Add(txtWidth);
            this.Controls.Add(new Label { Text = "Высота", Left = 50, Top = 185 });
            this.Controls.Add(txtHeight);
            this.Controls.Add(btnChange);
        }

        private void ChangeForm(string colorName, string title, int width, int height)
        {
            this.BackColor = Color.FromName(colorName);
            this.Text = title;
            this.Width = width;
            this.Height = height;
        }

        private void BtnChange_Click(object sender, EventArgs e)
        {
            ChangeForm(
                txtColor.Text,
                txtTitle.Text,
                int.Parse(txtWidth.Text),
                int.Parse(txtHeight.Text)
            );
        }
    }
}