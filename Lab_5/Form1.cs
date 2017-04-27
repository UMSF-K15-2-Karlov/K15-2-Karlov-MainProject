#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

#endregion

namespace Lab_5
{
    public sealed partial class Form1 : Form
    {
        private const int maxHeightBackground = 400;
        private const int maxWidthBackground = 360;
        private const int BALL_STEP = 10;
        private const int BALL_AMPLITUDE = 160;
        private const int BALL_Y_OFFSET = 500; 

        private Particle m_leaf;
        private Image m_img;
        private Image m_crewCut;
        private Random rndForPoint;
        private string[] m_files;
        private int currentIndexForPointX, currentIndexForPointY, currentIndexForFile;
        private int m_maxSizeFile;
        private int m_count;
        private int m_counter;

        public Form1()
        {
            InitializeComponent();
            m_count = 0;
            m_counter = 0;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer, true);
            if (initializeImages())
            {
                SetRandomPoint();
                m_leaf = new Particle
                {
                    Image = m_img,
                    Position = new PointF(currentIndexForPointX, currentIndexForPointY),
                    Velocity = new PointF(-10, 0)
                };
                if (!LoadImage(ref m_crewCut, "ball.png"))
                {
                    this.Close();
                }

                Application.Idle += new EventHandler(Application_Idle);
            }
        }
        private bool initializeImages()
        {
            string appRoot = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            m_files = System.IO.Directory.GetFiles(appRoot + @"\foliage");
            Random rnd = new Random();
            m_files = m_files.OrderBy(x => rnd.Next()).ToArray();
            m_maxSizeFile = m_files.Length;
            if(m_maxSizeFile > 1)
            {
                return true;
            }
            return false;
        }

       private void SetRandomPoint()
        {
            rndForPoint = new Random();
            currentIndexForFile = rndForPoint.Next(m_maxSizeFile);
            if (!LoadImage(ref m_img, m_files[currentIndexForFile]))
            {
                this.Close();
            }
            currentIndexForPointX = rndForPoint.Next(maxWidthBackground);
            currentIndexForPointY = rndForPoint.Next(maxHeightBackground);
            SetPoint();
        }

       private bool LoadImage(ref Image image, string fileName)
        {
            try
            {
                image = Image.FromFile(fileName);
            }
            catch (Exception e)
            {
                MessageBox.Show("Картинки не могут быть загружены!", "Ошибка");
                return false;
            }

            return true;
        }
        private void SetPoint()
        {
            if (currentIndexForPointY >= 50)
            {
                m_leaf = new Particle
                {
                    Image = m_img,
                    Position = new PointF(currentIndexForPointX, currentIndexForPointY),
                    Velocity = new PointF(-10, -25)
                };
            }
        }

        void Application_Idle(object sender, EventArgs e)
        {
            if (m_count == maxHeightBackground)
            {
                SetRandomPoint();
                m_count = 0;
            }
            m_count++;
            m_leaf.Update(1f);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(m_leaf.Image,
                                new PointF(m_leaf.Position.X - m_leaf.Size.Width / 2,
                                ClientSize.Height - (m_leaf.Position.Y + m_leaf.Size.Height / 2)));

            base.OnPaint(e);
            int x = m_counter % (2 * BALL_AMPLITUDE);
            if (x > BALL_AMPLITUDE)
            {
                x = (2 * BALL_AMPLITUDE) - x;
            }

            e.Graphics.DrawImage(m_crewCut, new Point(x, BALL_Y_OFFSET));            
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            SetRandomPoint();
        }

        private void Form1_Load(object sender, EventArgs e)
        { }

        private void timer1_Tick(object sender, EventArgs e)
        {
            m_counter += BALL_STEP;
            // форсируем перерисовку окна
            this.Refresh();
        }
    }

    public class Particle
    {
        public Image Image;
        public PointF Velocity;
        public PointF Position;

        public Size Size
        {
            get { return Image.Size; }
        }

        public const float g = 9.8f;
        public const float groundFriction = 2f;
        public const float airFriction = 0.09f;


        public void Update(float dt)
        {
            //сила тяжести
            var a = new PointF(0, -g);
            Velocity = Velocity.Add(a, dt);

            //трение об воздух
            Velocity = Velocity.Mult(airFriction);

            //приземление
            if (Position.Y - Size.Height / 2 < -25)
            {
                Velocity = Velocity.Mult(-groundFriction);
                Position = new PointF(Position.X, Position.Y);
            }
            else
                //движение
                Position = Position.Add(Velocity, dt);
        }
    }

    static class Helper
    {
        public static PointF Mult(this PointF p, float koeff)
        {
            return new PointF(p.X * koeff, p.Y * koeff);
        }

        public static PointF Add(this PointF p1, PointF p2, float koeff = 1f)
        {
            return new PointF(p1.X + p2.X * koeff, p1.Y + p2.Y * koeff);
        }
    }



}
