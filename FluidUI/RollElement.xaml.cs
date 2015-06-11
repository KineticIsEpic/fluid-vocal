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
    /// <summary>
    /// Interaction logic for RollElement.xaml
    /// </summary>
    public partial class RollElement : UserControl {
        public event ElementChangedEventArgs ElementPropertiesChanged;

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

        /// <summary>
        /// Gets a FluidSys.Note with the properties of this RollElement.
        /// </summary>
        public Note ElementNote { 
            get {
                Note n = new Note();
                n.Length = (int)(this.Width * LengthFactor);
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
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            
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
