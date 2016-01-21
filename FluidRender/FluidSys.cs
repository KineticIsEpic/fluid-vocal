/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FluidSys {
    public class FluidSys {
        public static string SettingsDir {
            get { return Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\FluidSynth\\cfg"; } 
        }

        public static string CreateTempDir() {
            Random rnd = new Random();

            string str = Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\FluidSynth\\ren\\" +
                rnd.Next().ToString() + "\\" + rnd.Next().ToString();

            if (!Directory.Exists(str)) Directory.CreateDirectory(str);

            return str;
        }

        public static void ClearCache() {
            Directory.Delete(Environment.ExpandEnvironmentVariables("%LocalAppData%") + 
                "\\FluidSynth\\ren\\", true);
        }
    }
}
