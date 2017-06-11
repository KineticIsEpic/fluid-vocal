/*====================================================*\
 *||          Copyright(c) KineticIsEpic.             ||
 *||          See LICENSE.TXT for details.            ||
 *====================================================*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FluidSys;
using OTOmate;

namespace FluidUI {
    public delegate void NoteSelectEventHandler(Note workingNote);
    /// <summary>
    /// HOW EVERYTHING WORKS:
    /// Notes are 2 objects: the RollElement on the roll, and the Note object
    /// that stores the important bits about the note. All the Note objects 
    /// are stored inside the NoteSheet, which also stores bits of info about
    /// the track (voicebank, tempo...). 
    /// 
    /// Adding a note is initiated by clicking and dragging, as detected 
    /// within bkgCanvas_MouseDown. The code there adds a temporary RollElement
    /// to the canvas and sets isCreatingNote to true, which enables code
    /// in bkgCanvas_MouseMove which makes the temporary note follow the user's
    /// mouse. Releasing the mouse (bkgCanvas_MouseUp) runs more code which 
    /// removes the temporary note and adds the real note by calling AddNote.
    /// 
    /// Most other roll operations work in a simiar manner; their code is
    /// also distributred between the three methods. I think the rest is all
    /// nicely commented.
    /// 
    /// Undo (still not working as I'm writing this) is supposed to work by 
    /// storing old versions of the bkgCanvas.Children and noteSheet 
    /// variables, then indexing through them. The notes don't stay where 
    /// they should - all empty space between them goes away.
    /// 
    /// The RollEditMode BS isn't used anymore. The original idea involved
    /// 2 distinct editing modes, one for edting notes and another for "tuning"
    /// voice parameters. This idea has been abandoned for a simpler approach.
    /// I should rip it out but can't be bothered to.
    /// </summary>
    public partial class NoteRoll : UserControl {
        public event NoteSelectEventHandler NoteSelected;

        //TODO: explain what all these variables do (and clean out unnecessary ones)
        private List<List<RollElement>> undoElements = new List<List<RollElement>>(11);
        private List<Sheet> undoSheets = new List<Sheet>(11);
        private List<List<int>> undoDists = new List<List<int>>(11);

        private Sheet noteSheet = new Sheet();
        private RollElement tempNote;
        private Rectangle rollCaret;
        private Rectangle selectionBox; 
        private OtoReader otoRead;
        private FluidFileWriter fileWriter;
        private NoteMenu rightClickMenu;
        private wavmod.WavMod wavMod;
        private Point mouseDownScrollLoc;
        private Point mouseDownSelLoc;

        private int MouseDownLoc = 0;
        private int baseNoteLength = 480;
        private int totalNotesLength = 0;
        private int currentNoteIndex = 0;
        private int mouseDownNoteSize = 0;
        private int internalBpm = 120;
        private int undoIndex = 0;

        private double dblNoteSnapping = 0;
        private double dblRollSnapping = 0;
        private double dblDefNoteSize = 0;
        private double hZoomFactor = 1;

        private bool isSizingNote = false;
        private bool isMovingNote = false;
        private bool isCreatingNote = false;
        private bool mouseOverNote = false;
        private bool isScrolling = false;
        private bool isEditMode = false;
        private bool isSelecting = false;
        private bool isSelection = false;
        private bool hasUndone = false;
        private bool tempSelect = false; //workaround for horizontal move bugfix

        private Snapping tempNS = Snapping.Quarter;
        private Snapping tempRS = Snapping.Quarter;
        private Snapping defNS = Snapping.Quarter;

        private string[] pitchNames = { "B", "A#", "A", "G#", "G", "F#", "F", "E", "D#", "D", "C#", "C", "B" };
        private List<int> noteDists = new List<int>(256);

        public Brush CurrentPianoKeyColor { get; set; }
        public Brush LightPianoKeyColor { get; set; }
        public Brush DarkPianoKeyColor { get; set; }

        public int undoLimit = 10;

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
                    topGrid.Visibility = System.Windows.Visibility.Visible;

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
                otoRead = new OtoReader();
                otoRead.OpenFile(noteSheet.Voicebank + "\\oto.ini");

                foreach (Note note in noteSheet.notes) {
                    note.VbPath = noteSheet.Voicebank + "\\oto.ini";
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

        public bool isNoteEditing { get; set; }

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

            // set up selection box;
            selectionBox = new Rectangle();
            selectionBox.Fill = new SolidColorBrush(Colors.DodgerBlue);
            selectionBox.Opacity = 0.6;
            selectionBox.MouseMove += bkgCanvas_MouseMove;
            selectionBox.MouseUp += bkgCanvas_MouseUp;

            // show tuning panel
            topGrid.Height = 170;
            topGrid.Visibility = System.Windows.Visibility.Visible;
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
        /// <param name="index">the index to insert the note at. </param>
        public RollElement AddNote(Point location, int length, int index, Note srcNote) {
            RollElement displayNote = new RollElement();
            Note logicalNote;

            // align the note with the others if needed
            foreach (RollElement element in bkgCanvas.Children) {
                if (Canvas.GetLeft(element) < location.X && Canvas.GetLeft(element) + element.Width > location.X) {
                    location.X = Canvas.GetLeft(element) + element.Width;
                    break;
                }
            }

            // add this note's distance (from the beginning of the roll) to the list,
            // used to keep the notes aligned
            noteDists.Add((int)location.X);

            // auto-set index based on position
            if (index == -1) {
                try { index = genNoteIndex((int)location.X); }
                catch (Exception) {
                    // get rid of screwy elements
                    for (int i = 0; i < bkgCanvas.Children.Count; i++) {
                        try { if (((RollElement)bkgCanvas.Children[i]).ActualHeight == 0) bkgCanvas.Children.RemoveAt(i); }
                        catch (IndexOutOfRangeException) { break; }
                    }
                    index = bkgCanvas.Children.Count - 1;
                }
            }

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
            displayNote.ElementEditingStateChanged += DisplayNote_ElementEditingStateChanged;
            displayNote.ZoomFactor = hZoomFactor;

            // add it to the canvas
            bkgCanvas.Children.Add(displayNote);
            Canvas.SetLeft(displayNote, location.X);
            Canvas.SetTop(displayNote, location.Y);

            // create new logical note if srcNote is null
            if (srcNote == null) logicalNote = displayNote.ElementNote;
            else {
                logicalNote = srcNote;
                displayNote.NoteName = logicalNote.DispName;
            }

            // setup logical note based off of the element
            logicalNote.NotePitch = generatePitchString((int)location.Y);
            logicalNote.VoiceProperties = otoRead.GetVoicePropFromSampleName(logicalNote.DispName);
            logicalNote.GenerateDefaultEnvelope();
            logicalNote.ScreenPosY = location.Y;
            logicalNote.PitchCode = "0";
           
            // set rest length 
            try {
                logicalNote.RestLength = ((int)Canvas.GetLeft(bkgCanvas.Children[index]) -
                    ((int)Canvas.GetLeft(bkgCanvas.Children[index - 1]) +
                    (int)((RollElement)bkgCanvas.Children[index - 1]).Width)) / (int)dblNoteSnapping * (int)dblNoteSnapping;
            }
            catch (Exception) { }
           
            // add it to the NoteSheet
            noteSheet.notes.Insert(index, logicalNote);          

            // used alongside noteDists for alignment purposes
            totalNotesLength += (int)((RollElement)bkgCanvas.Children[noteSheet.notes.Count - 1]).Width;

            // move notes to the right if needed
            foreach (RollElement element in bkgCanvas.Children) {
                if (Canvas.GetLeft(element) >= location.X && element != displayNote) {
                    element.NoteIndex++;
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + (int)tempNote.Width);
                }
            }

            // setup pitch:
            addPorta(logicalNote, displayNote, 12, true);

            // grow the sheet if needed
            if (totalNotesLength + 500 > bkgCanvas.Width) {
                bkgCanvas.Width += 600;
                timeBar.Width += 600;
                timeBarSlider.Width += 600;
                envelopePanel.envPanel.Width += 600;
            }

            return displayNote;
        }

        // used by MainWindow to determine when to use keyboard shortcuts
        private void DisplayNote_ElementEditingStateChanged(bool state) {
            isNoteEditing = state;
        }

        private void DisplayNote_ElementMouseUp(RollElement sender) {
            // setup and display right-click menu
            rightClickMenu.WorkingNoteIndex = sender.NoteIndex;
            rightClickMenu.WorkingNote = noteSheet.notes[sender.NoteIndex];

            Canvas.SetLeft(rightClickMenu, Mouse.GetPosition(overlayCanvas).X);
            Canvas.SetTop(rightClickMenu, Mouse.GetPosition(overlayCanvas).Y);

            // make sure the panel is fully viewable vertically
            if (Canvas.GetTop(rightClickMenu) - scroller.ContentVerticalOffset +
                rightClickMenu.Height > scroller.ActualHeight)
                Canvas.SetTop(rightClickMenu, scroller.ContentVerticalOffset +
                    (scroller.ActualHeight - rightClickMenu.Height) - 50);

            rightClickMenu.Visibility = Visibility.Visible;         
        }

        void displayNote_ElementMouseDown(RollElement sender) {   
            if (sender.IsMouseOverResize) {
                // deselect any selected notes 
                clearSelection();

                // add the current state to Undo
                addToUndo();

                // init resize, done by bkgCanvas_MouseMove
                isSizingNote = true; 
                currentNoteIndex = sender.NoteIndex; 

                // used in the resize job:
                mouseDownNoteSize = (int)sender.Width; 
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X - (int)sender.Width;
            }
            // move the note if the mouse isn't on the resizer or right-clicking
            else {
                // init move, done by bkgCanvas_MouseMove
                isMovingNote = true;
                sender.IsSelected = true;
                if (!isSelecting) tempSelect = true;
                isSelection = true;
                MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X;
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

        private void addPorta(Note nt, RollElement dispnt, int length, bool tempnotevisible) {
            int ntindex = bkgCanvas.Children.IndexOf(dispnt);
            if (tempnotevisible) ntindex--;

            if (ntindex > 0) {
                // clear current pitch code
                nt.PitchCode = "";

                // get the difference between the 2 notes
                int diff = (int)((Canvas.GetTop(bkgCanvas.Children[ntindex - 1])
                    - Canvas.GetTop(dispnt)) / 24) * -100;

                // makes steps down to the note pitch 
                for (int i = 0; i < length; i++) {
                    if (diff > 0) nt.PitchCode += (diff - ((diff / length) * i)).ToString() + " ";
                    else nt.PitchCode += (diff - ((diff / length) * i)).ToString() + " ";
                }

                nt.PitchCode = nt.PitchCode.Trim();
            }
        }

        public void ZoomIn() {
            hZoomFactor += 0.2;
            bkgCanvas.Background.Transform = new ScaleTransform(hZoomFactor, 1);

            paintTimeBarTicks();
            handleZoom();
        }

        public void ZoomOut() {
            hZoomFactor -= 0.2;
            bkgCanvas.Background.Transform = new ScaleTransform(hZoomFactor, 1);

            paintTimeBarTicks();
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

            // update undo list
            addToUndo();

            // debug info
            dataDisp.Text = "Removed note " + sender.NoteIndex.ToString() + ", \r\nnumber of display notes: " +
                bkgCanvas.Children.Count.ToString() + "\r\nnumber of logical notes: " + noteSheet.notes.Count.ToString();
        }

        private void updateNoteDists() {   
            // clear the current list
            noteDists.Clear();

            // add each note's distance to the list and sort it
            foreach (RollElement element in bkgCanvas.Children) {
                if (element.Opacity != 0.5 && element.ActualHeight != 0) 
                    noteDists.Add((int)Canvas.GetLeft(element));
            }
            noteDists.Sort();

            // use the now-sorted list to fix the note indexes
            foreach (RollElement element in bkgCanvas.Children) {
                element.NoteIndex = genNoteIndex((int)Canvas.GetLeft(element));
            }
        }

        public void updateRestLenghts() {
            foreach (RollElement element in bkgCanvas.Children) {
                if (bkgCanvas.Children.IndexOf(element) - 1 >= 0) {
                    noteSheet.notes[element.NoteIndex].RestLength = (int)Canvas.GetLeft(element) -
                        ((int)Canvas.GetLeft(bkgCanvas.Children[bkgCanvas.Children.IndexOf(element) - 1]) +
                        (int)((RollElement)bkgCanvas.Children[bkgCanvas.Children.IndexOf(element) - 1]).Width); 
                }
                else { noteSheet.notes[element.NoteIndex].RestLength = (int)Canvas.GetLeft(element); }
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
            rnd.UseMultiThread = true;
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

            // distance between ticks
            int tickDist = (int)(60 * hZoomFactor);

            // reset the time bar
            timeBar.Width = bkgCanvas.Width;
            timeBar.Children.Clear();
            timeBarSlider.Width = timeBar.Width;

            for (int i = 0; i <= bkgCanvas.Width; i += tickDist) {
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
                tick.Margin = new Thickness(0, 0, tickDist - tick.Width, 0);
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
                if (((string)noteLabel.Content).IndexOf(currentMousePitch) != -1)
                    noteLabel.Background = CurrentPianoKeyColor;
            }
        }

        private void bkgCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl | Key.RightCtrl) && Mouse.LeftButton == MouseButtonState.Pressed) {
                // de-select all notes if ctrl+shift isn't being used
                if (!Keyboard.IsKeyDown(Key.LeftShift | Key.RightShift))
                    foreach (RollElement element in bkgCanvas.Children) element.IsSelected = false; 

                // record mouse down location
                mouseDownSelLoc.X = Mouse.GetPosition(bkgCanvas).X;
                mouseDownSelLoc.Y = Mouse.GetPosition(bkgCanvas).Y;

                // set canvas location
                Canvas.SetLeft(selectionBox, mouseDownSelLoc.X);
                Canvas.SetTop(selectionBox, mouseDownSelLoc.Y);
                selectionBox.Height = 1;
                selectionBox.Width = 1;

                overlayCanvas.Children.Add(selectionBox);
                isSelecting = true;
            }
            
            if (!mouseOverNote && Mouse.LeftButton == MouseButtonState.Pressed &&
                !Keyboard.IsKeyDown(Key.LeftCtrl | Key.RightCtrl)) {
                // get mouse location on X
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
                isScrolling = true;
                bkgCanvas.Cursor = Cursors.SizeAll;
            }

            // hide the right-click menu
            rightClickMenu.Visibility = Visibility.Hidden;
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
        /// Clears all selected notes and sets isSelection to false.
        /// </summary>
        private void clearSelection() {
            if (isSelection) {
                foreach (RollElement element in bkgCanvas.Children) element.IsSelected = false;
                isSelection = tempSelect = false;
            }
        }

        /// <summary>
        /// Adds a step to the Undo system.
        /// </summary>
        private void addToUndo() {
            if (hasUndone) {
                undoSheets.RemoveAt(0);
                undoIndex = 0;
                hasUndone = false;
            }

            undoElements.Insert(0, new List<RollElement>(bkgCanvas.Children.Count));
            undoSheets.Insert(0, new Sheet());
            undoDists.Insert(0, new List<int>(noteDists.Count));

            undoSheets[0].Bpm = internalBpm;
            undoSheets[0].Name = noteSheet.Name;
            undoSheets[0].rendParams = noteSheet.rendParams;
            undoSheets[0].Resampler = noteSheet.Resampler;
            undoSheets[0].Voicebank = noteSheet.Voicebank;

            // this needs to be done to prevent the notes in undoSheets from changing
            foreach (Note n in noteSheet.notes) {
                // create a new note and copy the properties of n to it 
                Note newNote = new Note();
                newNote.Args = n.Args;
                newNote.DispName = n.DispName;
                newNote.Envelope = n.Envelope;
                newNote.FileName = n.FileName;
                newNote.Length = n.Length;
                newNote.Location = n.Location;
                newNote.Modulation = n.Modulation;
                newNote.NotePitch = n.NotePitch;
                newNote.Overlap = n.Overlap;
                newNote.PitchCode = n.PitchCode;
                newNote.RestLength = n.RestLength;
                newNote.ScreenPosY = n.ScreenPosY;
                newNote.UseDefaultVb = n.UseDefaultVb;
                newNote.UUnitLength = n.UUnitLength;
                newNote.VbPath = n.VbPath;
                newNote.Velocity = n.Velocity;
                newNote.VoiceProperties = n.VoiceProperties;
                newNote.Volume = n.Volume;
                // add the note
                undoSheets[0].notes.Add(newNote);
            }

            for (int i = 0; i < undoSheets.Count; i++) {
                try { if (undoSheets[i] == undoSheets[i - 1]) undoSheets.RemoveAt(i); }
                catch (Exception) { }
            }

            if (undoSheets.Count > undoLimit) undoSheets.RemoveAt(undoLimit);
        }

        public void Undo() {
            // check if undoIndex is within range, then up it and undo
            if (undoIndex < undoLimit && undoIndex + 1 < undoSheets.Count) {
                undoIndex++;
                Undo(undoIndex);
            }
        }

        public void Redo() {
            // check if undoIndex is within range, then recuce it and undo
            if (undoIndex > 0 && undoIndex - 1 < undoSheets.Count) {
                undoIndex--;
                Undo(undoIndex);
            }
        }

        public void Undo(int uindex) {
            int index = 0; 

            // clear everything
            noteSheet.notes.Clear();
            bkgCanvas.Children.Clear();
            noteDists.Clear();

            totalNotesLength = 0;

            // add each note back onto the roll
            foreach (Note n in undoSheets[uindex].notes) {
                dataDisp.Text += "\r\n" + n.RestLength.ToString();
                AddNote(new Point(totalNotesLength + n.RestLength, n.ScreenPosY), (int)(n.UUnitLength / 4 * hZoomFactor), index, n);
                index++;
            }

            hasUndone = true;
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

                // call AddNote
                currentDN = AddNote(mouseLoc, (int)tempNote.Width, -1, null); 

                // debug info
                dataDisp.Text = "totalNotesLength=\r\n" + totalNotesLength;
                dataDisp.Text += "\r\nmouseLoc.X=\r\n" + mouseLoc.X;
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

            // remove old selections
            if (!isMovingNote && !isScrolling || tempSelect) clearSelection();

            // selecting multiple notes
            if (isSelecting) {
                isSelecting = false;
                foreach (RollElement dispNote in bkgCanvas.Children) {
                    // select all notes that are inside selectionBox - top left ref point
                    if (Canvas.GetLeft(dispNote) >= Canvas.GetLeft(selectionBox) && 
                        Canvas.GetLeft(dispNote) <= Canvas.GetLeft(selectionBox) + selectionBox.Width &&
                        Canvas.GetTop(dispNote) >= Canvas.GetTop(selectionBox) && 
                        Canvas.GetTop(dispNote) <= Canvas.GetTop(selectionBox) + selectionBox.Height ) {
                        
                        dispNote.IsSelected = true;
                    }
                    // bottom right point
                    if (Canvas.GetLeft(dispNote) + dispNote.Width >= Canvas.GetLeft(selectionBox) &&
                        Canvas.GetLeft(dispNote) + dispNote.Width <= Canvas.GetLeft(selectionBox) + selectionBox.Width &&
                        Canvas.GetTop(dispNote) + dispNote.Height >= Canvas.GetTop(selectionBox) &&
                        Canvas.GetTop(dispNote) + dispNote.Height <= Canvas.GetTop(selectionBox) + selectionBox.Height) {

                        dispNote.IsSelected = true;
                    }
                    isSelection = true;
                    overlayCanvas.Children.Remove(selectionBox);
                }
            }

            // remove placeholder note 
            bkgCanvas.Children.Remove(tempNote);

            // update Undo list 
            if (isSizingNote || isCreatingNote || isMovingNote) {
                addToUndo();
                envelopePanel.UpdateView(noteSheet, internalBpm);
            }

            // update noteDists and restLengths
            if (isSizingNote || isMovingNote) {
                updateNoteDists();
                updateRestLenghts();
            }

            // update pitch
            if (isMovingNote) {
                addPorta(noteSheet.notes[getActiveElement(currentNoteIndex).NoteIndex],
                         getActiveElement(currentNoteIndex), 24, false);
            }

            if (tempSelect) clearSelection();

            // stop any note action being done now
            isSizingNote = false;
            isCreatingNote = false;
            isMovingNote = false;
            isSelecting = false;
            isScrolling = false;
            tempSelect = false;
 
            // reset cursor
            bkgCanvas.Cursor = Cursors.Arrow;
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
                //dataDisp.Text = ((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24).ToString();

                if (isSelection) {
                    bool hasHmoved = false;
                    double baseNoteLoc = Canvas.GetTop(getActiveElement(currentNoteIndex));
                    int moveVal = (int)((Mouse.GetPosition(bkgCanvas).Y -
                        Canvas.GetTop(getActiveElement(currentNoteIndex))) / 24 - 0.5);
                    int noteAdj = ((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc);// / (int)dblNoteSnapping * (int)dblNoteSnapping;
                    dataDisp.Text = moveVal.ToString();

                    foreach (RollElement element in bkgCanvas.Children) {
                        if (element.IsSelected) {
                            // move each note vertically
                            Canvas.SetTop(element, Canvas.GetTop(element) + moveVal * 24);
                            noteSheet.notes[element.NoteIndex].NotePitch =
                                generatePitchString((int)Canvas.GetTop(element) + moveVal * 24);
                            noteSheet.notes[element.NoteIndex].ScreenPosY = Canvas.GetTop(element);

                            // used for resetting the MouseDownLoc value
                            if (Canvas.GetLeft(element) != (int)Canvas.GetLeft(element) + noteAdj /
                                (int)dblNoteSnapping * (int)dblNoteSnapping) hasHmoved = true;

                            // move each note horizontally
                            Canvas.SetLeft(element, (int)Canvas.GetLeft(element) + noteAdj /
                                (int)dblNoteSnapping * (int)dblNoteSnapping);

                            // move following notes ahead if needed
                            try {
                                if ((Canvas.GetLeft(element) + element.Width >
                                    Canvas.GetLeft(bkgCanvas.Children[element.NoteIndex + 1])))
                                    if (!element.IsSelected) Canvas.SetLeft(bkgCanvas.Children[element.NoteIndex + 1],
                                        Canvas.GetLeft(element) + element.Width);
                            }
                            catch (Exception) { }
                        }
                    }
                    // update mouse location as neeeded
                    if (hasHmoved) MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X;
                }
                else {
                    int noteAdj = ((int)Mouse.GetPosition(bkgCanvas).X - MouseDownLoc);
                    bool hasHmoved = false;

                    // move current note vertically
                    RollElement current = getActiveElement(currentNoteIndex);
                    Canvas.SetTop(current, (int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);

                    // set pitch string accordingly 
                    noteSheet.notes[currentNoteIndex].NotePitch =
                        generatePitchString((int)Mouse.GetPosition(bkgCanvas).Y / 24 * 24);
                    noteSheet.notes[currentNoteIndex].ScreenPosY = Canvas.GetTop(current);

                    // used for resetting the MouseDownLoc value
                    if (Canvas.GetLeft(current) != (int)Canvas.GetLeft(current) + noteAdj /
                        (int)dblNoteSnapping * (int)dblNoteSnapping) hasHmoved = true;

                    // move current note horizontally
                    Canvas.SetLeft(current, (int)Canvas.GetLeft(current) + noteAdj / 
                        (int)dblNoteSnapping * (int)dblNoteSnapping);

                    // move following notes ahead if needed
                    try {
                        if (Canvas.GetLeft(current) + current.Width >
                            Canvas.GetLeft(bkgCanvas.Children[current.NoteIndex + 1]))
                            Canvas.SetLeft(bkgCanvas.Children[current.NoteIndex + 1],
                                Canvas.GetLeft(current) + current.Width);
                    }
                    catch (Exception) { }

                    // update mouse location if needed
                    if (hasHmoved) MouseDownLoc = (int)Mouse.GetPosition(bkgCanvas).X;
                }
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

            if (isSelecting) {
                // get current mouse position
                Point p2 = new Point(Mouse.GetPosition(bkgCanvas).X, Mouse.GetPosition(bkgCanvas).Y);

                // set X location of selectionBox
                if (p2.X < mouseDownSelLoc.X) {
                    Canvas.SetLeft(selectionBox, p2.X);
                    selectionBox.Width = mouseDownSelLoc.X - p2.X;
                }
                else selectionBox.Width = p2.X - mouseDownSelLoc.X;

                // set Y location 
                if (p2.Y < mouseDownSelLoc.Y) {
                    Canvas.SetTop(selectionBox, p2.Y);
                    selectionBox.Height = mouseDownSelLoc.Y - p2.Y;
                }
                else selectionBox.Height = p2.Y - mouseDownSelLoc.Y;
            }

            // middle mouse button scrolling
            if (isScrolling && Mouse.MiddleButton == MouseButtonState.Pressed) {
                //dataDisp.Text = scroller.HorizontalOffset + (mouseDownScrollLoc.X - Mouse.GetPosition(scroller).X).ToString();
                
                scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + (mouseDownScrollLoc.X - Mouse.GetPosition(scroller).X));
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset + (mouseDownScrollLoc.Y - Mouse.GetPosition(scroller).Y));
                //rightClickMenu.Visibility = System.Windows.Visibility.Hidden;

                mouseDownScrollLoc = Mouse.GetPosition(scroller);
            }

            // Safely stop the selection job if the mouse isn't down (the mouse left the canvas down...)
            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                isSelecting = false;
                overlayCanvas.Children.Remove(selectionBox);
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

        private void bkgCanvas_SizeChanged(object sender, SizeChangedEventArgs e) { }

        private void bkgCanvas_MouseLeave(object sender, MouseEventArgs e) { }

        private void overlayCanvas_MouseDown(object sender, MouseButtonEventArgs e) {

        }
    }
}
