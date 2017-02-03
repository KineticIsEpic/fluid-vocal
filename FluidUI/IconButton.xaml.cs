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
    /// Interaction logic for IconButton.xaml
    /// </summary>
    
    public delegate void IconButtonClickHandler(int mouseButton);

    public partial class IconButton : UserControl {
        /// <summary>
        /// 0 = left button, 1 = right button, 2 = middle button
        /// </summary>
        public event IconButtonClickHandler ButtonClicked;

        // UI colors/brushes
        public Brush NormalBackground = new SolidColorBrush(Color.FromArgb(0,40,40,41));
        public Brush HoverBackground = new SolidColorBrush(Color.FromRgb(33,33,34));

        public double DisabledIconOpacity = 0.7;

        /// <summary>
        /// 1.00 = full scale. 
        /// </summary>
        public double IconScale {
            get { return imgCanvas.Height / 24; }
            set { imgCanvas.Height = imgCanvas.Width = 24 * value; }
        }

        //TODO: regular, clicked and possibly disabled images

        public bool isActive {
            get { return active; }
            set {
                if (value) imgCanvas.Opacity = DisabledIconOpacity;
                else imgCanvas.Opacity = 1;
                active = value;
            }
        }

        private bool active;

        public IconButton() {
            InitializeComponent();
            Background = NormalBackground;
            isActive = true;
        }

        /// <summary>
        /// Sets the icon based on the file's index on the resource directory. 
        /// </summary>
        /// <param name="iconIndex"></param>
        public void SetIcon(int iconIndex) {
            ImageBrush brush = new ImageBrush();

            brush.ImageSource = new BitmapImage(new Uri(getIconFileList()[iconIndex]));
            brush.Stretch = Stretch.Uniform;

            imgCanvas.Background = brush;
        }

        /// <summary>
        /// Sets the icon based on the file's name (not including path) in the resource directory. 
        /// </summary>
        /// <param name="iconName"></param>
        public void SetIcon(string iconName) {
            ImageBrush brush = new ImageBrush();

            foreach (string path in getIconFileList()) {
                if (path.Substring(path.LastIndexOf("\\")).IndexOf(iconName) != -1) {
                    brush.ImageSource = new BitmapImage(new Uri(path));
                    break;
                }
            }

            brush.Stretch = Stretch.Uniform;

            imgCanvas.Background = brush;
        }

        private List<string> getIconFileList() {
            List<string> iconPaths = new List<string>();

            foreach (string path in System.IO.Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "\\res")) {
                if (path != "01.PNG" && path.IndexOf(".png") != -1) { iconPaths.Add(path); }
            }

            return iconPaths;
        }

        private void invokeEvent() {
            try {
                if (Mouse.LeftButton == MouseButtonState.Pressed) ButtonClicked.Invoke(0);
                else if (Mouse.RightButton == MouseButtonState.Pressed) ButtonClicked.Invoke(1);
                else if (Mouse.MiddleButton == MouseButtonState.Pressed) ButtonClicked.Invoke(2);
            }
            catch (Exception) { }
        }

        private void button_MouseUp(object sender, MouseButtonEventArgs e) {
            if (active) invokeEvent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e) {
            if (active) Background = HoverBackground;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e) {
            Background = NormalBackground;
        }
    }
}
