/*===================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
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
using FluidSys;
using OTOmate;

namespace FluidUI {
    /// <summary>
    /// Interaction logic for NoteRoll.xaml
    /// </summary>
    public partial class NoteRoll : UserControl {

        private List<RollElement> uiNotes = new List<RollElement>(2048);
        private Sheet noteSheet = new Sheet();
        private RollElement tempNote;
        private Canvas rollCaret;
        private OtoReader otoRead;
        private wavmod.WavMod wavMod;

        private int MouseDownLoc = 0;
        private int baseNoteLength = 480;
        private int totalNotesLength = 0;
        private int currentNoteIndex = 0;
        private int mouseDownNoteSize = 0;
        private int internalBpm = 120;

        private double dblNoteSnapping = 0;
        private double dblRollSnapping = 0;

        private bool isSizingNote = false;
        private bool isMovingNote = false;
        private bool isCreatingNote = false;
        private bool mouseOverNote = false;

        private string[] pitchNames = { "B", "A#", "A", "G#", "G", "F#", "F", "E", "D#", "D", "C#", "C", "B" };

        public Brush CurrentPianoKeyColor { get; set; }
        public Brush LightPianoKeyColor { get; set; }
        public Brush DarkPianoKeyColor { get; set; }

        public int GlobalBPM {
            get { return internalBpm; }
            set {
                foreach (RollElement note in bkgCanvas.Children) { note.BPM = value; }
                internalBpm = value;
                ResetAllLogicalNotesLength();
            }
        }

        public string MasterSampleBank {
            get { return noteSheet.Voicebank; }
            set {
                noteSheet.Voicebank = value;
                otoRead.OpenFile(noteSheet.Voicebank + "\\oto.ini");

                foreach (Note note in noteSheet.notes) {
                    note.VoiceProperties = otoRead.GetVoicePropFromSampleName(note.DispName);
                    System.Windows.Forms.MessageBox.Show("Test");
                }
            }
        }

        public string ResynthEngine {
            get { return noteSheet.Resampler; }
            set { noteSheet.Resampler = value; }
        }

        public enum Snapping {
            Quarter, Eighth, Sixteenth, Thirty_Second, None,
        }

        public Snapping NoteSnapping {
            get {
                if (dblNoteSnapping == 120) return Snapping.Quarter;
                else if (dblNoteSnapping == 60) return Snapping.Eighth;
                else if (dblNoteSnapping == 30) return Snapping.Sixteenth;
                else if (dblNoteSnapping == 15) return Snapping.Thirty_Second;
                else return Snapping.None;
            }
            set {
                if (value == Snapping.Quarter) dblNoteSnapping = 120;
                else if (value == Snapping.Eighth) dblNoteSnapping = 60;
                else if (value == Snapping.Sixteenth) dblNoteSnapping = 30;
                else if (value == Snapping.Thirty_Second) dblNoteSnapping = 15;
                else dblNoteSnapping = 0;
            }
        }

        public Snapping RollSnapping {
            get {
                if (dblRollSnapping == 120) return Snapping.Quarter;
                else if (dblRollSnapping == 60) return Snapping.Eighth;
                else if (dblRollSnapping == 30) return Snapping.Sixteenth;
                else if (dblRollSnapping == 15) return Snapping.Thirty_Second;
                else return Snapping.None;
            }
            set {
                if (value == Snapping.Quarter) dblRollSnapping = 120;
                else if (value == Snapping.Eighth) dblRollSnapping = 60;
                else if (value == Snapping.Sixteenth) dblRollSnapping = 30;
                else if (value == Snapping.Thirty_Second) dblRollSnapping = 15;
                else dblRollSnapping = 0;
            }
        }

        public NoteRoll() {
            InitializeComponent();

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "\\res\\01.PNG"));
            brush.TileMode = TileMode.Tile;
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(0, 0, brush.ImageSource.Width, brush.ImageSource.Height);
            brush.Stretch = Stretch.None;
            bkgCanvas.Background = brush;

            otoRead = new OtoReader();
            wavMod = new wavmod.WavMod();

            setDefaultColors();
            PaintPianoKeys();

            scroller.ScrollToVerticalOffset(560);

            NoteSnapping = Snapping.Sixteenth;
            RollSnapping = Snapping.Sixteenth;
        }

        /// <summary>
        /// Add a note to NoteSheet and add a RollElement representing the note
        /// to this control.
        /// </summary>
        /// <param name="location">The refrence point for locating the note. The note 
        /// will be further positioned to line up to the nearest lane. </param>
        /// <param name="length">The display length of the note. </param>
        /// <param name="index">the index to insert the note at, or -1 to
        /// add to the end of the list. </param>
        public void AddNote(Point location, int length, int index) {
            RollElement displayNote = new RollElement();
            Note logicalNote;
            
            displayNote.Width = length;
            displayNote.Height = 24;
            displayNote.BPM = internalBpm;
            displayNote.ElementPropertiesChanged += displayNote_ElementPropertiesChanged;
            displayNote.ElementRemoved += displayNote_ElementRemoved;
            displayNote.NoteIndex = index;
            displayNote.MouseEnter += displayNote_MouseEnter;
            displayNote.MouseLeave += displayNote_MouseLeave;
            displayNote.ElementMouseDown += displayNote_ElementMouseDown;
            displayNote.ElementMouseUp += displayNote_ElementMouseUp;

            bkgCanvas.Children.Add(displayNote);
            Canvas.SetLeft(displayNote, location.X);
            Canvas.SetTop(displayNote, location.Y);

            logicalNote = displayNote.ElementNote;
            logicalNote.NotePitch = generatePitchString((int)location.Y);
            logicalNote.VoiceProperties = otoRead.GetVoicePropFromSampleName(logicalNote.DispName);

            noteSheet.notes.Insert(index, logicalNote);

            //System.Windows.Forms.MessageBox.Show(noteSheet.notes[index].DispName + " " +
            //    noteSheet.notes[index].Length.ToString() + " " + noteSheet.notes[index].NotePitch);
        }

        void displayNote_ElementMouseUp(RollElement sender) { }
       
        void displayNote_ElementMouseDown(RollElement sender) {
            if (sender.IsMouseOverResize) {
                isSizingNote = true;
                currentNoteIndex = sender.NoteIndex;
                mouseDownNoteSize = (int)sender.Width;
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X - (int)sender.Width;
                Cursor = Cursors.SizeWE;
            }
            else if (Mouse.RightButton == MouseButtonState.Pressed) {
                
            }

            else {
                isMovingNote = true;
                currentNoteIndex = sender.NoteIndex;

                tempNote = new RollElement();
                tempNote.Width = sender.Width;
                tempNote.Height = 24;
                tempNote.Opacity = 0.5;

                //bkgCanvas.Children.Add(tempNote);
                //Canvas.SetLeft(tempNote, Mouse.GetPosition(bkgCanvas).X);
                //Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24); 
            }
        }

        void displayNote_MouseLeave(object sender, MouseEventArgs e) {
            mouseOverNote = false;
            dataDisp.Text = "Mouse left note " + ((RollElement)sender).NoteIndex;
        }

        void displayNote_MouseEnter(object sender, MouseEventArgs e) {
            mouseOverNote = true;
            dataDisp.Text = "Mouse entered note " + ((RollElement)sender).NoteIndex;
        }

        private void displayNote_ElementRemoved(RollElement sender) {
            noteSheet.notes.RemoveAt(sender.NoteIndex);
            foreach (RollElement dispNote in bkgCanvas.Children) {
                if (dispNote.NoteIndex >= sender.NoteIndex) {
                    Canvas.SetLeft(dispNote, Canvas.GetLeft(dispNote) - sender.Width);
                }
            }
            bkgCanvas.Children.Remove(sender);
            totalNotesLength -= (int)sender.Width;

            for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                ((RollElement)bkgCanvas.Children[i]).NoteIndex = i;
            }

            dataDisp.Text = "Removed note " + sender.NoteIndex.ToString() + ", \r\nnumber of display notes: " +
                bkgCanvas.Children.Count.ToString() + "\r\nnumber of logical notes: " + noteSheet.notes.Count.ToString();
        }

        private void displayNote_ElementPropertiesChanged(RollElement sender) {
            noteSheet.notes[sender.NoteIndex].VoiceProperties = otoRead.GetVoicePropFromSampleName(sender.NoteName);
        }

        /// <summary>
        /// Render and play 
        /// </summary>
        public void Play() {
            Renderer rnd = new Renderer(noteSheet);
            rnd.ShowRenderWindow = true;

            rnd.Render();

            wavMod.PlaybackTemp(rnd.TemporaryDir, noteSheet);
        }

        public void ResetAllLogicalNotesLength() {
            dataDisp.Text = "";
            for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                RollElement currentRollElement = (RollElement)bkgCanvas.Children[i];
                Note currentNote = noteSheet.notes[i];

                dataDisp.Text += " Old note length: " + currentNote.Length;

                currentNote.Length = currentRollElement.ElementNote.Length;
                currentNote.NotePitch = generatePitchString((int)Canvas.GetTop(currentRollElement));
                currentNote.VoiceProperties = otoRead.GetVoicePropFromSampleName(currentNote.DispName);

                dataDisp.Text += " New note length: " + currentNote.Length;
            }
        }

        private string generatePitchString(int noteLoc) {
            noteLoc = noteLoc / 24;
            int altNoteLoc = noteLoc;

            while (altNoteLoc > 12) { altNoteLoc = altNoteLoc - 12; }
            return pitchNames[altNoteLoc] + getOctave(noteLoc).ToString();
        }

        private void setDefaultColors() {
            LightPianoKeyColor = Brushes.Gainsboro;
            DarkPianoKeyColor = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
            CurrentPianoKeyColor = new SolidColorBrush(Color.FromArgb(255, 0, 132, 223)); 
        }

        private int getOctave(int noteLoc) {
            if (noteLoc / 12 == 0) return 6;
            else if (noteLoc / 12 == 1) return 5;
            else if (noteLoc / 12 == 2) return 4;
            else if (noteLoc / 12 == 3) return 3;
            else if (noteLoc / 12 == 4) return 2;
            else if (noteLoc / 12 == 5) return 1;
            else return 0;
        }

        private void PaintPianoKeys() { 
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

                c.Content = "C" + i.ToString();    cs.Content = "C#" + i.ToString();
                d.Content = "D" + i.ToString();    ds.Content = "D#" + i.ToString();
                e.Content = "E" + i.ToString();    f.Content = "F" + i.ToString();
                fs.Content = "F#" + i.ToString();  g.Content = "G" + i.ToString();
                gs.Content = "G#" + i.ToString();  a.Content = "A" + i.ToString();
                ar.Content = "A#" + i.ToString();  b.Content = "B" + i.ToString();

                cs.Background = DarkPianoKeyColor;
                ds.Background = DarkPianoKeyColor;
                fs.Background = DarkPianoKeyColor;
                gs.Background = DarkPianoKeyColor;
                ar.Background = DarkPianoKeyColor;

                cs.Foreground = LightPianoKeyColor;
                ds.Foreground = LightPianoKeyColor;
                fs.Foreground = LightPianoKeyColor;
                gs.Foreground = LightPianoKeyColor;
                ar.Foreground = LightPianoKeyColor;

                pianoPanel.Children.Add(b);   pianoPanel.Children.Add(ar);
                pianoPanel.Children.Add(a);   pianoPanel.Children.Add(gs);
                pianoPanel.Children.Add(g);   pianoPanel.Children.Add(fs);
                pianoPanel.Children.Add(f);   pianoPanel.Children.Add(e);
                pianoPanel.Children.Add(ds);  pianoPanel.Children.Add(d);
                pianoPanel.Children.Add(cs);  pianoPanel.Children.Add(c);
            }
        }

        private void highlightPianoKey(int position) {
            foreach (Label noteLabel in pianoPanel.Children) {
                if (((string)noteLabel.Content).IndexOf("#") != -1) {
                    noteLabel.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                }
                else noteLabel.Background = Brushes.Gainsboro;
            }

            foreach (Label noteLabel in pianoPanel.Children) {
                string currentMousePitch = generatePitchString((int)Mouse.GetPosition(bkgCanvas).Y);

                if (((string)noteLabel.Content).IndexOf(currentMousePitch) != -1) noteLabel.Background = CurrentPianoKeyColor;
            }
        }

        private void bkgCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
            if (!mouseOverNote) {
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X;

                tempNote = new RollElement();
                tempNote.Width = 40;
                tempNote.Height = 24;
                tempNote.Opacity = 0.5;

                bkgCanvas.Children.Add(tempNote);
                Canvas.SetLeft(tempNote, Mouse.GetPosition(bkgCanvas).X);
                Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24); 
            }

            isCreatingNote = true;
        }
        
        private void bkgCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
            if (MouseDownLoc + 30 < Mouse.GetPosition(bkgCanvas).X && !isMovingNote && !isSizingNote 
                && isCreatingNote) {
                Point mouseLoc = new Point();
                //mouseLoc.X = MouseDownLoc / 24 * 24;
                mouseLoc.X = totalNotesLength;
                mouseLoc.Y = (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24;

                AddNote(mouseLoc, (int)tempNote.Width, noteSheet.notes.Count);
                totalNotesLength += (int)((RollElement)bkgCanvas.Children[noteSheet.notes.Count - 1]).Width;
                dataDisp.Text = "totalNotesLength=\r\n" + totalNotesLength;
                dataDisp.Text += "\r\nmouseLoc.X=\r\n" + mouseLoc.X;
            }

            if (isMovingNote) {
                Canvas.SetTop(bkgCanvas.Children[currentNoteIndex], (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                noteSheet.notes[currentNoteIndex].NotePitch =
                    generatePitchString((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
            }
             
            if (isSizingNote) {
                foreach (RollElement dispNote in bkgCanvas.Children) {
                    if (dispNote.NoteIndex > currentNoteIndex) {
                        Canvas.SetLeft(dispNote, Canvas.GetLeft(dispNote)
                            - (mouseDownNoteSize - ((RollElement)bkgCanvas.Children[currentNoteIndex]).Width));
                    }
                }
                dataDisp.Text = "Total Notes Length: " + totalNotesLength.ToString() + " Difference: " +
                    (mouseDownNoteSize - (int)((RollElement)bkgCanvas.Children[currentNoteIndex]).Width).ToString();

                totalNotesLength -= (mouseDownNoteSize - (int)((RollElement)bkgCanvas.Children[currentNoteIndex]).Width);

                dataDisp.Text += " Total Notes Length after operation: " + totalNotesLength.ToString();
            }

            bkgCanvas.Children.Remove(tempNote);

            isSizingNote = false;
            isCreatingNote = false;
            isMovingNote = false;

            Cursor = Cursors.Arrow;
        }

        private void bkgCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (isCreatingNote) {
                try {
                    tempNote.Width = ((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc) 
                        / (int)dblNoteSnapping * (int)dblNoteSnapping;
                    Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                }
                catch (Exception) { }             
            }

            if (isMovingNote) {
                Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                dataDisp.Text = ((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24).ToString();
            }

            if (isSizingNote) {
                if (((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc > 0)) {
                    ((RollElement)bkgCanvas.Children[currentNoteIndex]).Width = ((int)Mouse.GetPosition(bkgCanvas).X
                        - MouseDownLoc) / (int)dblNoteSnapping * (int)dblNoteSnapping; 
                }
                noteSheet.notes[currentNoteIndex].Length =
                    ((RollElement)bkgCanvas.Children[currentNoteIndex]).ElementNote.Length;

                dataDisp.Text = "Logical note length=" + noteSheet.notes[currentNoteIndex].Length +
                    "\r\nDisplay note length=" + ((RollElement)bkgCanvas.Children[currentNoteIndex]).Width;
            }

            highlightPianoKey((int)Mouse.GetPosition(bkgCanvas).X);
        }

        private void scroller_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            fred.ScrollToVerticalOffset(scroller.VerticalOffset);
        }
    }
}
