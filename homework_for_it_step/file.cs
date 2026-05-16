using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private List<Point> snake = new List<Point>();
        private Point food;
        private int directionX = 1;
        private int directionY = 0;
        private int score = 0;
        private bool isPaused = false;
        private Timer gameTimer = new Timer();
        private Random random = new Random();
        private int size = 20;

        public Form1()
        {
            this.Width = 600;
            this.Height = 600;
            this.Text = "Snake Game";
            this.DoubleBuffered = true;
            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.Paint += new PaintEventHandler(OnPaint);

            gameTimer.Interval = 60;
            gameTimer.Tick += new EventHandler(UpdateGame);
            
            StartGame();
        }

        private void StartGame()
        {
            snake.Clear();
            snake.Add(new Point(10, 10));
            snake.Add(new Point(9, 10));
            snake.Add(new Point(8, 10));
            directionX = 1;
            directionY = 0;
            score = 0;
            isPaused = false;
            GenerateFood();
            gameTimer.Start();
        }

        private void GenerateFood()
        {
            int maxX = (this.ClientSize.Width / size) - 1;
            int maxY = (this.ClientSize.Height / size) - 1;
            food = new Point(random.Next(0, maxX), random.Next(0, maxY));
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            if (isPaused) return;

            Point head = snake[0];
            Point newHead = new Point(head.X + directionX, head.Y + directionY);

            if (newHead.X < 0 || newHead.Y < 0 || 
                newHead.X >= this.ClientSize.Width / size || 
                newHead.Y >= this.ClientSize.Height / size)
            {
                gameTimer.Stop();
                Console.WriteLine($"game over {score}");
                StartGame();
                return;
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (snake[i] == newHead)
                {
                    gameTimer.Stop();
                    Console.WriteLine($"game over {score}");
                    StartGame();
                    return;
                }
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                score++;
                GenerateFood();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Brush snakeFillBrush = Brushes.MediumPurple;
            Pen snakeOutlinePen = new Pen(Color.DeepPink, 2);

            for (int i = 0; i < snake.Count; i++)
            {
                Rectangle rect = new Rectangle(snake[i].X * size, snake[i].Y * size, size, size);
                g.FillRectangle(snakeFillBrush, rect);
                g.DrawRectangle(snakeOutlinePen, rect);
            }

            g.FillEllipse(Brushes.Crimson, new Rectangle(food.X * size, food.Y * size, size, size));

            if (isPaused)
            {
                Font pauseFont = new Font("Arial", 24, FontStyle.Bold);
                g.DrawString("paused", pauseFont, Brushes.Black, new PointF(220, 250));
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                isPaused = !isPaused;
                this.Invalidate();
                return;
            }

            if (isPaused) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (directionX != 1) { directionX = -1; directionY = 0; }
                    break;
                case Keys.Right:
                    if (directionX != -1) { directionX = 1; directionY = 0; }
                    break;
                case Keys.Up:
                    if (directionY != 1) { directionX = 0; directionY = -1; }
                    break;
                case Keys.Down:
                    if (directionY != -1) { directionX = 0; directionY = 1; }
                    break;
            }
        }
    }
}
