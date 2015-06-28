/*====================================================*\
 *|| Copyright(c) KineticIsEpic. All Rights Reserved. ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using FluidSys;
using OTOmate;
using wavmod;

namespace FluidUI {
    /// <summary>
    /// Interaction logic for Roll.xaml
    /// </summary>
    public partial class Roll : UserControl {
        private Sheet noteSheet = new Sheet();

        /// <summary>
        /// Gets or sets the global tempo for this FluidUI.Roll, in Beats Per Minute (BPM).
        /// </summary>
        public int Tempo {
            get { return baseTempo; }
            set {
                foreach (DockPanel notePane in notePanel.Children) {
                    foreach (RollElement note in notePane.Children) {
                        note.BPM = baseTempo;
                        noteSheet.notes[note.NoteIndex].Length = note.ElementNote.Length;
                        System.Windows.Forms.MessageBox.Show(noteSheet.notes[note.NoteIndex].Length.ToString());
                    }
                }
                baseTempo = value;
                noteSheet.Bpm = baseTempo;
            }
        }

        public string RendererPath { get; set; }
        public string Samplebank {
            get { return intVb; }
            set { 
                intVb = value;
                oRead.OpenFile(intVb + "\\oto.ini");
            }
        }

        List<RollElement>[] notes = new List<RollElement>[72];

        OtoReader oRead = new OtoReader();
        Rectangle NotePreviewRect;

        string intVb = "";

        int baseTempo = 0;
        int xRows = 72;
        int mouseDownPos = 0;
        int allNotesLength = 0;
        int totalNotes = 0;
        int[] usedLeftSpace = new int[72];
        int removedNoteLength = 0;

        RollElement NextNote = new RollElement();

        public Roll() {
            InitializeComponent();

            //Image Img = new Image();
            //Img.Source = new BitmapImage(new Uri("")); //TOOD: create path that is program directory + img name

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);

            int whatever = 0;
            int indexer = 0;
            int vartwo = 0;

            for (int i = 0; i < 72; i++) {

                notes[i] = new List<RollElement>(1024);

                DockPanel notePane = new DockPanel();
                notePane.Height = 24;
                notePane.Width = notePanel.Width;
                notePane.Tag = new object[2];
                notePane.Background = Brushes.DimGray;
                ((object[])notePane.Tag)[0] = i;

                if (i % 2 != 0) notePane.Background = Brushes.Gray;
                else notePane.Background = Brushes.DimGray;

                notePanel.Children.Add(notePane);

                notePane.MouseDown += rect_MouseDown;
                notePane.MouseUp += rect_MouseUp;
                notePane.MouseMove += notePane_MouseMove;

                if (whatever == 0) indexer = 6;
                if (whatever == 1) indexer = 5;
                if (whatever == 2) indexer = 4;
                if (whatever == 3) indexer = 3;
                if (whatever == 4) indexer = 2;
                if (whatever == 5) indexer = 1;
                if (whatever == 6) indexer = 0;

                vartwo = i - (whatever * 12);

                if (vartwo == 1) ((object[])notePane.Tag)[1] = "A#" + indexer.ToString();
                if (vartwo == 2) ((object[])notePane.Tag)[1] = "A" + indexer.ToString();
                if (vartwo == 3) ((object[])notePane.Tag)[1] = "G#" + indexer.ToString();
                if (vartwo == 4) ((object[])notePane.Tag)[1]  = "G" + indexer.ToString();
                if (vartwo == 5) ((object[])notePane.Tag)[1]  = "F#" + indexer.ToString();
                if (vartwo == 6) ((object[])notePane.Tag)[1]  = "F" + indexer.ToString();
                if (vartwo == 7) ((object[])notePane.Tag)[1]  = "E" + indexer.ToString();
                if (vartwo == 8) ((object[])notePane.Tag)[1]  = "D#" + indexer.ToString();
                if (vartwo == 9) ((object[])notePane.Tag)[1]  = "D" + indexer.ToString();
                if (vartwo == 10) ((object[])notePane.Tag)[1]  = "C#" + indexer.ToString();
                if (vartwo == 11) ((object[])notePane.Tag)[1]  = "C" + indexer.ToString();
                if (vartwo == 12) ((object[])notePane.Tag)[1]  = "B" + (indexer - 1).ToString();
 
                whatever = i / 12;               
            }

            for (int i = 6; i > 0; i--) {
                Label c = new Label();
                Label cs = new Label();
                Label d = new Label();
                Label ds = new Label();
                Label e = new Label();
                Label f = new Label();
                Label fs = new Label();
                Label g = new Label();
                Label gs = new Label();
                Label a = new Label();
                Label ar = new Label();
                Label b = new Label();

                c.Width = cs.Width = d.Width = ds.Width = e.Width = f.Width = fs.Width = g.Width =
                    gs.Width = a.Width = ar.Width = b.Width = pianoPanel.Width;

                c.Height = cs.Height = d.Height = ds.Height = e.Height = f.Height = fs.Height =
                    g.Height = gs.Height = a.Height = ar.Height = b.Height = 24;

                c.Content = "C" + i.ToString();        cs.Content = "C#" + i.ToString();
                d.Content = "D" + i.ToString();        ds.Content = "D#" + i.ToString();
                e.Content = "E" + i.ToString();        f.Content = "F" + i.ToString();
                fs.Content = "F#" + i.ToString();      g.Content = "G" + i.ToString();
                gs.Content = "G#" + i.ToString();      a.Content = "A" + i.ToString();
                ar.Content = "A#" + i.ToString();      b.Content = "B" + i.ToString();

                cs.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                ds.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                fs.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                gs.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                ar.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));

                cs.Foreground = Brushes.Gainsboro;
                ds.Foreground = Brushes.Gainsboro;
                fs.Foreground = Brushes.Gainsboro;
                gs.Foreground = Brushes.Gainsboro;
                ar.Foreground = Brushes.Gainsboro;

                pianoPanel.Children.Add(b);
                pianoPanel.Children.Add(ar);
                pianoPanel.Children.Add(a);
                pianoPanel.Children.Add(gs);
                pianoPanel.Children.Add(g);
                pianoPanel.Children.Add(fs);
                pianoPanel.Children.Add(f);
                pianoPanel.Children.Add(e);
                pianoPanel.Children.Add(ds);
                pianoPanel.Children.Add(d);
                pianoPanel.Children.Add(cs);
                pianoPanel.Children.Add(c);
            }

            scroller.ScrollToVerticalOffset(560);
        }

        /// <summary>
        /// Render all notes in queue, then play them back.
        /// </summary>
        public void Play() {
            try {
                noteSheet.Bpm = 120;
                noteSheet.Resampler = RendererPath;
                noteSheet.Voicebank = intVb;
                noteSheet.Name = "FluidUI Project"; 

                WavMod wvmd = new WavMod();
                Renderer rendSys = new Renderer(noteSheet);

                //rendSys.ShowRenderWindow = true;z
                rendSys.Render();

                wvmd.PlaybackTemp(rendSys.TemporaryDir, noteSheet);
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Get a string with basic details about the global tempo of this FluidUI.Roll.
        /// </summary>
        /// <returns></returns>
        public string GetBpmDetails() {
            string str = "Tempo = ";
            str += baseTempo.ToString() + "\r\nQuarter note = ";
            str += (60000 / baseTempo).ToString() + "mSec\r\n";
            return str;
        }
    
        public void RemoveSelectedNotes() {
            int indexer = 0;
            int indexer2 = 0;
            foreach (DockPanel notePane in notePanel.Children) {
                foreach (RollElement note in notePane.Children) {
                    if (note.IsSelected) {
                        notePane.Children.Remove(note);
                        allNotesLength -= (int)note.Width - (int)note.Margin.Left;
                        removedNoteLength = (int)note.Width;
                        totalNotes--;
                        RemoveSelectedNotes();
                        Roll_ElementRemoved(note);
                        break;
                    }
                    indexer++;
                }
                indexer2++;
            }
        }

        public void AddNote(int length, int index, string pitch, DockPanel panelToAddNote) {
            RollElement noteToAdd = new RollElement();
            int spaceToLeft = getAllLeftSpace(panelToAddNote);

            foreach (RollElement note in panelToAddNote.Children) spaceToLeft -= ((int)note.Width + (int)note.Margin.Left);

            noteToAdd.Height = 24;
            noteToAdd.Width = length;
            noteToAdd.Margin = new Thickness(spaceToLeft, 0, 0, 0);
            noteToAdd.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            noteToAdd.Pitch = pitch;
            noteToAdd.ElementPropertiesChanged += Roll_ElementPropertiesChanged;
            noteToAdd.ElementRemoved += Roll_ElementRemoved;
            noteToAdd.ElementSelected += Roll_ElementSelected;
            noteToAdd.NoteIndex = index;
            noteToAdd.BPM = baseTempo;

            panelToAddNote.Children.Insert(index - countNotesNotOnLane(panelToAddNote), noteToAdd);

            Note n = noteToAdd.ElementNote;
            n.VoiceProperties = oRead.GetVoicePropFromSampleName(n.DispName);
            noteSheet.notes.Insert(index, n);

            totalNotes++; 
        }

        private int getAllLeftSpace(DockPanel nPanel) {
            int allleftspace = 0;

            foreach (DockPanel notePane in notePanel.Children) {
                foreach (RollElement note in notePane.Children) {
                    if (note.Parent != nPanel) allleftspace += (int)note.Width;
                }
            }
            return allleftspace;
        }

        private int countNotesNotOnLane(DockPanel lane) {
            int notesNotInLane = 0;

            foreach (DockPanel notePane in notePanel.Children) {
                foreach (RollElement note in notePane.Children) {
                    if (lane.Children.IndexOf(note) == -1) notesNotInLane++;
                }
            }
            return notesNotInLane;
        }

        private void paintBarLines(int barsep, int beat) {

            foreach (DockPanel notePane in notePanel.Children) {
                for (int i = 0; i < 27; i++) {
                    
                    Rectangle bar = new Rectangle();
                    bar.Width = 2;
                    bar.Height = 24;
                    bar.Margin = new Thickness(0, 0, barsep, 0);
                    bar.Fill = Brushes.DimGray;
                    bar.MouseDown += rect_MouseDown;
                    bar.MouseUp += rect_MouseUp;

                    notePane.Children.Add(bar);
                }
            }
        }

        void notePane_MouseMove(object sender, MouseEventArgs e) {

        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) RemoveSelectedNotes();
        }

        private void rect_MouseUp(object sender, MouseButtonEventArgs e) {
            int mouseUpPos = (int)Mouse.GetPosition((DockPanel)sender).X;

            int xIndex = (int)((object[])((DockPanel)sender).Tag)[0];
            int yIndex = 0;

            if (mouseDownPos < mouseUpPos - 20) {
                yIndex = notes[xIndex].Count;

                int spaceToLeft = allNotesLength;

                foreach (RollElement note in ((DockPanel)sender).Children)
                    spaceToLeft -= ((int)note.Width + (int)note.Margin.Left);

                notes[xIndex].Add(new RollElement());

                notes[xIndex][yIndex].Height = 24;
                notes[xIndex][yIndex].Width = mouseUpPos - mouseDownPos;
                notes[xIndex][yIndex].Margin = new Thickness(spaceToLeft, 0, 0, 0);
                notes[xIndex][yIndex].Background = Brushes.Red;
                notes[xIndex][yIndex].HorizontalAlignment = HorizontalAlignment.Left;
                notes[xIndex][yIndex].Pitch = (string)((object[])((DockPanel)sender).Tag)[1];
                notes[xIndex][yIndex].ElementPropertiesChanged += Roll_ElementPropertiesChanged;
                notes[xIndex][yIndex].ElementRemoved += Roll_ElementRemoved;
                notes[xIndex][yIndex].ElementSelected += Roll_ElementSelected;
                notes[xIndex][yIndex].NoteIndex = totalNotes;
                notes[xIndex][yIndex].BPM = baseTempo;

                if (notes[xIndex][yIndex].Margin.Left < 0) notes[xIndex][yIndex].Margin = new Thickness(0, 0, 0, 0);

                ((DockPanel)sender).Children.Add(notes[xIndex][yIndex]);

                allNotesLength += (int)notes[xIndex][yIndex].Width;
                usedLeftSpace[xIndex] += (int)notes[xIndex][yIndex].Margin.Left + (int)notes[xIndex][yIndex].Width;

                Note n = notes[xIndex][yIndex].ElementNote;
                n.VoiceProperties = oRead.GetVoicePropFromSampleName(n.DispName);
                noteSheet.notes.Insert(totalNotes, n);
                totalNotes++;

                //AddNote(mouseUpPos - mouseDownPos, totalNotes, (string)((object[])((DockPanel)sender).Tag)[1],
                //    ((DockPanel)sender));
            }
        }

        void Roll_ElementSelected(RollElement sender, bool isSelected) { }

        void Roll_ElementRemoved(RollElement sender) {
            noteSheet.notes.RemoveAt(sender.NoteIndex);
            try {
                foreach (DockPanel notePane in notePanel.Children) {
                    foreach (RollElement note in notePane.Children) {
                        if (note.NoteIndex == sender.NoteIndex + 1) 
                            note.Margin = new Thickness(note.Margin.Left - sender.Width, 0, 0, 0);                        
                    }
                }

            }
            catch (Exception) { }
        }

        void Roll_ElementPropertiesChanged(RollElement sender) {
            noteSheet.notes[sender.NoteIndex].DispName = sender.NoteName;
            noteSheet.notes[sender.NoteIndex].VoiceProperties = oRead.GetVoicePropFromSampleName(sender.NoteName);
        }

        private void rect_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownPos = (int)Mouse.GetPosition((DockPanel)sender).X;
        }
        
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            //notePanel.Width = this.Width;
            //notePanel.Height = this.Height;
        }

        private void scroller_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            fred.ScrollToVerticalOffset(scroller.VerticalOffset);
        }

        private void Label_SourceUpdated(object sender, DataTransferEventArgs e) {
            
        }

    }
}
