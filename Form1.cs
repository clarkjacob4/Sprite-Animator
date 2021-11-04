using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

//cSprite needs more than one image, switched by cState
//Dictionary for cSprite to reference using string
//Incorporate alpha and color mapping

//Model rendering engine similar to COC

namespace WindowsFormsApp2
{
    public class cState
    {
        public float stSpeed = 30.0f; //Sprite frames per second
        public List<int> spFrameIndexes = new List<int>();
        private cSprite cParentSprite;
        public string stName; //Used for checking
        public const int AddFrames_HORIZONTAL = 0;
        public const int AddFrames_VERTICAL = 1;
        
        public cState(cSprite tParentSprite, string stName, float stSpeed, int stFrameOffsetX, int stFrameOffsetY, int stFrameLength, int stAddType)
        {
            //bAnimated = (stSpeed > 0) && (stFrameLength > 1); //If animation speed is 0 or frame count is 1, disable animation
            cParentSprite = tParentSprite;
            this.stName = stName;
            this.stSpeed = stSpeed;
            switch (stAddType)
            {
                case AddFrames_HORIZONTAL: AddFramesHorizontal(stFrameOffsetX, stFrameOffsetY, stFrameLength); break;
                case AddFrames_VERTICAL: AddFramesVertical(stFrameOffsetX, stFrameOffsetY, stFrameLength); break;
            }
        }
        public void AddFramesHorizontal(int stFrameOffsetX, int stFrameOffsetY, int stFrameLength) //offsets starting at 1
        {
            int iFramesHoriz = cParentSprite.iFramesHoriz;
            int iFramesVert = cParentSprite.iFramesVert;
            int remainder;
            Console.Write("AddFramesHorizontal debug: (" + stFrameOffsetX.ToString() + ", " + stFrameOffsetY.ToString() + ")");
            stFrameOffsetY += Math.DivRem(stFrameOffsetX - 1, iFramesHoriz, out remainder);
            stFrameOffsetX = remainder + 1;
            Console.Write(" to (" + stFrameOffsetX.ToString() + ", " + stFrameOffsetY.ToString() + ")");
            int iStarting = stFrameOffsetX + (stFrameOffsetY - 1) * iFramesHoriz;
            Console.WriteLine(", starting: " + iStarting.ToString() + ", length:" + stFrameLength.ToString());
            if ((iStarting + stFrameLength - 1) <= (iFramesHoriz * iFramesVert))
            {
                Console.Write("Frame");
                for (int tX = 0; tX < stFrameLength; tX++)
                {
                    int myFrame = iStarting + tX - 1;
                    spFrameIndexes.Add(myFrame);
                    Console.Write(", " + myFrame.ToString());
                }
                Console.WriteLine("");
            } else Console.WriteLine("AddFramesHorizontal exception, total length exceeds spritesheet");
        }
        public void AddFramesVertical(int stFrameOffsetX, int stFrameOffsetY, int stFrameLength) //offsets starting at 1
        {
            int iFramesHoriz = cParentSprite.iFramesHoriz;
            int iFramesVert = cParentSprite.iFramesVert;
            int remainder;
            Console.Write("AddFramesVertical debug: (" + stFrameOffsetX.ToString() + ", " + stFrameOffsetY.ToString() + ")");
            stFrameOffsetX += Math.DivRem(stFrameOffsetY - 1, iFramesVert, out remainder);
            stFrameOffsetY = remainder + 1;
            Console.Write(" to (" + stFrameOffsetX.ToString() + ", " + stFrameOffsetY.ToString() + ")");
            int iStarting = stFrameOffsetX + (stFrameOffsetY - 1) * iFramesHoriz;
            Console.Write(", starting: " + iStarting.ToString() + ", length:" + stFrameLength.ToString());
            int stFrameLengthX = stFrameOffsetX + Math.DivRem(stFrameOffsetY - 1 + stFrameLength - 1, iFramesVert, out remainder);
            int stFrameLengthY = stFrameOffsetY + remainder;
            Console.WriteLine(", ending: (" + stFrameLengthX.ToString() + ", " + stFrameLengthY.ToString() + ")");
            if (stFrameLengthX <= iFramesHoriz)
            {
                Console.Write("Frame");
                for (int tX = 0; tX < stFrameLength; tX++)
                {
                    stFrameLengthX = stFrameOffsetX + Math.DivRem(stFrameOffsetY - 1 + tX, iFramesVert, out remainder);
                    stFrameLengthY = stFrameOffsetY + remainder;
                    int myFrame = stFrameLengthX + (stFrameLengthY - 1) * iFramesHoriz - 1;
                    spFrameIndexes.Add(myFrame);
                    Console.Write(", " + myFrame.ToString() + " (" + stFrameLengthX.ToString() + ", " + stFrameLengthY.ToString() + ")");
                }
                Console.WriteLine("");
            } else Console.WriteLine("AddFramesVertical exception, total length exceeds spritesheet");
        }
        public void setAnimationTime(float fSeconds) //Change state animation speed to x seconds
        {
            stSpeed = spFrameIndexes.Count / fSeconds;
        }
    }

    public class cSprite
    {
        public cTimer cParentTimer; //Parent timer for getting fpsTarget
        private Image spImage; //Spritesheet
        public Dictionary<string, cState> spStates = new Dictionary<string, cState>(); //List of states

        public int iFramesHoriz; //Number of horizontal frames
        public int iFramesVert; //Number of vertical frames
        public Size spFrameSize = new Size(64, 64); //Spritesheet split size
        public List<Point> spFramePositions = new List<Point>(); //Horizontal based frame positions for cState
        
        public cSprite(cTimer tTimer, string tFile, int iFramesHoriz, int iFramesVert, int? frameWidth = null, int? frameHeight = null)
        {
            this.cParentTimer = tTimer;
            spImage = Image.FromFile(tFile);
            this.iFramesHoriz = iFramesHoriz;
            this.iFramesVert = iFramesVert;

            if (frameWidth == null || frameHeight == null)
            {
                spFrameSize.Width = spImage.Width / iFramesHoriz;
                spFrameSize.Height = spImage.Height / iFramesVert;
            }
            else
            {
                spFrameSize.Width = (int)frameWidth;
                spFrameSize.Height = (int)frameHeight;
            }

            for (int tY = 0; tY < iFramesVert; tY++) 
            {
                for (int tX = 0; tX < iFramesHoriz; tX++)
                {
                    spFramePositions.Add(new Point(tX * spFrameSize.Width, tY * spFrameSize.Height));
                }
            }

            /*Point spFrameOffset     = new Point(0, 0); //Offset from top left
            Point spFrameSeparation = new Point(0, 0); //Separation between each frame
            int tWidth  = spFrameSize.Width  + spFrameSeparation.X;
            int tHeight = spFrameSize.Height + spFrameSeparation.Y;
            int tX1 = spFrameOffset.X - spFrameSeparation.X;
            int tY1 = spFrameOffset.Y - spFrameSeparation.Y;
            for (int tY0 = 0; tY0 < iFramesVert;  tY0++) 
            for (int tX0 = 0; tX0 < iFramesHoriz; tX0++)
                spFramePositions.Add(new Point(tX1 + tX0 * tWidth, tY1 + tY0 * tHeight));*/
        }
        public void sStateAdd(string stName, float stSpeed, int stFrameOffsetX, int stFrameOffsetY, int stFrameLength, int stAddType = 0)
        {
            spStates[stName] = new cState(this, stName, stSpeed, stFrameOffsetX, stFrameOffsetY, stFrameLength, stAddType);
        }
        public void draw(PaintEventArgs e, Rectangle spDrawBoundsTL, int stFrameIndex, ImageAttributes imageAttr)
        {
            e.Graphics.DrawImage(spImage, spDrawBoundsTL, spFramePositions[stFrameIndex].X, spFramePositions[stFrameIndex].Y, spFrameSize.Width, spFrameSize.Height, GraphicsUnit.Pixel, imageAttr);
        }
    }

    public class cTimer
    {
        private Stopwatch swStopwatch = new Stopwatch();
        private long swTimePrevious; //Previous step time
        private long swDeltaTime; //Time since last step
        public Timer swTimer = new Timer(); //Step every frame
        public float swDeltaFactor = 1.0f; //DeltaTime / FPS
        public float swTotalFrames = 0;

        //FPS Calculation
        public int fpsTarget = 60; //Max 64 fps per System.Timers.Timer limitation
        public float fpsActual;
        private Queue<long> fpsSamples = new Queue<long>();
        private int fpsPeriod = 1000; //Interval
        private long fpsCount;

        public cTimer(EventHandler myEvent)
        {
            swTimer.Interval = (int)(1000.0f / (float)fpsTarget); //62.5 fps
            swTimer.Tick += myEvent;
            swTimer.Start();
            swStopwatch.Start();
        }
        public long update()
        {
            long swTimeCurrent = swStopwatch.ElapsedMilliseconds; //Never recall this
            swDeltaTime = swTimeCurrent - swTimePrevious;
            swTimePrevious = swTimeCurrent;
            swDeltaFactor = swDeltaTime * fpsTarget * 0.001f; //Number of seconds passed
            
            fpsCount += swDeltaTime;
            fpsSamples.Enqueue(swDeltaTime);
            while (fpsCount >= fpsPeriod)
                fpsCount -= fpsSamples.Dequeue();
            fpsActual = 1000.0f * fpsSamples.Count / fpsCount;

            swTotalFrames += swDeltaFactor;
            swTimer.Interval = Math.Max((int)((float)swTimer.Interval / swDeltaFactor),2); //FPS matching

            return swTimeCurrent;
        }
    }
    
    public class cObject
    {
        public cObjectController cParentController; //Parent controller
        public cTimer cParentTimer; //Parent timer

        public cSprite mySprite; //current sprite, may be chosen from list
        private int iDepth = 0; //No write access, use updatePosition instead
        public int iDepthOffset = 0; //Configurable anywhere

        public cState spCurrentState;
        private Size spFrameResize = new Size(64, 64); //Unscaled draw size
        public Rectangle spDrawBoundsTL = new Rectangle(0, 0, 64, 64);
        private Point spDrawBoundsBR = new Point(0, 0); //TL + WH, for quicker rendering on zoom change
        private Point spLocation = new Point(0, 0); //Actual location of sprite
        private Point spLocationOffset = new Point(0, 0); //Offset from actual location to spDrawBoundsTL

        public const int StateChange_ZERO = 0; //Start the next state from the very beginning
        public const int StateChange_FLOAT = 1; //Start the next state from exactly where the previous state left off at, including being in the middle of a frame
        public const int StateChange_INTEGER = 2; //Start the next state from the beginning of the frame where the previous state left off at
        public const int StateChange_REMAINDER = 3; //Start the next state from the very beginning, but don't reset the time until the next frame
        public const int StateChange_FLOAT_RESCALE = 4; //Start the next state at the same percentage where the previous state left off at, including being in the middle of a frame
        public const int StateChange_INTEGER_RESCALE = 5; //Start the next state at the beginning of the frame at the same percentage where the previous state left off at

        public bool bAnimated = false;
        public bool bAnimationDone = false;
        public float stFrameIndexFloat;
        public int stFrameIndex; //Sprite frame index
        public float stSpeedFactor; //
        ImageAttributes imageAttr = new ImageAttributes();

        public void updateState(float swDeltaFactor)
        {
            if (bAnimated)
            {
                int stFrameLength = spCurrentState.spFrameIndexes.Count;
                stFrameIndexFloat += swDeltaFactor * stSpeedFactor;
                if (stFrameIndexFloat >= stFrameLength)
                {
                    stFrameIndexFloat -= stFrameLength;
                    bAnimationDone = true;
                }
                stFrameIndex = spCurrentState.spFrameIndexes[(int)stFrameIndexFloat];
            }
            else
            {
                bAnimationDone = true;
            }
        }
        public void updateSpeedFactor()
        {
            stSpeedFactor = spCurrentState.stSpeed / (float)cParentTimer.fpsTarget;
        }

        public cObject(cTimer tTimer, string tFile, int iFramesHoriz, int iFramesVert, int? frameWidth = null, int? frameHeight = null)
        {
            cParentTimer = tTimer;
            mySprite = new cSprite(tTimer, tFile, iFramesHoriz, iFramesVert, frameWidth, frameHeight);
            init();
        }
        public cObject(cTimer tTimer, cSprite tSprite)
        {
            cParentTimer = tTimer;
            mySprite = tSprite;
            //if (spCurrentState == null) spCurrentState = mySprite.spStates[stName];
            init();
        }
        public void init()
        {
            bAnimated = true;
            spFrameResize = mySprite.spFrameSize;
            spLocationOffset.X = -(spFrameResize.Width / 2); //Default
            spLocationOffset.Y = -spFrameResize.Height; //Default
            updateDrawBoundsTL(0, 0, 1.0f); //Default position

            imageAttr.SetColorMatrix(new ColorMatrix(new float[][] { 
               new float[] {1, 0, 0, 0, 0},  // red scaling factor
               new float[] {0, 1, 0, 0, 0},  // green scaling factor
               new float[] {0, 0, 1, 0, 0},  // blue scaling factor
               new float[] {0, 0, 0, 0.5f, 0},  // alpha scaling factor
               new float[] {0, 0, 0, 0, 1}}),ColorMatrixFlag.Default,ColorAdjustType.Bitmap);

        }
        public void draw(PaintEventArgs e)
        {
            mySprite.draw(e, spDrawBoundsTL, stFrameIndex, imageAttr);
        }
        public void sStateChange(string stName, int stType = 0)
        {
            if (mySprite.spStates.ContainsKey(stName))
            {
                int tLastIndexesCount = spCurrentState.spFrameIndexes.Count;
                spCurrentState = mySprite.spStates[stName];
                bAnimationDone = false;
                switch (stType)
                {
                    case StateChange_ZERO: stFrameIndexFloat = 0.0f; break;
                    case StateChange_FLOAT: break;
                    case StateChange_INTEGER: stFrameIndexFloat = (int)stFrameIndexFloat; break;
                    case StateChange_REMAINDER: stFrameIndexFloat = stFrameIndexFloat - (int)stFrameIndexFloat; break;
                    case StateChange_FLOAT_RESCALE: stFrameIndexFloat = stFrameIndexFloat * (float)spCurrentState.spFrameIndexes.Count / (float)tLastIndexesCount; break;
                    case StateChange_INTEGER_RESCALE: stFrameIndexFloat = (int)(stFrameIndexFloat * (float)spCurrentState.spFrameIndexes.Count / (float)tLastIndexesCount); break;
                }
                updateSpeedFactor();
            }
        }
        public void updateFrameResize(int tWidth, int tHeight, float zFactor) //Resize the object during runtime, incomplete
        {
            //Should resize with Position as origin
            spFrameResize.Width = tWidth;
            spFrameResize.Height = tHeight;
            //Will need to update spLocationOffset using <original scale> * <factor> = <current scale> (to avoid floating errors)
            //Similar to spDrawBoundsTL.Width = (int)(zFactor * (float)spFrameResize.Width);
            updateDrawBoundsTL(spDrawBoundsTL.X, spDrawBoundsTL.Y, zFactor);
        }

        public void updateDrawBoundsTL(int x, int y, float zFactor)
        {
            spLocation.X = x; //Unscaled
            spLocation.Y = y; //Unscaled
            //spLocationOffset.X //Unscaled
            //spLocationOffset.Y //Unscaled
            spDrawBoundsTL.X = spLocation.X + spLocationOffset.X; //Need zoom factor
            spDrawBoundsTL.Y = spLocation.Y + spLocationOffset.Y; //Need zoom factor
            spDrawBoundsTL.Width = (int)(zFactor * (float)spFrameResize.Width);
            spDrawBoundsTL.Height = (int)(zFactor * (float)spFrameResize.Height);
            spDrawBoundsBR.X = spDrawBoundsTL.X + spDrawBoundsTL.Width;  //For screen bound culling
            spDrawBoundsBR.Y = spDrawBoundsTL.Y + spDrawBoundsTL.Height; //For screen bound culling
        }
        public void initializePosition(int x0, int y0, int depth)
        {
            iDepth = depth + iDepthOffset; //Configurable from here
            cParentController.gDepthUpdate(x0, y0, iDepth, this); //Add this to draw list
        }
        public void updatePosition(int x0, int y0, int depth)
        {
            cParentController.spSpriteList[iDepth].Remove(this); //Remove this from draw list
            initializePosition(x0, y0, depth);
        }
    }

    public class cObjectController
    {
        public SortedDictionary<int, List<cObject>> spSpriteList = new SortedDictionary<int, List<cObject>>();
        private List<cObject> spObjectList = new List<cObject>();
        private Random rand = new Random();
        public cObject cPlayer;

        public void init(cTimer myTimer, int width, int height)
        {
            //cPlayer = new cObject(myTimer, @"C:\Users\jclark\Working\VS\Images\ArrowSpriteSheet.png", 4, 8);
            //cPlayer.mySprite.sStateAdd("Right", 30, 1, 1, 4, cState.AddFrames_HORIZONTAL);
            //cPlayer.mySprite.sStateAdd("Down", 30, 4, 1, 8, cState.AddFrames_VERTICAL);
            
            cPlayer = new cObject(myTimer, @"C:\Users\jclark\Working\VS\Images\TcrtQ.png", 34, 16);
            cPlayer.mySprite.sStateAdd("Right", 30, 1, 1, 34, cState.AddFrames_HORIZONTAL);
            cPlayer.mySprite.sStateAdd("Down", 30, 1, 4, 34, cState.AddFrames_HORIZONTAL);

            cPlayer.spCurrentState = cPlayer.mySprite.spStates["Right"];
            cPlayer.updateSpeedFactor();
            cPlayer.cParentController = this;
            cPlayer.initializePosition(width / 2, height / 2, height / 2);
            spObjectList.Add(cPlayer);

            cSprite mySprite2 = new cSprite(myTimer, @"C:\Users\jclark\Working\VS\Images\TcrtQ.png", 34, 16);
            mySprite2.sStateAdd("Normal", 30, 1, 1, 34, cState.AddFrames_HORIZONTAL);
            for (int i = 0; i < rand.Next(175, 200); i++)
            {
                cObject cEnemy = new cObject(myTimer, mySprite2);
                cEnemy.spCurrentState = cEnemy.mySprite.spStates["Normal"];
                cEnemy.updateSpeedFactor();
                spObjectList.Add(cEnemy);
                int iPos = rand.Next(0, height);
                gDepthUpdate(rand.Next(0, width), iPos, iPos, cEnemy);
            }
        }
        
        public string update(cTimer myTimer, Panel oCanvas, long swTimeCurrent)
        {
            Point pCursorCurrent = oCanvas.PointToClient(Cursor.Position); //Configurable from here
            if (pCursorCurrent.X >= 0 && pCursorCurrent.X <= oCanvas.Width)
            {
                if (pCursorCurrent.Y >= 0 && pCursorCurrent.Y <= oCanvas.Height) //clamping
                {
                    cPlayer.updatePosition(pCursorCurrent.X, pCursorCurrent.Y, pCursorCurrent.Y);
                }
            }

            if (cPlayer.bAnimationDone) //Manual state changes
            {
                switch (cPlayer.spCurrentState.stName)
                {
                    case "Right": cPlayer.sStateChange("Down"); break;
                    case "Down": cPlayer.sStateChange("Right"); break;
                }
            }

            foreach (cObject spObject in spObjectList) //Update all objects
            {
                spObject.updateState(myTimer.swDeltaFactor);
            }

            return debugText(myTimer, cPlayer, swTimeCurrent);
        }

        public string debugText(cTimer myTimer, cObject cPlayerObj, long swTimeCurrent)
        {
            return "Calculated Seconds: " + (myTimer.swTotalFrames / myTimer.fpsTarget).ToString()
               + "\r\nActual Seconds: " + (swTimeCurrent * 0.001f).ToString()
               + "\r\nFrame Index: " + cPlayerObj.stFrameIndex.ToString()
               + "\r\nFrame Index: (" + cPlayerObj.mySprite.spFramePositions[cPlayerObj.stFrameIndex].X.ToString() + ", "+ cPlayerObj.mySprite.spFramePositions[cPlayerObj.stFrameIndex].Y.ToString() + ")"
               + "\r\nDelta Factor: " + myTimer.swDeltaFactor.ToString()
               + "\r\nSpeed Factor: " + cPlayerObj.stSpeedFactor.ToString()
               + "\r\nActual FPS: " + myTimer.fpsActual.ToString("f0") + " / " + myTimer.fpsTarget.ToString()
               + "\r\nTimer Interval: " + myTimer.swTimer.Interval.ToString()
               + "\r\nCurrent State: " + cPlayerObj.spCurrentState.stName;
        }

        public void keyPress(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                cPlayer.sStateChange("Down", cObject.StateChange_INTEGER_RESCALE);
                Console.WriteLine("Down");
            }
            if (e.KeyCode == Keys.Right)
            {
                cPlayer.sStateChange("Right", cObject.StateChange_INTEGER_RESCALE);
                Console.WriteLine("Right");
            }
        }
        
        public void gDepthUpdate(int x0, int y0, int depth, cObject tObject)
        {
            tObject.updateDrawBoundsTL(x0, y0, 1.0f);
            if (!spSpriteList.ContainsKey(depth))
            {
                spSpriteList.Add(depth, new List<cObject>());
            }
            spSpriteList[depth].Add(tObject);
        }

        public void paint(PaintEventArgs e)
        {
            foreach(KeyValuePair<int, List<cObject>> spObjectsHoriz in spSpriteList) //Draw objects
            {
                foreach (cObject spObject in spObjectsHoriz.Value) //Depth ordered
                {
                    spObject.draw(e);
                }
            }
        }
    }

    public partial class Form1 : Form
    {
        private Panel oCanvas = null;
        private cObjectController objController = new cObjectController();
        private cTimer myTimer;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            oCanvas = Controls.Find("panel1", true).FirstOrDefault() as Panel;
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, oCanvas, new object[] { true });
            oCanvas.Invalidate();
            this.KeyDown += new KeyEventHandler(panel1_KeyDown);
            
            myTimer = new cTimer(Update);
            objController.init(myTimer, oCanvas.Width, oCanvas.Height);
        }

        private void Update(object sender, EventArgs eventArgs)
        {
            long swTimeCurrent = myTimer.update();
            label1.Text = objController.update(myTimer, oCanvas, swTimeCurrent);
            oCanvas.Invalidate();
        }

        private void panel1_KeyDown(object sender, KeyEventArgs e)
        {
            objController.keyPress(e);
        }

        private void panel1_Paint(object sender, PaintEventArgs e) //invalidate method
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            //Draw background
            objController.paint(e);
        }
    }
}
