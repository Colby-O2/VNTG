using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteWriter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class PaletteWriter
    {
        public static void WriteHex(string path, List<Color> colors)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < colors.Count; i++)
            {
                sb.Append(ColorUtility.ToHtmlStringRGB(colors[i]));
                if (i < colors.Count - 1) sb.Append("\n");
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static void WritePaintNet(string path, string paletteName, List<Color> colors)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(";paint.net Palette File");
            sb.AppendLine($";Palette Name: {paletteName}");
            sb.AppendLine($";Description:");
            sb.AppendLine($";Colors: {colors.Count}");
            foreach (var color in colors)
            {
                Color32 c32 = color;
                sb.AppendLine($"{c32.a:X2}{c32.r:x2}{c32.g:x2}{c32.b:x2}");
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static void WriteGpl(string path, string paletteName, List<Color> colors)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GIMP Palette");
            sb.AppendLine($"Name: {paletteName}");
            sb.AppendLine("Columns: 4");
            sb.AppendLine("#");
            foreach (var color in colors)
            {
                Color32 c32 = color;
                sb.AppendLine($"{c32.r,3} {c32.g,3} {c32.b,3}\tUntitled Swatch");
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static void WriteJascPal(string path, List<Color> colors)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("JASC-PAL");
            sb.AppendLine("0100");
            sb.AppendLine(colors.Count.ToString());
            foreach (var color in colors)
            {
                Color32 c32 = color;
                sb.AppendLine($"{c32.r} {c32.g} {c32.b}");
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static void WriteAse(string path, List<Color> colors)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(Encoding.ASCII.GetBytes("ASEF"));
                WriteBigEndianUInt16(bw, 1);
                WriteBigEndianUInt16(bw, 0);
                WriteBigEndianUInt32(bw, (uint)colors.Count);

                for (int i = 0; i < colors.Count; i++)
                {
                    WriteBigEndianUInt16(bw, 0x0001);

                    string name = $"Color {i}\0";
                    uint blockLength = (uint)(2 + (name.Length * 2) + 4 + 12 + 2);
                    WriteBigEndianUInt32(bw, blockLength);

                    WriteBigEndianUInt16(bw, (ushort)name.Length);
                    foreach (char c in name)
                    {
                        WriteBigEndianUInt16(bw, (ushort)c);
                    }

                    bw.Write(Encoding.ASCII.GetBytes("RGB "));
                    WriteBigEndianFloat(bw, colors[i].r);
                    WriteBigEndianFloat(bw, colors[i].g);
                    WriteBigEndianFloat(bw, colors[i].b);

                    WriteBigEndianUInt16(bw, 0);
                }
            }
        }

        private static void WriteBigEndianUInt16(BinaryWriter bw, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            bw.Write(bytes);
        }

        private static void WriteBigEndianUInt32(BinaryWriter bw, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            bw.Write(bytes);
        }

        private static void WriteBigEndianFloat(BinaryWriter bw, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            bw.Write(bytes);
        }
    }
}