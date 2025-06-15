using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WinFormsApp1.Compression
{
    public static partial class CompressionManager
    {
        // استرجاع كل الملفات (مسار نسبي + كامل)
        public static List<(string RelativePath, string FullPath)> GetFilesRecursively(string folderPath)
        {
            int baseLen = folderPath.TrimEnd(Path.DirectorySeparatorChar).Length + 1;

            return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                .Select(f => (f[baseLen..], f))
                .ToList();
        }

        // إنشاء أرشيف من مجلد كامل
        public static void CreateArchiveFromFolder(string folderPath,
            string archivePath,
            string algorithm = "Huffman")
        {
            var files = GetFilesRecursively(folderPath);
            if (files.Count == 0)
                throw new ArgumentException("Folder is empty.", nameof(folderPath));

            using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write(files.Count);

            foreach (var (relative, full) in files)
            {
                byte[] data = File.ReadAllBytes(full);
                byte[] comp = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out _)
                    : new HuffmanCompressor().Compress(data, out _);

                bw.Write(relative);
                bw.Write(data.Length);
                bw.Write(comp.Length);
                bw.Write(comp);
            }
        }
    }
}