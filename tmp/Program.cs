using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Configuration;
using System.Windows.Forms;

namespace tmp
{
    class Program
    {
        public static ContextMenu menu;
        public static MenuItem mnuExit;
        public static NotifyIcon notifyIcon;
        public static bool check = true;

        public static System.Timers.Timer aTimer;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main()
        {
            IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(h, 0);

            while (true)
            {
                Thread.Sleep(1);
                //Do what you want
                if (check)
                    startInfo();               
            }
        }

        private static void startInfo()
        {
            menu = new ContextMenu();
            mnuExit = new MenuItem("Exit");
            menu.MenuItems.Add(0, mnuExit);

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = ScoreUpdates.Properties.Resources.notification;
            notifyIcon.ContextMenu = menu;
            notifyIcon.Visible = true;
            mnuExit.Click += new EventHandler(mnuExit_Click);
            aTimer = new System.Timers.Timer(1000);
            //aTimer.Elapsed += new System.Timers.ElapsedEventHandler(score);
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(score);
            aTimer.Enabled = true;
            Application.Run();
        }

        private static void mnuExit_Click(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            check = false;
            Environment.Exit(0);
        }

        public static void score(object source, System.Timers.ElapsedEventArgs e)
        {
            string display = DownloadText(ConfigurationManager.AppSettings["URL"]);
            //string display = aTimer.
            notifyIcon.Text = display;
            notifyIcon.BalloonTipText = display;
            notifyIcon.ShowBalloonTip(1);
            if (aTimer.Interval == 1000 )
            aTimer.Interval = TimeSpan.FromSeconds(60 * Convert.ToInt32(ConfigurationManager.AppSettings["timeinterval"])).TotalMilliseconds;
        }

        public static string DownloadText(string url)
        {
            WebClient x = new WebClient();
            string source = x.DownloadString(url);
            string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
            string tt = Regex.Match(title, @"\(([^)]*)\)").Groups[1].Value;

            //Console.WriteLine(tt);

            score c_s = new score();
            string[] c_t_r = title.Split(' ');
            c_s.teambatting = c_t_r[0];
            c_s._score = c_t_r[1];
            string[] c_t_in = tt.Split(',');

            bool displaylength = checkdisplaylength(c_t_r, c_t_in);

            if (c_t_in.Length > 1)
            {
                for (int k = 0; k < c_t_in.Length; k++)
                {
                    if (k == 0)
                        c_s.over = Convert.ToDecimal(c_t_in[k].Split(' ')[0]);
                    else
                    {
                        Current_Player tmp = new Current_Player();
                        int j = 1;
                        foreach (string nm in c_t_in[k].Trim().Split(' '))
                        {
                            if (tmp.bm == "")
                                tmp.bm = nm;
                            else
                            {
                                if (j < c_t_in[k].Trim().Split(' ').Length)
                                {
                                    if (displaylength)
                                        tmp.bm = tmp.bm + " " + nm;
                                    else
                                        tmp.bm = nm;
                                }
                            }
                            j++;
                        }
                        tmp.runs_wkt = c_t_in[k].Split(' ')[c_t_in[k].Split(' ').Length - 1];
                        c_s.B.Add(tmp);
                    }
                }
            }
            return c_s.score_info();
        }

        private static bool checkdisplaylength(string[] c_t_r, string[] c_t_in)
        {
            bool check = false;
            int length = c_t_r[0].Length;

            for (int k = 0; k < c_t_in.Length; k++)
            {
                if (k == 0)
                    continue;
                for (int j = 0; j < c_t_in[k].Trim().Split(' ').Length - 1; j++)
                {
                    length = length + c_t_in[k].Trim().Split(' ')[j].Length;
                }
            }
            if (length + 35 < 65)
                check = true;
            return check;
        }
    }


    public class score
    {
        public string teambatting;
        public string _score;
        public List<Current_Player> B = new List<Current_Player>();
        public decimal over;
        public score() { }

        public string score_info()
        {
            string sc;
            sc = this.teambatting + " " + this._score;
            int cc = 1;
            foreach (Current_Player cp in B)
            {
                if (cc < B.Count)
                    sc = sc + " " + cp.c_pinfo();
                else
                    sc = sc + "\n" + cp.c_pinfo();
            }
            sc = sc + " Ovr:" + over;
            return sc;
        }

        public int score_info_length()
        {
            return 0;
        }
    }


    public class Current_Player
    {
        public string bm = "";
        public string runs_wkt;
        public Current_Player() { }

        public string c_pinfo()
        {
            string cinfo;
            cinfo = this.bm + " " + runs_wkt;
            return cinfo;
        }
    }
}
