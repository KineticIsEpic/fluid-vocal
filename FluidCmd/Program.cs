/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidCmd {
    class Program {
        static void Main(string[] args) {
            Console.Out.WriteLine("Fluid Vocal Synthesis System, testing build 2");
            Console.Out.WriteLine("Copyright (c) 2015 KineticIsEpic. All rights reserved.");

            new CmdSys().Cmd();
        }
    }
}
