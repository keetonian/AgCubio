// Created by Daniel Avery and Keeton Hodgson
// November 2015

using System;
using System.Windows.Forms;

namespace AgCubio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Display());
        }
    }
}
