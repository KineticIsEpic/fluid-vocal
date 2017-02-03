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
    public delegate void NoteSelectEventHandler(Note workingNote);
    /// <summary>
    /// Interaction logic for NoteRoll.xaml
    /// </summary>
    public partial class NoteRoll : UserControl {
        public event NoteSelectEventHandler NoteSelected;

        private List<RollElement> uiNotes = new List<RollElement>(2048);
        private Sheet noteSheet = new Sheet();
        private RollElement tempNote;
        private Rectangle rollCaret;
        private OtoReader otoRead;
        private FluidFileWriter fileWriter;
        private NoteMenu rightClickMenu;
        private wavmod.WavMod wavMod;
        private Point mouseDownScrollLoc;

        private int MouseDownLoc = 0;
        private int baseNoteLength = 480;
        private int totalNotesLength = 0;
        private int currentNoteIndex = 0;
        private int mouseDownNoteSize = 0;
        private int internalBpm = 120;
        private int currentNoteDist;

        private double dblNoteSnapping = 0;
        private double dblRollSnapping = 0;
        private double dblDefNoteSize = 0;
        private double hZoomFactor = 1;

        private bool isSizingNote = false;
        private bool isMovingNote = false;
        private bool isCreatingNote = false;
        private bool mouseOverNote = false;
        private bool isInitalScroll = false;
        private bool isEditMode = false;

        private Snapping tempNS = Snapping.Quarter;
        private Snapping tempRS = Snapping.Quarter;
        private Snapping defNS = Snapping.Quarter;

        private string[] pitchNames = { "B", "A#", "A", "G#", "G", "F#", "F", "E", "D#", "D", "C#", "C", "B" };
        private List<int> noteDists = new List<int>(256);

        public Brush CurrentPianoKeyColor { get; set; }
        public Brush LightPianoKeyColor { get; set; }
        public Brush DarkPianoKeyColor { get; set; }

        /// <summary>
        /// Was going to be used for the 2-mode editor (one view for adding notes and one for tuning),
        /// however I scrapped that idea. Leaving for now to prevent wrecking the code. 
        /// </summary>
        public MainWindow.EditorMode RollEditMode {
            get {
                if (isEditMode) return MainWindow.EditorMode.Tuning;
                else return MainWindow.EditorMode.Editing;
            }
            set {
                if (value == MainWindow.EditorMode.Tuning) {
                    topGrid.Height = 170;
                    topGrid.Visibility = System.Windows.Visibility.Visible;

                    isEditMode = true;
                }
                else {
                    topGrid.Height = 0;
                    topGrid.Visibility = System.Windows.Visibility.Hidden;

                    isEditMode = false;
                }
            }
        }

        /// <summary>
        /// Pencil: drag to add notes
        /// Brush: note is added on mousedown, move to relocate it
        /// </summary>
        public enum EditorTool {
            Pencil, Brush, Eraser,
        }

        /// <summary>
        /// makeshift note rest path 
        /// </summary>
        public string RestPath { get; set; }

        /// <summary>
        /// Used(?) to tell if the project's been saved
        /// </summary>
        public bool IsNewProject {
            get {
                if (fileWriter == null) return false;
                else return true;
            }
        }

        /// <summary>
        /// Global tempo - will perhaps also be used for zooming in the future
        /// </summary>
        public int GlobalBPM {
            get { return internalBpm; }
            set {
                foreach (var note in bkgCanvas.Children) {
                    try { ((RollElement)note).BPM = value; }
                    catch (Exception) { }
                }
                internalBpm = value;
                noteSheet.Bpm = internalBpm;
                ResetAllLogicalNotesLength();
            }
        }

        /// <summary>
        /// Voice bank
        /// </summary>
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

        public string WavTool {
            get { return new ConfigMgr().DefaultWavTool; }
            set { new ConfigMgr().DefaultWavTool = value; }
        }

        public string ResynthEngine {
            get { return noteSheet.Resampler; }
            set { noteSheet.Resampler = value; }
        }

        public enum Snapping {
            Quarter, Eighth, Sixteenth, Thirty_Second, None,
        }

        public Snapping NoteSnapping {
            get { return tempNS; }
            set {
                if (value == Snapping.Quarter) dblNoteSnapping = 120 * hZoomFactor;
                else if (value == Snapping.Eighth) dblNoteSnapping = 60 * hZoomFactor;
                else if (value == Snapping.Sixteenth) dblNoteSnapping = 30 * hZoomFactor;
                else if (value == Snapping.Thirty_Second) dblNoteSnapping = 15 * hZoomFactor;
                else dblNoteSnapping = 1;
                tempNS = value;
            }
        }

        /// <summary>
        /// IDK what this is for, keeping (for now) to keep from wrecking the code
        /// </summary>
        public Snapping RollSnapping {
            get { return tempRS; }
            set {
                if (value == Snapping.Quarter) dblRollSnapping = 120 * hZoomFactor;
                else if (value == Snapping.Eighth) dblRollSnapping = 60 * hZoomFactor;
                else if (value == Snapping.Sixteenth) dblRollSnapping = 30 * hZoomFactor;
                else if (value == Snapping.Thirty_Second) dblRollSnapping = 15 * hZoomFactor;
                else dblRollSnapping = 1;
                tempRS = value;
            }
        }

        /// <summary>
        /// To be used for the brush tool most likely
        /// </summary>
        public Snapping DefNoteSize {
            get { return defNS; }
            set {
                if (value == Snapping.Quarter) dblRollSnapping = 120;
                else if (value == Snapping.Eighth) dblRollSnapping = 60;
                else if (value == Snapping.Sixteenth) dblRollSnapping = 30;
                else if (value == Snapping.Thirty_Second) dblRollSnapping = 15;
                else dblDefNoteSize = 1;
                defNS = value;
            }
        }

        public EditorTool editorTool { get; set; }

        public NoteRoll() {
            InitializeComponent();
            ImageBrush brush = new ImageBrush();

            // Set background image 
            //TODO: Draw the background image by code (this was done in the lost version)
            try {
                brush.ImageSource = new BitmapImage
                    (new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "\\res\\01.PNG"));
                brush.TileMode = TileMode.Tile;
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Viewport = new Rect(0, 0, brush.ImageSource.Width, brush.ImageSource.Height);
                brush.Stretch = Stretch.None;
                bkgCanvas.Background = brush;
            }
            catch (Exception) { }

            otoRead = new OtoReader();
            wavMod = new wavmod.WavMod();

            // setup stuff
            setDefaultColors();
            PaintPianoKeys();
            paintTimeBarTicks();

            //TODO: could use some code to seek to C4 
            scroller.ScrollToVerticalOffset(560);

            //TODO: perhaps recall user's last setting
            NoteSnapping = Snapping.Sixteenth;
            RollSnapping = Snapping.Sixteenth;

            // set up roll caret (the small blue thing in the timebar)
            rollCaret = new Rectangle();
            rollCaret.Width = 6;
            rollCaret.Height = 100;
            rollCaret.Fill = Brushes.DodgerBlue;
            rollCaret.Stroke = null;
            timeBarSlider.Children.Add(rollCaret);

            // set up right-click menu
            rightClickMenu = new NoteMenu();
            rightClickMenu.Visibility = System.Windows.Visibility.Hidden;
            rightClickMenu.CloseClicked += rightClickMenu_CloseClicked;
            overlayCanvas.Children.Add(rightClickMenu);

            // sync tempo
            noteSheet.Bpm = internalBpm;

            // set up zooming/scaling
        }

        // deselects the right-clicked note when the menu is closed
        void rightClickMenu_CloseClicked() {
            ((RollElement)bkgCanvas.Children[rightClickMenu.WorkingNoteIndex]).IsSelected = false;
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
        public RollElement AddNote(Point location, int length, int index) {
            RollElement displayNote = new RollElement();
            Note logicalNote;
            
            // configure the RollElement
            displayNote.Width = length;
            displayNote.Height = 24;
            displayNote.BPM = internalBpm;
            displayNote.ElementPropertiesChanged += displayNote_ElementPropertiesChanged;
            displayNote.ElementRemoved += displayNote_ElementRemoved;
            displayNote.NoteIndex = index;
            displayNote.MouseEnter += displayNote_MouseEnter;
            displayNote.MouseLeave += displayNote_MouseLeave;
            displayNote.ElementMouseDown += displayNote_ElementMouseDown;
            displayNote.ElementMouseUp += DisplayNote_ElementMouseUp;
            displayNote.ZoomFactor = hZoomFactor;

            // add it to the canvas
            bkgCanvas.Children.Add(displayNote);
            Canvas.SetLeft(displayNote, location.X);
            Canvas.SetTop(displayNote, location.Y);

            // setup logical note based off of the element
            logicalNote = displayNote.ElementNote;
            logicalNote.NotePitch = generatePitchString((int)location.Y);
            logicalNote.VoiceProperties = otoRead.GetVoicePropFromSampleName(logicalNote.DispName);
            logicalNote.GenerateDefaultEnvelope();

            // add it to the NoteSheet
            noteSheet.notes.Insert(index, logicalNote);
            return displayNote;
        }

        private void DisplayNote_ElementMouseUp(RollElement sender) {
                // setup and display right-click menu
                rightClickMenu.WorkingNoteIndex = sender.NoteIndex;
                rightClickMenu.WorkingNote = noteSheet.notes[sender.NoteIndex];
                Canvas.SetLeft(rightClickMenu, Mouse.GetPosition(overlayCanvas).X);
                Canvas.SetTop(rightClickMenu, Mouse.GetPosition(overlayCanvas).Y);
                rightClickMenu.Visibility = Visibility.Visible;
            
        }

        void displayNote_ElementMouseDown(RollElement sender) {
     
            if (sender.IsMouseOverResize) {
                // init resize, done by bkgCanvas_MouseMove
                isSizingNote = true; 
                currentNoteIndex = sender.NoteIndex; 

                // used in the resize job:
                mouseDownNoteSize = (int)sender.Width; 
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X - (int)sender.Width;

                // set the cursor to a resize arrow
                Cursor = Cursors.SizeWE;
            }
            // move the note if the mouse isn't on the resizer or right-clicking
            else {
                // init move, done by bkgCanvas_MouseMove
                isMovingNote = true;
                currentNoteIndex = sender.NoteIndex;
            }
        }

        void displayNote_MouseLeave(object sender, MouseEventArgs e) {
            // debug info
            mouseOverNote = false;
            dataDisp.Text = "Mouse left note " + ((RollElement)sender).NoteIndex;
        }

        void displayNote_MouseEnter(object sender, MouseEventArgs e) {
            // debug info
            mouseOverNote = true;
            dataDisp.Text = "Mouse entered note " + ((RollElement)sender).NoteIndex;
        }

        public void ZoomIn() {
            hZoomFactor += 0.2;
            bkgCanvas.Background.Transform = new ScaleTransform(hZoomFactor, 1);
            handleZoom();
        }

        public void ZoomOut() {
            hZoomFactor -= 0.2;
            bkgCanvas.Background.Transform = new ScaleTransform(hZoomFactor, 1);
            handleZoom();
        }

        private void handleZoom() {
            // rescale everything
            foreach (RollElement element in bkgCanvas.Children) {
                element.Width = element.Width / element.ZoomFactor * hZoomFactor;
                Canvas.SetLeft(element, Canvas.GetLeft(element) / element.ZoomFactor * hZoomFactor);
                element.ZoomFactor = hZoomFactor;
            }

            // redo the note distance list
            updateNoteDists();

            // reset scaling options, to get them to reflect the change
            NoteSnapping = tempNS;
            RollSnapping = tempRS;
            DefNoteSize = defNS;
        }

        private void displayNote_ElementRemoved(RollElement sender) {
            // remove the corresponding logical note (needs testing to assure it's removing the right note!)
            noteSheet.notes.RemoveAt(sender.NoteIndex);

            // move notes to the right backward to fill the space
            // perhaps an option to fill with rest instead (like UTAU) would be nice
            foreach (RollElement dispNote in bkgCanvas.Children) {
                if (dispNote.NoteIndex >= sender.NoteIndex) {
                    Canvas.SetLeft(dispNote, Canvas.GetLeft(dispNote) - sender.Width);
                }
            }

            // remove the element itself
            bkgCanvas.Children.Remove(sender);

            // keep the note length count correct
            totalNotesLength -= (int)sender.Width;

            // update noteDists
            updateNoteDists();

            // fix the created error in the NoteIndex properites 
            for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                ((RollElement)bkgCanvas.Children[i]).NoteIndex = i;
            } 

            // debug info
            dataDisp.Text = "Removed note " + sender.NoteIndex.ToString() + ", \r\nnumber of display notes: " +
                bkgCanvas.Children.Count.ToString() + "\r\nnumber of logical notes: " + noteSheet.notes.Count.ToString();
        }

        private void updateNoteDists() {
            noteDists.Clear();
            foreach (RollElement element in bkgCanvas.Children) {
                if (element.Opacity != 0.5 && element.ActualHeight != 0) noteDists.Add((int)Canvas.GetLeft(element));
            }
        }

        private void displayNote_ElementPropertiesChanged(RollElement sender) {
            // Set VoiceProperties and DispName
            noteSheet.notes[sender.NoteIndex].VoiceProperties = otoRead.GetVoicePropFromSampleName(sender.NoteName);
            noteSheet.notes[sender.NoteIndex].DispName = sender.NoteName;
 
            // Set the RollElement background to red if the sample doesn't exist
            if (!otoRead.SampleExists(sender.NoteName)) sender.IsInvalidtext = true;
            else sender.IsInvalidtext = false;
        }

        /// <summary>
        /// Render and play 
        /// </summary>
        public void Play() {
            noteSheet.Bpm = internalBpm; // probably unnecessary, the bug was fixed elsewhere

            // setup and start renderer
            Renderer rnd = new Renderer(noteSheet);
            rnd.ShowRenderWindow = true;
            rnd.NoteRendered += rnd_NoteRendered;
            rnd.restNotePath = RestPath;
            rnd.UseMultiThread = false;
            rnd.Render();

            // start wavmod (which plays the file too)
            wavMod.ExtWavtoolInit(rnd.TemporaryDir, FluidSys.FluidSys.CreateTempDir() + "\\out.wav", noteSheet, WavTool, true);
        }

        /// <summary>
        /// Export a render
        /// </summary>
        /// <param name="outputPath"></param>
        public void ExportWav(string outputPath, bool external) {
            // setup and start renderer (disabled multithread to prevent bugs in the past)
            Renderer rnd = new Renderer(noteSheet);
            rnd.ShowRenderWindow = true;
            rnd.NoteRendered += rnd_NoteRendered;
            rnd.restNotePath = RestPath;
            rnd.UseMultiThread = false;
            rnd.Render();

            // start wavmod
            if (external) wavMod.ExtWavtoolInit(rnd.TemporaryDir, outputPath, noteSheet, WavTool, false);
            else wavMod.SaveTemp(rnd.TemporaryDir, outputPath, noteSheet);

            // clear junk files
            try {
                System.IO.File.Delete(outputPath + ".whd");
                System.IO.File.Delete(outputPath + ".dat");
            }
            catch (Exception) { }
        }

        void rnd_NoteRendered(int noteIndex) {
            //TODO: Renderer GUI feedback (will need some changes to renderer to prevent hanging the main thread)
        }

        /// <summary>
        /// Used for tempo changes; edits the lengths of the note to the new tempo.
        /// </summary>
        public void ResetAllLogicalNotesLength() {
            dataDisp.Text = "";
            for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                // get the current element 
                RollElement currentRollElement = (RollElement)bkgCanvas.Children[i];
                // geet the current logical note 
                Note currentNote = noteSheet.notes[i];

                // debug info
                dataDisp.Text += " Old note length: " + currentNote.Length;

                // resize the note
                currentNote.Length = currentRollElement.ElementNote.Length;
                currentNote.NotePitch = generatePitchString((int)Canvas.GetTop(currentRollElement));
                currentNote.VoiceProperties = otoRead.GetVoicePropFromSampleName(currentNote.DispName);

                // debug info
                dataDisp.Text += " New note length: " + currentNote.Length;
            }
        }

        /// <summary>
        /// Saves the document. Probably useless for now.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath) {
            if (fileWriter == null || fileWriter.FilePath != filePath && filePath != "") 
                fileWriter = new FluidFileWriter(filePath, noteSheet);
            fileWriter.SaveFile();
        }

        // converts note location on the roll into a string usable by resamplers
        private string generatePitchString(int noteLoc) {
            noteLoc = noteLoc / 24;
            int altNoteLoc = noteLoc;

            while (altNoteLoc > 12) { altNoteLoc = altNoteLoc - 12; }
            return pitchNames[altNoteLoc] + getOctave(noteLoc).ToString();
        }

        // setup gui colors
        private void setDefaultColors() {
            LightPianoKeyColor = Brushes.Gainsboro;
            DarkPianoKeyColor = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
            CurrentPianoKeyColor = new SolidColorBrush(Color.FromArgb(255, 0, 132, 223)); 
        }

        // used by generatePitchString to get a pitch name for resampler
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
            // six octaves (running backwards to provide numbering in the right direction)
            for (int i = 6; i > 0; i--) {
                // each key is a label
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

                // fill the piano panel
                c.Width = cs.Width = d.Width = ds.Width = e.Width = f.Width = fs.Width = g.Width =
                    gs.Width = a.Width = ar.Width = b.Width = pianoPanel.Width;

                // 24 units high, might need to be variable for zooming in the future
                c.Height = cs.Height = d.Height = ds.Height = e.Height = f.Height = fs.Height =
                    g.Height = gs.Height = a.Height = ar.Height = b.Height = 24;

                // set the label names, using i for the octave label
                c.Content = "C" + i.ToString();    cs.Content = "C#" + i.ToString();
                d.Content = "D" + i.ToString();    ds.Content = "D#" + i.ToString();
                e.Content = "E" + i.ToString();    f.Content = "F" + i.ToString();
                fs.Content = "F#" + i.ToString();  g.Content = "G" + i.ToString();
                gs.Content = "G#" + i.ToString();  a.Content = "A" + i.ToString();
                ar.Content = "A#" + i.ToString();  b.Content = "B" + i.ToString();

                // setup key colors
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

                // add to the panel
                pianoPanel.Children.Add(b);   pianoPanel.Children.Add(ar);
                pianoPanel.Children.Add(a);   pianoPanel.Children.Add(gs);
                pianoPanel.Children.Add(g);   pianoPanel.Children.Add(fs);
                pianoPanel.Children.Add(f);   pianoPanel.Children.Add(e);
                pianoPanel.Children.Add(ds);  pianoPanel.Children.Add(d);
                pianoPanel.Children.Add(cs);  pianoPanel.Children.Add(c);
            }
        }

        private void paintTimeBarTicks() {
            Rectangle tick;

            // used to make the ticks in 2 sizes
            bool isMajorTick = true;

            // reset the time bar
            timeBar.Width = bkgCanvas.Width;
            timeBar.Children.Clear();
            timeBarSlider.Width = timeBar.Width;

            for (int i = 0; i <= bkgCanvas.Width; i += 60) {
                // reset the tick
                tick = new Rectangle();

                // setup the ticks depending on whether isMajorTick or not
                // will probably need modified for zooming support
                if (isMajorTick) {
                    tick.Fill = new SolidColorBrush(Color.FromRgb(68, 68, 68));
                    tick.Stroke = null;
                    tick.Width = 6;
                    tick.Height = timeBar.Height / 4;
                }
                else {
                    tick.Fill = new SolidColorBrush(Color.FromRgb(60,60,60));
                    tick.Stroke = null;
                    tick.Width = 4;
                    tick.Height = timeBar.Height / 4;
                }

                // position the tick
                tick.Margin = new Thickness(0, 0, 60 - tick.Width, 0);
                tick.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                // add the tick, and alternate between big and small ticks
                timeBar.Children.Add(tick);
                isMajorTick = !isMajorTick;
            }
        }

        // lights up the piano key in line with whatever note lane your mouse is in
        private void highlightPianoKey(int position) {
            // reset the key colors
            foreach (Label noteLabel in pianoPanel.Children) {
                if (((string)noteLabel.Content).IndexOf("#") != -1) {
                    noteLabel.Background = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                }
                else noteLabel.Background = Brushes.Gainsboro;
            }

            // convert the mouse position to a ptch string
            string currentMousePitch = generatePitchString((int)Mouse.GetPosition(bkgCanvas).Y);

            // highlight whichever key has a matching pitch name
            foreach (Label noteLabel in pianoPanel.Children) {
                if (((string)noteLabel.Content).IndexOf(currentMousePitch) != -1) noteLabel.Background = CurrentPianoKeyColor;
            }
        }

        private void bkgCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
            if (!mouseOverNote && Mouse.LeftButton == MouseButtonState.Pressed) {
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X;

                // setup placeholder note
                tempNote = new RollElement();
                if (editorTool == EditorTool.Pencil) tempNote.Width = 40 * hZoomFactor;
                else tempNote.Width = 120 * hZoomFactor; //TODO: make adjustable
                tempNote.Height = 24;
                tempNote.Opacity = 0.5;

                // add it to the bkgCanvas
                bkgCanvas.Children.Add(tempNote);
                Canvas.SetLeft(tempNote, Mouse.GetPosition(bkgCanvas).X);
                Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);

                // init the note cration process, mostly handled in bkgCanvas_MouseUp 
                isCreatingNote = true;

                // debug info
                dataDisp.Text = "Note dists:\r\n";
                foreach (int dist in noteDists) {
                    dataDisp.Text += dist.ToString() + "\r\n";
                }
            }

            // enable scroll mode on middle mouse
            if (Mouse.MiddleButton == MouseButtonState.Pressed) {
                mouseDownScrollLoc = Mouse.GetPosition(scroller);
                isInitalScroll = true;
            }

            // hide the right-click menu
            //rightClickMenu.Visibility = Visibility.Hidden;

            // sort the note distance list here
            noteDists.Sort();
        }

        /// <summary>
        /// generates the needed index for the note based on its 
        /// location in the bkgCanvas. uses noteDists.
        /// </summary> 
        private int genNoteIndex(int dist) {
            int index = 0;
            foreach (int notedist in noteDists) {
                if (notedist >= dist) return index;
                index++;
            }
            return 0;
        }

        /// <summary>
        /// Get the active note, since the noteIndex's don't align with the order they're in bkgCanvas.Children
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private RollElement getActiveElement(int index) {
            foreach (RollElement element in bkgCanvas.Children) {
                if (element.NoteIndex == index) return element;
            }
            return null;
        }

        private void bkgCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
            // Create a new note on the roll:
            if (MouseDownLoc + 30 < Mouse.GetPosition(bkgCanvas).X && !isMovingNote && !isSizingNote 
                && isCreatingNote) {
                Point mouseLoc = new Point();
                RollElement currentDN;

                // set up note position
                mouseLoc.X = MouseDownLoc / (int)dblNoteSnapping * (int)dblNoteSnapping;              
                mouseLoc.Y = (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24;

                // add this note's distance (from the beginning of the roll) to the list,
                // used to keep the notes aligned
                noteDists.Add((int)mouseLoc.X);

                // call AddNote
                try { currentDN = AddNote(mouseLoc, (int)tempNote.Width, genNoteIndex((int)mouseLoc.X)); }
                catch (Exception) {
                    // get rid of screwy elements
                    for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                        try { if (((RollElement)bkgCanvas.Children[i]).ActualHeight == 0) bkgCanvas.Children.RemoveAt(i); }
                        catch (IndexOutOfRangeException) { break; }
                    }
                    currentDN = AddNote(mouseLoc, (int)tempNote.Width, bkgCanvas.Children.Count - 1);
                }

                // used alongside noteDists for alignment purposes
                totalNotesLength += (int)((RollElement)bkgCanvas.Children[noteSheet.notes.Count - 1]).Width;

                // debug info
                dataDisp.Text = "totalNotesLength=\r\n" + totalNotesLength;
                dataDisp.Text += "\r\nmouseLoc.X=\r\n" + mouseLoc.X;

                // move notes to the right if needed
                foreach (RollElement element in bkgCanvas.Children) {
                    if (Canvas.GetLeft(element) >= mouseLoc.X && element != currentDN) {
                        element.NoteIndex++;
                        Canvas.SetLeft(element, Canvas.GetLeft(element) + (int)tempNote.Width);
                    }
                }

                // grow the sheet if needed
                if (totalNotesLength + 500 > bkgCanvas.Width) {
                    bkgCanvas.Width += 600;
                    timeBar.Width += 600;
                    timeBarSlider.Width += 600;
                    envelopePanel.envPanel.Width += 600;
                }  
            }

            // move an existing note
            if (isMovingNote) {
                RollElement current = getActiveElement(currentNoteIndex);
                Canvas.SetTop(current, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                noteSheet.notes[currentNoteIndex].NotePitch =
                    generatePitchString((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
            }
             
            // size an existing note
            if (isSizingNote) {
                // adjust the size of following notes
                foreach (RollElement dispNote in bkgCanvas.Children) {
                    if (dispNote.NoteIndex > currentNoteIndex) {
                        Canvas.SetLeft(dispNote, Canvas.GetLeft(dispNote)
                            - (mouseDownNoteSize - getActiveElement(currentNoteIndex).Width));
                    }
                }

                // debug info
                dataDisp.Text = "Total Notes Length: " + totalNotesLength.ToString() + " Difference: " +
                    (mouseDownNoteSize - (int)getActiveElement(currentNoteIndex).Width).ToString();

                // set total notes length (for keeping alignment)
                totalNotesLength -= (mouseDownNoteSize - (int)getActiveElement(currentNoteIndex).Width);

                // debug info
                dataDisp.Text += " Total Notes Length after operation: " + totalNotesLength.ToString();

                // set noteDist (for keeping alignment)
                updateNoteDists();

                // debug info
                foreach (int dist in noteDists) {
                    dataDisp.Text += "\r\n" + dist.ToString();
                }
            }

            // remove placeholder note 
            bkgCanvas.Children.Remove(tempNote);

            // stop any note action being done now
            isSizingNote = false;
            isCreatingNote = false;
            isMovingNote = false;

            // reset cursor
            Cursor = Cursors.Arrow;
        }

        private void bkgCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (isCreatingNote) {
                // make the placeholder note follow the mouse, when in pencil mode
                // it follows with the size as well.
                //TODO: in brush mode it should move horizontally as well. 
                try {
                    if (editorTool == EditorTool.Pencil) {
                        tempNote.Width = ((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc)
                    / (int)dblNoteSnapping * (int)dblNoteSnapping; 
                    }
                    Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                }
                catch (Exception) { }             
            }

            // useless since the TempNote isn't shown on note move anyway, should
            // make it so the note moves as your mouse does
            if (isMovingNote) {
                Canvas.SetTop(tempNote, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                dataDisp.Text = ((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24).ToString();
            }

            // size the note while the mouse is moving
            if (isSizingNote) {
                if (((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc > 0)) {
                    getActiveElement(currentNoteIndex).Width = ((int)Mouse.GetPosition(bkgCanvas).X
                        - MouseDownLoc) / (int)dblNoteSnapping * (int)dblNoteSnapping; 
                }
                //noteSheet.notes[currentNoteIndex].Envelope[2][0] += 
                //    ((RollElement)bkgCanvas.Children[currentNoteIndex]).ElementNote.Length - noteSheet.notes[currentNoteIndex].Length;
                //noteSheet.notes[currentNoteIndex].Envelope[3][0] +=
                //    ((RollElement)bkgCanvas.Children[currentNoteIndex]).ElementNote.Length - noteSheet.notes[currentNoteIndex].Length;

                noteSheet.notes[currentNoteIndex].Length =
                    getActiveElement(currentNoteIndex).ElementNote.Length;

                noteSheet.notes[currentNoteIndex].UUnitLength =
                    getActiveElement(currentNoteIndex).ElementNote.UUnitLength;

                dataDisp.Text = "Logical note length=" + noteSheet.notes[currentNoteIndex].Length +
                    "\r\nDisplay note length=" + getActiveElement(currentNoteIndex).Width;
            }

            // middle mouse button scrolling
            if (Mouse.MiddleButton == MouseButtonState.Pressed) {
                //dataDisp.Text = scroller.HorizontalOffset + (mouseDownScrollLoc.X - Mouse.GetPosition(scroller).X).ToString();
                
                scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + (mouseDownScrollLoc.X - Mouse.GetPosition(scroller).X));
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset + (mouseDownScrollLoc.Y - Mouse.GetPosition(scroller).Y));
                //rightClickMenu.Visibility = System.Windows.Visibility.Hidden;

                mouseDownScrollLoc = Mouse.GetPosition(scroller);
            }

            // start the highlighting function
            highlightPianoKey((int)Mouse.GetPosition(bkgCanvas).X);
        }

        // keep the scrollers in line
        private void scroller_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            fred.ScrollToVerticalOffset(scroller.VerticalOffset);
            timeBarScroller.ScrollToHorizontalOffset(scroller.HorizontalOffset);
            envelopePanel.envScroller.ScrollToHorizontalOffset(scroller.HorizontalOffset);
        }

        // set the time bar marker/caret
        private void timeBar_MouseDown(object sender, MouseButtonEventArgs e) {
            Canvas.SetLeft(rollCaret, ((int)Mouse.GetPosition(timeBarSlider).X / (int)dblNoteSnapping * 
                (int)dblNoteSnapping));
        }

        private void bkgCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
            
        }
    }
}
