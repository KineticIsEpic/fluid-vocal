using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FluidUI
{
    /// <summary>
    /// Interaction logic for EnvUI
    /// </summary>
    public partial class EnvUI : UserControl {
        private List<Line> linkingLines = new List<Line>(20);
        private List<Ellipse> envPoints = new List<Ellipse>(20);

        private int movingPoint = -1;

        private Point pointMovingMouseLoc;

        public SolidColorBrush LineBrush { get; set; }
        public SolidColorBrush PointBrush { get; set; }
        public SolidColorBrush HighlightPointBrush { get; set; }

        public FluidSys.Note WorkingNote { get; set; }
        public int Tempo { get; set; }
        public int SizeChangeHelper { get; set; }

        public int Crossover {
            get { return int.Parse(ovrTxtBox.Text); }
            set { ovrTxtBox.Text = value.ToString(); }
        }

        public EnvUI() {
            InitializeComponent();

            PointBrush = Brushes.DodgerBlue;
            LineBrush = Brushes.DodgerBlue;

            this.MouseUp += EnvUI_MouseUp;
            this.MouseMove += EnvUI_MouseMove;
        }

        public void InitPoints() {
            //createPoint((int)convertEnvPoint(WorkingNote.Envelope[0]).X, (int)convertEnvPoint(WorkingNote.Envelope[0]).Y);
            //createPoint((int)convertEnvPoint(WorkingNote.Envelope[1]).X, (int)convertEnvPoint(WorkingNote.Envelope[1]).Y);
            //createPoint((int)convertEnvPoint(WorkingNote.Envelope[2]).X + (int)getLengthInPx(Tempo, WorkingNote.Overlap),
            //    (int)convertEnvPoint(WorkingNote.Envelope[2]).Y);
            //createPoint((int)convertEnvPoint(WorkingNote.Envelope[3]).X + (int)getLengthInPx(Tempo, WorkingNote.Overlap),
            //    (int)convertEnvPoint(WorkingNote.Envelope[3]).Y);

            //System.Windows.Forms.MessageBox.Show(Canvas.GetBottom(envPoints[1]).ToString());

            // remove old components
            envDrawCanvas.Children.Clear();
            envPoints.Clear();
            linkingLines.Clear();

            createPoint(WorkingNote.Envelope[0][0], WorkingNote.Envelope[0][1] + 28);
            createPoint(WorkingNote.Envelope[1][0], WorkingNote.Envelope[1][1] + 28);
            createPoint((int)Width - WorkingNote.Envelope[2][0], WorkingNote.Envelope[2][1] + 28);
            createPoint((int)Width - WorkingNote.Envelope[3][0], WorkingNote.Envelope[3][1] + 28);

            createLine(0, 1);
            createLine(1, 2);
            createLine(2, 3);
        }

        private Line createLine(int point1, int point2) {
            Line linkingLine = new Line();

            linkingLine.X1 = Canvas.GetLeft(envPoints[point1]) + 2;
            linkingLine.Y1 = envDrawCanvas.Height - Canvas.GetBottom(envPoints[point1]) - 4;
            linkingLine.X2 = Canvas.GetLeft(envPoints[point2]) + 2;
            linkingLine.Y2 = envDrawCanvas.Height - Canvas.GetBottom(envPoints[point2]) - 4;

            linkingLine.Stroke = Brushes.DodgerBlue;
            linkingLine.StrokeThickness = 2;

            envDrawCanvas.Children.Add(linkingLine);
            linkingLines.Add(linkingLine);

            return linkingLine;
        }

        private void moveLine(Line linkingLine, int point1, int point2) {
            linkingLine.X1 = Canvas.GetLeft(envPoints[point1]) - 2;
            linkingLine.Y1 = envDrawCanvas.Height - Canvas.GetBottom(envPoints[point1]) - 2;
            linkingLine.X2 = Canvas.GetLeft(envPoints[point2]) - 2;
            linkingLine.Y2 = envDrawCanvas.Height - Canvas.GetBottom(envPoints[point2]) - 2;

            y1Label.Content = envDrawCanvas.Height - Canvas.GetBottom(envPoints[point1]) - 2;
            x1Label.Content = Canvas.GetLeft(envPoints[point1]) - 2;
        }

        private Ellipse createPoint(int x, int y) {
            Ellipse point = new Ellipse();

            // Set Ellipse's properties 
            point.Width = 8;
            point.Height = 8;
            point.Fill = PointBrush;
            point.Stroke = null;
            point.MouseDown += point_MouseDown;
            point.MouseEnter += point_MouseEnter;
            point.MouseLeave += point_MouseLeave;

            envPoints.Add(point);
            point.Tag = envPoints.Count - 1;

            // Add to canvas
            envDrawCanvas.Children.Add(point);
            Canvas.SetLeft(point, x);
            Canvas.SetBottom(point, y);
            Canvas.SetZIndex(point, int.MaxValue);

            return point;
        }

        private int convertYEnvPoints(int screenValue) {
            return screenValue * 100 / 120;
        }

        private double convertXEnvPoints(double screenValue) {
            // Using quarter note as a reference for calculation
            double qNotePx = 120;
            int qNoteMillis = 0;

            // Get reference length for note based on current tempo
            try { qNoteMillis = 60000 / Tempo; }
            catch (Exception) { return -1; }

            double NoteLengthFactor = screenValue / qNotePx;

            return qNoteMillis * NoteLengthFactor;
        }

        private Point convertEnvPoint(int[] logicalPoint) {
            Point internalPoint = new Point();

            internalPoint.Y = logicalPoint[1] * 120 / 100;

            double qNotePx = 120;
            int qNoteMillis = 60000 / Tempo;

            internalPoint.X = logicalPoint[0] * qNotePx / qNoteMillis;

            return internalPoint;
        }

        private void redrawLines() {
            foreach (Line line in linkingLines) {
                envDrawCanvas.Children.Remove(line);
            }

            linkingLines.Clear();

            createLine(0, 1);
            createLine(1, 2);
            createLine(2, 3);
        }

        private double getLengthInPx(int BPM, int lengthMs) {
            // Using quarter note as a reference for calculation
            int qNotePx = 120;
            int qNoteMillis = 0;

            // Get reference length for note based on current tempo
            try { qNoteMillis = 60000 / BPM; }
            catch (Exception) { return -1; }

            return lengthMs * qNotePx / qNoteMillis;
        }

        void point_MouseDown(object sender, MouseButtonEventArgs e) {
            movingPoint = (int)((Ellipse)sender).Tag;
            pointMovingMouseLoc = Mouse.GetPosition(this);
        }

        private void point_MouseLeave(object sender, MouseEventArgs e) {
            x1Label.Content = "";
            y1Label.Content = "";
        }

        void point_MouseEnter(object sender, MouseEventArgs e) {
            x1Label.Content = WorkingNote.Envelope[(int)((Ellipse)sender).Tag][0];
            y1Label.Content = WorkingNote.Envelope[(int)((Ellipse)sender).Tag][1];

        }

        void EnvUI_MouseUp(object sender, MouseButtonEventArgs e) {
            //if (movingPoint == 0 || movingPoint == 3) Canvas.SetBottom(envPoints[movingPoint], 0);
            //redrawLines();
            movingPoint = -1;
        }

        void EnvUI_MouseMove(object sender, MouseEventArgs e) {
            if (movingPoint != -1) {
                Ellipse point = envPoints[movingPoint];             

                // Move point
                Canvas.SetLeft(point, Mouse.GetPosition(this).X - 4);
                Canvas.SetBottom(point, envDrawCanvas.Height -
                        ((Mouse.GetPosition(this).Y - pointMovingMouseLoc.Y + pointMovingMouseLoc.Y) + 4));

                // Move point back to 0, if it goes too far
                if (Canvas.GetBottom(point) < 28) Canvas.SetBottom(point, 28);
                if (Canvas.GetBottom(point) > 128) Canvas.SetBottom(point, 128);

                redrawLines();

                // Set Values 
                if (movingPoint < 2) WorkingNote.Envelope[movingPoint][0] = (int)Canvas.GetLeft(point);                   
                else WorkingNote.Envelope[movingPoint][0] = (int)Width - (int)Canvas.GetLeft(point);
                WorkingNote.Envelope[movingPoint][1] = (int)Canvas.GetBottom(point) - 28; 

                //WorkingNote.Envelope[movingPoint][0] = (int)convertXEnvPoints((int)Canvas.GetLeft(envPoints[movingPoint]));
                //WorkingNote.Envelope[movingPoint][1] = convertYEnvPoints((int)Canvas.GetBottom(envPoints[movingPoint])) + 4;

                //if (movingPoint > 1) {
                //    WorkingNote.Envelope[movingPoint][0] -= (int)getLengthInPx(Tempo, WorkingNote.Overlap);
                //}

                // Set info display to match data
                x1Label.Content = WorkingNote.Envelope[movingPoint][0];
                y1Label.Content = WorkingNote.Envelope[movingPoint][1];
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                WorkingNote.Overlap = int.Parse(ovrTxtBox.Text);
                Width = getLengthInPx(Tempo, WorkingNote.Length) + getLengthInPx(Tempo, WorkingNote.Overlap);
                Canvas.SetLeft(this, SizeChangeHelper - getLengthInPx(Tempo, WorkingNote.Overlap));

                Crossover = WorkingNote.Overlap;
            }
            catch (Exception) { }
        }
    }
}
