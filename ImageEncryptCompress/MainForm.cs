using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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
        private void enc_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                enc_load.Visible = false;
                panel1.Visible = true;
                txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            }
            
        }
        private void enc_clear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            panel2.Visible = false;
            enc_save.Visible = false;
            panel1.Visible = false;
            enc_load.Visible = true;
            label2.Visible = false;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
/**
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
**/
        }
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            /*double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);*/

            string initSeed = txtGaussSigma.Text;
            int tap = (int)nudMaskSize.Value;

            //if (!(radioButton1.Checked || radioButton2.Checked || radioButton3.Checked))
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

            string method = comboBox1.SelectedItem.ToString();

            if (method == "Binary")
                ImageMatrix_copy = ImageOperations.LFSR(ImageMatrix_copy, tap, initSeed, true);

            //float ratio = ImageOperations.Huffman_Compress(ImageMatrix_copy, tap, initSeed);

            //textBox3.Text = ratio.ToString();

            panel2.Visible = true;
            enc_save.Visible = true;
            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox2);

            //radioButton1.Checked = false;
            //radioButton2.Checked = false;
            //radioButton3.Checked = false;
        }
        private void enc_save_Click(object sender, EventArgs e)
        {
            if (ImageOperations.saveImage(pictureBox2) >= 0)
            {
                label2.Visible = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        /**
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
        **/

        private void button2_Click(object sender, EventArgs e)
        {
            //string filepath = textBox3.Text;
            string filepath = "D:\\[1] Image Encryption and Compression\\Startup Code\\[TEMPLATE] ImageEncryptCompress\\compImg.bin";
            (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.Huffman_Decompress(filepath);
            //ImageOperations.LFSR(decompressedImage, tap, seed, false, 2);
            ImageOperations.LFSR(decompressedImage, tap, seed, false);
            ImageOperations.DisplayImage(decompressedImage, pictureBox2);
            
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void enc_b_Click(object sender, EventArgs e)
        {
            Menu_Panel.Visible = false;
            Enc_Panel.Visible = true;
        }

    }
}