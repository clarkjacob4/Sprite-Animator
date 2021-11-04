using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ZLayerAlpha
{
    public class cImage
    {
        public Size spFrameResize = new Size(48, 48);
        public Rectangle spDrawBoundsTL = new Rectangle(0, 0, 48, 48);
        public Point spDrawBoundsBR = new Point(0, 0);
        public Point spLocation = new Point(0, 0);
        public Point spLocationOffset = new Point(0, 0);

        public Image myImage;

        public cImage(Image tImage)
        {
            myImage = tImage;
            spFrameResize.Width = myImage.Width;
            spFrameResize.Height = myImage.Height;
            spLocationOffset.X = -(spFrameResize.Width / 2);
            spLocationOffset.Y = -spFrameResize.Height;
        }
        public void calculatePosition()
        {
            spDrawBoundsTL.X = spLocation.X + spLocationOffset.X;
            spDrawBoundsTL.Y = spLocation.Y + spLocationOffset.Y;
            spDrawBoundsBR.X = spDrawBoundsTL.X + spFrameResize.Width;
            spDrawBoundsBR.Y = spDrawBoundsTL.Y + spFrameResize.Height;
        }
    }

    public partial class Form1 : Form
    {
        Panel oCanvas;
        Random rand = new Random();
        bool bFoundPanel = false;
        Image myImage;
        Point myMouse = new Point();
        cImage myMouseClass;
        SortedDictionary<int, List<cImage>> spSpriteList3 = new SortedDictionary<int, List<cImage>>();
        Label namelabel = new Label();
        ImageAttributes imageAttr = new ImageAttributes();

        public Form1()
        {
            namelabel.Location = new Point(0, 0);
            namelabel.Text = "Hello";
            namelabel.AutoSize = true;
            this.Controls.Add(namelabel);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            imageAttr.SetColorMatrix(new ColorMatrix(new float[][] { 
               new float[] {1, 0, 0, 0, 0},  // red scaling factor
               new float[] {0, 1, 0, 0, 0},  // green scaling factor
               new float[] {0, 0, 1, 0, 0},  // blue scaling factor
               new float[] {0, 0, 0, 0.5f, 0},  // alpha scaling factor
               new float[] {0, 0, 0, 0, 1}}),ColorMatrixFlag.Default,ColorAdjustType.Bitmap);

            oCanvas = Controls.Find("panel1", true).FirstOrDefault() as Panel;
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, oCanvas, new object[] { true });
            oCanvas.MouseMove += new MouseEventHandler(panel1_MouseMove);
            bFoundPanel = true;
            myImage = Bitmap.FromFile(@"C:\Users\jclark\Working\VS\Images\Gradient.png");
            Image myImage2 = Bitmap.FromFile(@"C:\Users\jclark\Working\VS\Images\Gradient - Copy.png");

            myMouseClass = new cImage(myImage2);
            myMouse.Y = oCanvas.Height/2;
            gImageSet(oCanvas.Width/2, myMouse.Y, myMouseClass);

            for (int i = 0; i < rand.Next(175, 200); i++) 
            {
                gImageSet(rand.Next(0, oCanvas.Width), rand.Next(0, oCanvas.Height), new cImage(myImage));
            }

            if (bFoundPanel == true) oCanvas.Invalidate();
        }

        private void gImageSet(int x0, int y0, cImage tImage)
        {
            tImage.spLocation.X = x0;
            tImage.spLocation.Y = y0;
            tImage.calculatePosition();
            if (!spSpriteList3.ContainsKey(y0))
            {
                spSpriteList3.Add(y0, new List<cImage>());
            }
            spSpriteList3[y0].Add(tImage);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.Black);

            //int rDrawCanvasX0 = 0;   int rDrawCanvasY0 = 0;   //Screen Point to Canvas, Top Left
            //int rDrawCanvasX1 = 100; int rDrawCanvasY1 = 100; //Screen Point to Canvas, Bottom Right

            foreach(KeyValuePair<int, List<cImage>> item in spSpriteList3)
            {
                foreach (cImage item2 in item.Value)
                {
                    e.Graphics.DrawImage(item2.myImage, item2.spDrawBoundsTL.X, item2.spDrawBoundsTL.Y);
                }
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Point pCursor = oCanvas.PointToClient(Cursor.Position);
            spSpriteList3[myMouse.Y].Remove(myMouseClass);
            myMouse.X = pCursor.X;
            myMouse.Y = pCursor.Y;
            gImageSet(myMouse.X, myMouse.Y, myMouseClass);

            namelabel.Text = myMouseClass.spLocation.X.ToString();
            //Console.WriteLine(myMouseClass.spLocation.X.ToString());
            if (bFoundPanel == true) oCanvas.Invalidate();
        }
    }
}
