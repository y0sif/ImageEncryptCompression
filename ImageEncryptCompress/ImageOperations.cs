using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
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

        //linear feedback shift register encryption
        public static RGBPixel[,] LFSR_Enc(RGBPixel[,] ImageMatrix, int tapPosition, uint initSeed)
        {
            throw new NotImplementedException();
        }

        //linear feedback shift register decryption
        public static RGBPixel[,] LFSR_Dec(RGBPixel[,] ImageMatrix, int tapPosition, uint initSeed)
        {
            throw new NotImplementedException();
        }

        //use binarywriter to write the image to the file

        public static Dictionary<byte, int> R = new Dictionary<byte, int>();
        public static Dictionary<byte, int> G = new Dictionary<byte, int>();
        public static Dictionary<byte, int> B = new Dictionary<byte, int>();

        private static void Construct_Dictionaries(RGBPixel[,] ImageMatrix)
        {
            // make sure dictionary is empty before adding new values of new picture to it
            R.Clear();
            G.Clear();
            B.Clear();
            for(int i = 0; i < GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < GetWidth(ImageMatrix); j++)
                {
                    RGBPixel pixel = ImageMatrix[i, j];
                    // red dictionary
                    if(R.ContainsKey(pixel.red))
                    {
                        R[pixel.red]++;
                    }
                    else
                    {
                        R.Add(pixel.red, 1);
                    }
                    // green dictionary
                    if (G.ContainsKey(pixel.green))
                    {
                        G[pixel.green]++;
                    }
                    else
                    {
                        G.Add(pixel.green, 1);
                    }
                    // blue dictionary
                    if (B.ContainsKey(pixel.blue))
                    {
                        B[pixel.blue]++;
                    }
                    else
                    {
                        B.Add(pixel.blue, 1);
                    }
                }
            }
        }

        private static Node<byte?> BuildHuffmanTree(Dictionary<byte, int> color)
        {
            PriorityQueue<int, Node<byte?>> pq = new PriorityQueue<int, Node<byte?>>();

            foreach(byte value in color.Keys)
            {
                int freq = color[value];
                Node<byte?> node = new Node<byte?>(value, freq);
                pq.Enqueue(freq, node);
            }
            for(int i = 0; i < color.Count - 1;  i++)
            {
                Node<byte?> node = new Node<byte?>(null, 0);
                Node<byte?> left = pq.Dequeue();
                Node<byte?> right = pq.Dequeue();
                node.left = left;
                node.right = right;
                node.freq = left.freq + right.freq;
                pq.Enqueue(node.freq, node);
            }
            return pq.Dequeue();
        }

        private static void dfs(Node<byte?> node)
        {
            if (node == null)
            {
                return;
            }

            if (node.value == null) { 
                Console.WriteLine("node freq: " + node.freq);
            }else
            {
                Console.WriteLine("leaf node");
                Console.WriteLine("node value: " + node.value);
                Console.WriteLine("node freq: " + node.freq);
                Console.WriteLine("end of leaf node");
            }

            dfs(node.left);
            dfs(node.right);
            
        }

        public static void Huffman_Compress(RGBPixel[,] ImageMatrix)
        {
            //throw new NotImplementedException();

            Construct_Dictionaries(ImageMatrix);

            Node<byte?> root_red = BuildHuffmanTree(R);
            Node<byte?> root_green = BuildHuffmanTree(G);
            Node<byte?> root_blue = BuildHuffmanTree(B);

            Console.WriteLine("red tree: ");
            dfs(root_red);
            Console.WriteLine("green tree: ");
            dfs(root_green);
            Console.WriteLine("blue tree: ");
            dfs(root_blue);

        }

        public static void Huffman_Decompress(RGBPixel[,] ImageMatrix)
        {
            throw new NotImplementedException();
        }

    }
}
