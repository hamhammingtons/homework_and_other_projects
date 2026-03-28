using System;
using System.Drawing;
using System.Windows.Forms;

public class MyForm : Form
{
    private Button myButton;

    public MyForm()
    {
        myButton = new Button();
        myButton.Text = "click me";
        myButton.Location = new Point(100, 100);

        myButton.Click += new EventHandler(MyButtonClick);

        this.Controls.Add(myButton);
    }

    private void MyButtonClick(object sender, EventArgs e)
    {
        this.BackColor = Color.Red;
    }

    static void Main()
    {
        Application.Run(new MyForm());
    }
}