using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ChromaticAdaptationMatrixCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        double[] toBradford = new double[9] { 0.8951, 0.2664, -0.1614, 
            -0.7502, 1.7135, 0.0367,
            0.0389, -0.0685, 1.0296  };
        double[] fromBradford = new double[9] { 0.986993, -0.147054, 0.159963, 
            0.432305, 0.51836, 0.0492912, 
            -0.00852866, 0.0400428, 0.968487 };

        private void doUpdate()
        {
            double[] tristimulusIn = new double[3];
            double[] tristimulusOut = new double[3];
            double.TryParse(inX_text.Text, out tristimulusIn[0]);
            double.TryParse(inY_text.Text, out tristimulusIn[1]);
            double.TryParse(inZ_text.Text, out tristimulusIn[2]);
            double.TryParse(outX_text.Text, out tristimulusOut[0]);
            double.TryParse(outY_text.Text, out tristimulusOut[1]);
            double.TryParse(outZ_text.Text, out tristimulusOut[2]);

            double[] tristimulusInToBradford = {tristimulusIn[0]*toBradford[0] + tristimulusIn[1]*toBradford[1] + tristimulusIn[2]*toBradford[2] ,
                tristimulusIn[0]*toBradford[3] + tristimulusIn[1]*toBradford[4] + tristimulusIn[2]*toBradford[5],
                tristimulusIn[0]*toBradford[6] + tristimulusIn[1]*toBradford[7] + tristimulusIn[2]*toBradford[8]
            };

            double[] tristimulusOutToBradford = {tristimulusOut[0]*toBradford[0] + tristimulusOut[1]*toBradford[1] + tristimulusOut[2]*toBradford[2] ,
                tristimulusOut[0]*toBradford[3] + tristimulusOut[1]*toBradford[4] + tristimulusOut[2]*toBradford[5],
                tristimulusOut[0]*toBradford[6] + tristimulusOut[1]*toBradford[7] + tristimulusOut[2]*toBradford[8]
            };

            double[] adaptationInBradford = { tristimulusOutToBradford[0]/tristimulusInToBradford[0],0,0,
                0,tristimulusOutToBradford[1]/tristimulusInToBradford[1],0,
                0,0,tristimulusOutToBradford[2]/tristimulusInToBradford[2]
            };

            // Chain matrices through multiplication (order must be reversed from how it will actually affect data)
            double[] matrix = multiplyMatrices(multiplyMatrices(fromBradford,adaptationInBradford),toBradford);
            //matrix = adaptationInBradford;

            //string numberFormat = "0." + (new string('#', 339));
            string numberFormat = "0." + (new string('#', 4));

            matrix_text.Text = matrix[0].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[1].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[2].ToString(numberFormat, CultureInfo.InvariantCulture) + "\r\n"+
                matrix[3].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[4].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[5].ToString(numberFormat, CultureInfo.InvariantCulture) + "\r\n"+
                matrix[6].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[7].ToString(numberFormat, CultureInfo.InvariantCulture) + " " + matrix[8].ToString(numberFormat, CultureInfo.InvariantCulture) + "\r\n";
        }

        // a1b1 + a4b2 + a7b3, a2b1 + a5b2 + a8b3 , a3b1 + a6b2 + a9b3
        private double[] multiplyMatrices(double[] B, double[] A)
        {
            double[] multiplied = { A[0]*B[0] + A[3]*B[1] + A[6]*B[2], A[1] * B[0] + A[4] * B[1] + A[7] * B[2], A[2] * B[0] + A[5] * B[1] + A[8] * B[2],
                A[0]*B[3] + A[3]*B[4] + A[6]*B[5], A[1] * B[3] + A[4] * B[4] + A[7] * B[5], A[2] * B[3] + A[5] * B[4] + A[8] * B[5],
                A[0]*B[6] + A[3]*B[7] + A[6]*B[8], A[1] * B[6] + A[4] * B[7] + A[7] * B[8], A[2] * B[6] + A[5] * B[7] + A[8] * B[8]
            };
            return multiplied;
        }

        private void tristimulus_KeyUp(object sender, KeyEventArgs e)
        {
            doUpdate();
        }
    }
}
