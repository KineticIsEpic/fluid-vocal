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
    public delegate void RenderNoteCompleteEventArgs(int noteIndex);

    public class Renderer {
        public event RenderNoteCompleteEventArgs NoteRendered;

        bool debug = false;
        public bool ShowRenderWindow { get; set; }
        public bool UseMultiThread { get; set; }

        public int RenderThreads { get; set; }

        string tempDir = "";

        /// <summary>
        /// Gets the current temporary directory used at render time.
        /// </summary>
        public string TemporaryDir { get { return tempDir; } }

        public string restNotePath = "";

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

            RenderThreads = 6;

            UseMultiThread = true;
        }

        /// <summary>
        /// Render the Sheet object associated with this instance of the Renderer class.
        /// </summary>
        public void Render() {
            // Used for file naming
            int run = 0;
            // Used to track threads
            int currentThreads = 0;

            foreach (var item in sheet.notes) {
                Process p = new Process();
                
                // Render arguments
                string args;

                // Compensate for trimming
                int length = item.Length + item.VoiceProperties.Start + item.VoiceProperties.End;

                // Get voicebank path
                string vbpath = item.VbPath;
                if (item.UseDefaultVb) vbpath = sheet.Voicebank; 

                if (item.DispName == "r") {
                    // Makeshift rest system, to be replaced with WavMOD
                    args = "\"" + restNotePath + "\\" + "rest.wav" + "\" \"" + tempDir + "\\" +
                        run.ToString() + ".wav\" \"" + "C2" + "\"" + " 0 B0 " + "0" + " " +
                        length.ToString() + " 0 0 1 0";
                }
                else {
                    // Generate resampler arguments
                    args = "\"" + vbpath + "\\" + item.VoiceProperties.FileName + "\" \"" + tempDir + "\\" +
                        run.ToString() + ".wav\" \"" + item.NotePitch + "\" " + item.Velocity.ToString() + " \"" + item.Args + "\" " + 
                        item.VoiceProperties.StartString + " " + length.ToString() + " " + item.VoiceProperties.ConsonantString
                        + " " + item.VoiceProperties.EndString + " " + item.Volume.ToString() + " " + item.Modulation.ToString() + 
                        " " + item.PitchCode;
                }

                // Setup process properties
                p.StartInfo.FileName = sheet.Resampler;
                p.StartInfo.Arguments = args;
                if (!ShowRenderWindow) p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                if (debug) System.Windows.Forms.MessageBox.Show(args);

                // Start rendering
                try { p.Start(); }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }

                // Raise NoteRendered event
                try { NoteRendered.Invoke(run); }
                catch (Exception) { }

                if (!UseMultiThread) p.WaitForExit();

                // Show the temporary folder if debug mode is enabled
                if (debug) {
                    Process file = new Process();
                    file.StartInfo.FileName = tempDir;
                    file.Start(); 
                }

                // Increase run & currentThreads
                run++;
                currentThreads++;

                // Wait on last render
                if (run + 1 >= sheet.notes.Count || currentThreads > RenderThreads && UseMultiThread) {
                    p.WaitForExit();
                    currentThreads = 0;
                }
            }
        }

        private void genTempDir() {
            tempDir += Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\FluidSynth\\ren\\" +
                rand.Next().ToString() + "\\" + rand.Next().ToString();
           
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            if (debug) System.Windows.Forms.MessageBox.Show(tempDir);
        }
    }
}
