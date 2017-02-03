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
    /// Interaction logic for AboutControl.xaml
    /// </summary>
    public partial class AboutControl : UserControl {

        public AboutControl() {
            InitializeComponent();

            aboutTxt.Text = "Version " + new FluidSys.ConfigMgr().VersionNum.ToString() + 
                "\r\n\r\nCopyright (c) 2015-2017 KineticIsEpic.\r\nSee license.txt for usage info." +
                "\r\n\r\nUses NAudio, (c) 2001-2014 Mark Heath. \r\n" +
                "Art by PastelZephyr.";
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            ((Window)this.Parent).Close();
        }
    }
}
