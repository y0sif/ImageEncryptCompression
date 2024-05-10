using System;
using System.IO;
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
using System.Runtime.Remoting.Channels;
using System.Reflection.Emit;

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

        public static int saveImage(PictureBox PicBox)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                PicBox.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
                return 0; 
            }

            return -1; //failed
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
        private static bool keyFlag;

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
                    char res = XOR(seed[(bitSize - tapPosition) - 1], shiftOut);

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
        public static void AlphaNumKeyGeneration(int tapPosition, char[] seed, char[][] alphaBinarySeed, char[] concatSeed, byte alphaMethod)
        {
            if (alphaMethod == 0)
            {
                int bitSize = concatSeed.Length;

                for (int k = 0; k < RGBKeys.Length; k++)
                {
                    char[] keyString = new char[KEY_SIZE];
                    for (int i = 0; i < KEY_SIZE; i++)
                    {
                        char shiftOut = concatSeed[0];
                        char res = XOR(concatSeed[(bitSize - tapPosition) - 1], shiftOut);

                        for (int j = 1; j < bitSize; j++)
                        {
                            concatSeed[j - 1] = concatSeed[j];
                        }

                        concatSeed[bitSize - 1] = res;
                        keyString[i] = res;
                    }

                    RGBKeys[k] = keyString;
                }
            }
            else if (alphaMethod == 1)
            {
                int bitSize = alphaBinarySeed.Length;

                for (int k = 0; k < RGBKeys.Length; k++)
                {
                    char[] shiftOut = alphaBinarySeed[0];

                    char[] result = new char[KEY_SIZE];

                    for (int i = 0; i < 8; i++)
                    {
                        result[i] = XOR(alphaBinarySeed[(bitSize - tapPosition) - 1][i], shiftOut[i]);
                    }

                    for (int j = 1; j < bitSize; j++)
                    {
                        alphaBinarySeed[j - 1] = alphaBinarySeed[j];
                    }

                    alphaBinarySeed[bitSize - 1] = result;

                    RGBKeys[k] = result;
                }
            }
        }

        public static RGBPixel[,] LFSR(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed, bool encrypt)
        {

            char[] seed = initSeed.ToCharArray();

            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    ref RGBPixel pixel = ref ImageMatrix[row, col];

                    KeyGeneration(tapPosition, seed);

                    char[] redKey = RGBKeys[0];
                    char[] greenKey = RGBKeys[1];
                    char[] blueKey = RGBKeys[2];

                    string red = Convert.ToString(pixel.red, 2).PadLeft(8, '0');
                    string green = Convert.ToString(pixel.green, 2).PadLeft(8, '0'); ;
                    string blue = Convert.ToString(pixel.blue, 2).PadLeft(8, '0');

                    char[] redVal = new char[8];
                    char[] greenVal = new char[8];
                    char[] blueVal = new char[8];

                    for (int i = 0; i < KEY_SIZE; i++)
                    {
                        redVal[i] = (char)(((red[i] - '0') ^ (redKey[i] - '0')) + '0');
                        greenVal[i] = (char)(((green[i] - '0') ^ (greenKey[i] - '0')) + '0');
                        blueVal[i] = (char)(((blue[i] - '0') ^ (blueKey[i] - '0')) + '0');
                    }

                    pixel.red = ConvertToDecimal(redVal);
                    pixel.green = ConvertToDecimal(greenVal);
                    pixel.blue = ConvertToDecimal(blueVal);
                }
            }
            
            return ImageMatrix;
        }

        public static RGBPixel[,] alphaNumLFSR(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed, bool encrypt, byte alphaMethod)
        {
            char[] seed = initSeed.ToCharArray();
            char[][] alphaBinarySeed = new char[seed.Length][];
            StringBuilder concatBinaryASCII = new StringBuilder();

            CheckAlpha(seed, alphaBinarySeed, concatBinaryASCII);
            char[] concatSeed = concatBinaryASCII.ToString().ToCharArray();

            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    ref RGBPixel pixel = ref ImageMatrix[row, col];

                    AlphaNumKeyGeneration(tapPosition, seed, alphaBinarySeed, concatSeed, alphaMethod);
                
                    char[] redKey = RGBKeys[0];
                    char[] greenKey = RGBKeys[1];
                    char[] blueKey = RGBKeys[2];

                    string red = Convert.ToString(pixel.red, 2).PadLeft(8, '0');
                    string green = Convert.ToString(pixel.green, 2).PadLeft(8, '0'); ;
                    string blue = Convert.ToString(pixel.blue, 2).PadLeft(8, '0');

                    char[] redVal = new char[8];
                    char[] greenVal = new char[8];
                    char[] blueVal = new char[8];

                    for (int i = 0; i < KEY_SIZE; i++)
                    {
                        redVal[i] = (char)(((red[i] - '0') ^ (redKey[i] - '0')) + '0');
                        greenVal[i] = (char)(((green[i] - '0') ^ (greenKey[i] - '0')) + '0');
                        blueVal[i] = (char)(((blue[i] - '0') ^ (blueKey[i] - '0')) + '0');
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

        //Break the Ecnryption
        public static (string, int) Break_Encryption(RGBPixel[,] ImageMatrix, int N)
        {

            Dictionary<(string, int), int> frequency_deviations = new Dictionary<(string, int), int>();
            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            for (int i=0; i < Math.Pow(2, N); i++)
            {

                string seed = Convert.ToString(i, 2).PadLeft(N, '0');
                
                for (int tapPosition = 0; tapPosition < N; tapPosition++)
                {
                    RGBPixel[,] ImageMatrix_copy = new RGBPixel[ImageMatrix.GetLength(0), ImageMatrix.GetLength(1)];

                    for (int n = 0; n < ImageMatrix.GetLength(0); n++)
                    {
                        for (int m = 0; m < ImageMatrix.GetLength(1); m++)
                        {
                            ImageMatrix_copy[n, m] = ImageMatrix[n, m];
                        }
                    }

                    ImageMatrix_copy = LFSR(ImageMatrix_copy, tapPosition, seed, false);
                    frequency_deviations[(seed, tapPosition)] = 0;

                    for (int row = 0; row < height ; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            ref RGBPixel pixel = ref ImageMatrix_copy[row, col];

                            frequency_deviations[(seed, tapPosition)] += Math.Abs(pixel.red - 128);
                            frequency_deviations[(seed, tapPosition)] += Math.Abs(pixel.green - 128);
                            frequency_deviations[(seed, tapPosition)] += Math.Abs(pixel.blue - 128);     
                            
                        }
                    }
                }
            }

            int max = 0;
            (string, int) best_seed_and_tap = ("", 0);

            foreach (var entry in frequency_deviations)
            {
                if (entry.Value > max)
                {
                    max = entry.Value;
                    best_seed_and_tap = entry.Key;
                }
            }

            return best_seed_and_tap;
        }

        //--------------------------------//
        // COMPRESSION & DECOMPRESSION    //
        //--------------------------------//

        //use binarywriter to write the image to the file

        public static Dictionary<int, int> R = new Dictionary<int, int>();
        public static Dictionary<int, int> G = new Dictionary<int, int>();
        public static Dictionary<int, int> B = new Dictionary<int, int>();

        public static Dictionary<int, string> R_TREE = new Dictionary<int, string>();
        public static Dictionary<int, string> G_TREE = new Dictionary<int, string>();
        public static Dictionary<int, string> B_TREE = new Dictionary<int, string>();

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

        private static Node<int> BuildHuffmanTree(Dictionary<int, int> color)
        {
            PriorityQueue<int, Node<int>> pq = new PriorityQueue<int, Node<int>>();

            foreach(int value in color.Keys)
            {
                int freq = color[value];
                Node<int> node = new Node<int>(value, freq);
                pq.Enqueue(freq, node);
            }
            for(int i = 0; i < color.Count - 1;  i++)
            {
                Node<int> node = new Node<int>(256, 0);
                Node<int> firstMin = pq.Dequeue();
                Node<int> secondMin = pq.Dequeue();  
                node.left = secondMin;
                node.right = firstMin;
                node.freq = firstMin.freq + secondMin.freq;
                pq.Enqueue(node.freq, node);
            }
            return pq.Dequeue();
        }

        private static void dfs(Node<int> node, string binary, Dictionary<int, string> tree, Dictionary<int, int> freqTree, ref float channelSize)
        {
            if (node == null)
            {
                return;
            }

            if (node.value != 256)
            {
                node.binary = binary;
                tree.Add(node.value, binary);
                channelSize += binary.Length * freqTree[node.value];
            }

            dfs(node.left, binary + '0', tree, freqTree, ref channelSize);
            dfs(node.right, binary + '1', tree, freqTree, ref channelSize);
        }

        public static float Huffman_Compress(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed)
        {
            Construct_Dictionaries(ImageMatrix);

            Node<int> root_red = BuildHuffmanTree(R);
            Node<int> root_green = BuildHuffmanTree(G);
            Node<int> root_blue = BuildHuffmanTree(B);

            float rChannel = 0;
            float gChannel = 0;
            float bChannel = 0;

            dfs(root_red, "", R_TREE, R,  ref rChannel);
            dfs(root_green, "", G_TREE, G, ref gChannel);
            dfs(root_blue, "", B_TREE, B, ref bChannel);

            float imgChannelSize = GetHeight(ImageMatrix) * GetWidth(ImageMatrix) * 8;

            float redChannelRatio = (rChannel / imgChannelSize) * 100;
            float greenChannelRatio = (gChannel / imgChannelSize) * 100;
            float blueChannelRatio = (bChannel / imgChannelSize) * 100;
            float compRatio = (redChannelRatio + greenChannelRatio + blueChannelRatio) / 3;

            string[] arrays = PixelEncoding(ImageMatrix);

            string filePath = "D:\\[1] Image Encryption and Compression\\Startup Code\\[TEMPLATE] ImageEncryptCompress\\compImg.bin";

            WriteCompressedImage(filePath, initSeed, tapPosition, root_red, root_green, root_blue, GetWidth(ImageMatrix), GetHeight(ImageMatrix), arrays);

            return compRatio;
        }

        private static string[] PixelEncoding(RGBPixel[,] ImageMatrix)
        {
            StringBuilder redBuilder = new StringBuilder();
            StringBuilder greenBuilder = new StringBuilder();
            StringBuilder blueBuilder = new StringBuilder();
            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    RGBPixel pixel = ImageMatrix[i, j];
                    redBuilder.Append(R_TREE[pixel.red]);
                    greenBuilder.Append(G_TREE[pixel.green]);
                    blueBuilder.Append(B_TREE[pixel.blue]);
                }
            }

            string red = redBuilder.ToString();
            string green = greenBuilder.ToString();
            string blue = blueBuilder.ToString();

            string[] arrays = new string[]{ red, green, blue };
            return arrays;
        }

        private static void WriteCompressedImage(string fileName, string initSeed, int tapPosition, 
            Node<int> red_root, Node<int> green_root, Node<int> blue_root,int imgWidth, int imgHeight, string[] rgbChannels)
        {
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(tapPosition);
                    writer.Write(initSeed);

                    writer.Write(imgWidth);
                    writer.Write(imgHeight);

                    Console.WriteLine("=============== red tree ======================");
                    WriteTree(writer, red_root);

                    Console.WriteLine("=============== green tree ======================");
                    WriteTree(writer, green_root);

                    Console.WriteLine("=============== blue tree ======================");
                    WriteTree(writer, blue_root);

                    writer.Write(rgbChannels[0].Length);
                    writer.Write(rgbChannels[1].Length);
                    writer.Write(rgbChannels[2].Length);

                    WriteChannels(writer, rgbChannels);
                }
            }
        }


        private static void WriteTree(BinaryWriter writer, Node<int> node)
        {
            if (node == null)
            {
                return;
            }
            if(node.value == 256)
            {
                writer.Write(node.value);
                Console.WriteLine("node freq: " + node.freq);
            }
            else
            {
                writer.Write(node.value);
                Console.WriteLine("leaf freq: " + node.freq);
                Console.WriteLine("leaf value: " + node.value);
            }

            WriteTree(writer, node.left);
            WriteTree(writer, node.right);
        }

        private static void WriteChannels(BinaryWriter writer, string[] channels)
        {
            foreach (string channel in channels)
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
            }
        }

        private static (int tapPosition, string initSeed, int imgWidth, int imgHeight, Node<int> red_root, Node<int> green_root, Node<int> blue_root, string[] rgbChannels) ReadCompressedImage(string fileName)
        {
            int tapPosition;
            string initSeed;
            Node<int> red_root, green_root, blue_root;
            int imgWidth, imgHeight;
            string[] rgbChannels = new string[3];
            int[] channelLength = new int[3];

            using (var stream = File.OpenRead(fileName))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    tapPosition = reader.ReadInt32();
                    initSeed = reader.ReadString();

                    imgWidth = reader.ReadInt32();
                    imgHeight = reader.ReadInt32();

                    Console.WriteLine("=============== red tree ======================");
                    red_root = ReadTree(reader);

                    Console.WriteLine("============== green tree=============");
                    green_root = ReadTree(reader);

                    Console.WriteLine("============= blue tree ==============");
                    blue_root = ReadTree(reader);

                    channelLength[0] = reader.ReadInt32();
                    channelLength[1] = reader.ReadInt32();
                    channelLength[2] = reader.ReadInt32();

                    rgbChannels = ReadChannels(reader, channelLength);

                }
            }

            return (tapPosition, initSeed, imgWidth, imgHeight, red_root, green_root, blue_root, rgbChannels);
        }

        private static Node<int> ReadTree(BinaryReader reader)
        {
            
            int value = reader.ReadInt32();

            if (value == 256) // Indicates a null node
            {
                Node<int> node = new Node<int>(value, 0);
                node.left = ReadTree(reader);
                node.right = ReadTree(reader);
                return node;
            }
            else
            {
                return new Node<int>(value, 0);
            }
        }

        private static string[] ReadChannels(BinaryReader reader, int[] channelLengths)
        {
            List<string> channels = new List<string>();

            foreach (int channelLength in channelLengths)
            {
                byte[] bytes = reader.ReadBytes((channelLength + 7) / 8);
                StringBuilder channel = new StringBuilder();

                for (int i = 0; i < channelLength; i++)
                {
                    int byteIndex = i / 8;
                    int bitOffset = i % 8;
                    byte currentByte = bytes[byteIndex];
                    bool isSet = (currentByte & (1 << (7 - bitOffset))) != 0; // Check if the bit is set (1)
                    channel.Append(isSet ? '1' : '0'); // Append '1' for set bit, '0' otherwise
                }

                channels.Add(channel.ToString());
            }

            return channels.ToArray();
        }

        static void PrintTree(Node<int> node)
        {
            if (node == null)
            {
                return;
            }

            if (node.value == 256)
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

        public static (RGBPixel[,] , int , string) Huffman_Decompress(string filePath)
        {
            //throw new NotImplementedException();

            (int tapPosition, string initSeed, int imgWidth, int imgHeight, Node<int> red_root, Node<int> green_root, Node<int> blue_root, string[] rgbChannels) = ReadCompressedImage(filePath);

            RGBPixel[,] decompressedImg = new RGBPixel[imgHeight, imgWidth];
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i < imgHeight; i++)
            {
                for (int j = 0; j < imgWidth; j++)
                {
                    Node<int> red_node = red_root;
                    Node<int> green_node = green_root;
                    Node<int> blue_node = blue_root;
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

            return (decompressedImg, tapPosition, initSeed);
        }


        //--------------------------------//
        //       AUXILLARY FUNCTIONS      //
        //--------------------------------//
        public static void CheckAlpha(char[] seed, char[][] alphaBinarySeed, StringBuilder concatBinaryASCII)
        {
            byte[] alphaSeed;               
            keyFlag = false;

            foreach (char c in seed)
            {
                if (char.IsLetterOrDigit(c))
                {
                    keyFlag = true;
                    break;
                }
            }
            if (keyFlag)
            {
                alphaSeed = Encoding.ASCII.GetBytes(seed);

                for (int i = 0; i < alphaSeed.Length; i++)
                {
                    alphaBinarySeed[i] = ConvertToBinary(alphaSeed[i]);
                }    
                               
                foreach(char[] arr in alphaBinarySeed)
                {
                    concatBinaryASCII.Append(arr);
                }                                                                                                                       
            }
        }
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
        public static char XOR(char char1, char char2)
        {
            return (char)(((char1 - '0') ^ (char2 - '0')) + '0');
        }
    }
}
