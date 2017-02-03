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
using System.Windows.Shapes;

namespace FluidUI {
    /// <summary>
    /// Interaction logic for NewUI.xaml
    /// </summary>
    public partial class NewUI : Window {
        public NewUI() {
            InitializeComponent();
            System.Windows.Forms.MessageBox.Show(noteroll.area.Width.ToString());
            System.Windows.Forms.MessageBox.Show(noteroll.Width.ToString());
        }

        public bool launch() {
            this.Show();
            return true;
        }
    }
}
