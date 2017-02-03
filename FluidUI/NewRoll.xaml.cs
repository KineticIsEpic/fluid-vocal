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
    /// Interaction logic for NewRoll.xaml
    /// </summary>
    public partial class NewRoll : UserControl {
        public NewRoll() {
            InitializeComponent();
            DrawBkg();
        }

        private int sizeX = 120; // length one quarter note
        private int sizeY = 20; // height of one row
        private int scrollOfstX = 0; // how far we've scrolled
        private int scrollOfstY = 0; // "
        private int rowCount = 12 * 7; // 12 notes in 1 octave * 7 octaves 

        // scroll offset for X minus repititions 
        private int adjustedOfstX {
            get {
                try { return scrollOfstX - ((int)scrollOfstX / sizeX * sizeX); }
                catch (Exception) { return 0; }
            }
        }
        // TODO: scroll offset for Y minus repititions
        private int adjustedOfstY {
            get { return 0; }
        }

        private bool bkgInit = false;

        // Color defs
        public Brush DarkRowBrush = new SolidColorBrush(Color.FromRgb(24, 24, 25));
        public Brush LightRowBrush = new SolidColorBrush(Color.FromRgb(38, 38, 39));
        public Brush DividerBrush = new SolidColorBrush(Color.FromRgb(45, 45, 47));

        // Roll components
        private List<Rectangle> dividers;
        private List<Rectangle> rows;

        private void DrawBkg() {
            // How far to offset the background, ignoring pattern repititions 
            int bkgOfstX = adjustedOfstX;
            int bkgOfstY = adjustedOfstY; 

            if (!bkgInit) InitBkg(bkgOfstX, bkgOfstY);
        }

        private void InitBkg(int offsetX, int offsetY) {
            // optimal count of dividers/rows
            int optd = (((int)Width) / sizeX) + 1;
            int optr = (((int)Height) / sizeY) + 1;

            // count of how many dividers/rows are added
            int dCount = 0;
            int rCount = 0;

            // init dividers and rows
            dividers = new List<Rectangle>(optd);
            rows = new List<Rectangle>(optr);

            for (int i = 0; i < optr; i++) {
                rows.Add(new Rectangle());

                // setup size
                rows[i].Height = sizeY;
                rows[i].Width = Width;

                // paint the row the right color
                if (rCount != 0 && rCount / 2 * 2 == rCount)
                    rows[i].Fill = LightRowBrush;
                else rows[i].Fill = DarkRowBrush;

                // stick that bad boy up there
                Canvas.SetTop(rows[i], rCount * sizeY + offsetY);
                area.Children.Add(rows[i]);

                // keep count
                rCount++;
            }

            for (int i = 0; i < optd; i++) {
                dividers.Add(new Rectangle());

                // setup size + color
                dividers[i].Height = Height + 5;
                dividers[i].Width = 2;
                dividers[i].Fill = DividerBrush;

                // plop er right on there
                int loc = dCount * sizeX + offsetX;
                if (loc < Width) {
                    Canvas.SetLeft(dividers[i], loc);
                    area.Children.Add(dividers[i]);
                    Canvas.SetTop(dividers[i], 0);
                }

                // keep count
                dCount++;
            }
        }

        private void ResizeBkg() {
            foreach (Rectangle div in dividers) { div.Height = Height; }
            foreach (Rectangle row in rows) { row.Width = Width; }
        }

        private void RescaleBkg() {
            // indexes of rows/dividers to remove
            List<int> divRemLst = new List<int>(dividers.Count);
            List<int> rowRemLst = new List<int>(rows.Count);
            // count unwanted dividers 
            for (int i = 0; i < dividers.Count; i++) {
                if (Canvas.GetLeft(dividers[i]) > Width) divRemLst.Add(i);
            }
            // count unwatned rows
            for (int i = 0; i < rows.Count; i++) {
                if (Canvas.GetTop(rows[i]) > Height) rowRemLst.Add(i);
            }
            // remove unwanted dividers
            foreach (int index in divRemLst) dividers.RemoveAt(index);
            // remove unanted rows
            foreach (int index in rowRemLst) rows.RemoveAt(index);

            // relocate first divider and row
            Canvas.SetLeft(dividers[0], adjustedOfstX);
            Canvas.SetTop(rows[0], adjustedOfstY);
            // relocate the rest of the rows/dividers based off of that
            for (int i = 1; i == dividers.Count; i++) {
                Canvas.SetLeft(dividers[i], sizeX * i + adjustedOfstX);
            }
            for (int i = 1; i == rows.Count; i++) {
                Canvas.SetTop(rows[i], sizeY * i + adjustedOfstY);
            }
        }

        private void mainroll_SizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeBkg();
            try {
                RescaleBkg();
            }
            catch (Exception) { }
        }

        private void area_MouseWheel(object sender, MouseWheelEventArgs e) {
            sizeX += e.Delta / 20;
            ResizeBkg();
            RescaleBkg();
            FluidSys.DebugLog.Write("MouseWheel: set sizeX to: " + sizeX.ToString());
        }

    }
}
