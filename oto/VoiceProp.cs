/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTOmate {
    public class VoiceProp {
        /// <summary>
        /// 
        /// </summary>
        public string SampleName { get; set; }
        public string FileName { get; set; }
        public string FileDir { get; set; }
        public string StartString { get; set; }
        public string ConsonantString { get; set; }
        public string EndString { get; set; }
        public string PreutteranceString { get; set; }
        public string OverlapString { get; set; }

        public string SamplePath {
            get {
                return FileDir + FileName;
            }
        }

        public int Start {
            get {
                try { return int.Parse(StartString); }
                catch (Exception) { return 0; }
            }
            set { StartString = value.ToString(); }
        }

        public int Consonant {
            get {
                try { return int.Parse(ConsonantString); }
                catch (Exception) { return 0; }
            }
            set { ConsonantString = value.ToString(); }
        }

        public int End {
            get {
                try { return int.Parse(EndString); }
                catch (Exception) { return 0; }
            }
            set { EndString = value.ToString(); }
        }

        public int Preutterance {
            get {
                try { return int.Parse(PreutteranceString); }
                catch (Exception) { return 0; }
            }
            set { PreutteranceString = value.ToString(); }
        }

        public int Overlap {
            get {
                try { return int.Parse(OverlapString); }
                catch (Exception) { return 0; }
            }
            set { OverlapString = value.ToString(); }
        }
    }
}
