/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace FluidSys {
    public class Renderer {
        bool debug = false;
        public bool ShowRenderWindow { get; set; }

        string tempDir = "";

        /// <summary>
        /// Gets the current temporary directory used at render time.
        /// </summary>
        public string TemporaryDir { get { return tempDir; } } 

        Sheet sheet;
        Random rand = new Random();

        /// <summary>
        /// Creates a new instance of the renderer class. The specied Sheet object 
        /// will be used for rendering.
        /// </summary>
        public Renderer(Sheet st) {
            genTempDir();

            sheet = new Sheet();

            sheet.rendParams = st.rendParams;
            sheet.notes = st.notes;
            sheet.Resampler = st.Resampler;
            sheet.Voicebank = st.Voicebank;
        }

        /// <summary>
        /// Render the Sheet object associated with this instance of the Renderer class.
        /// </summary>
        public void Render() {
            // Used for file naming
            int run = 0;

            foreach (var item in sheet.notes) {
                Process p = new Process();

                // Compensate for trimming
                int length = item.Length + item.VoiceProperties.Start + item.VoiceProperties.End;

                // Get voicebank path
                string vbpath = item.VbPath;
                if (item.UseDefaultVb) vbpath = sheet.Voicebank;

                // Generate resampler arguments
                string args = "\"" + sheet.Voicebank + "\\" + item.VoiceProperties.FileName + "\" \"" + tempDir + "\\" +
                    run.ToString() + ".wav\" \"" + item.NotePitch + "\"" + " 100 B0 " + item.VoiceProperties.StartString + " " +
                    length.ToString() + " " + item.VoiceProperties.ConsonantString + " " + item.VoiceProperties.EndString;

                // Setup process properties
                p.StartInfo.FileName = sheet.Resampler;
                p.StartInfo.Arguments = args;
                if (!ShowRenderWindow) p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                if (debug) System.Windows.Forms.MessageBox.Show(args);

                // Start rendering
                try { 
                    p.Start();
                }
                catch (Exception ex) {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }

                // Wait on last render
                p.WaitForExit();

                // Show the temporary folder if debug mode is enabled
                if (debug) {
                    Process file = new Process();
                    file.StartInfo.FileName = tempDir;
                    file.Start(); 
                }

                // Increase run 
                run++;
            }
        }

        private void genTempDir() {
            tempDir += Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\FluidSynth\\RenCache\\" +
                rand.Next().ToString() + "\\" + rand.Next().ToString();
           
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            if (debug) System.Windows.Forms.MessageBox.Show(tempDir);
        }
    }
}
