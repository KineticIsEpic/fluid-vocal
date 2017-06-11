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

        /// <summary>
        /// Remaps a number from one range to another.
        /// </summary>
        /// <param name="x">the number to map</param>
        /// <param name="in_min">the number's current range's minimum value</param>
        /// <param name="in_max">the current range's highest value</param>
        /// <param name="out_min">the new range's minimum value</param>
        /// <param name="out_max">the new range's highest value</param>
        /// <returns></returns>
        public static long Map(long x, long in_min, long in_max, long out_min, long out_max) {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
