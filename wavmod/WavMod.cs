/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using FluidSys;
using System.Diagnostics;

namespace wavmod {
    public class WavMod {
        string currentFile = "";

        bool debug = false;

        /// <summary>
        /// Merge multiple .wav files together and save the output.
        /// </summary>
        /// <param name="outputFile">The path to save the output to.</param>
        /// <param name="sourceFiles">An IEnumerable list of files to merge.</param>
        private static void ConcatenateWav(string outputFile, IEnumerable<string> sourceFiles) {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;
            
            try {
                foreach (string sourceFile in sourceFiles) {
                    using (WaveFileReader reader = new WaveFileReader(sourceFile)) {
                        if (waveFileWriter == null) {
                            // first time in create new Writer
                            waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                        }
                        else {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat)) {
                                throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                            }
                        }

                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0) {
                            waveFileWriter.WriteData(buffer, 0, read);
                        }
                    }
                }
            }
            finally {
                if (waveFileWriter != null) {
                    waveFileWriter.Dispose();
                }
            }

        }

        /// <summary>
        /// Quickly play back the contents of a temporary render directory, using the
        /// specified Sheet as reference.
        /// </summary>
        public void PlaybackTemp(string tempDir, Sheet playbackSheet) {
            string[] files = Directory.GetFiles(tempDir);
            string tempdir = "";

            // Generate trimmed files ready for splicing 
            tempdir = GenEditedFiles(files, playbackSheet.notes);

            // Show the output in explorer if debug mode is on
            if (debug) {
                Process p = new Process();
                p.StartInfo.FileName = tempdir;
                p.Start();
            }

            // Splice the files
            ConcatenateWav(tempdir + "\\render.wav", Directory.GetFiles(tempdir));

            // Play back resulting file
            new System.Media.SoundPlayer(tempdir + "\\render.wav").Play();
        }

        /// <summary>
        /// Render the project somehwere. 
        /// </summary>
        /// <param name="tempDir"></param>
        /// <param name="playbackSheet"></param>
        public void SaveTemp(string tempDir, string outDir, Sheet playbackSheet) {
            string[] files = Directory.GetFiles(tempDir);
            string tempdir = "";

            // Generate trimmed files ready for splicing 
            tempdir = GenEditedFiles(files, playbackSheet.notes);

            // Show the output in explorer if debug mode is on
            if (debug) {
                Process p = new Process(); 
                p.StartInfo.FileName = tempdir;
                p.Start();
            }

            // Splice the files
            ConcatenateWav(outDir, Directory.GetFiles(tempdir));
        }

        public void ExtWavtoolInit(string tempDir, string outFile, Sheet playbackSheet, string toolPath, bool play) {
            string[] files = Directory.GetFiles(tempDir);
            int noteIndex = 0;

            foreach (Note nt in playbackSheet.notes) {
                if (noteIndex > 0) GenAdjTime(nt, playbackSheet.notes[noteIndex - 1]);
                else {
                    nt.VoiceProperties.Adj_Overlap = nt.Overlap;
                    nt.VoiceProperties.Adj_Preutterance = nt.VoiceProperties.Preutterance;
                }

                noteIndex++;
            }

            noteIndex = 0;

            foreach (string file in files) {
                // create overlap string
                string ovldoodad = "+";
                double ovlint = playbackSheet.notes[noteIndex].VoiceProperties.Adj_Preutterance;

                try {
                    ovlint -= playbackSheet.notes[noteIndex + 1].VoiceProperties.Adj_Preutterance;
                    ovlint += playbackSheet.notes[noteIndex + 1].VoiceProperties.Adj_Overlap;
                }
                catch (Exception) { }

                if (ovlint < 0) ovldoodad = ovlint.ToString();
                else ovldoodad += ovlint.ToString();

                string arguments;
                arguments = outFile + " " + file + " 0 " + playbackSheet.notes[noteIndex].UUnitLength 
                    + "@" + playbackSheet.Bpm + ovldoodad + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[0][0] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[1][0] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[2][0] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[0][1] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[1][1] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[2][1] + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[3][1] + " ";
                arguments += playbackSheet.notes[noteIndex].VoiceProperties.Overlap + " ";
                arguments += playbackSheet.notes[noteIndex].Envelope[3][0] + " ";

                DebugLog.WriteLine("WavMod Arguments for file: " + file);
                DebugLog.WriteLine(arguments);

                Process p = new Process();
                p.StartInfo.FileName = toolPath;
                p.StartInfo.Arguments = arguments;
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForExit();

                noteIndex++;
            }

            Process p2 = new Process();
            p2.StartInfo.FileName = "cmd.exe";
            p2.StartInfo.Arguments = "/C copy /Y \"" + outFile + "\".whd /B + \"" + outFile + "\".dat /B \"" + outFile + "\"";
            p2.Start();
            p2.WaitForExit();

            try { if (play) new System.Media.SoundPlayer(outFile).Play(); }
            catch (Exception) { System.Windows.Forms.MessageBox.Show("An error occured while attempting to play the file, " + 
                "this is most likely due to an error during the splicing process. \r\n\r\nSome things you can do \r\n" + 
                "-Change the tempo\r\n-Adjust envelopes\r\n-Restart FVSS", "Playback Error", 
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation); }
        }

        private void GenAdjTime(Note current, Note prev) {
            if (current.VoiceProperties.Preutterance 
                - current.VoiceProperties.Overlap > prev.Length / 2) {

                double factor = (prev.Length / 2) /
                    (current.VoiceProperties.Preutterance - current.VoiceProperties.Overlap);

                current.VoiceProperties.Adj_Preutterance =
                    current.VoiceProperties.Preutterance * factor;

                current.VoiceProperties.Adj_Overlap =
                    current.VoiceProperties.Overlap * factor;            
            }
            else {
                current.VoiceProperties.Adj_Preutterance =
                    current.VoiceProperties.Preutterance;
                current.VoiceProperties.Adj_Overlap =
                    current.VoiceProperties.Overlap;
            }
        }
        
        private string GenEditedFiles(string[] files, List<Note> notes) {
            string tempdir = FluidSys.FluidSys.CreateTempDir();
            string tempfile = "";
            string tempfile2 = "";

            int run = 0;

            // Trim each note
            foreach (string file in files) {
                tempfile = tempdir + "\\" + run.ToString() + ".wav";
                tempfile2 = tempdir + "\\" + run.ToString() + "0.wav";

                //WavFileUtils.TrimWavFile(file, tempfile2, TimeSpan.FromMilliseconds(notes[run].VoiceProperties.Start),
                //    TimeSpan.FromMilliseconds(notes[run].VoiceProperties.End));

                var afr = new AudioFileReader(file); 
                var fade = new DelayFadeOutSampleProvider(afr);

                fade.BeginFadeIn(100);
                //fade.BeginFadeOut(afr.TotalTime.TotalMilliseconds , afr.TotalTime.TotalMilliseconds * 2);
                //fade.BeginFadeIn(35);

                var stwp = new NAudio.Wave.SampleProviders.SampleToWaveProvider(fade);
                WaveFileWriter.CreateWaveFile(tempfile, stwp);

                new System.Media.SoundPlayer(tempfile).Play();

                //File.Delete(tempfile2);

                run++;
            }
            return tempdir;
        }
    }
}
