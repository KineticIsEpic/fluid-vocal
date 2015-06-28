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
using FluidSys;

namespace FluidUI {
    public delegate void ElementChangedEventArgs(RollElement sender);
    public delegate void ElementRemovedEventArgs(RollElement sender);
    public delegate void ElementSelectedChangedEventArgs(RollElement sender, bool isSelected);

    /// <summary>
    /// Interaction logic for RollElement.xaml
    /// </summary>
    public partial class RollElement : UserControl {
        public event ElementChangedEventArgs ElementPropertiesChanged;
        public event ElementRemovedEventArgs ElementRemoved;
        public event ElementSelectedChangedEventArgs ElementSelected;

        public Brush SelectedForegroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public Brush SelectedBackgroundBrush { get; set; }
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Determines if the control is selected. 
        /// </summary>
        public bool IsSelected {
            get { return baseIsSelected; }
            set {
                if (value) setSelector(true);
                else setSelector(false);

                if (baseIsSelected != value) {
                    try { ElementSelected.Invoke(this, value); }
                    catch (Exception) { }
                }

                baseIsSelected = value;
            }
        }

        /// <summary>
        /// Gets or sets the tempo value.
        /// </summary>
        public int BPM { get; set; }
        /// <summary>
        /// Gets or sets a value representing pitch.
        /// </summary>
        public string Pitch { get; set; }
        /// <summary>
        /// Gets or sets a value representing note name.
        /// </summary>
        public string NoteName {
            get { return nameTxtBox.Text; }
            set { nameTxtBox.Text = value; }
        }       
        /// <summary>
        /// Gets or sets the factor for calculating note length from screen length.
        /// </summary>
        public int LengthFactor { get; set; }
        /// <summary>
        /// Gets or sets the zero-based index of the note.
        /// </summary>
        public int NoteIndex { get; set; }

        private bool baseIsSelected = false;

        /// <summary>
        /// Gets a FluidSys.Note with the properties of this RollElement.
        /// </summary>
        public Note ElementNote { 
            get {
                Note n = new Note();
                n.Length = (int)GetLength();
                n.NotePitch = Pitch;
                n.DispName = nameTxtBox.Text;

                return n;
            }
        }

        public RollElement() {
            InitializeComponent();
            LengthFactor = 4;
            Pitch = "C#4";
            NoteIndex = 0;
            BPM = 120;

            SelectedForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 45, 157, 215));
            ForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 83, 83, 85));
            SelectedBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 135, 171, 189));
            BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 145, 145, 145));

            gripper1.MouseDown += gripper_Selected;
            gripper2.MouseDown += gripper_Selected;
            gripper2.MouseDown += gripper_Selected;
            bkgGrid.MouseDown += gripper_Selected;
        }

        public double GetLength() {
            // Using quarter note as a reference for calculation
            int qNotePx = 100;
            int qNoteMillis = 0;

            // Get reference length for note based on current tempo
            try { qNoteMillis = 60000 / BPM; }
            catch (Exception) { return -1; }

            double NoteLengthFactor = this.Width / qNotePx; 

            return qNoteMillis * NoteLengthFactor;
        }
        
        private void setSelector(bool isSel) {
            if (isSel) {
                gripper1.Fill = SelectedForegroundBrush;
                gripper2.Fill = SelectedForegroundBrush;
                gripper3.Fill = SelectedForegroundBrush;
                bkgGrid.Background = SelectedBackgroundBrush;
            }
            else {
                gripper1.Fill = ForegroundBrush;
                gripper2.Fill = ForegroundBrush;
                gripper3.Fill = ForegroundBrush;
                bkgGrid.Background = BackgroundBrush;
            }
        }

        private void gripper_Selected(object sender, MouseButtonEventArgs e) {
            if (!IsSelected) IsSelected = true;
            else IsSelected = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (nameTxtBox.IsVisible) nameTxtBox.Visibility = Visibility.Hidden;
            else nameTxtBox.Visibility = Visibility.Visible;
        }

        private void nameTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            try { ElementPropertiesChanged.Invoke(this); }
            catch (Exception) { }
        }
    }
}
