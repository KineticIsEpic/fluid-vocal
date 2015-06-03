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
    /// Interaction logic for Roll.xaml
    /// </summary>
    public partial class Roll : UserControl {
        int xRows = 48;

        public Roll() {
            InitializeComponent();

           for (int i = 0; i < 48; i++) {

                Rectangle rect = new Rectangle();
                rect.Height = 24;
                rect.Width = this.Width;

                if (i % 2 != 0) rect.Fill = Brushes.Gainsboro;
                else rect.Fill = Brushes.LightGray;

                notePanel.Children.Add(rect);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            //notePanel.Width = this.Width;
            //notePanel.Height = this.Height;
        }
    }
}
