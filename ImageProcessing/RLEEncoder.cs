// using System.Text;
//
// namespace WinFormsApp1.ImageProcessing
// {
//     public static class RleEncoder
//     {
//         public static string Encode(string input)
//         {
//             StringBuilder sb = new StringBuilder();
//             int count = 1;
//             char prev = input[0];
//
//             for (int i = 1; i < input.Length; i++)
//             {
//                 if (input[i] == prev)
//                     count++;
//                 else
//                 {
//                     sb.Append(prev);
//                     sb.Append(count);
//                     prev = input[i];
//                     count = 1;
//                 }
//             }
//             sb.Append(prev);
//             sb.Append(count);
//
//             return sb.ToString();
//         }
//     }
// }