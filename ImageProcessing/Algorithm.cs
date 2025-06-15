// using System;
// using System.Drawing;
// using System.Drawing.Imaging;
// using System.IO;
// using System.Text;
// using System.Windows.Forms;
//
// namespace WinFormsApp1
// {
//     public static class Algorithm
//     {
//         public static Bitmap ConvertToBinaryImage(Bitmap image)
//         {
//             Bitmap binaryImage = new Bitmap(image.Width, image.Height);
//             for (int y = 0; y < image.Height; y++)
//             {
//                 for (int x = 0; x < image.Width; x++)
//                 {
//                     Color pixelColor = image.GetPixel(x, y);
//                     int grayscale = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
//                     Color binaryColor = grayscale > 128 ? Color.White : Color.Black;
//                     binaryImage.SetPixel(x, y, binaryColor);
//                 }
//             }
//             return binaryImage;
//         }
//
//         public static string ApplyRLE(Bitmap binaryImage)
//         {
//             using (MemoryStream ms = new MemoryStream())
//             {
//                 binaryImage.Save(ms, ImageFormat.Png);
//                 byte[] imageBytes = ms.ToArray();
//
//                 StringBuilder binaryStringBuilder = new StringBuilder();
//                 foreach (byte b in imageBytes)
//                 {
//                     binaryStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
//                 }
//
//                 string binaryString = binaryStringBuilder.ToString();
//                 StringBuilder rleEncoded = new StringBuilder();
//
//                 char prevChar = binaryString[0];
//                 int count = 1;
//
//                 for (int i = 1; i < binaryString.Length; i++)
//                 {
//                     if (binaryString[i] == prevChar)
//                     {
//                         count++;
//                     }
//                     else
//                     {
//                         rleEncoded.Append(prevChar).Append(count);
//                         prevChar = binaryString[i];
//                         count = 1;
//                     }
//                 }
//
//                 rleEncoded.Append(prevChar).Append(count);
//                 return rleEncoded.ToString();
//             }
//         }
//
//         public static void ShowImages(Bitmap originalImage, Bitmap binaryImage)
//         {
//             Form form = new Form
//             {
//                 Text = "Original and Binary Images",
//                 Size = new Size(800, 400)
//             };
//
//             PictureBox pictureBoxOriginal = new PictureBox
//             {
//                 Image = originalImage,
//                 SizeMode = PictureBoxSizeMode.Zoom,
//                 Dock = DockStyle.Left
//             };
//
//             PictureBox pictureBoxBinary = new PictureBox
//             {
//                 Image = binaryImage,
//                 SizeMode = PictureBoxSizeMode.Zoom,
//                 Dock = DockStyle.Right
//             };
//
//             form.Controls.Add(pictureBoxOriginal);
//             form.Controls.Add(pictureBoxBinary);
//
//             Application.Run(form);
//         }
//     }
// }
