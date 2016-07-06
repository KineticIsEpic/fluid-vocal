using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FluidSys {
    public static class DebugLog {
        public static void Write(string data) {
            StreamWriter sw = new StreamWriter(new ConfigMgr().DebugLogDir, true);
            sw.Write(data);
            sw.Close();
        }

        public static void WriteLine(string data) {
            StreamWriter sw = new StreamWriter(new ConfigMgr().DebugLogDir, true);
            sw.Write(DateTime.Now.ToString() + ": ");
            sw.Write(data);
            sw.Write("\r\n");
            sw.Close();
        }
    }
}
