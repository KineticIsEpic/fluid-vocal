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
using FluidSys;

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
            sheet.Bpm = st.Bpm;

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

                // How CADENCII (presumably legacy Utau) resampler arguments work:
                // - Everything up to the Modulation command as normal
                // - The next line is <fistpith>Q<bpm>, ex. 0.00Q120
                // - after that, each pitch value in format X.XX, each in its own argument. 
                
                // Render arguments
                string args;

                // Compensate for trimming
                int length = item.Length + ((int)(item.VoiceProperties.Preutterance +
                    item.VoiceProperties.Overlap));
                int consonant = (int)item.VoiceProperties.Consonant;

                // Create file names that sort properly alphabetically 
                string zeros = "0000";
                string runName = zeros.Substring(run.ToString().Length) + run.ToString();

                // Get voicebank path
                string vbpath = item.VbPath; 
                if (item.UseDefaultVb) vbpath = sheet.Voicebank;

                // get pitches
                string[] pitches = new string[(int)(96 * ((double)item.UUnitLength / 480))];
                if (string.IsNullOrEmpty(item.PitchCode)) pitches[0] = "0.00";
                else pitches = splitPitchCode(item.PitchCode);

                // make string from pitches
                string pitchstr = "";
                foreach (string str in pitches) pitchstr += str;

                if (item.DispName == "r") {
                    // Makeshift rest system, to be replaced with WavMOD
                    args = "\"" + restNotePath + "\\" + "rest.wav" + "\" \"" + tempDir + "\\" +
                        runName + ".wav\" \"" + "C2" + "\"" + " 0 B0 " + "0" + " " +
                        length.ToString() + " 0 0 1 0";
                }
                else {
                    // Generate resampler arguments
                    args = "\"" + vbpath + "\\" + item.VoiceProperties.FileName + "\" \"" //in
                        + tempDir + "\\" + runName + ".wav\" \"" //out
                        + item.NotePitch + "\" " //pitch
                        + item.Velocity.ToString() //vel
                        + " \"" + item.Args + "\" " //flag
                        + item.VoiceProperties.StartString + " " //offset
                        + length.ToString() + " " //length_require
                        + item.VoiceProperties.ConsonantString + " " //fixed_length
                        + item.VoiceProperties.EndString + " " //end
                        + item.Volume.ToString() + " " //vol
                        + item.Modulation.ToString() + " " //mod
                        /*+ pitches[0] */+ "!" + sheet.Bpm.ToString() + " " //tempo thing
                        + pitchstr;
                }

                // Setup process properties
                p.StartInfo.FileName = sheet.Resampler;
                p.StartInfo.Arguments = args;
                if (!ShowRenderWindow) p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                DebugLog.WriteLine("Resampler Arguments for note: " + item.DispName);
                DebugLog.WriteLine(args);
                
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
                //TODO: send an event when it's done instead of freezing the thread
                if (run + 1 >= sheet.notes.Count || currentThreads > RenderThreads && UseMultiThread) {
                    p.WaitForExit();
                    currentThreads = 0;
                }
            }
        }

        // split each number in the pitch code into its own line in an arry
        private string[] splitPitchCode(string pitchCode) {
            string[] retstr = new string[pitchCode.Length / 2]; //TODO more efficient length
            int index = 0;
            int lastIndex = 0;
            int run = 0;

            foreach (var item in pitchCode) {
                if (item == ' ') {
                    index = pitchCode.IndexOf(item, lastIndex + 1);
                    retstr[run] = pitchCode.Substring(lastIndex, index - lastIndex);

                    // converts the pitch to base 64 for resampler - positive numbers
                    if (int.Parse(retstr[run]) > 0) {
                        retstr[run] = UnitConverter.Encode(int.Parse(retstr[run]), UnitConverter.Base64); 
                    }
                    
                    // handles the negative numbers: utau understands negative pitch 
                    // as 4095 + <-pitch>
                    else {
                        int val = int.Parse(retstr[run]);
                        retstr[run] = UnitConverter.Encode(4095 + int.Parse(retstr[run]), UnitConverter.Base64);
                    }

                    // make sure each code has 2 chars
                    if (retstr[run].Length != 2) retstr[run].Insert(0, "A");

                    lastIndex = index;
                    run++;
                } 
            }

            return retstr;
        }

        private void genTempDir() {
            tempDir += Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\FluidSynth\\ren\\" +
                rand.Next().ToString() + "\\" + rand.Next().ToString();
           
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            if (debug) System.Windows.Forms.MessageBox.Show(tempDir);
        }
    }
}
