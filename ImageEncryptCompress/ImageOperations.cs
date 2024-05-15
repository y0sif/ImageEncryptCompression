using System;
using System.IO;
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

        #region File Writing & Loading

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
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb ||
                    original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
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

        public static int saveBinary(string initSeed, int tapPosition,
            Node<short> red_root, Node<short> green_root, Node<short> blue_root, int imgWidth, int imgHeight, string[] rgbChannels)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                WriteCompressedImage(saveFileDialog1.FileName, initSeed, tapPosition, red_root, green_root, blue_root, imgWidth, imgHeight, rgbChannels);
                return 0;
            }

            return -1; //failed
        }

        public static (string fileName, long fileSize) loadBinary()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = openFileDialog1.FileName;
                    long fileSize = new FileInfo(fileName).Length;

                    return (fileName, fileSize);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return (null, 0); //failed
        }
        #endregion

        #region Image Operations
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
        #endregion

        #region Encryption & Decryption Code

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

                    char res = (char)(((seed[(bitSize - tapPosition) - 1] - '0') ^ (shiftOut - '0')) + '0');

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
        public static void XORKeyGeneration(int tapPosition, char[][] alphaBinarySeed)
        {
            int bitSize = alphaBinarySeed.Length;

            for (int k = 0; k < RGBKeys.Length; k++)
            {
                char[] shiftOut = alphaBinarySeed[0];
                char[] keyString = new char[KEY_SIZE];

                for (int i = 0; i < 8; i++)
                {
                    keyString[i] = (char)(((alphaBinarySeed[(bitSize - tapPosition) - 1][i] - '0') ^ (shiftOut[i] - '0')) + '0');
                }

                for (int j = 1; j < bitSize; j++)
                {
                    alphaBinarySeed[j - 1] = alphaBinarySeed[j];
                }

                alphaBinarySeed[bitSize - 1] = keyString;

                RGBKeys[k] = keyString;
            }
        }

        public static RGBPixel[,] LFSR(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed)
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

                    pixel.red = (byte)(pixel.red ^ ConvertToByte(redKey));
                    pixel.green = (byte)(pixel.green ^ ConvertToByte(greenKey));
                    pixel.blue = (byte)(pixel.blue ^ ConvertToByte(blueKey));
                }
            }

            return ImageMatrix;
        }

        public static RGBPixel[,] AlphaNumLFSR(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed, bool isXOR)
        {
            char[] seed = initSeed.ToCharArray();
            char[][] alphaBinarySeed = new char[seed.Length][];
            StringBuilder concatBinaryASCII = (isXOR) ? null : new StringBuilder();

            CheckAlpha(seed, alphaBinarySeed, concatBinaryASCII);

            char[] concatSeed = (isXOR) ? null : concatBinaryASCII.ToString().ToCharArray();

            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    ref RGBPixel pixel = ref ImageMatrix[row, col];

                    if (isXOR)
                    {
                        XORKeyGeneration(tapPosition, alphaBinarySeed);
                    }
                    else
                    {
                        KeyGeneration(tapPosition, concatSeed);
                    }

                    char[] redKey = RGBKeys[0];
                    char[] greenKey = RGBKeys[1];
                    char[] blueKey = RGBKeys[2];

                    pixel.red = (byte)(pixel.red ^ ConvertToByte(redKey));
                    pixel.green = (byte)(pixel.green ^ ConvertToByte(greenKey));
                    pixel.blue = (byte)(pixel.blue ^ ConvertToByte(blueKey));
                }
            }

            return ImageMatrix;
        }

        //Break the Ecnryption
        public static (string, int) Break_Encryption(RGBPixel[,] ImageMatrix, int N)
        {
            Dictionary<(string, int), int> frequency_deviations = new Dictionary<(string, int), int>();
            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            int posibilities = (int)Math.Pow(2, N);

            for (int i = 0; i < posibilities; i++)
            {

                string seed = Convert.ToString(i, 2).PadLeft(N, '0');

                for (int tapPosition = 0; tapPosition < N; tapPosition++)
                {
                    RGBPixel[,] ImageMatrix_copy = (RGBPixel[,])ImageMatrix.Clone();

                    ImageMatrix_copy = LFSR(ImageMatrix_copy, tapPosition, seed);
                    frequency_deviations[(seed, tapPosition)] = 0;

                    for (int row = 0; row < height; row++)
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

        public static Dictionary<short, int> R = new Dictionary<short, int>();
        public static Dictionary<short, int> G = new Dictionary<short, int>();
        public static Dictionary<short, int> B = new Dictionary<short, int>();

        public static Dictionary<short, string> R_TREE = new Dictionary<short, string>();
        public static Dictionary<short, string> G_TREE = new Dictionary<short, string>();
        public static Dictionary<short, string> B_TREE = new Dictionary<short, string>();

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
                    if (R.ContainsKey(pixel.red))
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

        private static Node<short> BuildHuffmanTree(Dictionary<short, int> color)
        {
            PriorityQueue<int, Node<short>> pq = new PriorityQueue<int, Node<short>>();

            foreach (short value in color.Keys)
            {
                int freq = color[value];
                Node<short> node = new Node<short>(value, freq);
                pq.Enqueue(freq, node);
            }
            for (int i = 0; i < color.Count - 1; i++)
            {
                Node<short> node = new Node<short>(256, 0);
                Node<short> firstMin = pq.Dequeue();
                Node<short> secondMin = pq.Dequeue();
                node.left = secondMin;
                node.right = firstMin;
                node.freq = firstMin.freq + secondMin.freq;
                pq.Enqueue(node.freq, node);
            }
            return pq.Dequeue();
        }

        private static void dfs(Node<short> node, string binary, Dictionary<short, string> tree, Dictionary<short, int> freqTree, ref float channelSize)
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

        public static (float, Node<short>, Node<short>, Node<short>, string[]) Huffman_Compress(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed)
        {
            Construct_Dictionaries(ImageMatrix);

            Node<short> root_red = BuildHuffmanTree(R);
            Node<short> root_green = BuildHuffmanTree(G);
            Node<short> root_blue = BuildHuffmanTree(B);

            float rChannel = 0;
            float gChannel = 0;
            float bChannel = 0;
            R_TREE.Clear();
            G_TREE.Clear();
            B_TREE.Clear();
            dfs(root_red, "", R_TREE, R, ref rChannel);
            dfs(root_green, "", G_TREE, G, ref gChannel);
            dfs(root_blue, "", B_TREE, B, ref bChannel);

            float imgChannelSize = GetHeight(ImageMatrix) * GetWidth(ImageMatrix) * 8;

            float redChannelRatio = (rChannel / imgChannelSize) * 100;
            float greenChannelRatio = (gChannel / imgChannelSize) * 100;
            float blueChannelRatio = (bChannel / imgChannelSize) * 100;
            float compRatio = (redChannelRatio + greenChannelRatio + blueChannelRatio) / 3;

            string[] arrays = PixelEncoding(ImageMatrix);


            return (compRatio, root_red, root_green, root_blue, arrays);
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

            string[] arrays = new string[] { red, green, blue };
            return arrays;
        }

        public static void WriteCompressedImage(string fileName, string initSeed, int tapPosition,
            Node<short> red_root, Node<short> green_root, Node<short> blue_root, int imgWidth, int imgHeight, string[] rgbChannels)
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
                    PrintTree(red_root);

                    Console.WriteLine("=============== green tree ======================");
                    WriteTree(writer, green_root);
                    PrintTree(green_root);

                    Console.WriteLine("=============== blue tree ======================");
                    WriteTree(writer, blue_root);
                    PrintTree(blue_root);

                    writer.Write(rgbChannels[0].Length);
                    writer.Write(rgbChannels[1].Length);
                    writer.Write(rgbChannels[2].Length);

                    WriteChannels(writer, rgbChannels);
                }
            }
        }


        private static void WriteTree(BinaryWriter writer, Node<short> node)
        {
            if (node == null)
            {
                return;
            }
            if (node.value == 256)
            {
                writer.Write(node.value);
                //Console.WriteLine("node freq: " + node.freq);
            }
            else
            {
                writer.Write(node.value);
                //Console.WriteLine("leaf freq: " + node.freq);
                //Console.WriteLine("leaf value: " + node.value);
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

        private static (int tapPosition, string initSeed, int imgWidth, int imgHeight, Node<short> red_root, Node<short> green_root,
            Node<short> blue_root, string[] rgbChannels) ReadCompressedImage(string fileName)
        {
            int tapPosition;
            string initSeed;
            Node<short> red_root, green_root, blue_root;
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
                    PrintTree(red_root);

                    Console.WriteLine("============== green tree=============");
                    green_root = ReadTree(reader);
                    PrintTree(green_root);

                    Console.WriteLine("============= blue tree ==============");
                    blue_root = ReadTree(reader);
                    PrintTree(blue_root);

                    channelLength[0] = reader.ReadInt32();
                    channelLength[1] = reader.ReadInt32();
                    channelLength[2] = reader.ReadInt32();

                    rgbChannels = ReadChannels(reader, channelLength);

                }
            }

            return (tapPosition, initSeed, imgWidth, imgHeight, red_root, green_root, blue_root, rgbChannels);
        }

        private static Node<short> ReadTree(BinaryReader reader)
        {

            short value = reader.ReadInt16();

            if (value == 256) // Indicates a null node
            {
                Node<short> node = new Node<short>(value, 0);
                node.left = ReadTree(reader);
                node.right = ReadTree(reader);
                return node;
            }
            else
            {
                return new Node<short>(value, 0);
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

        static void PrintTree(Node<short> node)
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

        public static (RGBPixel[,], int, string) Huffman_Decompress(string filePath)
        {
            //throw new NotImplementedException();

            (int tapPosition, string initSeed, int imgWidth, int imgHeight, Node<short> red_root, Node<short> green_root,
                Node<short> blue_root, string[] rgbChannels) = ReadCompressedImage(filePath);

            RGBPixel[,] decompressedImg = new RGBPixel[imgHeight, imgWidth];
            int r = 0;
            int g = 0;
            int b = 0;
            int rLen = rgbChannels[0].Length;
            int gLen = rgbChannels[1].Length;
            int bLen = rgbChannels[2].Length;

            for (int i = 0; i < imgHeight; i++)
            {
                for (int j = 0; j < imgWidth; j++)
                {
                    Node<short> red_node = red_root;
                    Node<short> green_node = green_root;
                    Node<short> blue_node = blue_root;
                    for (int k = r; k <= rLen; k++)
                    {
                        if (red_node.left == null || red_node.right == null)
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

                    for (int k = g; k <= gLen; k++)
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

                    for (int k = b; k <= bLen; k++)
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
        //       Run Length Encoding      //
        //--------------------------------//

        public static (float ratio, List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq, List<int> greenFreq,
            List<int> blueFreq) RunLengthEncoding(RGBPixel[,] ImageMatrix, int tapPosition, string initSeed)
        {
            List<byte> redVal = new List<byte>();
            List<byte> greenVal = new List<byte>();
            List<byte> blueVal = new List<byte>();

            List<int> redFreq = new List<int>();
            List<int> greenFreq = new List<int>();
            List<int> blueFreq = new List<int>();

            int height = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            int currentRed = 0;
            int currentGreen = 0;
            int currentBlue = 0;

            redVal.Add(ImageMatrix[0, 0].red);
            greenVal.Add(ImageMatrix[0, 0].green);
            blueVal.Add(ImageMatrix[0, 0].blue);

            redFreq.Add(1);
            greenFreq.Add(1);
            blueFreq.Add(1);

            bool first = true;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }

                    //red
                    if (redVal[currentRed] == ImageMatrix[i, j].red)
                    {
                        redFreq[currentRed]++;
                    }
                    else
                    {
                        currentRed++;
                        redVal.Add(ImageMatrix[i, j].red);
                        redFreq.Add(1);
                    }

                    //green
                    if (greenVal[currentGreen] == ImageMatrix[i, j].green)
                    {
                        greenFreq[currentGreen]++;
                    }
                    else
                    {
                        currentGreen++;
                        greenVal.Add(ImageMatrix[i, j].green);
                        greenFreq.Add(1);
                    }

                    //blue
                    if (blueVal[currentBlue] == ImageMatrix[i, j].blue)
                    {
                        blueFreq[currentBlue]++;
                    }
                    else
                    {
                        currentBlue++;
                        blueVal.Add(ImageMatrix[i, j].blue);
                        blueFreq.Add(1);
                    }

                }
            }
            float imgChannelSize = GetHeight(ImageMatrix) * GetWidth(ImageMatrix);

            float redChannelRatio = (redVal.Count * 5 / imgChannelSize) * 100;
            float greenChannelRatio = (greenVal.Count * 5 / imgChannelSize) * 100;
            float blueChannelRatio = (blueVal.Count * 5 / imgChannelSize) * 100;
            float compRatio = (redChannelRatio + greenChannelRatio + blueChannelRatio) / 3;

            return (compRatio, redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq);

        }

        private static void RLEWrite(string filePath, List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq,
            List<int> greenFreq, List<int> blueFreq, int tapPosition, string initSeed, int imgWidth, int imgHeight)
        {

            using (var stream = File.Open(filePath, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(tapPosition);
                    writer.Write(initSeed);

                    writer.Write(imgWidth);
                    writer.Write(imgHeight);

                    writer.Write(redVal.Count);
                    RLEWriteChannel(writer, redVal, redFreq);

                    writer.Write(greenVal.Count);
                    RLEWriteChannel(writer, greenVal, greenFreq);

                    writer.Write(blueVal.Count);
                    RLEWriteChannel(writer, blueVal, blueFreq);

                }
            }
        }

        private static void RLEWriteChannel(BinaryWriter writer, List<byte> val, List<int> freq)
        {
            for (int i = 0; i < val.Count; i++)
            {
                writer.Write(freq[i]);
                writer.Write(val[i]);
            }
        }

        private static (List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq, List<int> greenFreq,
            List<int> blueFreq, int tapPosition, string initSeed, int imgWidth, int imgHeight) ReadRLE(string filePath)
        {
            int tapPosition;
            string initSeed;
            int imgWidth;
            int imgHeight;

            List<byte> redVal = new List<byte>();
            List<byte> greenVal = new List<byte>();
            List<byte> blueVal = new List<byte>();

            List<int> redFreq = new List<int>();
            List<int> greenFreq = new List<int>();
            List<int> blueFreq = new List<int>();


            using (var stream = File.OpenRead(filePath))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {

                    tapPosition = reader.ReadInt32();
                    initSeed = reader.ReadString();

                    imgWidth = reader.ReadInt32();
                    imgHeight = reader.ReadInt32();

                    int redCount = reader.ReadInt32();

                    (redVal, redFreq) = ReadRLELists(reader, redCount);

                    int greenCount = reader.ReadInt32();

                    (greenVal, greenFreq) = ReadRLELists(reader, greenCount);

                    int blueCount = reader.ReadInt32();

                    (blueVal, blueFreq) = ReadRLELists(reader, blueCount);
                }
            }

            return (redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq, tapPosition, initSeed, imgWidth, imgHeight);
        }

        private static (List<byte> val, List<int> freq) ReadRLELists(BinaryReader reader, int count)
        {
            List<byte> val = new List<byte>();
            List<int> freq = new List<int>();

            for (int i = 0; i < count; i++)
            {
                freq.Add(reader.ReadInt32());
                val.Add(reader.ReadByte());
            }

            return (val, freq);
        }

        public static int saveBinaryForRLE(List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq, List<int> greenFreq, List<int> blueFreq, int tapPosition, string initSeed, int imgWidth, int imgHeight)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                RLEWrite(saveFileDialog1.FileName, redVal, greenVal, blueVal, redFreq, greenFreq, blueFreq, tapPosition, initSeed, imgWidth, imgHeight);
                return 0;
            }

            return -1; //failed
        }


        public static (RGBPixel[,], int, string) RunLengthDecoding(string filePath)
        {

            (List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq, List<int> greenFreq, List<int> blueFreq, int tapPosition, string initSeed, int imgWidth, int imgHeight) = ReadRLE(filePath);

            RGBPixel[,] decompressedImg = new RGBPixel[imgHeight, imgWidth];

            int currentRed = 0;
            int currentGreen = 0;
            int currentBlue = 0;

            for (int i = 0; i < imgHeight; i++)
            {
                for (int j = 0; j < imgWidth; j++)
                {
                    //red
                    if (redFreq[currentRed] != 0)
                    {
                        decompressedImg[i, j].red = (byte)redVal[currentRed];
                        redFreq[currentRed]--;
                    }
                    else
                    {
                        currentRed++;
                        decompressedImg[i, j].red = (byte)redVal[currentRed];
                        redFreq[currentRed]--;
                    }

                    //green
                    if (greenFreq[currentGreen] != 0)
                    {
                        decompressedImg[i, j].green = (byte)greenVal[currentGreen];
                        greenFreq[currentGreen]--;
                    }
                    else
                    {
                        currentGreen++;
                        decompressedImg[i, j].green = (byte)greenVal[currentGreen];
                        greenFreq[currentGreen]--;
                    }

                    //blue
                    if (blueFreq[currentBlue] != 0)
                    {
                        decompressedImg[i, j].blue = (byte)blueVal[currentBlue];
                        blueFreq[currentBlue]--;
                    }
                    else
                    {
                        currentBlue++;
                        decompressedImg[i, j].blue = (byte)blueVal[currentBlue];
                        blueFreq[currentBlue]--;
                    }
                }
            }

            return (decompressedImg, tapPosition, initSeed);

        }

        //before writing it disk we can write it to memory first to calculate the size
        public static long CalculateCompressedImageSize(string initSeed, int tapPosition,
            Node<short> red_root, Node<short> green_root, Node<short> blue_root, int imgWidth, int imgHeight, string[] rgbChannels)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    // Write data to the memory stream
                    writer.Write(tapPosition);
                    writer.Write(initSeed);
                    writer.Write(imgWidth);
                    writer.Write(imgHeight);
                    WriteTree(writer, red_root);
                    WriteTree(writer, green_root);
                    WriteTree(writer, blue_root);
                    writer.Write(rgbChannels[0].Length);
                    writer.Write(rgbChannels[1].Length);
                    writer.Write(rgbChannels[2].Length);
                    WriteChannels(writer, rgbChannels);
                }

                //Get the size of the data in the memory stream
                return memoryStream.Length;
            }
        }

        public static long CalculateCompressedImageSizeForRLE(List<byte> redVal, List<byte> greenVal, List<byte> blueVal, List<int> redFreq, List<int> greenFreq, List<int> blueFreq, int tapPosition, string initSeed, int imgWidth, int imgHeight)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    // Write data to the memory stream
                    writer.Write(tapPosition);
                    writer.Write(initSeed);

                    writer.Write(imgWidth);
                    writer.Write(imgHeight);

                    writer.Write(redVal.Count);
                    RLEWriteChannel(writer, redVal, redFreq);

                    writer.Write(greenVal.Count);
                    RLEWriteChannel(writer, greenVal, greenFreq);

                    writer.Write(blueVal.Count);
                    RLEWriteChannel(writer, blueVal, blueFreq);
                }

                //Get the size of the data in the memory stream
                return memoryStream.Length;
            }
        }



        //--------------------------------//
        //       AUXILLARY FUNCTIONS      //
        //--------------------------------//
        public static void CheckAlpha(char[] seed, char[][] alphaBinarySeed, StringBuilder concatBinaryASCII)
        {
            byte[] alphaSeed;

            foreach (char c in seed)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    throw new Exception("Alphanumeric encryption must have alphanum seed");
                }
            }

            alphaSeed = Encoding.ASCII.GetBytes(seed);

            for (int i = 0; i < alphaSeed.Length; i++)
            {
                alphaBinarySeed[i] = Convert.ToString(alphaSeed[i], 2).PadLeft(8, '0').ToCharArray();
            }
            if (concatBinaryASCII != null)
            {
                foreach (char[] bytes in alphaBinarySeed)
                {
                    concatBinaryASCII.Append(bytes);
                }
            }
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
        private static byte ConvertToByte(char[] binaryKey)
        {
            byte result = 0;
            for (int i = 0; i < binaryKey.Length; i++)
            {
                result = (byte)((result << 1) | (binaryKey[i] - '0'));
            }
            return result;
        }
        public static bool IsBinary(string input)
        {
            foreach (char c in input)
            {
                if (c != '0' && c != '1')
                {
                    return false;
                }
            }
            return true;
        }

    }
}
#endregion