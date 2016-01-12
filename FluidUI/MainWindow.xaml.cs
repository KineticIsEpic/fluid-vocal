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
using FluidSys;

namespace FluidUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        int mouseDownLoc;
        public int internalRefBpm = 120;

        bool mouseDownOverBPM = false;

        public MainWindow() {
            InitializeComponent();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            InputForm inForm = new InputForm();
            System.Windows.Forms.DialogResult dr = inForm.ShowDialog("Set Sample Bank");
            if (dr == System.Windows.Forms.DialogResult.Yes) noteRoll.MasterSampleBank = inForm.Value;
        }

        private void Label_MouseDown_1(object sender, MouseButtonEventArgs e) {
            noteRoll.Play();
        }

        private void Label_MouseDown_2(object sender, MouseButtonEventArgs e) {
            InputForm inForm = new InputForm();
            System.Windows.Forms.DialogResult dr = inForm.ShowDialog("Set Resynthesizer");
            if (dr == System.Windows.Forms.DialogResult.Yes) {
                if (System.IO.File.Exists(inForm.Value) && inForm.Value.IndexOf(".exe") != -1) 
                    noteRoll.ResynthEngine = inForm.Value;
                else System.Windows.Forms.MessageBox.Show("The path " +  inForm.Value + " is not a valid executable file.");
            }
        }

        private void BpmLabel_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDownLoc = (int)Mouse.GetPosition(BpmLabel).Y;
            mouseDownOverBPM = true;
        }

        private void BpmLabel_MouseUp(object sender, MouseButtonEventArgs e) {
            noteRoll.GlobalBPM = internalRefBpm;
            mouseDownOverBPM = false;
        }

        private void BpmLabel_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDownOverBPM) {
                internalRefBpm += (mouseDownLoc - (int)Mouse.GetPosition(BpmLabel).Y) / 2;
                BpmLabel.Content = internalRefBpm.ToString();  
            }
        }
    }
}
