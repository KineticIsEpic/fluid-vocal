/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace OTOmate {
    /// <summary>
    /// Allows easy reading of UTAU voicbank configuration files.
    /// </summary>
    public class OtoReader {
        List<VoiceProp> voices = new List<VoiceProp>(4096);

        String otoText = ""; 
        StreamReader sr;

        string txtBoxFile = "";
        string voiceFilename = "";

        bool txtBoxScrollbarsShowing = false;
        int lines = 0;
        int vbCtlrCount = 0;

        /// <summary>
        /// Opens a voicebank configuration file to use within the OTOMmate.OtoReder class.
        /// </summary>
        /// <param name="fileLoc">The location of the voicebank config file.</param>
        public void OpenFile(string fileLoc) {
            try {
                // Used to add to the voices list later
                VoiceProp vp;

                int index = 0; // Used for seperating lines
                int lastIndex = 0; // Also used for seperating lines
                int run = 0; // Who knows what this does
                int run2 = 0; // I'm run2

                // Read text file
                sr = new StreamReader(fileLoc);
                otoText = sr.ReadToEnd();
                sr.Close();

                // Get lines count
                lines = Count4(otoText);

                // Each line from oto.ini
                string[] otoLines = new string[lines];

                // Seperate text file into lines
                foreach (var item in otoText) {
                    if (item == '\n') {
                        index = otoText.IndexOf(item, lastIndex + 1);
                        otoLines[run] = otoText.Substring(lastIndex, index - lastIndex);
                        lastIndex = index;
                        run++;
                    }
                }

                // Generate VoiceProp classes from seperated lines
                foreach (var item in otoLines) {
                    vp = new VoiceProp();

                    try {
                        // Get indexes from line
                        int index1 = item.IndexOf('=');
                        int index2 = item.IndexOf(",", index1 + 1);
                        int index3 = item.IndexOf(",", index2 + 1);
                        int index4 = item.IndexOf(",", index3 + 1);
                        int index5 = item.IndexOf(",", index4 + 1);
                        int index6 = item.IndexOf(",", index5 + 1);
                        
                        // Chop up line into individual properties and add them to vp
                        vp.FileName = item.Substring(0, index1);
                        vp.SampleName = item.Substring(index1 + 1, index2 - index1 - 1);
                        vp.StartString = item.Substring(index2 + 1, index3 - index2 - 1);
                        vp.ConsonantString = item.Substring(index3 + 1, index4 - index3 - 1);
                        vp.EndString = item.Substring(index4 + 1, index5 - index4 - 1);
                        vp.PreutteranceString = item.Substring(index5 + 1, index6 - index5 - 1);
                        vp.OverlapString = item.Substring(index6 + 1);
                        vp.FileDir = fileLoc.Substring(0, fileLoc.LastIndexOf("\\")) + "\\";

                        // Removes /n from FileName property
                        if (vp.FileName.Contains("\n")) vp.FileName = vp.FileName.Substring(1);

                        // Add vp to voices
                        voices.Add(vp);
                    }
                    catch (Exception ex) { break; }
                    run2++;
                }
            }
            catch (Exception ex) { throw; }
        }

        /// <summary>
        /// Returns the VoiceProp class from OtoReader.voices with a matching
        /// SampleName property, or a new VoiceProp if none matching.
        /// </summary>
        /// <param name="sampleName">The VoiceProp.SampleName property to search for.</param>
        /// <returns>A VoiceProp class with the matching VoiceProp.SampleName property.</returns>
        public VoiceProp GetVoicePropFromSampleName(string sampleName) {
            VoiceProp vp = new VoiceProp(); 

            foreach (var item in voices) {
                if (item.SampleName == sampleName) return item;
                else if (item.FileName == sampleName + ".wav") return item;
            }
            return vp;
        }

        /// <summary>
        /// Creates a new instance of the OtoReader class based on the specified
        /// configuration file.
        /// </summary>
        /// <param name="file">The configuration file to open.</param>
        public static OtoReader FromFile(string file) {
            OtoReader or = new OtoReader();
            or.OpenFile(file);
            return or; 
        }

        /// <summary>
        /// This method is something else entirely, one of a kind. It can do wonders. 
        /// For when this file was but nothing, this method was born. Unlike most ordindary
        /// lines of useless code, this one was destined to be more -- a masterpiece, a
        /// work of biblical porportions. Something to be remembered through the ages as
        /// one of the biggest achievements that mankind has ever witnessed. Contained
        /// within it is something never seen before, something with unjustifiable capabilites.
        /// Something so powerful that no person has the ability to comprehend it's abailities.
        /// So great that it eclipses even the entire universe and all galaxies encompassed
        /// within it. However, the coincidence so happened that none of its power could
        /// be harnessed. Through eons of time, through the decades, it roamed with no
        /// use. However, as of now, it has found a permanent home in this application -- 
        /// OtoReader.cs. If you are reading this, you are about to behold the most powerful 
        /// of powers, the most energized of energy, the most colossal of masses. Please, my
        /// friend, use this power with caution and wisdom, as any misuse could spell disaster
        /// for the human race.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int Count4(string s) {
            int n = 0;
            foreach (var c in s) {
                if (c == '\n') n++;
            }
            return n + 1;
        }
    }
}
