using System;
using System.Drawing;
using System.Windows.Forms;

namespace HelenkaApp
{
   class Images
   {
      private string samplePath;
      private Bitmap sample;
      public Point lastPosition;
      public Point startSearch;
      public Point endSearch;

      /// <summary>
      /// constructor
      /// </summary>
      /// <param name="samplePath">path to sample image</param>
      public Images(string samplePath)
      {
         this.samplePath = samplePath;
         sample = (Bitmap)Image.FromFile(samplePath);
         lastPosition = new Point(-1, -1);
      }

      /// <summary>
      /// Check if sample images is appearing on exact screenshot.
      /// </summary>
      /// <param name="startSearch">start searching point on display screen</param>
      /// <param name="endSearch">end searching point on display screen</param>
      /// <returns>True: sample image is appearing on exact screenshot, False: sample image is not appearing on exact screenshot.</returns>
      public bool ImageCheck(Point startSearch, Point endSearch)
      {
         try
         {
            Bitmap screen = ExactScreenshot(startSearch, endSearch);
            if (lastPosition.X == -1)
            {
               for (int x = 0; x < screen.Width - (sample.Width - 1); x++)
               {
                  for (int y = 0; y < screen.Height - (sample.Height - 1); y++)
                  {
                     if (sample.GetPixel(0, 0) == screen.GetPixel(x, y) && IsInnerImage(x, y, sample, screen))
                     {
                        lastPosition = new Point(x, y);
                        return true; //sample is on screen
                     }
                  }
               }
               return false;
            }
            else //save point of sample image on exact screenshot is ready
            {
               if (!(sample.GetPixel(0, 0) == screen.GetPixel(lastPosition.X, lastPosition.Y) && IsInnerImage(lastPosition.X, lastPosition.Y, sample, screen)))
               {
                  lastPosition = new Point(-1, -1);
                  ImageCheck(startSearch, endSearch); //go again and check whole screen - if not appears nowhere - return false;
               }
               return true;
            }
         }
         catch (Exception e)
         {
            Console.WriteLine($"Images.ImageCheck error: {e.GetType()}");
            Program.ShowConsoleWindow();
            Program.EndProcess = true;
            return false;
         }
      }

      /// <summary>
      /// Check if sample image is appearing on exact screenshot on fixed position.
      /// </summary>
      /// <param name="start">start searching point on display screen</param>
      /// <param name="end">end searching point on display screen</param>
      /// <returns>True: sample image is appearing on exact screenshot on fixed position, False: sample image is not appearing on exact screenshot on fixed position.<</returns>
      public bool ExactImageCheck(Point start, Point end)
      {
         try
         {
            Bitmap screen = ExactScreenshot(start, end);
            if (sample.GetPixel(0, 0) == screen.GetPixel(0, 0) && IsInnerImage(0, 0, sample, screen))
            {
               return true; //sample is on screen
            }
         }
         catch (Exception e)
         {
            Console.WriteLine($"Images.ExactImageCheck error: {e.GetType()}");
            Program.ShowConsoleWindow();
            Program.EndProcess = true;
            return false;
         }
         return false;
      }

      /// <summary>
      /// Check if sample is inner image of screen, when first pixel is matched
      /// </summary>
      private bool IsInnerImage(int left, int top, Bitmap sample, Bitmap screen)
      {
         for (int x = 0; x < sample.Width; x++)
         {
            for (int y = 0; y < sample.Height; y++)
            {
               if (sample.GetPixel(x, y) != screen.GetPixel(left + x, top + y))
               {
                  return false; //sample is not inner image of screen
               }
            }
         }
         return true; //sample is inner image of screen
      }

      /// <summary>
      /// Get screenshot of exact area on screen.
      /// </summary>
      /// <param name="startScreen">start point of screen (left, up)</param>
      /// <param name="endScreen">end point of screen (right, down)</param>
      /// <returns>screenshot bitmap</returns>
      private Bitmap ExactScreenshot(Point startScreen, Point endScreen)
      {
         Size size = new Size(endScreen.X - startScreen.X, endScreen.Y - startScreen.Y);
         Bitmap screenshot = new Bitmap(size.Width, size.Height);
         Graphics gfx = Graphics.FromImage(screenshot);
         gfx.CopyFromScreen(startScreen.X, startScreen.Y, 0, 0, size);
         return screenshot;
      }

      /// <summary>
      /// Test if uptasia needs logon and process it.
      /// </summary>
      public void TestLogon(Point startSearch, Point endSearch)
      {
         if (ImageCheck(startSearch, endSearch))
         {
            Program.NanoSleep(420);
            Point lastMouseCursorPos = Cursor.Position;
            Program.SendLeftMouse(new Point(677, 96));
            Program.SendLeftMouse(new Point(677, 96));
            Program.SendText("username");
            Program.SendLeftMouse(new Point(866, 93));
            Program.SendLeftMouse(new Point(866, 93));
            Program.SendText("password");
            Program.SendLeftMouse(new Point(1090, 90));
            Program.MicroSleep();
            Cursor.Position = lastMouseCursorPos;
         }
      }

   }
}