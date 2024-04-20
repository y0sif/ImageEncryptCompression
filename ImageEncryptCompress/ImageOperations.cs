using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    
  
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        //--------------------------------//
        // ENCRYPTION & DECRYPTION CODE   //
        //--------------------------------//

        //Global Attributes
        private static char[][] RGBKeys = new char[3][];
        private const int KEY_SIZE = 8;

        public static void KeyGeneration(int tapPosition, char[] seed)
        {                        
            int bitSize = seed.Length;

            for (int k = 0; k < RGBKeys.Length; k++)
            {
                char[] keyString = new char[KEY_SIZE];
                for (int i = 0; i < KEY_SIZE; i++)
                {
                    char shiftOut = seed[0];
                    char res = (char)(((seed[bitSize - tapPosition] - '0') ^ (shiftOut - '0')) + 48);

                    for (int j = 1; j < bitSize; j++)
                    {
                        seed[j - 1] = seed[j];
                    }

                    seed[bitSize - 1] = res;
                    keyString[i] = res;
                }
                
                RGBKeys[k] = keyString;
            }
        }

        public static RGBPixel[,] LFSR(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed, bool encrypt)
        {
            char[] seed = initSeed.ToCharArray();

            for (int row = 0; row < GetHeight(ImageMatrix); row++)
            {
                for(int col = 0; col < GetWidth(ImageMatrix); col++)
                {
                    ref RGBPixel pixel = ref ImageMatrix[row, col];

                    KeyGeneration(tapPosition, seed);

                    char[] redKey = RGBKeys[0];
                    char[] greenKey = RGBKeys[1];
                    char[] blueKey = RGBKeys[2];

                    char[] redVal = ConvertToBinary(pixel.red);
                    char[] greenVal = ConvertToBinary(pixel.green);
                    char[] blueVal = ConvertToBinary(pixel.blue);

                    for (int i = 0; i < KEY_SIZE; i++)
                    {
                        redVal[i] = (char)(((redVal[i] - '0') ^ (redKey[i] - '0')) + 48);
                        greenVal[i] = (char)(((greenVal[i] - '0') ^ (greenKey[i] - '0')) + 48);
                        blueVal[i] = (char)(((blueVal[i] - '0') ^ (blueKey[i] - '0')) + 48);
                    }
                    
                    pixel.red = ConvertToDecimal(redVal);
                    pixel.green = ConvertToDecimal(greenVal);
                    pixel.blue = ConvertToDecimal(blueVal);
                }
            }

            /*if(encrypt)
                Huffman_Compress(ImageMatrix, tapPosition, initSeed);                
            else*/
                return ImageMatrix;
        }

        //--------------------------------//
        // COMPRESSION & DECOMPRESSION    //
        //--------------------------------//

        //use binarywriter to write the image to the file
        public static void Huffman_Compress(RGBPixel[,] ImageMatrix)
        {
            throw new NotImplementedException();
        }

        public static void Huffman_Decompress(RGBPixel[,] ImageMatrix)
        {
            throw new NotImplementedException();
        }


        //--------------------------------//
        //       AUXILLARY FUNCTIONS      //
        //--------------------------------//
        public static char[] ConvertToBinary(byte dec)
        {
            StringBuilder Byte = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                char remainder = (dec % 2 == 0) ? '0' : '1';
                Byte.Insert(0, remainder);

                dec /= 2;
            }
            return Byte.ToString().ToCharArray();
        }
        public static byte ConvertToDecimal(char[] binary)
        {
            byte total = 0;
            for (int i = 0; i < 8; i++)
            {
                total += (byte)((binary[i] - '0') * Math.Pow(2, 7 - i));
            }

            return total;
        }        
    }
}
