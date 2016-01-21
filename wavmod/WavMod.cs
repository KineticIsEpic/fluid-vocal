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
using NAudio;
using NAudio.Wave;
using FluidSys;
using System.Timers;
using System.Diagnostics;
using System.Threading;

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
