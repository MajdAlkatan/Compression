// using System.Drawing;
//
// namespace WinFormsApp1.ImageProcessing
// {
//     public static class ImageConverter
//     {
//         public static Bitmap ConvertToBinary(Bitmap image)
//         {
//             Bitmap binaryImage = new Bitmap(image.Width, image.Height);
//             for (int y = 0; y < image.Height; y++)
//             {
//                 for (int x = 0; x < image.Width; x++)
//                 {
//                     Color pixelColor = image.GetPixel(x, y);
//                     int gray = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
//                     Color binaryColor = gray > 128 ? Color.White : Color.Black;
//                     binaryImage.SetPixel(x, y, binaryColor);
//                 }
//             }
//             return binaryImage;
//         }
//     }
// }