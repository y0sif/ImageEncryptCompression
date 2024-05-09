using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
            
            if(radioButton1.Checked)
            {
                alphaMethod = 0; //Concat
            }
            else if(radioButton2.Checked)
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

            if(!(radioButton1.Checked || radioButton2.Checked || radioButton3.Checked))
            {
                throw new Exception("Must choose Method");
            }
            ImageMatrix = ImageOperations.LFSR(ImageMatrix, tap, initSeed, false);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}