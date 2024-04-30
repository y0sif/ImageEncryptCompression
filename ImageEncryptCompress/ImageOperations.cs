using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Pipes;
using System.Runtime.InteropServices;
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

        public static Dictionary<byte, string> R_TREE = new Dictionary<byte, string>();
        public static Dictionary<byte, string> G_TREE = new Dictionary<byte, string>();
        public static Dictionary<byte, string> B_TREE = new Dictionary<byte, string>();

        private static void Construct_Dictionaries(RGBPixel[,] ImageMatrix)
        {
            // make sure dictionary is empty before adding new values of new picture to it
            R.Clear();
            G.Clear();
            B.Clear();
            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
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
                Node<byte?> firstMin = pq.Dequeue();
                Node<byte?> secondMin = pq.Dequeue();  
                node.left = secondMin;
                node.right = firstMin;
                node.freq = firstMin.freq + secondMin.freq;
                pq.Enqueue(node.freq, node);
            }
            return pq.Dequeue();
        }

        private static void dfs(Node<byte?> node, string binary, Dictionary<byte, string> tree, ref int counter)
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
                Console.WriteLine("binary: " + binary);
                Console.WriteLine("end of leaf node");
                node.binary = binary;
                tree.Add(node.value.Value, binary);
            }

            dfs(node.left, binary + '0', tree, ref counter);
            dfs(node.right, binary + '1', tree, ref counter);
            counter++;
        }

        public static void Huffman_Compress(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed)
        {
            Construct_Dictionaries(ImageMatrix);

            Node<byte?> root_red = BuildHuffmanTree(R);
            Node<byte?> root_green = BuildHuffmanTree(G);
            Node<byte?> root_blue = BuildHuffmanTree(B);
            int r_count = 0, g_count = 0, b_count = 0;
            dfs(root_red, "", R_TREE, ref r_count);
            dfs(root_green, "", G_TREE, ref g_count);
            dfs(root_blue, "", B_TREE, ref b_count);

            string[] arrays = PixelEncoding(ImageMatrix);

            string filePath = "D:\\[1] Image Encryption and Compression\\Startup Code\\[TEMPLATE] ImageEncryptCompress\\compImg.bin";


            WriteCompressedImage(filePath, initSeed, tapPosition, r_count, g_count, b_count, root_red, root_green, root_blue, GetWidth(ImageMatrix), GetHeight(ImageMatrix), arrays);

        }

        private static string[] PixelEncoding(RGBPixel[,] ImageMatrix)
        {
            string red = "";
            string green = "";
            string blue = "";

            for (int i = 0; i < GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < GetWidth(ImageMatrix); j++)
                {
                    RGBPixel pixel = ImageMatrix[i, j];
                    red += R_TREE[pixel.red];
                    green += G_TREE[pixel.green];
                    blue += B_TREE[pixel.blue];
                }
            }
            string[] arrays = new string[]{ red, green, blue };
            return arrays;
        }

        private static void WriteCompressedImage(string fileName, string initSeed, int tapPosition, 
            int r_count, int g_count, int b_count, Node<byte?> red_root, Node<byte?> green_root, Node<byte?> blue_root,int imgWidth, int imgHeight, string[] rgbChannels)
        {
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(tapPosition);
                    writer.Write(initSeed);

                    writer.Write(imgWidth);
                    writer.Write(imgHeight);

                    writer.Write(r_count);
                    WriteTree(writer, red_root);

                    writer.Write(g_count);
                    WriteTree(writer, green_root);

                    writer.Write(b_count);
                    WriteTree(writer, blue_root);

                    WriteChannels(writer, rgbChannels[0]);
                    WriteChannels(writer, rgbChannels[1]);
                    WriteChannels(writer, rgbChannels[2]);
                }
            }
        }


        private static void WriteTree(BinaryWriter writer, Node<byte?> node)
        {
            if (node == null)
            {
                return;
            }
            if(node.value == null)
            {
                writer.Write('\n');
                writer.Write(node.freq);
            }
            else
            {
                writer.Write((byte)node.value);
                writer.Write(node.freq);
            }

            WriteTree(writer, node.left);
            WriteTree(writer, node.right);
        }

        private static void WriteChannels(BinaryWriter writer, string channel)
        {
            int bitLength = channel.Length;

            byte[] bytes = new byte[(bitLength + 7) / 8]; // Round up to the nearest byte

            for (int i = 0; i < bitLength; i++)
            {
                if (channel[i] == '1')
                {
                    int byteIndex = i / 8;
                    int bitOffset = i % 8;
                    bytes[byteIndex] |= (byte)(1 << (7 - bitOffset)); // Set the corresponding bit to 1
                }
                // Note: if channel[i] == '0', the bit remains 0 (default)
            }

            writer.Write(bytes); // Write the byte array to the file
            writer.Write('\n');
        }

        private static (int tapPosition, string initSeed, int imgWidth, int imgHeight, int r_count, int g_count, int b_count, Node<byte?> red_root, Node<byte?> green_root, Node<byte?> blue_root, string[] rgbChannels) ReadCompressedImage(string fileName)
        {
            int tapPosition;
            string initSeed;
            Node<byte?> red_root, green_root, blue_root;
            int imgWidth, imgHeight;
            int r_count, g_count, b_count;
            string[] rgbChannels = new string[3];

            using (var stream = File.OpenRead(fileName))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    tapPosition = reader.ReadInt32();
                    initSeed = reader.ReadString();

                    imgWidth = reader.ReadInt32();
                    imgHeight = reader.ReadInt32();

                    r_count = reader.ReadInt32();
                    red_root = ReadTree(reader, r_count);

                    g_count = reader.ReadInt32();
                    green_root = ReadTree(reader, g_count);

                    b_count = reader.ReadInt32();
                    blue_root = ReadTree(reader, b_count);

                    rgbChannels[0] = ReadChannels(reader);
                    rgbChannels[1] = ReadChannels(reader);
                    rgbChannels[2] = ReadChannels(reader);
                }
            }

            return (tapPosition, initSeed, imgWidth, imgHeight, r_count, g_count, b_count, red_root, green_root, blue_root, rgbChannels);
        }

        private static Node<byte?> ReadTree(BinaryReader reader, int count)
        {
            if(count == 0)
            {
                return null;
            }
            byte value = reader.ReadByte();

            if (value == '\n') // Indicates a null node
            {
                int freq = reader.ReadInt32();
                Node<byte?> node = new Node<byte?>(null, freq);
                node.left = ReadTree(reader, count - 1);
                node.right = ReadTree(reader, count - 1);
                return node;
            }
            else
            {
                int freq = reader.ReadInt32();
                return new Node<byte?>(value, freq);
            }
        }

        private static string ReadChannels(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();

            byte currentByte;
            while ((currentByte = reader.ReadByte()) != '\n')
            {
                bytes.Add(currentByte);
            }

            StringBuilder channel = new StringBuilder();

            foreach (byte b in bytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bool isSet = (b & (1 << i)) != 0; // Check if the bit is set (1)
                    channel.Append(isSet ? '1' : '0'); // Append '1' for set bit, '0' otherwise
                }
            }

            return channel.ToString();
        }

        static void PrintTree(Node<byte?> node)
        {
            if (node == null)
            {
                return;
            }

            if (node.value == null)
            {
                Console.WriteLine("node freq: " + node.freq);
            }
            else
            {
                Console.WriteLine("leaf node");
                Console.WriteLine("node value: " + node.value);
                Console.WriteLine("node freq: " + node.freq);
                Console.WriteLine("end of leaf node");

            }

            PrintTree(node.left);
            PrintTree(node.right);
        }

        public static RGBPixel[,] Huffman_Decompress(string filePath)
        {
            //throw new NotImplementedException();

            (int tapPosition, string initSeed, int imgWidth, int imgHeight, int r_count, int g_count, int b_count, Node<byte?> red_root, Node<byte?> green_root, Node<byte?> blue_root, string[] rgbChannels) = ReadCompressedImage(filePath);
            Console.WriteLine($"Tap Position: {tapPosition}");
            Console.WriteLine($"Init Seed: {initSeed}");
            Console.WriteLine($"Image Width: {imgWidth}");
            Console.WriteLine($"Image Height: {imgHeight}");

            Console.WriteLine("Red Tree:");
            PrintTree(red_root);

            Console.WriteLine("Green Tree:");
            PrintTree(green_root);

            Console.WriteLine("Blue Tree:");
            PrintTree(blue_root);

            Console.WriteLine("RGB Channels:");
            Console.WriteLine($"Red Channel: {rgbChannels[0]}");
            Console.WriteLine($"Green Channel: {rgbChannels[1]}");
            Console.WriteLine($"Blue Channel: {rgbChannels[2]}");
            RGBPixel[,] decompressedImg = new RGBPixel[imgHeight, imgWidth];
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i < imgHeight; i++)
            {
                for (int j = 0; j < imgWidth; j++)
                {
                    Node<byte?> red_node = red_root;
                    Node<byte?> green_node = green_root;
                    Node<byte?> blue_node = blue_root;
                    for(int k = r; k < rgbChannels[0].Length; k++)
                    {
                        if(red_node.left == null || red_node.right == null)
                        {
                            decompressedImg[i, j].red = (byte)red_node.value;
                            break;
                        }
                        if (rgbChannels[0][k] == '0')
                        {
                            red_node = red_node.left;
                            r++;
                        }
                        else
                        {
                            red_node = red_node.right;
                            r++;
                        }
                    }

                    for (int k = g; k < rgbChannels[1].Length; k++)
                    {
                        if (green_node.left == null || green_node.right == null)
                        {
                            decompressedImg[i, j].green = (byte)green_node.value;
                            break;
                        }
                        if (rgbChannels[1][k] == '0')
                        {
                            green_node = green_node.left;
                            g++;
                        }
                        else
                        {
                            green_node = green_node.right;
                            g++;
                        }
                    }

                    for (int k = b; k < rgbChannels[2].Length; k++)
                    {
                        if (blue_node.left == null || blue_node.right == null)
                        {
                            decompressedImg[i, j].blue = (byte)blue_node.value;
                            break;
                        }
                        if (rgbChannels[2][k] == '0')
                        {
                            blue_node = blue_node.left;
                            b++;
                        }
                        else
                        {
                            blue_node = blue_node.right;
                            b++;
                        }
                    }
                }
            }

            return decompressedImg;
        }

    }
}
