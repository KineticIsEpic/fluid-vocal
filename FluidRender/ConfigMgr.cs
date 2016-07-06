using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FluidSys {
    public class ConfigMgr {
        public string DebugLogDir { get { return "debug.log"; } }
        /// <summary>
        /// Gets or sets the default sample bank.
        /// </summary>
        public string DefaultSamplebank {
            get {
                if (File.Exists(FluidSys.SettingsDir + "\\defsmpbank")) {
                    StreamReader sr = new StreamReader(FluidSys.SettingsDir + "\\defsmpbank");
                    string srDat = sr.ReadToEnd();
                    sr.Close();
                    return srDat;
                }
                else return null;
            }
            set {
                StreamWriter sw = new StreamWriter(FluidSys.SettingsDir + "\\defsmpbank");
                sw.Write(value);
                sw.Close();
            }
        }

        public string DefaultWavTool {
            get {
                if (File.Exists(FluidSys.SettingsDir + "\\defwavtool")) {
                    StreamReader sr = new StreamReader(FluidSys.SettingsDir + "\\defwavtool");
                    string srDat = sr.ReadToEnd();
                    sr.Close();
                    return srDat;
                }
                else return null;
            }
            set {
                StreamWriter sw = new StreamWriter(FluidSys.SettingsDir + "\\defwavtool");
                sw.Write(value);
                sw.Close();
            }
        }

        public List<string> SampleBanks { get; private set; } //TODO

        public bool UseMultithreadRender {
            get {
                if (File.Exists(FluidSys.SettingsDir + "\\multithread")) return true;
                else return false; 
            }
            set { if (value) File.Create(FluidSys.SettingsDir + "\\multithread"); }
        }

        public ConfigMgr() {
            if (!Directory.Exists(FluidSys.SettingsDir)) 
                Directory.CreateDirectory(FluidSys.SettingsDir);
        }
    }
}
