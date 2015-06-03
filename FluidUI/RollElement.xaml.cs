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
    /// Interaction logic for RollElement.xaml
    /// </summary>
    public partial class RollElement : UserControl {
        int currentNoteEnd = 80;

        public RollElement() {
            InitializeComponent();
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            RollNote rn = new RollNote();
            //rn.Margin = new Thickness(0, 0, mainGrid.Width - currentNoteEnd, 0);

            mainGrid.Children.Add(rn);
        }
    }
}
