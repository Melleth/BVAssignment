using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;
        Color[,] Labels;
        // Dictionary that holds the pixel values that belong to the object with label as key
        Dictionary<int, List<Tuple<int, int>>> Objects;
        public INFOIBV()
        {
            InitializeComponent();
            // Initialize objects dictionary
            Objects = new Dictionary<int, List<Tuple<int, int>>>();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // TODO: include here your own code
            // example: create a negative image
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor;
                    //Thresholding, we assume we have bright triangles
                    if (pixelColor.R < 90)
                    {
                        updatedColor = Color.FromArgb(0, 0, 0);
                    }
                    else
                    {
                        updatedColor = Color.FromArgb(255, 255, 255);
                    }
                
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                           // Increment progress bar
                }
            }
            //Close gaps, first make a copy of the image NOTE: arraycopy = originalarray only gives a refference! BEWARE!
            // Declare color vars because somehow Color.White != Color.FromArgb(255,255,255)!!!!
            Color white = Color.FromArgb(255, 255, 255);
            Color black = Color.FromArgb(0, 0, 0);
            Color[,] CopyImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            Array.Copy(Image, 0, CopyImage, 0, Image.Length);
            //++++++EROSION++++++
            /*for (int x = 0; x < InputImage.Size.Width-1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height-1; y++)
                {
                    Color pixelColor = Image[x, y];
                    if (pixelColor == white)
                    {
                        // Found a white pixel.
                        // Check if we are in bounds foy y
                        if (y > 1 && y < InputImage.Size.Height)
                        {
                            // Check neighbours (we're sort of evaluating 3 values at a time to decide the color of the middle one)
                            if (Image[x, y - 1] == white && Image[x, y + 1] == white)
                                CopyImage[x, y] = white;
                            else
                                CopyImage[x, y] = black;

                        }
                        // Same stuff for x
                        if (x > 1 && x < InputImage.Size.Height)
                        {
                            if (Image[x - 1, y] == white && Image[x + 1, y] == white)
                                CopyImage[x, y] = white;
                            else
                                CopyImage[x, y] = black;
                        }
                    }
                }
            }
            */
            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pixelColor = Image[x, y];
                    if (pixelColor == white)
                    {
                        // Found a white pixel.
                        // Check if we are in bounds foy y
                        if (y > 1 && y < InputImage.Size.Height - 2)
                        {
                            // Check neighbours (we're sort of evaluating 3 values at a time to decide the color of the middle one)
                            if (Image[x, y - 2] == white && Image[x, y - 1] == white && Image[x, y + 1] == white && Image[x, y + 2] == white)
                                CopyImage[x, y] = white;
                            else
                                CopyImage[x, y] = black;

                        }
                        // Same stuff for x
                        if (x > 1 && x < InputImage.Size.Width - 2)
                        {
                            if (Image[x - 2, y] == white && Image[x - 1, y] == white && Image[x + 1, y] == white && Image[x + 2, y] == white)
                                CopyImage[x, y] = white;
                            else
                                CopyImage[x, y] = black;
                        }
                    }
                }
            }
            // Flush the erosion to the image
            Array.Copy(CopyImage, 0, Image, 0, CopyImage.Length);
            //++++++END OF ERIOSION+++++
            //++++++DILATION++++++
            /*for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pixelColor = Image[x, y];
                    if (pixelColor == white)
                    {
                        // Check bounds for y
                        if (y > 1 && y < InputImage.Size.Height-2)
                        {
                            // Do the actual dilation for y
                            if (Image[x, y - 2] == black)
                                CopyImage[x, y - 2] = white;
                            if (Image[x, y - 1] == black)
                                CopyImage[x, y - 1] = white;
                            if (Image[x, y + 1] == black)
                                CopyImage[x, y + 1] = white;
                            if (Image[x, y + 2] == black)
                                CopyImage[x, y + 2] = white;
                        }
                        if (x > 1 && x < InputImage.Size.Width-2)
                        {
                            // Do the actual dilation for x
                            if (Image[x - 2, y] == black)
                                CopyImage[x - 2, y] = white;
                            if (Image[x - 1, y] == black)
                                CopyImage[x - 1, y] = white;
                            if (Image[x + 1, y] == black)
                                CopyImage[x + 1, y] = white;
                            if (Image[x + 2, y] == black)
                                CopyImage[x + 2, y] = white;
                        }
                    }
                }
            }
            */
            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pixelColor = Image[x, y];
                    if (pixelColor == white)
                    {
                        // Check bounds for y
                        if (y > 1 && y < InputImage.Size.Height)
                        {
                            // Do the actual dilation for y
                            if (Image[x, y - 1] == black)
                                CopyImage[x, y - 1] = white;
                            if (Image[x, y + 1] == black)
                                CopyImage[x, y + 1] = white;
                        }
                        if (x > 1 && x < InputImage.Size.Height)
                        {
                            // Do the actual dilation for x
                            if (Image[x - 1, y] == black)
                                CopyImage[x - 1, y] = white;
                            if (Image[x + 1, y] == black)
                                CopyImage[x + 1, y] = white;
                        }
                    }
                }
            }
            // Flush the dilation to the image
            Array.Copy(CopyImage, 0, Image, 0, CopyImage.Length);
            //++++++END OF DILATION++++++
            //watershed
            int[,] watershedImg = new int[InputImage.Size.Width, InputImage.Size.Height];

            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pxcolor = Image[x, y];
                    if (pxcolor == white)
                        watershedImg[x, y] = 512;
                    else
                        watershedImg[x, y] = 0;
                }
            }


            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pxcolor = Image[x, y];

                    //padding
                    if (x == 0 && y == 0)
                        watershedImg[x, y] = watershedImg[x + 1, y + 1];
                    else if (x == 0)
                        watershedImg[x, y] = watershedImg[x + 1, y];
                    else if (y == 0)
                        watershedImg[x, y] = watershedImg[x, y + 1];
                    else
                    {
                        watershedImg[x, y] = Math.Min(Math.Min(Math.Min(Math.Min(
                                                watershedImg[x - 1, y - 1] + 2,
                                                watershedImg[x, y - 1] + 1),
                                                watershedImg[x + 1, y - 1] + 2),
                                                watershedImg[x - 1, y] + 1),
                                                watershedImg[x, y]);
                    }
                }
            }


            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pxcolor = Image[x, y];
                    if (x == 0 && y == 0)
                        watershedImg[x, y] = watershedImg[x + 1, y + 1];
                    else if (x == 0)
                        watershedImg[x, y] = watershedImg[x + 1, y];
                    else if (y == 0)
                        watershedImg[x, y] = watershedImg[x, y + 1];
                    else
                    {
                        watershedImg[x, y] = Math.Min(Math.Min(Math.Min(Math.Min(
                                            watershedImg[x, y],
                                            watershedImg[x + 1, y] + 1),
                                            watershedImg[x - 1, y + 1] + 2),
                                            watershedImg[x, y + 1] + 1),
                                            watershedImg[x + 1, y + 1] + 2);
                    }
                }
            }
            // Complement distance transform with binary
            for (int x = 0; x < InputImage.Size.Width-1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height-1; y++)
                {
                    watershedImg[x, y] = 255 - watershedImg[x, y];
                }
            }

            // Seperate where distances meet
            /*for (int x = 3; x < InputImage.Size.Width-3; x++)
            {
                for (int y = 3; y < InputImage.Size.Width-3; y++)
                {
                    if (watershedImg[x + 1, y] == watershedImg[x - 2, y] && watershedImg[x, y] == watershedImg[x - 1, y] && watershedImg[x - 3, y] == watershedImg[x + 2, y])
                    {
                        watershedImg[x, y] = 0;
                        watershedImg[x - 1, y] = 0;
                    }
 
                }
            }*/
            //++++++LABEL++++++
            Labels = new Color[InputImage.Size.Width, InputImage.Size.Height];
            Array.Copy(Image, 0, Labels, 0, Image.Length);
            Color labelColor = Color.FromArgb(1,1,1);
            int width = InputImage.Size.Width;
            int height = InputImage.Size.Height;
            for (int x = 0; x < InputImage.Size.Width - 1; x++)
            {
                for (int y = 0; y < InputImage.Size.Height - 1; y++)
                {
                    Color pixelColor = Labels[x, y];
                    if (pixelColor == white)
                    {

                        var thread = new Thread(
                        () => Label(labelColor, x, y, width, height), 100000000);
                        thread.Start();
                        thread.Join();
                        Label(labelColor, x, y, width, height);
                        if (labelColor.R == 255)
                        {
                            for (int a = 0; a < InputImage.Size.Width; a++)
                            {
                                for (int b = 0; b < InputImage.Size.Height; b++)
                                {
                                    OutputImage.SetPixel(a, b, Labels[a, b]);               // Set the pixel color at coordinate (x,y)
                                }
                            }
                            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                                OutputImage.Save(saveImageDialog.FileName);
                            return;
                        }
                        //thread.Abort();
                        labelColor = Color.FromArgb(labelColor.R + 1, labelColor.G + 1, labelColor.B + 1);
                        
                    }
                }
            }
            Console.WriteLine("Objects labeled: " + Objects.Count);
            // Flush labels to image
            Array.Copy(Labels, 0, Image, 0, Labels.Length);
            // Check if the objects are triangles.
            List<List<Tuple<int,int>>> triangles = new List<List<Tuple<int,int>>>();
            foreach (KeyValuePair<int, List<Tuple<int,int>>> pair in Objects)
            {
                if (IsTriangle(pair.Value))
                    triangles.Add(pair.Value);
            }
            Console.WriteLine("Triangles: " + triangles.Count);
            // Set output image to black
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Color.FromArgb(0,0,0));               // Set the pixel color at coordinate (x,y)
                }
            }
            // Paint triangles in output
            foreach (List<Tuple<int, int>> triangle in triangles)
            {
                foreach (Tuple<int, int> pixel in triangle)
                {
                    OutputImage.SetPixel(pixel.Item1,pixel.Item2,Color.FromArgb(255,255,255));
                }
            }
            //TODO: step 1: analyze labeled objects. remove not "triangily" objects
            

            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    //OutputImage.SetPixel(x, y, Image[x, y]);
                    OutputImage.SetPixel(x, y, Color.FromArgb(watershedImg[x, y],watershedImg[x,y],watershedImg[x,y]));// Set the pixel color at coordinate (x,y)
                }
            }
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }

        private bool IsTriangle(List<Tuple<int, int>> o)
        {
            int minX = Int32.MaxValue;
            int minY = Int32.MaxValue;
            int maxX = 0;
            int maxY = 0;
            float area = 0;
            foreach (Tuple<int, int> coord in o)
            {
                area++;
                if (coord.Item1 < minX)
                    minX = coord.Item1;
                if (coord.Item1 > maxX)
                    maxX = coord.Item1;
                if (coord.Item2 < minY)
                    minY = coord.Item2;
                if (coord.Item2 > maxY)
                    maxY = coord.Item2;
            }
            // Calculate the area of the bounding box.
            int sideX = maxX - minX;
            int sideY = maxY - minY;
            int areaRect = sideX * sideY;
            if (sideX == 0 || sideY == 0) {
                // Object is a line, therefore no triangle
                return false;
            }
            float devider = area / areaRect;
            Console.WriteLine("sideX: "+ sideX + " sideY: "+ sideY + " area: "+ area + " areaRect: "+ areaRect +" devider: " + devider);
            if (devider > .45f && devider < .55f)
            {
                // Probably a triangle!
                return true;
            }
            return false;
        }

        private void Label(Color color, int x, int y, int width, int height)
        {
            //Console.WriteLine("x: " + x +" y: "+ y + " Color: " + color.R);
            //Console.WriteLine("Color: " + color.R);
            if (Labels[x, y] != Color.FromArgb(255,255,255))
                return;
            if (x < width - 1 && y < height -1 && Labels[x, y] == Color.FromArgb(255, 255, 255))
            {
                // Set the label
                Labels[x, y] = color;
                // Set the dictionary entry
                List<Tuple<int,int>> list;
                Objects.TryGetValue(color.R, out list);
                if (list != null)
                {
                    list.Add(new Tuple<int, int>(x, y));
                }
                else
                {
                    Objects.Add(color.R, new List<Tuple<int, int>>());
                    Objects[color.R].Add(new Tuple<int, int>(x, y));
                }
                if (x - 1 > 0 && Labels[x - 1, y] == Color.FromArgb(255,255,255))
                    Label(color, x - 1, y, width, height); // Left neighbour
                if (Labels[x + 1, y] == Color.FromArgb(255, 255, 255))
                    Label(color, x + 1, y, width, height); // Right neighbour
                if (y - 1 > 0 && Labels[x, y - 1] == Color.FromArgb(255, 255, 255))
                    Label(color, x, y - 1, width, height); // Top neighbour
                if (Labels[x , y + 1] == Color.FromArgb(255, 255, 255))
                    Label(color, x, y + 1, width, height); // Bottom neighbour
                if (x - 1 > 0 &&  y - 1 > 0 && Labels[x - 1, y - 1] == Color.FromArgb(255, 255, 255))
                    Label(color, x - 1, y - 1, width, height); // Left top neighbour
                if (y - 1 > 0 &&Labels[x + 1, y -1] == Color.FromArgb(255, 255, 255))
                    Label(color, x + 1, y - 1, width, height); // Right top neighbour
                if (Labels[x + 1, y + 1] == Color.FromArgb(255, 255, 255))
                    Label(color, x + 1, y + 1, width, height); // Right bottom neighbour
                if (x - 1 > 0 && Labels[x - 1, y + 1] == Color.FromArgb(255, 255, 255))
                    Label(color, x - 1, y + 1, width, height); // Left Bottom neighbour
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
