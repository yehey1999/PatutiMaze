using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PatutiMaze
{
    public partial class MainPage : ContentPage
    {
        SKBitmap patuti = null, box, steelbox;
        int boxX = 0, boxY = 0, patutiX = 0, patutiY = 0, targetX = 0, targetY = 0, direction = 0;
        bool[,] mat = new bool[,]{
            {false,true ,false,false,false,false,false,false,false,false},
            {false,true , true,false,false,false,false,false,false,false},
            {false,false, true,false,false,false,false,false,false,false},
            {false,false, true,false,false,false,false,false,false,false},
            {false,false, true, true,false,false,false,false,false,false},
            {false,false,false, true,false,false,false,false,false,false},
            {false,false,false, true, true,false,false,false,false,false},
            {false,false,false,false, true, true,false,false,false,false},
            {false,false,false,false,false, true,false,false,false,false},
            {false,false,false,false,false, true,false,false,false,false}
        };
        Stack<string> path = new Stack<string>(), allpath = new Stack<string>();

        DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;

        bool patutiMoving = false, tocheck = false;

        [Obsolete]
        public MainPage()
        {
            InitializeComponent();
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("PatutiM.idle1.png"))
            {
                patuti = SKBitmap.Decode(stream);
                var dstInfo = new SKImageInfo((int)(mainDisplayInfo.Width / 12),
                    (int)(patuti.Height * (mainDisplayInfo.Width / 12)) / patuti.Width);

                patuti = patuti.Resize(dstInfo, SKBitmapResizeMethod.Hamming);
                targetX = patutiX = patuti.Width;
            }
            using (Stream stream = assembly.GetManifestResourceStream("PatutiM.box1.png"))
            {
                box = SKBitmap.Decode(stream);
                var dstInfo = new SKImageInfo((int)(mainDisplayInfo.Width / 12),
                    (int)(box.Height * (mainDisplayInfo.Width / 12)) / box.Width);

                box = box.Resize(dstInfo, SKBitmapResizeMethod.Hamming);
            }

            using (Stream stream = assembly.GetManifestResourceStream("PatutiM.steelbox.png"))
            {
                steelbox = SKBitmap.Decode(stream);
                var dstInfo = new SKImageInfo(box.Width, box.Height);

                steelbox = steelbox.Resize(dstInfo, SKBitmapResizeMethod.Hamming);
            }

            Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
            {
                canvasView.InvalidateSurface();
                return true;
            });

        }
        private void Controls_Clicked(object sender, EventArgs e)
        {
            if (patutiMoving) return;
            Button button1 = (Button)sender;
            string commandParameter = button1.CommandParameter.ToString();
            switch (commandParameter)
            {
                case "1":
                    direction = 1;
                    patutiMoving = true;
                    targetY = patutiY - patuti.Width; // using the width because box's height is patuti's width..
                    break;
                case "2":
                    direction = 2;
                    patutiMoving = true;
                    targetY = patutiY + patuti.Width; // using the width because box's height is patuti's width..
                    break;
                case "3":
                    if (patutiX <= patuti.Width)
                        return;
                    direction = 3;
                    patutiMoving = true;
                    targetX = patutiX - patuti.Width;
                    break;
                case "4":
                    if (patutiX >= patuti.Width * 10)
                        return;
                    direction = 4;
                    patutiMoving = true;
                    targetX = patutiX + patuti.Width;
                    break;
            }
        }

        private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();

            canvas.DrawBitmap(steelbox, 0, 0);
            canvas.DrawBitmap(steelbox, 11 * box.Width, 0);
            for (int i = 1; i < mainDisplayInfo.Height / steelbox.Height; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if ((i == 11 || i == 12) && j > 0 && j < 11)
                        continue;
                    if (allpath.Contains((j * box.Width) + ":" + (i * box.Width)) && path.Contains((j * box.Width) + ":" + (i * box.Width)))
                        continue;
                    canvas.DrawBitmap(steelbox, j * steelbox.Width, i * steelbox.Height);
                }
            }
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    if (allpath.Contains((j * box.Width) + ":" + (i * box.Width)))
                        continue;
                    canvas.DrawBitmap(box, j * box.Width, i * box.Height);
                }
            }


            canvas.DrawBitmap(patuti, patutiX, patutiY);

            if (patutiMoving)
            {
                switch (direction)
                {
                    case 1: // up
                        if (patutiY > targetY)
                            patutiY -= 2;
                        else
                        {
                            patutiMoving = false;
                            tocheck = true;
                        }
                        break;
                    case 2: // down
                        if (patutiY < targetY)
                            patutiY += 2;
                        else
                        {
                            patutiMoving = false;
                            tocheck = true;
                        }
                        break;
                    case 3: // left
                        if (patutiX > targetX)
                            patutiX -= 2;
                        else
                        {
                            patutiMoving = false;
                            tocheck = true;
                        }
                        break;
                    case 4: // right
                        if (patutiX < targetX)
                            patutiX += 2;
                        else
                        {
                            patutiMoving = false;
                            tocheck = true;
                        }
                        break;
                }
            }
            else
            {
                // check if valid grid...
                // box located at 1,1 to 10,10 grid. normalization needed.
                if (tocheck && patutiY > 1)
                {
                    allpath.Push(patutiX + ":" + patutiY);
                    if (mat[(int)(patutiY / patuti.Width) - 1, (int)(patutiX / patuti.Width) - 1])
                    {
                        path.Push(patutiX + ":" + patutiY);
                    }
                    tocheck = false;
                }
            }

        }




    }
}
