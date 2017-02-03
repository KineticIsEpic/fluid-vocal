/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidCmd {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            Console.Out.WriteLine("Fluid Vocal Synthesis System, testing build OVER 9000 BOI");
            Console.Out.WriteLine("Copyright (c) 2017 KineticIsEpic. ");

            FluidUI.MainWindow mw = new FluidUI.MainWindow();
            mw.ShowDialog();

            try {
                System.IO.Directory.Delete(Environment.ExpandEnvironmentVariables("%LocalAppData%") +
                    "\\FluidSynth\\ren", true);

                Console.Out.WriteLine("Render cache cleared.");
            }
            catch (Exception ex) { Console.Out.WriteLine("e: " + ex.Message); }

            Program.Exit();
        }
        public static void Exit() { return; }
    }
}
