using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KDVisuals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// KD-Tree which stores the points.
        /// </summary>
        private KDTree.KDTree<EllipseWrapper> pTree = new KDTree.KDTree<EllipseWrapper>(2);

        /// <summary>
        /// Bitmap which renders them quickly.
        /// </summary>
        private WriteableBitmap pBitmap = null;

        /// <summary>
        /// A data item which is stored in each kd node.
        /// </summary>
        private class EllipseWrapper
        {
            public bool Filled;
            public double x;
            public double y;

            public EllipseWrapper(double x, double y)
            {
                this.x = x;
                this.y = y;
                this.Filled = false;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Randomise the layout of points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create a new bitmap and set it as our canvas background.
            pBitmap = BitmapFactory.New((int)cnvPoints.ActualWidth, (int)cnvPoints.ActualHeight);
            var pBrush = new ImageBrush();
            pBrush.ImageSource = pBitmap;
            cnvPoints.Background = pBrush;

            // Clear the bitmap to light blue.
            using (pBitmap.GetBitmapContext())
                pBitmap.Clear(Colors.LightBlue);


            // Get the number we want to generate and update the UI.
            var iResult = 0;
            if (!int.TryParse(txtPoints.Text, out iResult))
            {
                txtPoints.Foreground = Brushes.Red;
                return;
            }
            if (iResult < 0)
            {
                txtPoints.Foreground = Brushes.Red;
                return;
            }
            txtPoints.Foreground = Brushes.Black;

            // Clear the tree and canvas.
            cnvPoints.Children.Clear();
            pTree = new KDTree.KDTree<EllipseWrapper>(2);

            // Create a list of points and draw a ghost ellipse for each one.
            using (pBitmap.GetBitmapContext())
            {
                // Generate X new random items.
                var pRandom = new Random();
                for (int i = 0; i < iResult; ++i)
                {
                    // Position it and add it to the canvas.
                    var x = pRandom.NextDouble() * cnvPoints.ActualWidth;
                    var y = pRandom.NextDouble() * cnvPoints.ActualHeight;

                    // Add it to the tree.
                    pTree.AddPoint(new double[] { x, y }, new EllipseWrapper(x, y));

                    // Draw a ghost visual for it.
                    //pBitmap.DrawEllipse((int)x - 2, (int)y - 2, (int)x + 2, (int)y + 2, Colors.Green);
                    pBitmap.DrawEllipse((int)x - 2, (int)y - 2, (int)x + 2, (int)y + 2, Colors.Orange);

                }
            }
        }

        /// <summary>
        /// When the mouse is moved, highlight the nearby nodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cnvPoints_MouseMove(object sender, MouseEventArgs e)
        {
            // Bail if nothing to draw on.
            if (pBitmap == null)
                return;

            // Get valid values for the search from the text box.
            int iMax = 0;
            txtFindMax.Foreground = Brushes.Black;
            if (!int.TryParse(txtFindMax.Text, out iMax))
                txtFindMax.Foreground = Brushes.Red;

            double fThreshold = -1;
            txtFindThreshold.Foreground = Brushes.Black;
            if (!double.TryParse(txtFindThreshold.Text, out fThreshold))
                txtFindThreshold.Foreground = Brushes.Red;

            // Compute the square threshold as we use a square distance function.
            var bNegative = fThreshold < 0;
            fThreshold = fThreshold * fThreshold;
            if (bNegative)
                fThreshold = -fThreshold;


            // Get the drawing context.
            using (pBitmap.GetBitmapContext())
            {
                // Get the point to query from.
                var vPoint = e.GetPosition(cnvPoints);

                // Perform a nearest neighbour search around that point.
                var pIter = pTree.NearestNeighbors(new double[] { vPoint.X, vPoint.Y }, iMax, fThreshold);
                while (pIter.MoveNext())
                {
                    // Get the ellipse.
                    var pEllipse = pIter.Current;

                    // Draw it if necessary.
                    if (pEllipse.Filled == false)
                    {
                        pBitmap.FillEllipse((int)pEllipse.x - 2, (int)pEllipse.y - 2, (int)pEllipse.x + 2, (int)pEllipse.y + 2, Colors.Red);
                        pBitmap.DrawEllipse((int)pEllipse.x - 2, (int)pEllipse.y - 2, (int)pEllipse.x + 2, (int)pEllipse.y + 2, Colors.Green);
                        pEllipse.Filled = true;
                    }
                }
            
                
            }
        }
    }
}
