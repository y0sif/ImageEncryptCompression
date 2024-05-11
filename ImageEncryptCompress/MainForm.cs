using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
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

        //Enc Panel
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
            label2.Visible = false;
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

        private void enc_back_Click(object sender, EventArgs e)
        {
            Enc_Panel.Visible = false;
            Menu_Panel.Visible = false;
            panel1.Visible = false;
            panel2.Visible = false;
            enc_save.Visible = false;
            label2.Visible = false;
            Menu_Panel.Visible = true;
            enc_load.Visible = true;
            pictureBox1.Image = null;
        }
        //End of Enc Panel

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

        //Menu Panel
        private void enc_b_Click(object sender, EventArgs e)
        {
            Menu_Panel.Visible = false;
            Enc_Panel.Visible = true;
            comboBox1.SelectedItem = "Binary";
        }        
        private void break_b_Click(object sender, EventArgs e)
        {
            Menu_Panel.Visible = false;
            Break_Panel.Visible = true;
        }

        private void comp_b_Click(object sender, EventArgs e)
        {
            Menu_Panel.Visible = false;
            Comp_Panel.Visible = true;
            comp_comp.Visible = true;
            comp_radio.Checked = true;
            comp_method.SelectedItem = "Huffman";
        }
        //End of Menu Panel


        //Break Panel
        private void break_back_Click(object sender, EventArgs e)
        {
            Break_Panel.Visible = false;
            Menu_Panel.Visible = true;
            panel13.Visible = false;
            panel14.Visible = false;
            break_insights.Visible = false;
            break_output.Visible = false;
            break_load.Visible = true;
            pictureBox10.Image = null;
        }

        private void break_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox10);
                break_load.Visible = false;
                break_insights.Visible = true;
                panel13.Visible = true;
                textBox17.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                textBox16.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
                break_bits_ValueChanged(sender, e);

            }
        }

        private void break_clear_Click(object sender, EventArgs e)
        {
            panel13.Visible = false;
            panel14.Visible = false;
            break_insights.Visible = false;
            break_output.Visible = false;
            break_load.Visible = true;
            pictureBox10.Image = null;
        }

        private void attack_Click(object sender, EventArgs e)
        {
            int bits = (int)break_bits.Value;
            //break here
            string seed;
            int tap;
            break_output.Visible = false;
            panel14.Visible = false;

            (seed, tap) = ImageOperations.Break_Encryption(ImageMatrix, bits);

            break_output.Visible = true;
            break_seed.Text = seed;
            break_tap.Text = tap.ToString();
        }

        private void break_display_Click(object sender, EventArgs e)
        {
            RGBPixel[,] ImageMatrix_copy = new RGBPixel[ImageMatrix.GetLength(0), ImageMatrix.GetLength(1)];
            for (int n = 0; n < ImageMatrix.GetLength(0); n++)
            {
                for (int m = 0; m < ImageMatrix.GetLength(1); m++)
                {
                    ImageMatrix_copy[n, m] = ImageMatrix[n, m];
                }
            }

            
            ImageMatrix_copy = ImageOperations.LFSR(ImageMatrix_copy, int.Parse(break_tap.Text), break_seed.Text, false);

            panel14.Visible = !panel14.Visible;

            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox9);
        }

        private void break_bits_ValueChanged(object sender, EventArgs e)
        {
            int bits = (int)break_bits.Value;
            long possibilities = bits * (int)Math.Pow(2, bits);
            break_possibilities.Text = possibilities.ToString();
            double time = int.Parse(textBox16.Text) * int.Parse(textBox17.Text) * bits * Math.Pow(2, bits) / 470000;
            time = Math.Ceiling(time + (time / 10));
            if (time < 60)
                break_time.Text = time.ToString() + " seconds";
            else if (time < 3600)
                break_time.Text = Math.Ceiling(time/60).ToString() + " minutes";
            else if (time < 86400)
                break_time.Text = Math.Ceiling(time/3600).ToString() + " hours";
            else
                break_time.Text = Math.Ceiling(time/86400).ToString() + " days";




        }
        //End of Break Panel


        //Comp Panel

        private void comp_radio_CheckedChanged(object sender, EventArgs e)
        {
            //comp_decomp.Visible = false;
            comp_comp.Visible = true;
            comp_output.Visible = false;
            comp_done.Visible = false;
            comp_method.SelectedItem = "Huffman";
        }

        private void decomp_radio_CheckedChanged(object sender, EventArgs e)
        {
            //comp_decomp.Visible = true;
            comp_comp.Visible = false;
            //decomp_method.SelectedItem = "Huffman";
        }

        private void comp_back_Click(object sender, EventArgs e)
        {
            Comp_Panel.Visible = false;
            panel18.Visible = false;
            comp_output.Visible = false;
            comp_load.Visible = true;
            pictureBox12.Image = null;
            Menu_Panel.Visible = true;
            comp_done.Visible = false;

        }

        private void comp_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox12);
                comp_load.Visible = false;
                panel18.Visible = true;
                comp_width.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                comp_height.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            }
        }

        private void comp_clear_Click(object sender, EventArgs e)
        {
            panel18.Visible = false;
            comp_load.Visible = true;
            pictureBox12.Image = null;
            comp_output.Visible = false;
            comp_done.Visible = false;


        }


        // Compression global variables
        // Useful because this function only calculated size but doesn't save file
        // So any change of seed after compression will ruin the new binary file
        Node<int> red_root, green_root, blue_root;
        string[] rgbChannels;
        int tap;
        string seed;
        int width ;
        int height ;


        private void comp_button_Click(object sender, EventArgs e)
        {

            comp_output.Visible = false;
            comp_done.Visible = false;

            string method = comp_method.SelectedItem.ToString();

            RGBPixel[,] ImageMatrix_copy = new RGBPixel[ImageMatrix.GetLength(0), ImageMatrix.GetLength(1)];
            for (int n = 0; n < ImageMatrix.GetLength(0); n++)
            {
                for (int m = 0; m < ImageMatrix.GetLength(1); m++)
                {
                    ImageMatrix_copy[n, m] = ImageMatrix[n, m];
                }
            }

            float ratio = 0;
            tap = (int)comp_tap.Value;
            seed = comp_seed.Text;
            width = int.Parse(comp_width.Text);
            height = int.Parse(comp_height.Text);


            if (method == "Huffman")
            {
                (ratio, red_root, green_root, blue_root, rgbChannels) = ImageOperations.Huffman_Compress(ImageMatrix_copy, tap, seed);
                comp_ratio.Text = ratio.ToString() + " %";
                comp_size.Text = ImageOperations.CalculateCompressedImageSize(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels).ToString() + " Bytes";
            }

            comp_output.Visible = true;

        }

        private void comp_save_Click(object sender, EventArgs e)
        {
            if (ImageOperations.saveBinary(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels) >= 0)
            {
                comp_done.Visible = true;
            }
        }

        private void comp_tap_ValueChanged(object sender, EventArgs e)
        {
            comp_done.Visible = false;
            comp_output.Visible = false;
        }

        private void comp_seed_TextChanged(object sender, EventArgs e)
        {
            comp_done.Visible = false;
            comp_output.Visible = false;
        }

        //End of Comp panel


    }
}