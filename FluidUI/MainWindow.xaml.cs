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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private bool setBpmFromString(string tempobpm) {
            try {
                pianoRoll.Tempo = int.Parse(tempobpm);
                return true;
            }
            catch (Exception ex) {
                pianoRoll.Tempo = 120;
                return false; 
            }
        }

        private void testBtn_Click(object sender, RoutedEventArgs e) {
            pianoRoll.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            InputForm inf = new InputForm();
            System.Windows.Forms.DialogResult dr = inf.ShowDialog("Voicebank path");

            if (dr == System.Windows.Forms.DialogResult.Yes)
                pianoRoll.Samplebank = inf.Value;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            InputForm inf = new InputForm();
            System.Windows.Forms.DialogResult dr = inf.ShowDialog("Renderer path");

            if (dr == System.Windows.Forms.DialogResult.Yes)
                pianoRoll.RendererPath = inf.Value;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e) {
            if (setBpmFromString(((TextBox)sender).Text)) ((TextBox)sender).Background = Brushes.White;
            else ((TextBox)sender).Background = Brushes.Red;

            if (setBpmFromString(((TextBox)sender).Text)) ((TextBox)sender).Background = Brushes.White;
            else ((TextBox)sender).Background = Brushes.Red;
        }
    }
}
