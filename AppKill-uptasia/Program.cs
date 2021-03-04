using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Timers;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

namespace HelenkaApp
{
   class Program
   { 
      private static string[] processToKill; //processes to Process.Kill
      public static Size screenSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height); //size of display
      private static System.Timers.Timer KeyDownTimer; //timer for keycatch

      //base-shutdown recognize samples:
      private static Images desktopImage; //sample image to check for close app - desktop background
      private static Images iconImage; //icon image of process in taskbar
      //login sample:
      private static Images loginImage; //image for check if have to login

      public static bool EndProcess = false; //end of this process (exit uptasia)

      /// <summary>
      /// Main method
      /// </summary>
      /// <param name="args">arguments on start of program</param>
      static void Main(string[] args)
      {
         try
         {
            ShowConsoleWindow("hide"); //hide console window
            processToKill = "upjers;my little farm;unitycrash".Split(';');

            if (AnotherKillerRunning(out List<int> ID)) //check if another this process is running
            {
               if (StartedInLast15Seconds()) //exit this process, because of waiting for another process started (15 seconds after last start)
               {
                  Environment.Exit(0);
                  return;
               }
               else //kill all duplicate processes and go on this process
               {
                  foreach (int id in ID.ToList())
                  {
                     Process duplicateKiller = Process.GetProcessById(id);
                     duplicateKiller.Kill(); //kill duplicate processes
                  }
                  Process[] runningProcesses = Process.GetProcesses();
                  foreach (var runProcess in runningProcesses) //go through all running processes
                  {
                     foreach (string proc in processToKill) //get all process to kill (upjers)
                     {
                        KillMatchedProcess(runProcess, proc); //kill matched processes to kill in running processes
                     }
                  }
                  Thread.Sleep(600); //something for sure
               }
            }
            File.WriteAllText(Path.Combine(Application.StartupPath, "started.txt"), DateTime.Now.ToString()); //write datetime, when last process was started

            //desktop image sample:
            desktopImage = new Images(Path.Combine(Application.StartupPath, "blue.png"));
            desktopImage.startSearch = new Point(1900, 994);
            desktopImage.endSearch = new Point(1906, 1000);
            //uptasia icon image:
            iconImage = new Images(Path.Combine(Application.StartupPath, "uptasia_icon.png"));
            iconImage.startSearch = new Point(187, 1062);
            iconImage.endSearch = new Point(1690, 1068); //its +1 (like others) - test
                                                         //uptasia login image:
            loginImage = new Images(Path.Combine(Application.StartupPath, "login.png"));

            //start process:
            string procesPath = Path.Combine(Application.StartupPath, @"uptasia\upjers-playground2\upjers Home.exe");
            if (File.Exists(procesPath))
            { Process.Start(procesPath, "upjers://start/uptasia"); }
            else
            {
               ShowConsoleWindow();
               Console.WriteLine($"File to start process on {procesPath} is not exist!");
            }
            processToKill = "upjers;my little farm;unitycrash".Split(';');
            //wait for till app icon get ready in taskbar:
            while (!iconImage.ImageCheck(iconImage.startSearch, iconImage.endSearch)) { Thread.Sleep(500); }

            //timer for keydown catcher:
            KeyDownTimer = new System.Timers.Timer(); //for RMB to login and accept cookies
            KeyDownTimer.Interval = 25; //for keydown catch
            KeyDownTimer.Elapsed += timer_tick;
            KeyDownTimer.Enabled = true;
            //timer of while loop:
            WhileTimer(500); //'timer' on while loop, 500ms sleep - two times per second (checking if uptasia is closed - no taskbar icon, desktop pixels)

            Console.ReadKey();
         }
         catch (Exception e)
         {
            ShowConsoleWindow();
            Console.WriteLine($"Main method error: {e.GetType()}");
         }
      }

      /// <summary>
      /// Check If Another process is running.
      /// </summary>
      /// <param name="ID"></param>
      /// <returns>True: when another same this process is running. False: another this process is not running.</returns>
      private static bool AnotherKillerRunning(out List<int> ID)
      {
         Process currentProces = Process.GetCurrentProcess(); //current process
         string processName = currentProces.ProcessName; //current process name
         int processId = currentProces.Id; //current process ID
         Process[] runningProcesses = Process.GetProcesses();
         ID = new List<int>(); //list of PID of duplicates of current process
         foreach (var runningProcess in runningProcesses) //get duplicate list of this program
         {
            if (runningProcess.ProcessName == processName && runningProcess.Id != processId)
            { ID.Add(runningProcess.Id); }
            return true;
         }
         return false;
      }

      /// <summary>
      /// Check if app started in last 15 seconds.
      /// </summary>
      /// <returns>True: this process was started in last 15 seconds. False: this process was not started in last 15 seconds.</returns>
      private static bool StartedInLast15Seconds()
      {
         Thread.Sleep(250);
         string[] input = File.ReadAllText(Path.Combine(Application.StartupPath, "started.txt")).Split(' ');
         string[] date = input[0].Split('.');
         string[] time = input[1].Split(':');
         DateTime date1 = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]), Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
         DateTime date2 = DateTime.Now;
         int seconds = Convert.ToInt32((date2 - date1).TotalSeconds);
         if (seconds < 15)
         {
            return true;
         }
         return false;
      }

      /// <summary>
      /// Close program.
      /// </summary>
      private static void CloseApp()
      {
         EndProcess = true;
         Process[] runningProcesses = Process.GetProcesses(); //get all running process
         foreach (var runProcess in runningProcesses) //go through all running processes
         {
            foreach (string proc in processToKill) //get all process to kill
            {
               KillMatchedProcess(runProcess, proc); //kill matched processes to kill in running processes
            }
         }
         KeyDownTimer.Enabled = false;
         KeyDownTimer.Dispose();
         Thread.Sleep(250);
         DeleteIconAfterEnd();
         Thread.Sleep(10);
         Environment.Exit(0);
      }

      /// <summary>
      /// Kill selected process if matched running process.
      /// </summary>
      /// <param name="runProces">Running proces from all running processes</param>
      /// <param name="proc">Proces to kill</param>
      private static void KillMatchedProcess(Process runProcess, string proc)
      {
         if (Regex.IsMatch(runProcess.ProcessName, proc, RegexOptions.IgnoreCase)) //kill matched process
         { runProcess.Kill(); }
      }

      /// <summary>
      /// Delete icon from right side of taskbar (running program icons) after end of program.
      /// </summary>
      private static void DeleteIconAfterEnd()
      {
         try
         {
            Point lastCursorPosition = Cursor.Position; MicroSleep();
            Cursor.Position = new Point(1763, 1065); Thread.Sleep(8); //lasted icon in running tasks
            Cursor.Position = lastCursorPosition;
         }
         catch (Exception e)
         {
            ShowConsoleWindow();
            Console.WriteLine($"DeleteIIconAfterEnd error: {e.GetType()}");
         }
      }

      /// <summary>
      /// while timer for end of this program and all selected processes
      /// </summary>
      private static void WhileTimer(int sleep = 10)
      {
         while (!EndProcess) //this bool end while cycle when some error occurs
         {
            if (desktopImage.ExactImageCheck(desktopImage.startSearch, desktopImage.endSearch) && !iconImage.ImageCheck(iconImage.startSearch, iconImage.endSearch)) // Desktop background appears and icon is not in taskbar.
            {
               CloseApp(); //close app when met condition
               break;
            }
            Thread.Sleep(sleep);
         }
      }

      /// <summary>
      /// Timer for login and cookies accept.
      /// </summary>
      private static void timer_tick(object sender, EventArgs args)
      {
         if (GetBuffKey() == "RButton" && loginImage != null) //RMB is pressed and login image is on screen
         {
            try
            {
               loginImage.TestLogon(new Point(1050, 80), new Point(1080, 110)); //on-maximized
            }
            catch (Exception e)
            {
               ShowConsoleWindow();
               Console.WriteLine($"loginImage.TestLogon error: {e.GetType()}");
               KeyDownTimer.Enabled = false;
            }
         }
      }

      /// <summary>
      /// Write chosen text by SendKeys.SendWait(charsFromText).
      /// </summary>
      /// <param name="text">text to write</param>
      public static void SendText(string text)
      {
         Random random = new Random();
         for (int i = 0; i < text.Length; i++)
         {
            Thread.Sleep(random.Next(44, 86));
            SendKeys.SendWait(text[i].ToString());
         }
      }

      #region Keydown catcher
      [DllImport("User32.dll")]
      private static extern short GetAsyncKeyState(int key);
      /// <summary>
      /// Return catched keydown.
      /// </summary>
      /// <returns>keydown key</returns>
      private static string GetBuffKey()
      {
         foreach (Int32 i in Enum.GetValues(typeof(Keys)))
         {
            if (GetAsyncKeyState(i) == -32767)
            {
               return Enum.GetName(typeof(Keys), i);
            }
         }
         return string.Empty;
      }
      #endregion

      #region Console showing
      //hide console:
      [DllImport("kernel32.dll")]
      private static extern IntPtr GetConsoleWindow();
      [DllImport("user32.dll")]
      private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
      private const int SW_HIDE = 0;
      private const int SW_SHOW = 1;
      /// <summary>
      /// Show or hide console window.
      /// </summary>
      /// <param name="type">Type "hide" to hide console window instead of show.</param>
      public static void ShowConsoleWindow(string type = "show")
      {
         var handle = GetConsoleWindow(); //for hide console window
         var showing = type.ToLower() == "hide" ? SW_HIDE : SW_SHOW;
         ShowWindow(handle, showing); //hide console window
      }

      //determine if console window is visible:
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      static extern bool IsWindowVisible(IntPtr hWnd);
      #endregion

      #region HandleMouse
      [DllImport("user32.dll")]
      public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
      public enum MouseEventFlags
      {
         LEFTDOWN = 0x00000002,
         LEFTUP = 0x00000004,
         MIDDLEDOWN = 0x00000020,
         MIDDLEUP = 0x00000040,
         RIGHTDOWN = 0x00000008,
         RIGHTUP = 0x00000010
      }

      /// <summary>
      /// Send left mouse click on selected position.
      /// </summary>
      /// <param name="p">Point for position to send left mouse click.</param>
      public static void SendLeftMouse(Point p)
      {
         MicroSleep();
         Cursor.Position = p;
         MicroSleep();
         LMclick();
      }

      /// <summary>
      /// Send left mouse click.
      /// </summary>
      public static void LMclick()
      { mouse_event((int)MouseEventFlags.LEFTDOWN | (int)MouseEventFlags.LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }
      /// <summary>
      /// Send middle mouse click.
      /// </summary>
      public static void MMBclick()
      { mouse_event((int)MouseEventFlags.MIDDLEDOWN | (int)MouseEventFlags.MIDDLEUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }
      /// <summary>
      /// Send right mouse click.
      /// </summary>
      public static void RMClick()
      { mouse_event((int)MouseEventFlags.RIGHTDOWN | (int)MouseEventFlags.RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0); }
      #endregion

      #region Sleeping
      /// <summary>
      /// Thread.Sleep between 52 and 164 milliseconds.
      /// </summary>
      public static void Sleep()
      {
         Random random = new Random();
         Thread.Sleep(random.Next(52, 164));
      }
      /// <summary>
      /// Thread.Sleep between 32 and 64 milliseconds.
      /// </summary>
      public static void MicroSleep()
      {
         Random random = new Random();
         Thread.Sleep(random.Next(32, 64));
      }
      /// <summary>
      /// Thread.Sleep low amount of time
      /// </summary>
      /// <param name="ms">miliseconds to sleep, default: 10</param>
      public static void NanoSleep(int ms = 10)
      {
         Thread.Sleep(ms);
      }
      #endregion

   }
}
