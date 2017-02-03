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

        public double Start {
            get {
                try { return double.Parse(StartString); }
                catch (Exception) { return 0; }
            }
            set { StartString = value.ToString(); }
        }

        public double Consonant {
            get {
                try { return double.Parse(ConsonantString); }
                catch (Exception) { return 0; }
            }
            set { ConsonantString = value.ToString(); }
        }

        public double End {
            get {
                try { return double.Parse(EndString); }
                catch (Exception) { return 0; }
            }
            set { EndString = value.ToString(); }
        }

        public double Preutterance {
            get {
                try { return double.Parse(PreutteranceString); }
                catch (Exception) { return 0; }
            }
            set { PreutteranceString = value.ToString(); }
        }

        public double Overlap {
            get {
                try { return double.Parse(OverlapString); }
                catch (Exception) { return 0; }
            }
            set { OverlapString = value.ToString(); }
        }
    }
}
