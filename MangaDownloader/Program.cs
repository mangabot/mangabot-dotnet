﻿using MangaDownloader.GUIs;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace MangaDownloader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool run = false;
            Process process = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(process.ProcessName);
            foreach (Process p in processes)
            {
                // Get the first instance that is not this instance, has the
                // same process name and was started from the same file name
                // and location. Also check that the process has a valid
                // window handle in this session to filter out other user's
                // processes (p.MainWindowHandle != IntPtr.Zero).

                //MessageBox.Show(p.Id + " - " + p.MainModule.FileName + " - " + p.MainWindowHandle);

                if (p.Id != process.Id && p.MainModule.FileName == process.MainModule.FileName)
                {
                    run = true;
                    break;
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!run)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new ThreadExceptionEventHandler(GlobalExceptionCatcher.UnhandledThreadExceptionHandler);
                Application.Run(args.Length == 0 ? new MainForm() : new MainForm(args[0]));
            }
            else
            {
                MessageBox.Show("Manga Downloader is running, please check it in taskbar.", "Manga Downloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var binPath = Path.GetFullPath(Application.StartupPath + "\\bin\\" + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll");

            if (File.Exists(binPath))
                return Assembly.LoadFrom(binPath);

            return null;
        }
    }
}
