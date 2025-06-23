using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.io
{
    public static class BinaryWriterExtensions
    {
        public static async Task WriteAsync(this BinaryWriter writer, int value, CancellationToken token = default)
        {
            await writer.BaseStream.WriteAsync(BitConverter.GetBytes(value), token);
        }

        public static async Task WriteAsync(this BinaryWriter writer, string value, CancellationToken token = default)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            await writer.WriteAsync(bytes.Length, token);
            await writer.BaseStream.WriteAsync(bytes, token);
        }

        public static async Task WriteAsync(this BinaryWriter writer, byte[] value, CancellationToken token = default)
        {
            await writer.BaseStream.WriteAsync(value, token);
        }
    }
}
