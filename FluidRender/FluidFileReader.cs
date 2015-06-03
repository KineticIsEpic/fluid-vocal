/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FluidSys {
    public class FluidFileReader {
        string filePath;
        XmlDocument baseDoc;

        public List<Sheet> Sheets = new List<Sheet>(); // List for future multi-sheet file system
        public Sheet Sheet1 = new Sheet();

        public FluidFileReader(string fileLoc) {
            filePath = fileLoc;
            Sheet1 = new Sheet();
            ParseXml();
        }

        private void ParseXml() {
            Note note;
            baseDoc = new XmlDocument();
            baseDoc.Load(filePath);

            // Parse XML File
            System.Windows.Forms.MessageBox.Show(baseDoc.DocumentElement.FirstChild.FirstChild.Value);
        }
    }
}
