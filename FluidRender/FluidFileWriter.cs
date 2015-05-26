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
using System.Xml;

namespace FluidSys {
    public class FluidFileWriter {
        private FileStream baseWriter;
        private Sheet baseSheet;
        private XmlDocument baseXml;
        private string textForWrite;

        public FluidFileWriter(string path, Sheet sheet) {
            baseWriter = new FileStream(path,FileMode.Create);
            baseSheet = sheet;
            genXml();
        }

        private void genXml() {
            baseXml = new XmlDocument();
            baseXml.CreateNode(XmlNodeType.DocumentFragment, "test", "www.example.com");
            System.Windows.Forms.MessageBox.Show(baseXml.InnerText);    
        }
    }
}
