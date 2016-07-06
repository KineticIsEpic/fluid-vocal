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
            Console.Out.WriteLine("Fluid Vocal Synthesis System, testing build 9.2");
            Console.Out.WriteLine("Copyright (c) 2016 KineticIsEpic. Type \"about\" for details.");

            new CmdSys().Cmd();
        }
        public static void Exit() { return; }
    }
}
