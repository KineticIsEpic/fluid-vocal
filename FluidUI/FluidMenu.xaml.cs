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

    public delegate void MenuItemEventHandler();
    /// <summary>
    /// Interaction logic for FluidMenu.xaml
    /// </summary>
    public partial class FluidMenu : UserControl {
        public event MenuItemEventHandler newEvent;
        public event MenuItemEventHandler openEvent;
        public event MenuItemEventHandler saveEvent;
        public event MenuItemEventHandler saveAsEvent;
        public event MenuItemEventHandler importMidiEvent;
        public event MenuItemEventHandler exportEvent;
        public event MenuItemEventHandler sbLibEvent;
        public event MenuItemEventHandler prjLibEvent;
        public event MenuItemEventHandler prefEvent;

        public FluidMenu() {
            InitializeComponent();
        }

        private void newItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { newEvent.Invoke(); }
            catch (Exception) { }
        }

        private void openItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { openEvent.Invoke(); }
            catch (Exception) { }
        }

        private void saveItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { saveEvent.Invoke(); }
            catch (Exception) {  }
        }

        private void saveAsItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { saveAsEvent.Invoke(); }
            catch (Exception) { }
        }

        private void importMidiItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { importMidiEvent.Invoke(); }
            catch (Exception) { }
        }

        private void exportItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { exportEvent.Invoke(); }
            catch (Exception) { }
        }

        private void sbLibItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { sbLibEvent.Invoke(); }
            catch (Exception) { }
        }

        private void prjLibItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { prjLibEvent.Invoke(); }
            catch (Exception) { }
        }

        private void prefItem_MouseUp(object sender, MouseButtonEventArgs e) {
            try { prefEvent.Invoke(); }
            catch (Exception) { }
        }
    }
}
