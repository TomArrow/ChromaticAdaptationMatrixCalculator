using Be.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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


        string numberFormat = "0." + (new string('#', 4));

        private double[] getWhiteAdaptationMatrix(double[] tristimulusIn, double[] tristimulusOut)
        {
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
            double[] matrix = multiplyMatrices(multiplyMatrices(fromBradford, adaptationInBradford), toBradford);
            //matrix = adaptationInBradford;

            //string numberFormat = "0." + (new string('#', 339));

            return matrix;
        }

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

            double[] matrix = getWhiteAdaptationMatrix(tristimulusIn, tristimulusOut);

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

        private double[] xyToTristimulus(double[] xy)
        {
            double Y = 1;
            double[] tristimulus = new double[3];

            tristimulus[0] = xy[0] * Y / xy[1];
            tristimulus[1] = Y;
            tristimulus[2] = (1 - xy[0] - xy[1]) * Y / xy[1];

            return tristimulus;

        }



        // Convert x,y to XYZ
        private void xy_KeyUp(object sender, KeyEventArgs e)
        {
            double[] inWhite = new double[2];
            double[] outWhite = new double[2];

            double.TryParse(inx_text.Text, out inWhite[0]);
            double.TryParse(iny_text.Text, out inWhite[1]);
            double.TryParse(outx_text.Text, out outWhite[0]);
            double.TryParse(outy_text.Text, out outWhite[1]);

            double[] tristimulusIn = xyToTristimulus(inWhite);//new double[3];
            double[] tristimulusOut = xyToTristimulus(outWhite);//new double[3];

            /*
            double Y = 1;

            tristimulusIn[1] = Y;
            tristimulusOut[1] = Y;

            tristimulusIn[0] = inWhite[0] * Y / inWhite[1];
            tristimulusIn[2] = (1 - inWhite[0] - inWhite[1]) * Y / inWhite[1];

            tristimulusOut[0] = outWhite[0] * Y / outWhite[1];
            tristimulusOut[2] = (1 - outWhite[0] - outWhite[1]) * Y / outWhite[1];
            */
            inX_text.Text = tristimulusIn[0].ToString(numberFormat, CultureInfo.InvariantCulture);
            inY_text.Text = tristimulusIn[1].ToString(numberFormat, CultureInfo.InvariantCulture);
            inZ_text.Text = tristimulusIn[2].ToString(numberFormat, CultureInfo.InvariantCulture);

            outX_text.Text = tristimulusOut[0].ToString(numberFormat, CultureInfo.InvariantCulture);
            outY_text.Text = tristimulusOut[1].ToString(numberFormat, CultureInfo.InvariantCulture);
            outZ_text.Text = tristimulusOut[2].ToString(numberFormat, CultureInfo.InvariantCulture);

            doUpdate();
        }

        //
        //
        // All the RGB XYZ stuff:
        //

        private void rgbxyz_KeyUp(object sender, KeyEventArgs e)
        {
            RGBXYZupdate();
        }

        private void RGBXYZdoChromaticallyAdapt_Checked(object sender, RoutedEventArgs e)
        {
            RGBXYZupdate();
        }

        private double[] VectorXMatrix(double[] vector, double[] matrix)
        {
            double[] outVector =  { matrix[0]*vector[0] + matrix[1] * vector[1] + matrix[2] * vector[2],
                matrix[3]*vector[0] + matrix[4] * vector[1] + matrix[5] * vector[2],
                matrix[6]*vector[0] + matrix[7] * vector[1] + matrix[8] * vector[2]
            };
            return outVector;
        }

        // its two matrices as an array. First is RGB to XYZ, second is XYZ to RGB
        private double[] getRGBXYZMatrix()
        {
            double[] xyWhite = new double[2];
            double[] xyRed = new double[2];
            double[] xyGreen = new double[2];
            double[] xyBlue = new double[2];

            double.TryParse(RGBXYZwhitex_text.Text, out xyWhite[0]);
            double.TryParse(RGBXYZwhitey_text.Text, out xyWhite[1]);
            double.TryParse(RGBXYZredx_text.Text, out xyRed[0]);
            double.TryParse(RGBXYZredy_text.Text, out xyRed[1]);
            double.TryParse(RGBXYZgreenx_text.Text, out xyGreen[0]);
            double.TryParse(RGBXYZgreeny_text.Text, out xyGreen[1]);
            double.TryParse(RGBXYZbluex_text.Text, out xyBlue[0]);
            double.TryParse(RGBXYZbluey_text.Text, out xyBlue[1]);

            double[] tristimulusWhite = xyToTristimulus(xyWhite);
            double[] tristimulusRed = xyToTristimulus(xyRed);
            double[] tristimulusGreen = xyToTristimulus(xyGreen);
            double[] tristimulusBlue = xyToTristimulus(xyBlue);

            double[] someMatrix = { tristimulusRed[0], tristimulusGreen[0], tristimulusBlue[0],
                tristimulusRed[1], tristimulusGreen[1], tristimulusBlue[1],
                tristimulusRed[2], tristimulusGreen[2], tristimulusBlue[2],
            };

            someMatrix = matrixInvert(someMatrix);

            double[] S = VectorXMatrix(tristimulusWhite, someMatrix);
            /*double[] S = { someMatrix[0]*tristimulusWhite[0] + someMatrix[1] * tristimulusWhite[1] + someMatrix[2] * tristimulusWhite[2],
                someMatrix[3]*tristimulusWhite[0] + someMatrix[4] * tristimulusWhite[1] + someMatrix[5] * tristimulusWhite[2],
                someMatrix[6]*tristimulusWhite[0] + someMatrix[7] * tristimulusWhite[1] + someMatrix[8] * tristimulusWhite[2]
            };*/

            double[] matrix = { S[0]*tristimulusRed[0], S[1]*tristimulusGreen[0], S[2]*tristimulusBlue[0],
                S[0]*tristimulusRed[1], S[1]*tristimulusGreen[1], S[2]*tristimulusBlue[1],
                S[0]*tristimulusRed[2], S[1]*tristimulusGreen[2], S[2]*tristimulusBlue[2],
            };


            bool chromaticAdaptNeededD50 = RGBXYZdoChromaticallyAdapt.IsChecked == true;
            bool chromaticAdaptNeededXYZ = RGBXYZdoChromaticallyAdaptXYZ.IsChecked == true;

            if (chromaticAdaptNeededD50)
            {
                double[] D50tristimulusWhite = xyToTristimulus(new double[] { 0.34567, 0.35850 }) ;
                double[] chromaticAdaptionMatrix = getWhiteAdaptationMatrix(tristimulusWhite, D50tristimulusWhite);
                matrix = multiplyMatrices(chromaticAdaptionMatrix, matrix);
            } else if (chromaticAdaptNeededXYZ)
            {
                double[] XYZtristimulusWhite = xyToTristimulus(new double[] { 0.333333, 0.333333 }) ;
                double[] chromaticAdaptionMatrix = getWhiteAdaptationMatrix(tristimulusWhite, XYZtristimulusWhite);
                matrix = multiplyMatrices(chromaticAdaptionMatrix, matrix);
            }

            return matrix;
        }

        private void RGBXYZupdate()
        {
            double[] matrix = getRGBXYZMatrix();
            matrixRGBtoXYZ_text.Text = matrixToString(matrix);
            matrixXYZtoRGB_text.Text = matrixToString(matrixInvert(matrix));
        }

        private string matrixToString(double[] matrix)
        {

            return doubleToString(matrix[0]) + " " + doubleToString(matrix[1]) + " " + doubleToString(matrix[2]) + "\r\n" +
                doubleToString(matrix[3]) + " " + doubleToString(matrix[4]) + " " + doubleToString(matrix[5]) + "\r\n" +
                doubleToString(matrix[6]) + " " + doubleToString(matrix[7]) + " " + doubleToString(matrix[8]) + "\r\n";
        }

        private string doubleToString(double number)
        {
            return number.ToString(numberFormat, CultureInfo.InvariantCulture);
        }

        private double[] matrixInvert(double[] matrix)
        {
            double[][] twoDimMatrix = new double[3][];
            twoDimMatrix[0] =  new double[3] { matrix[0], matrix[1], matrix[2] };
            twoDimMatrix[1] =  new double[3] { matrix[3], matrix[4], matrix[5] };
            twoDimMatrix[2] =  new double[3] { matrix[6], matrix[7], matrix[8] };
            double[][] invertedMatrix;
            try
            {

                invertedMatrix = MatrixOps.MatrixInverse(twoDimMatrix);
            } catch (Exception e)
            {
                // Inversion not possible (not all values filled correctly yet?)

                invertedMatrix = MatrixOps.MatrixCreate(3,3);
            }

            return new double[9] { 
                invertedMatrix[0][0],invertedMatrix[0][1],invertedMatrix[0][2],
                invertedMatrix[1][0],invertedMatrix[1][1],invertedMatrix[1][2],
                invertedMatrix[2][0],invertedMatrix[2][1],invertedMatrix[2][2]
            };
        }

        private void btnSaveRGBtoXYZCHA_Click(object sender, RoutedEventArgs e)
        {

            double[] matrix = getRGBXYZMatrix();
            CHASave(matrix, "_RGBtoXYZ");
        }

        private void btnSaveXYZtoRGBCHA_Click(object sender, RoutedEventArgs e)
        {

            double[] matrix = getRGBXYZMatrix();
            matrix = matrixInvert(matrix);
            CHASave(matrix, "_RGBtoXYZ");
        }

        private void CHASave(double[] matrix, string proposedFileName = "matrix")
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Photoshop RGB Channel Mixer Preset (.cha)|*.cha";

            bool normalizationNeeded = false;
            double normalizationMaxValue = 2;
            for (int i = 0; i < 9; i++)
            {
                if (matrix[i] > 2 || matrix[i] < -2)
                {
                    normalizationNeeded = true;
                    if(Math.Abs(matrix[i]) > normalizationMaxValue)
                    {
                        normalizationMaxValue = Math.Abs(matrix[i]);
                    }
                }
            }

            double multiplier = 100;

            if (normalizationNeeded)
            {
                MessageBoxResult wish = MessageBox.Show("The values of this matrix unfortunately exceed the -200% to 200% limit of the CHA file format. Do you wish to normalize the values? If you hit 'No', values will be clipped instead (not recommended), which will throw off color balance. Normalizing will only affect brightness.","Decision needed",MessageBoxButton.YesNoCancel);
                if (wish == MessageBoxResult.Yes)
                {
                    multiplier = 100.0 * 2.0 / normalizationMaxValue;
                    proposedFileName += "_normalized";
                }
                else if(wish==MessageBoxResult.No){

                    proposedFileName += "_clipped";
                } else
                {
                    return; // Cancel.
                }
            }


            sfd.FileName = proposedFileName+".cha";
            if (sfd.ShowDialog() == true)
            {
                string filename = sfd.FileName;
                using (FileStream fs = File.Open(filename, FileMode.Create))
                {

                    using (BeBinaryWriter binWriter = new BeBinaryWriter(fs))
                    {

                        binWriter.Write((short)1);//Version
                        binWriter.Write((short)0);//Monochrome
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[0] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[1] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[2] * multiplier)));
                        binWriter.Write((short)0); //Useless (CMYK?)
                        binWriter.Write((short)0); //Constant
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[3] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[4] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[5] * multiplier)));
                        binWriter.Write((short)0); //Useless (CMYK?)
                        binWriter.Write((short)0); //Constant
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[6] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[7] * multiplier)));
                        binWriter.Write((short)Math.Max(-200, Math.Min(200, matrix[8] * multiplier)));
                        binWriter.Write((short)0); //Useless (CMYK?)
                        binWriter.Write((short)0); //Constant

                        // No idea what the next values mean, but they are necessary apparently:
                        binWriter.Write((short)0);
                        binWriter.Write((short)0);
                        binWriter.Write((short)0);
                        binWriter.Write((short)100);
                        binWriter.Write((short)0);


                    }
                }
            }
        }
    }
}
