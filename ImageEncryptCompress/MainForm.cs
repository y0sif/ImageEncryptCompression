using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        byte alphaMethod;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
            {
                alphaMethod = 0; //Concat
            }
            else if (radioButton2.Checked)
            {
                alphaMethod = 1; //XOR
            }
            else if (radioButton3.Checked)
            {
                alphaMethod = 2; //Binary
            }

        }
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            /*double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);*/

            string initSeed = txtGaussSigma.Text;
            int tap = (int)nudMaskSize.Value;

            //---ENCRYPTION BREAKING---//
            //int N = 8;
            //(initSeed, tap) = ImageOperations.Break_Encryption(ImageMatrix, N);
            //---ENCRYPTION BREAKING---//

            if (!(radioButton1.Checked || radioButton2.Checked || radioButton3.Checked))
            {
                //throw new Exception("Must choose Method");
            }

            //Do not modify the uploaded image, instead take a copy
            RGBPixel[,] ImageMatrix_copy = new RGBPixel[ImageMatrix.GetLength(0), ImageMatrix.GetLength(1)];
            for (int n = 0; n < ImageMatrix.GetLength(0); n++)
            {
                for (int m = 0; m < ImageMatrix.GetLength(1); m++)
                {
                    ImageMatrix_copy[n, m] = ImageMatrix[n, m];
                }
            }

            //ImageOperations.LFSR(ImageMatrix_copy, tap, initSeed, true, alphaMethod);
            ImageOperations.LFSR(ImageMatrix_copy, tap, initSeed, true);
            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox2);

            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //seed bits
            int N = (int)numericUpDown1.Value;
            string seed;
            int tap;
            (seed, tap) = ImageOperations.Break_Encryption(ImageMatrix, N);
            textBox2.Text = seed;
            textBox1.Text = tap.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string filepath = textBox3.Text;
            string filepath = "D:\\[1] Image Encryption and Compression\\Startup Code\\[TEMPLATE] ImageEncryptCompress\\compImg.bin";
            (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.Huffman_Decompress(filepath);
            //ImageOperations.LFSR(decompressedImage, tap, seed, false, 2);
            ImageOperations.LFSR(decompressedImage, tap, seed, false);
            ImageOperations.DisplayImage(decompressedImage, pictureBox2);
            
        }
    }
}