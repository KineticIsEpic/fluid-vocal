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
        private Sheet baseSheet;
        private XmlDocument baseXml;
        string filePath;

        public FluidFileWriter(string path, Sheet sheet) {
            baseSheet = sheet;
            filePath = path;
            genXml();
        }

        /// <summary>
        /// Saves the current project.
        /// </summary>
        public void SaveFile() {
            baseXml.Save(filePath);
        }

        private void genXml() {
            baseXml = new XmlDocument();

            // Create XML declaration
            XmlDeclaration xmlDeclaration = baseXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = baseXml.DocumentElement;
            baseXml.InsertBefore(xmlDeclaration, root);

            // Create root
            XmlElement element1 = baseXml.CreateElement(string.Empty, "Project", string.Empty);
            baseXml.AppendChild(element1);

            // Create Details section
            XmlElement element2 = baseXml.CreateElement(string.Empty, "Details", string.Empty);
            element1.AppendChild(element2);

            XmlElement element3 = baseXml.CreateElement(string.Empty, "Name", string.Empty);
            XmlText text1 = baseXml.CreateTextNode(baseSheet.Name);
            element3.AppendChild(text1);
            element2.AppendChild(element3);

            // Create Tracks section
            XmlElement tracksElement = baseXml.CreateElement(string.Empty, "Tracks", string.Empty);
            element1.AppendChild(tracksElement);

            // Add main track
            XmlElement track1Element = baseXml.CreateElement(string.Empty, "Track1", string.Empty);
            track1Element.SetAttribute("Name", "Main");
            tracksElement.AppendChild(track1Element);

            // Add track properties
            XmlElement track1Vb = baseXml.CreateElement(string.Empty, "SampleBank", string.Empty);
            XmlElement track1Render = baseXml.CreateElement(string.Empty, "Renderer", string.Empty);
            XmlText t1VbText = baseXml.CreateTextNode(baseSheet.Voicebank);
            XmlText t1RText = baseXml.CreateTextNode(baseSheet.Resampler);
            
            track1Vb.AppendChild(t1VbText);
            track1Render.AppendChild(t1RText);

            track1Element.AppendChild(track1Vb);
            track1Element.AppendChild(track1Render);

            // Add note list
            XmlElement noteListE = baseXml.CreateElement(string.Empty, "NoteList", string.Empty);
            track1Element.AppendChild(noteListE);

            int run = 0;

            // Add notes
            foreach (var note in baseSheet.notes) {
                XmlElement noteElement = baseXml.CreateElement(string.Empty, "Note" + (run).ToString(), string.Empty);
                noteListE.AppendChild(noteElement);

                XmlElement vbPath = baseXml.CreateElement(string.Empty, "VbPath", string.Empty);
                XmlElement dispName = baseXml.CreateElement(string.Empty, "DispName", string.Empty);
                XmlElement fileName = baseXml.CreateElement(string.Empty, "FileName", string.Empty);
                XmlElement notePitch = baseXml.CreateElement(string.Empty, "NotePitch", string.Empty);
                XmlElement args = baseXml.CreateElement(string.Empty, "Args", string.Empty);
                XmlElement length = baseXml.CreateElement(string.Empty, "Length", string.Empty);
                XmlElement location = baseXml.CreateElement(string.Empty, "Location", string.Empty);
                XmlElement useDefVb = baseXml.CreateElement(string.Empty, "UseDefaultVb", string.Empty);

                XmlText vbPathTxt = baseXml.CreateTextNode(note.VbPath);
                XmlText dispNameTxt = baseXml.CreateTextNode(note.DispName);
                XmlText fileNameTxt = baseXml.CreateTextNode(note.FileName);
                XmlText notePitchTxt = baseXml.CreateTextNode(note.NotePitch);
                XmlText argsTxt = baseXml.CreateTextNode(note.Args);
                XmlText lengthTxt = baseXml.CreateTextNode(note.Length.ToString());
                XmlText locationTxt = baseXml.CreateTextNode(note.Location.ToString());
                XmlText useDefVbTxt = baseXml.CreateTextNode(note.UseDefaultVb.ToString());

                vbPath.AppendChild(vbPathTxt);
                dispName.AppendChild(dispNameTxt);
                fileName.AppendChild(fileNameTxt);
                notePitch.AppendChild(notePitchTxt);
                args.AppendChild(argsTxt);
                length.AppendChild(lengthTxt);
                location.AppendChild(locationTxt);
                useDefVb.AppendChild(useDefVbTxt);

                noteElement.AppendChild(vbPath);
                noteElement.AppendChild(dispName);
                noteElement.AppendChild(fileName);
                noteElement.AppendChild(notePitch);
                noteElement.AppendChild(args);
                noteElement.AppendChild(length);
                noteElement.AppendChild(location);
                noteElement.AppendChild(useDefVb);

                run++;
            }
        }
    }
}
