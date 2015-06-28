using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidSys {
    class SampleBank {
        /// <summary>
        /// Gets or sets the path to the root of the sample path.
        /// </summary>
        public string SampleRoot { get; set; }

        /// <summary>
        /// Gets or sets the path to the image associated with this 
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Determines if this sample bank is a megabank. A megabank 
        /// functions as a container for multiple sample bank components.
        /// </summary>
        public bool IsMegaBank { get; set; }

        /// <summary>
        /// Gets or sets the components of this sample bank. This value is only
        /// used when SampleBank.IsMegaBank is true.
        /// </summary>
        public List<SampleBank> Components {
            get { return baseSampleBanks; }
            set {
                if (value.Count > 1) IsMegaBank = true;
                baseSampleBanks = value;
            }
        }

        private List<SampleBank> baseSampleBanks = new List<SampleBank>(12);

        public SampleBank() {
            
        }

        public static SampleBank FromPath(string fileName) {
            SampleBank sb = new SampleBank();

            if (System.IO.File.Exists(System.IO.Path.Combine(fileName, "oto.ini"))) {
                sb.SampleRoot = fileName;
                return sb;
            }
            else throw new ArgumentException
                ("The path \"" + fileName + "\" does not refer to a valid UTAU-style sample bank. ");
        }

        public static SampleBank FromPaths(IEnumerable<String> paths) {
            SampleBank mainsb = new SampleBank();
            mainsb.IsMegaBank = true;

            foreach (string path in paths) {
                if (System.IO.File.Exists(System.IO.Path.Combine(path, "oto.ini"))) {
                    mainsb.Components.Add(SampleBank.FromPath(path));
                    System.Windows.Forms.MessageBox.Show("added " + path);
                }
                else System.Windows.Forms.MessageBox.Show("added " + path);
            }
            return mainsb;
        }
    }
}
