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
    /// Interaction logic for EditorPanel
    /// </summary>
    public partial class EditorPanel : UserControl {

        List<Line> fillLines = new List<Line>();
        List<Rectangle> points = new List<Rectangle>();

        Point downLoc = new Point();

        int Bpm = 0;
        int maxPos = 0;

        public EditorPanel() {
            InitializeComponent();

            for (int i = 0; i < Width / 2; i++) {
                addpoint(new Point(i * 4, Height / 2));
            }
        }

        public void UpdateView(FluidSys.Sheet noteSheet, int tempo) { }

        void envelope_SizeChanged(object sender, SizeChangedEventArgs e) {  }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {  }

        private void envPanel_MouseMove(object sender, MouseEventArgs e) {
        }

        private Rectangle pointundermouse() {
            int ptndx = 0;

            while (Canvas.GetLeft((Rectangle)envPanel.Children[ptndx])
                < Mouse.GetPosition(envPanel).X && ptndx + 1
                < envPanel.Children.Count)
                    ptndx++;

            return (Rectangle)envPanel.Children[ptndx];
        }

        private void addpoint(Point pos) {
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush(Colors.DodgerBlue);
            rect.Width = rect.Height = 3;
            envPanel.Children.Add(rect);
            Canvas.SetTop(rect, pos.Y);
            Canvas.SetLeft(rect, pos.X);
            Canvas.SetZIndex(rect, 1);
            points.Add(rect);

            Rectangle clickarea = new Rectangle();
            clickarea.Fill = new SolidColorBrush(Color.FromRgb(40,40,42));
            clickarea.Height = envPanel.Height;
            clickarea.Width = 3;
            envPanel.Children.Add(clickarea);
            Canvas.SetTop(clickarea, 0);
            Canvas.SetLeft(clickarea, pos.X);
            Canvas.SetZIndex(clickarea, 0);
            clickarea.Tag = rect;
            clickarea.MouseMove += Clickarea_MouseMove;
            clickarea.MouseDown += Clickarea_MouseDown;
        }

        private void Clickarea_MouseDown(object sender, MouseButtonEventArgs e) {
            downLoc = Mouse.GetPosition(envPanel);
        }

        private void Clickarea_MouseMove(object sender, MouseEventArgs e) {
            if (Mouse.LeftButton == MouseButtonState.Pressed) {
                Canvas.SetTop(((Rectangle)((Rectangle)sender).Tag), Mouse.GetPosition(this).Y);
                ((Rectangle)((Rectangle)sender).Tag).Tag = true;

                for (int i = points.IndexOf(((Rectangle)((Rectangle)sender).Tag)) - 1; i > 0; i--) {
                    try { if ((bool)points[i].Tag == true) break; }
                    catch (Exception) { }
                    if (Canvas.GetLeft(points[i]) < downLoc.X) break;

                    Canvas.SetTop(points[i], Mouse.GetPosition(envPanel).Y);
                } 
            }
            
        }

        private void envPanel_MouseDown(object sender, MouseButtonEventArgs e) { }

        private void envPanel_MouseUp(object sender, MouseButtonEventArgs e) {
            foreach (Rectangle point in points) point.Tag = false;
        }
    }
}
