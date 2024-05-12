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


            //Do not modify the uploaded image, instead take a copy
            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

            string method = comboBox1.SelectedItem.ToString();

            if (method == "Binary")
                ImageMatrix_copy = ImageOperations.LFSR(ImageMatrix_copy, tap, initSeed, true);


            panel2.Visible = true;
            enc_save.Visible = true;
            label2.Visible = false;
            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox2);


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
            decomp_method.SelectedItem = "Huffman";

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
            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

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
            if (comp_radio.Checked == false) return;

            comp_decomp.Visible = false;
            comp_comp.Visible = true;
            comp_output.Visible = false;
            comp_done.Visible = false;
            comp_method.SelectedItem = "Huffman";
        }

        private void decomp_radio_CheckedChanged(object sender, EventArgs e)
        {
            if (decomp_radio.Checked == false) return;

            comp_decomp.Visible = true;
            comp_comp.Visible = false;
            decomp_done.Visible = false;
            panel6.Visible = false;
            panel16.Visible = false;
            decomp_save.Visible = false;
            decomp_method.SelectedItem = "Huffman";
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
        List<int> redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq;



        private void comp_button_Click(object sender, EventArgs e)
        {

            comp_output.Visible = false;
            comp_done.Visible = false;

            string method = comp_method.SelectedItem.ToString();

            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

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
            else
            {
                (ratio, redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq) = ImageOperations.RunLengthEncoding(ImageMatrix, tap, seed);
                comp_ratio.Text = ratio.ToString() + " %";
                comp_size.Text = ImageOperations.CalculateCompressedImageSizeForRLE(redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq, tap, seed, width, height).ToString() + " Bytes";

            }

            comp_output.Visible = true;

        }

        private void comp_save_Click(object sender, EventArgs e)
        {
            string method = comp_method.SelectedItem.ToString();
            if (method == "Huffman")
            {
                if (ImageOperations.saveBinary(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels) >= 0)
                {
                    comp_done.Visible = true;
                }
            }
            else
            {
                if(ImageOperations.saveBinaryForRLE(redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq, tap, seed, width, height) >= 0)
                {
                    comp_done.Visible = true;
                }
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

        private void decomp_load_Click(object sender, EventArgs e)
        {
            (string fileName, long fileSize) = ImageOperations.loadBinary();
            if (fileName != null)
            {
                decomp_name.Text = fileName;
                decomp_size.Text = fileSize.ToString() + " Bytes";
                panel6.Visible = true;
            }
        }

        private void decomp_button_Click(object sender, EventArgs e)
        {
            decomp_done.Visible = false;
            decomp_save.Visible = false;
            panel16.Visible = false;

            string filePath = decomp_name.Text;
            string method = decomp_method.Text.ToString();
            if(method == "Huffman")
            {
                (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.Huffman_Decompress(filePath);
                ImageOperations.DisplayImage(decompressedImage, pictureBox11);
            }
            else
            {
                (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.RunLengthDecoding(filePath);
                ImageOperations.DisplayImage(decompressedImage, pictureBox11);

            }
            

            decomp_save.Visible = true;
            panel16.Visible = true;
        }

        private void decomp_save_Click(object sender, EventArgs e)
        {
            if (ImageOperations.saveImage(pictureBox11) >= 0)
            {
                decomp_done.Visible = true;
            }
        }

        //End of Comp panel



    }
}