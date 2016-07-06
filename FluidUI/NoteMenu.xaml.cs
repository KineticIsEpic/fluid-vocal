/*====================================================*\
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

namespace FluidUI {
    public delegate void NoteMenuItemHandler();
    public delegate void DockingEventHandler(NoteMenu sender);
    /// <summary>
    /// Interaction logic for NoteMenu.xaml
    /// </summary>
    public partial class NoteMenu : UserControl {
        public event NoteMenuItemHandler CloseClicked;
        public event DockingEventHandler Docking;

        private int mouseDownVolumePos;
        private int mouseDownModPos;
        private int mouseDownVelPos;
        private int internalRefVol;
        private int internalRefMod;
        private int internalRefVel;

        private bool mouseDownOverVol;
        private bool mouseDownOverMod;
        private bool mouseDownOverVel;

        public int WorkingNoteIndex { get; set; }

        public bool IsDocked {
            get {
                if (dockItem.IsVisible) return true;
                else return false;
            }
            set {
                if (value) {
                    dockItem.Visibility = System.Windows.Visibility.Hidden;
                    closeItem.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current note this FluidUI.NoteMenu is associated with. 
        /// </summary>
        public FluidSys.Note WorkingNote { 
           get { return intNote; }
           set {
                intNote = value;
                noteChanged();
           }
        }

        private FluidSys.Note intNote; 

        public NoteMenu() {
            InitializeComponent();
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e) {
            try { CloseClicked.Invoke(); }
            catch (Exception) { }
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void noteChanged() {
            volumeSlider.Content = "Volume: " + intNote.Volume.ToString() +"%";
            modSlider.Content = "Modulation: " + intNote.Modulation.ToString() + "%";
            velSlider.Content = "Velocity: " + intNote.Velocity.ToString() + "%";
            flagsTxtBox.Text = intNote.Args;
            useGlobalSmpBox.IsChecked = intNote.UseDefaultVb;
            smpBankTxtBox.Text = intNote.VbPath;
            FileNameTxtBox.Text = intNote.VoiceProperties.FileName;
            StartTxtBox.Text = intNote.VoiceProperties.StartString;
            ConsTxtBox.Text = intNote.VoiceProperties.ConsonantString;
            EndTxtBox.Text = intNote.VoiceProperties.EndString;
            PitchTxtBox.Text = intNote.PitchCode;

            internalRefVol = intNote.Volume;
            internalRefMod = intNote.Modulation;

            flagsTxtBox.TextChanged += flagsTxtBox_TextChanged;
            useGlobalSmpBox.Checked += useGlobalSmpBox_Checked;
            smpBankTxtBox.TextChanged += smpBankTxtBox_TextChanged;
            FileNameTxtBox.TextChanged += FileNameTxtBox_TextChanged;
            StartTxtBox.TextChanged += StartTxtBox_TextChanged;
            ConsTxtBox.TextChanged += ConsTxtBox_TextChanged;
            EndTxtBox.TextChanged += EndTxtBox_TextChanged;
            PitchTxtBox.TextChanged += PitchTxtBox_TextChanged;
        }

        private void volumeSlider_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownVolumePos = (int)Mouse.GetPosition(volumeSlider).X;
            mouseDownOverVol = true;
        }

        private void volumeSlider_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDownOverVol) {
                internalRefVol -= (mouseDownVolumePos - (int)Mouse.GetPosition(volumeSlider).X) / 6;

                if (internalRefVol > 100) internalRefVol = 100;
                if (internalRefVol < 0) internalRefVol = 0;

                intNote.Volume = internalRefVol;
                volumeSlider.Content = "Volume: " + intNote.Volume.ToString() + "%";
            }
        }

        private void volumeSlider_MouseUp(object sender, MouseButtonEventArgs e) {
            mouseDownOverVol = false;
        }

        private void volumeSlider_MouseLeave(object sender, MouseEventArgs e) {
            mouseDownOverVol = false;
        }

        private void modSlider_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownModPos = (int)Mouse.GetPosition(modSlider).X;
            mouseDownOverMod = true;
        }

        private void modSlider_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDownOverMod) {
                internalRefMod -= (mouseDownModPos - (int)Mouse.GetPosition(modSlider).X) / 6;

                if (internalRefMod > 100) internalRefMod = 100;
                if (internalRefMod < 0) internalRefMod = 0;

                intNote.Modulation = internalRefMod;
                modSlider.Content = "Modulation: " + intNote.Modulation.ToString() + "%";
            }
        }

        private void velSlider_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownVelPos = (int)Mouse.GetPosition(velSlider).X;
            mouseDownOverVel = true;
        }

        private void velSlider_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDownOverVel) {
                internalRefVel -= (mouseDownVelPos - (int)Mouse.GetPosition(velSlider).X) / 6;

                if (internalRefVel > 100) internalRefVel = 100;
                if (internalRefVel < 0) internalRefVel = 0;

                intNote.Velocity = internalRefVel;
                velSlider.Content = "Velocity: " + intNote.Velocity + "%";
            }
        }

        private void velSlider_MouseUp(object sender, MouseButtonEventArgs e) {
            mouseDownOverVel = false;
        }

        private void velSlider_MouseLeave(object sender, MouseEventArgs e) {
            mouseDownOverVel = false;
        }

        private void modSlider_MouseUp(object sender, MouseButtonEventArgs e) {
            mouseDownOverMod = false;
        }

        private void modSlider_MouseLeave(object sender, MouseEventArgs e) {
            mouseDownOverMod = false;
        }

        private void flagsTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.Args = flagsTxtBox.Text;
        }

        private void useGlobalSmpBox_Checked(object sender, RoutedEventArgs e) {
            intNote.UseDefaultVb = (bool)useGlobalSmpBox.IsChecked;
        }

        private void smpBankTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.VbPath = smpBankTxtBox.Text;
        }

        private void FileNameTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.VoiceProperties.FileName = FileNameTxtBox.Text;
        }

        private void StartTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.VoiceProperties.StartString = StartTxtBox.Text;
        }

        private void ConsTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.VoiceProperties.ConsonantString = ConsTxtBox.Text;
        }

        private void EndTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.VoiceProperties.EndString = EndTxtBox.Text;
        }

        private void PitchTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
            intNote.PitchCode = PitchTxtBox.Text;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            try { Docking.Invoke(this); } 
            catch (Exception) { }
        }
    }
}
