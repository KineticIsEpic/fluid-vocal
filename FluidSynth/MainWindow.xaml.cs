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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Media;
using FluidSys;
using wavmod;
using OTOmate;

namespace FluidSynth {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Process p = new Process();

        public MainWindow() {
            InitializeComponent();
            new FluidFileWriter("ee", new Sheet());
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            //string args = "\"" + VbPathTB.Text + "\\" + NameTB.Text + ".wav" + "\" \"" +
            //    outTb.Text + "\" \"" + PitchTB.Text + "\"" + " 100 B0 10 " + LengthTB.Text + " 10";

            //p.StartInfo.Arguments = args;
            //p.StartInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables("UserProfile") + "\\Desktop";
            //p.StartInfo.FileName = RsmpPathTB.Text;
            //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //p.Exited += p_Exited;

            //logTb.Text += "\r\nResampler: " + RsmpPathTB.Text;
            //logTb.Text += "\r\nSample Name: " + VbPathTB.Text + "\\" + NameTB.Text + ".wav";
            //logTb.Text += "\r\nArguments: " + p.StartInfo.Arguments;
            //logTb.ScrollToEnd();

            //try { p.Start(); }
            //catch (Exception ex) {
            //    logTb.Text += "\r\n\r\nError: " + ex.Message;
            //    logTb.ScrollToEnd();
            //}
        }

        void p_Exited(object sender, EventArgs e) {
            //logTb.Text += "\r\n\r\nRender Output: ";
            //logTb.Text += p.StandardOutput.ReadToEnd();

            //try { new SoundPlayer(outTb.Text).Play(); }
            //catch (Exception) {
            //    logTb.Text += "\r\n\r\nCould not play the render output. It may or may not have " +
            //        "rendered correctly. \r\n";
            //    logTb.ScrollToEnd();
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            //Sheet sheet = new Sheet();
            //Note note = new Note();

            //note.Length = int.Parse(LengthTB.Text);
            //note.FileName = NameTB.Text;
            //note.NotePitch = PitchTB.Text;

            //sheet.notes.Add(note);
            //sheet.Resampler = RsmpPathTB.Text;
            //sheet.Voicebank = VbPathTB.Text;

            //Renderer rnd = new Renderer(sheet);

            //rnd.Render();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            //Sheet sheet = new Sheet();
            //OtoReader or = OtoReader.FromFile(VbPathTB.Text + "\\oto.ini");
            //Note note1 = new Note(or.GetVoicePropFromSampleName(NameTB.Text));
            //Note note2 = new Note(or.GetVoicePropFromSampleName(NameTB.Text));
            //WavMod wvmd = new WavMod();

            //note1.Length = int.Parse(LengthTB.Text);
            //note1.FileName = NameTB.Text;
            //note1.NotePitch = "C4";

            //note2.Length = int.Parse(LengthTB.Text);
            //note2.FileName = NameTB.Text;
            //note2.NotePitch = "G4";
            //note2.Location = int.Parse(LengthTB.Text);

            //sheet.notes.Add(note1);
            //sheet.notes.Add(note2);
            //sheet.Resampler = RsmpPathTB.Text;
            //sheet.Voicebank = VbPathTB.Text;

            //Renderer rnd = new Renderer(sheet);

            //rnd.Render();

            //wvmd.PlaybackTemp(rnd.TemporaryDir, sheet);
        }

        private void BeginBtn_Click(object sender, RoutedEventArgs e) {
            FluidFileWriter ffw = new FluidFileWriter(outTb.Text, new Sheet());
        }
    }
}
