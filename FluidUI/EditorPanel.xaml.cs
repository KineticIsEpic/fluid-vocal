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

namespace FluidUI {
    /// <summary>
    /// Interaction logic for EditorPanel
    /// </summary>
    public partial class EditorPanel : UserControl {

        int Bpm = 0;

        public EditorPanel() {
            InitializeComponent();
        }

        public void UpdateView(FluidSys.Sheet noteSheet, int tempo) {
            envPanel.Children.Clear();
            Bpm = tempo;

            envScroller.Margin = new Thickness(76, 0, 0, -18);

            foreach (FluidSys.Note note in noteSheet.notes) {
                EnvUI envelope = new EnvUI();
                envelope.Width = getLengthInPx(tempo, note.Length);
                envelope.Height = 170;
                envelope.WorkingNote = note;
                envelope.Tempo = tempo;
                envelope.InitPoints();
                envelope.Crossover = note.Overlap;
                envelope.SizeChangeHelper = (int)getTotalEnvelopesLength();
            
                // Compensate for preutterance
                Canvas.SetLeft(envelope, getTotalEnvelopesLength() - getLengthInPx(Bpm, note.Overlap));
                Canvas.SetTop(envelope, 0);

                envelope.SizeChanged += envelope_SizeChanged;
                envPanel.Children.Add(envelope);
            }
        }

        void envelope_SizeChanged(object sender, SizeChangedEventArgs e) {  }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {  }

        private double getTotalEnvelopesLength() {
            double totallength = 0;

            foreach (EnvUI envelope in envPanel.Children) {
                totallength += envelope.Width - getLengthInPx(Bpm, envelope.WorkingNote.Overlap);
            }
            return totallength;
        }

        private double getTotalEnvelopesLength(int index) {
            double totallength = 0;
            int counter = 0;

            foreach (EnvUI envelope in envPanel.Children) {
                totallength += envelope.Width;
                counter++;
                
                if (counter >= index) return totallength;
            }
            return totallength; 
        }

        private double getLengthInPx(int BPM, int lengthMs) {
            // Using quarter note as a reference for calculation
            int qNotePx = 120; 
            int qNoteMillis = 0;

            // Get reference length for note based on current tempo
            try { qNoteMillis = 60000 / BPM; }
            catch (Exception) { return -1; }

            return lengthMs * qNotePx / qNoteMillis;
        }
    }
}
