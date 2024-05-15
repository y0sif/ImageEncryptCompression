using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
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
            try
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
            catch (Exception ex)
            {
                enc_load.Visible = true;
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
            label56.Visible = false;
            enc_timeBox.Visible = false;
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {

            string initSeed = txtGaussSigma.Text;
            int tap = (int)nudMaskSize.Value;

            label56.Visible = false;
            enc_timeBox.Visible = false;

            //Do not modify the uploaded image, instead take a copy
            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

            string method = comboBox1.SelectedItem.ToString();

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (method == "Binary")
                ImageMatrix_copy = ImageOperations.LFSR(ImageMatrix_copy, tap, initSeed);
            else if (method == "Alphanumerical Concat")
                ImageMatrix_copy = ImageOperations.AlphaNumLFSR(ImageMatrix_copy, tap, initSeed, false);
            else if (method == "Alphanumerical XOR")
                ImageMatrix_copy = ImageOperations.AlphaNumLFSR(ImageMatrix_copy, tap, initSeed, true);
            
            stopwatch.Stop();

            double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

            label56.Visible = true;
            enc_timeBox.Visible = true;
            int minutes = (int)elapsedTimeInSeconds / 60;
            int seconds = (int)elapsedTimeInSeconds % 60;
            enc_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";

            panel2.Visible = true;
            enc_save.Visible = true;
            label2.Visible = false;
            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox2);


        }
        private void nudMaskSize_ValueChanged(object sender, EventArgs e)
        {
            panel2.Visible = false;
            label2.Visible = false;
            enc_save.Visible = false;
            enc_timeBox.Visible = false;
            label56.Visible = false;
        }

        private void txtGaussSigma_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Binary")
            {
                if (!ImageOperations.IsBinary(txtGaussSigma.Text))
                    txtGaussSigma.Text = "1";
                nudMaskSize.Maximum = txtGaussSigma.Text.Length - 1;
            }
            else if (comboBox1.SelectedItem.ToString() == "Alphanumerical Concat")
            {
                nudMaskSize.Maximum = (txtGaussSigma.Text.Length * 8) - 1;
            }
            else if (comboBox1.SelectedItem.ToString() == "Alphanumerical XOR")
            {
                nudMaskSize.Maximum = txtGaussSigma.Text.Length - 1;
            }
            panel2.Visible = false;
            label2.Visible = false;
            enc_save.Visible = false;
            enc_timeBox.Visible = false;
            label56.Visible = false;
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
            label56.Visible = false;
            enc_timeBox.Visible = false;
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            panel2.Visible = false;
            label2.Visible = false;
            enc_save.Visible = false;
            label56.Visible = false;
            enc_timeBox.Visible = false;

            if (comboBox1.SelectedItem.ToString() == "Binary")
            {
                if (!ImageOperations.IsBinary(txtGaussSigma.Text))
                    txtGaussSigma.Text = "1";
                nudMaskSize.Maximum = txtGaussSigma.Text.Length - 1;
            }
            else if (comboBox1.SelectedItem.ToString() == "Alphanumerical Concat")
            {
                nudMaskSize.Maximum = (txtGaussSigma.Text.Length * 8) - 1;
            }
            else if (comboBox1.SelectedItem.ToString() == "Alphanumerical XOR")
            {
                nudMaskSize.Maximum = txtGaussSigma.Text.Length - 1;
            }

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


        private void op_b_Click(object sender, EventArgs e)
        {
            Menu_Panel.Visible = false;
            Complete_Panel.Visible = true;
            fwd_panel.Visible = true;
            complete_fwd.Checked = true;
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
            label61.Visible = false;
            break_timeBox.Visible = false;
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
            label61.Visible = false;
            break_timeBox.Visible = false;
        }

        private void attack_Click(object sender, EventArgs e)
        {
            int bits = (int)break_bits.Value;
            //break here
            string seed;
            int tap;
            break_output.Visible = false;
            panel14.Visible = false;
            label61.Visible = false;
            break_timeBox.Visible = false;

            Stopwatch stopwatch = Stopwatch.StartNew();

            (seed, tap) = ImageOperations.Break_Encryption(ImageMatrix, bits);
            stopwatch.Stop();
            double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

            label61.Visible = true;
            break_timeBox.Visible = true;
            int minutes = (int)elapsedTimeInSeconds / 60;
            int seconds = (int)elapsedTimeInSeconds % 60;
            break_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";

            break_output.Visible = true;
            break_seed.Text = seed;
            break_tap.Text = tap.ToString();
        }

        private void break_display_Click(object sender, EventArgs e)
        {
            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

            ImageMatrix_copy = ImageOperations.LFSR(ImageMatrix_copy, int.Parse(break_tap.Text), break_seed.Text);

            panel14.Visible = !panel14.Visible;

            ImageOperations.DisplayImage(ImageMatrix_copy, pictureBox9);
        }

        private void break_bits_ValueChanged(object sender, EventArgs e)
        {
            int bits = (int)break_bits.Value;
 
            long possibilities = bits * (long)Math.Pow(2, bits);

            break_possibilities.Text = possibilities.ToString();

            double time = int.Parse(textBox16.Text) * int.Parse(textBox17.Text) * bits * Math.Pow(2, bits) / 1000000;
            time = Math.Ceiling(time + (time / 10));
            if (time < 60)
                break_time.Text = time.ToString() + " seconds";
            else if (time < 3600)
                break_time.Text = Math.Ceiling(time/60).ToString() + " minutes";
            else if (time < 86400)
                break_time.Text = Math.Ceiling(time/3600).ToString() + " hours";
            else if (time < 31536000)
                break_time.Text = Math.Ceiling(time/86400).ToString() + " days";
            else
                break_time.Text = Math.Ceiling(time / 31536000).ToString() + " years";




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
            comp_timeBox.Visible = false;
            label58.Visible = false;
            panel18.Visible = false;
            comp_load.Visible = true;
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
            decomp_timeBox.Visible = false;
            label60.Visible = false;
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
            label58.Visible = false;
            comp_timeBox.Visible = false;

        }

        private void comp_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            try
            {
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
                    comp_fail.Visible = false;
                }
            }
            catch (Exception ex)
            {
                comp_load.Visible = true;
            }
        }

        private void comp_clear_Click(object sender, EventArgs e)
        {
            panel18.Visible = false;
            comp_load.Visible = true;
            pictureBox12.Image = null;
            comp_output.Visible = false;
            comp_done.Visible = false;
            label58.Visible = false;
            comp_timeBox.Visible = false;

        }


        // Compression global variables
        // Useful because this function only calculated size but doesn't save file
        // So any change of seed after compression will ruin the new binary file
        Node<short> red_root, green_root, blue_root;
        string[] rgbChannels;
        int tap;
        string seed;
        int width ;
        int height ;
        List<byte> redVal, greenVal, blueVal;
        List<int> redFreq, greenFreq, blueFreq;


        private void comp_button_Click(object sender, EventArgs e)
        {
            comp_fail.Visible = false;
            comp_output.Visible = false;
            comp_done.Visible = false;
            label58.Visible = false;
            comp_timeBox.Visible = false;

            string method = comp_method.SelectedItem.ToString();

            RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

            float ratio = 0;
            tap = (int)comp_tap.Value;
            seed = comp_seed.Text;
            width = int.Parse(comp_width.Text);
            height = int.Parse(comp_height.Text);


            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                if (method == "Huffman")
                {
                    (ratio, red_root, green_root, blue_root, rgbChannels) = ImageOperations.Huffman_Compress(ImageMatrix_copy, tap, seed);

                }
                else
                {
                    (ratio, redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq) = ImageOperations.RunLengthEncoding(ImageMatrix, tap, seed);

                }

                stopwatch.Stop();

                double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
                label58.Visible = true;
                comp_timeBox.Visible = true;
                int minutes = (int)elapsedTimeInSeconds / 60;
                int seconds = (int)elapsedTimeInSeconds % 60;
                comp_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";

                comp_ratio.Text = ratio.ToString() + " %";

                if (method == "Huffman")
                    comp_size.Text = ImageOperations.CalculateCompressedImageSize(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels).ToString() + " Bytes";
                else
                    comp_size.Text = ImageOperations.CalculateCompressedImageSizeForRLE(redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq, tap, seed, width, height).ToString() + " Bytes";

                comp_output.Visible = true;
            }
            catch (Exception ex) {
                comp_fail.Visible = true;
            }
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
            comp_timeBox.Visible = false;
            label60.Visible = false;

        }

        private void comp_seed_TextChanged(object sender, EventArgs e)
        {
            comp_done.Visible = false;
            comp_output.Visible = false;
            comp_timeBox.Visible = false;
            label60.Visible = false;
            comp_tap.Maximum = comp_seed.Text.Length - 1;
        }


        private void comp_method_SelectionChangeCommitted(object sender, EventArgs e)
        {
            comp_output.Visible = false;
            comp_timeBox.Visible = false;
            label58.Visible = false;
        }

        private void decomp_load_Click(object sender, EventArgs e)
        {
            (string fileName, long fileSize) = ImageOperations.loadBinary();
            if (fileName != null)
            {
                decomp_name.Text = fileName;
                decomp_size.Text = fileSize.ToString() + " Bytes";
                panel6.Visible = true;
                panel16.Visible = false;
                decomp_done.Visible = false;
                decomp_timeBox.Visible = false;
                decomp_save.Visible = false;
                label60.Visible = false;
                decomp_fail.Visible = false;

            }
        }

        private void decomp_button_Click(object sender, EventArgs e)
        {
            decomp_done.Visible = false;
            decomp_save.Visible = false;
            panel16.Visible = false;
            decomp_fail.Visible = false;
            label60.Visible = false;
            decomp_timeBox.Visible = false;

            string filePath = decomp_name.Text;
            string method = decomp_method.Text.ToString();
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (method == "Huffman")
                {
                    (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.Huffman_Decompress(filePath);
                    stopwatch.Stop();
                    ImageOperations.DisplayImage(decompressedImage, pictureBox11);
                }
                else
                {
                    (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.RunLengthDecoding(filePath);
                    stopwatch.Stop();
                    ImageOperations.DisplayImage(decompressedImage, pictureBox11);
                }

                double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

                label60.Visible = true;
                decomp_timeBox.Visible = true;

                int minutes = (int)elapsedTimeInSeconds / 60;
                int seconds = (int)elapsedTimeInSeconds % 60;
                decomp_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";

                decomp_save.Visible = true;
                panel16.Visible = true;
            }
            catch (Exception ex)
            {
                decomp_fail.Visible = true;
            }

            
        } 

        private void decomp_save_Click(object sender, EventArgs e)
        {
            if (ImageOperations.saveImage(pictureBox11) >= 0)
            {
                decomp_done.Visible = true;
            }
        }

        private void decomp_method_SelectionChangeCommitted(object sender, EventArgs e)
        {
            panel16.Visible = false;
            decomp_save.Visible = false;
            decomp_done.Visible = false;
            decomp_timeBox.Visible = false;
            label60.Visible = false;
        }

        //End of Comp panel



        //Complete panel

        private void complete_fwd_CheckedChanged(object sender, EventArgs e)
        {
            if (complete_fwd.Checked == false) return;

            bck_panel.Visible = false;
            fwd_panel.Visible = true;
            fwd_output.Visible = false;
            fwd_done.Visible = false;
            fwd_panel1.Visible = false;
            fwd_panel2.Visible = false;
            fwd_timeBox.Visible = false;
            label64.Visible = false;
            fwd_load.Visible = true; 

        }

        private void complete_bck_CheckedChanged(object sender, EventArgs e)
        {
            if (complete_bck.Checked == false) return;

            bck_panel.Visible = true;
            fwd_panel.Visible = false;
            bck_done.Visible = false;
            panel20.Visible = false;
            panel21.Visible = false;
            bck_save.Visible = false;
            bck_timeBox.Visible = false;
            label65.Visible = false;
        }

        private void complete_back_Click(object sender, EventArgs e)
        {
            Complete_Panel.Visible = false;
            Menu_Panel.Visible = true;
            fwd_done.Visible = false;
            fwd_output.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
            fwd_panel1.Visible = false;
            fwd_panel2.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
            fwd_load.Visible = true;
        }

        private void fwd_load_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Open the browsed image and display it
                    string OpenedFilePath = openFileDialog1.FileName;
                    ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                    ImageOperations.DisplayImage(ImageMatrix, pictureBox13);
                    fwd_load.Visible = false;
                    fwd_panel1.Visible = true;
                    fwd_panel2.Visible = true;
                    fwd_width.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                    fwd_height.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
                    fwd_fail.Visible = false;
                }
            }
            catch (Exception ex)
            {
                fwd_fail.Visible = true;
            }
        }

        private void fwd_seed_TextChanged(object sender, EventArgs e)
        {
            fwd_tap.Maximum = fwd_seed.Text.Length - 1;
            fwd_output.Visible = false;
            fwd_timeBox.Visible = false;
            label64.Visible = false;
        }

        private void fwd_tap_ValueChanged(object sender, EventArgs e)
        {
            fwd_output.Visible = false;
            fwd_timeBox.Visible = false;
            label64.Visible = false;
        }

        private void fwd_clear_Click(object sender, EventArgs e)
        {
            pictureBox13.Image = null;
            fwd_load.Visible = true;
            fwd_panel1.Visible = false;
            fwd_panel2.Visible = false;
            fwd_output.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
            fwd_done.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
        }

        private void fwd_button_Click(object sender, EventArgs e)
        {
            fwd_output.Visible = false;
            fwd_done.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
            label64.Visible = false;
            fwd_timeBox.Visible = false;
            fwd_fail.Visible = false;

            try
            {
                RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

                float ratio = 0;
                tap = (int)fwd_tap.Value;
                seed = fwd_seed.Text;
                width = int.Parse(fwd_width.Text);
                height = int.Parse(fwd_height.Text);

                Stopwatch stopwatch = Stopwatch.StartNew();

                ImageOperations.LFSR(ImageMatrix_copy, tap, seed);

                (ratio, red_root, green_root, blue_root, rgbChannels) = ImageOperations.Huffman_Compress(ImageMatrix_copy, tap, seed);

                fwd_ratio.Text = ratio.ToString() + " %";
                fwd_size.Text = ImageOperations.CalculateCompressedImageSize(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels).ToString() + " Bytes";

                stopwatch.Stop();
                double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

                label64.Visible = false;
                fwd_timeBox.Visible = false;

                int minutes = (int)elapsedTimeInSeconds / 60;
                int seconds = (int)elapsedTimeInSeconds % 60;
                fwd_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";

                fwd_output.Visible = true;
                label64.Visible = true;
                fwd_timeBox.Visible = true;
            }
            catch (Exception ex)
            {
                fwd_fail.Visible = true;
            }
        }

        private void fwd_save_Click(object sender, EventArgs e)
        {
           
            if (ImageOperations.saveBinary(seed, tap, red_root, green_root, blue_root, width, height, rgbChannels) >= 0)
            {
                fwd_done.Visible = true;
            }
            
        }


        private void bck_load_Click(object sender, EventArgs e)
        {
            try
            {
                (string fileName, long fileSize) = ImageOperations.loadBinary();
                if (fileName != null)
                {
                    bck_file.Text = fileName;
                    bck_size.Text = fileSize.ToString() + " Bytes";
                    panel20.Visible = true;
                    panel21.Visible = false;
                    bck_save.Visible = false;
                    bck_done.Visible = false;
                    label65.Visible = false;
                    bck_timeBox.Visible = false;
                    bck_fail.Visible = false;
                }
            }
            catch (Exception ex)
            {
                bck_fail.Visible = true;
            }
        }

        private void bck_button_Click(object sender, EventArgs e)
        {
            bck_done.Visible = false;
            bck_save.Visible = false;
            panel21.Visible = false;
            label65.Visible = false;
            bck_timeBox.Visible = false;
            bck_fail.Visible = false;

            try
            {
                string filePath = bck_file.Text;

                Stopwatch stopwatch = Stopwatch.StartNew();
                (RGBPixel[,] decompressedImage, int tap, string seed) = ImageOperations.Huffman_Decompress(filePath);
                ImageOperations.LFSR(decompressedImage, tap, seed);
                stopwatch.Stop();
                double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

                ImageOperations.DisplayImage(decompressedImage, pictureBox14);


                bck_save.Visible = true;
                panel21.Visible = true;
                bck_timeBox.Visible = true;
                label65.Visible = true;

                int minutes = (int)elapsedTimeInSeconds / 60;
                int seconds = (int)elapsedTimeInSeconds % 60;
                bck_timeBox.Text = minutes.ToString() + " min, " + seconds.ToString() + " sec";
            }
            catch (Exception ex)
            {
                bck_fail.Visible = true;
            }
        }

        private void bck_save_Click(object sender, EventArgs e)
        {
            if (ImageOperations.saveImage(pictureBox14) >= 0)
            {
                bck_done.Visible = true;
            }
        }
        //End of complete panel


    }
}