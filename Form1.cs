using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
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
                    if (pixelColor.R < 180)
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
            Color[,] ClosedImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            Array.Copy(Image, 0, ClosedImage, 0, Image.Length);
            //++++++EROSION++++++
            for (int x = 0; x < InputImage.Size.Width-1; x++)
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
                                ClosedImage[x, y] = white;
                            else
                                ClosedImage[x, y] = black;

                        }
                        // Same stuff for x
                        if (x > 1 && x < InputImage.Size.Height)
                        {
                            if (Image[x - 1, y] == white && Image[x + 1, y] == white)
                                ClosedImage[x, y] = white;
                            else
                                ClosedImage[x, y] = black;
                        }
                    }
                }
            }
            //++++++END OF ERIOSION+++++
            //TODO: ACTUALLY CLOSE THE IMAGE! We need to perform dilation after the erosion (which is implemented above).
            // Set the image to the closed image
            Image = ClosedImage;

            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
