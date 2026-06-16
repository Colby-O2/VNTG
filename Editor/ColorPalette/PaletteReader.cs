using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteReader.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class PaletteReader
    {
        public static List<Color> ParseHex(string path)
        {
            List<Color> colors = new List<Color>();
            string text = File.ReadAllText(path);
            string[] tokens = text.Split(new char[] { ',', '\n', '\r', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                string cleanHex = token.Trim().Replace("#", "");
                if (ColorUtility.TryParseHtmlString("#" + cleanHex, out Color color))
                {
                    colors.Add(color);
                }
            }
            return colors;
        }

        public static List<Color> ParsePaintNet(string path)
        {
            List<Color> colors = new List<Color>();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
                {
                    continue;
                }

                string cleanHex = trimmed.Replace("#", "");

                if (cleanHex.Length == 8)
                {
                    string alpha = cleanHex.Substring(0, 2);
                    string rgb = cleanHex.Substring(2, 6);
                    cleanHex = rgb + alpha;
                }

                if (ColorUtility.TryParseHtmlString("#" + cleanHex, out Color color))
                {
                    colors.Add(color);
                }
            }

            return colors;
        }

        public static List<Color> ParseGpl(string path)
        {
            List<Color> colors = new List<Color>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("GIMP") || trimmed.StartsWith("Name:") || trimmed.StartsWith("Columns:") || trimmed.StartsWith("#"))
                {
                    continue;
                }

                string[] tokens = trimmed.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 3 && int.TryParse(tokens[0], out int r) && int.TryParse(tokens[1], out int g) && int.TryParse(tokens[2], out int b))
                {
                    colors.Add(new Color(r / 255f, g / 255f, b / 255f, 1f));
                }
            }
            return colors;
        }

        public static List<Color> ParseAse(string path)
        {
            List<Color> colors = new List<Color>();

            if (!File.Exists(path)) return colors;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] signatureBytes = br.ReadBytes(4);
                string signature = System.Text.Encoding.ASCII.GetString(signatureBytes);
                if (signature != "ASEF")
                {
                    throw new Exception("Not a valid Adobe Swatch Exchange (.ase) file.");
                }

                ushort versionMajor = ReadBigEndianUInt16(br);
                ushort versionMinor = ReadBigEndianUInt16(br);

                uint blockCount = ReadBigEndianUInt32(br);

                for (int i = 0; i < blockCount; i++)
                {
                    if (fs.Position >= fs.Length) break;

                    ushort blockType = ReadBigEndianUInt16(br);
                    uint blockLength = ReadBigEndianUInt32(br);

                    long blockStartPos = fs.Position;

                    if (blockType == 0x0001)
                    {
                        ushort nameLength = ReadBigEndianUInt16(br);

                        fs.Position += (nameLength * 2);

                        byte[] modelBytes = br.ReadBytes(4);
                        string colorModel = System.Text.Encoding.ASCII.GetString(modelBytes).Trim();

                        if (colorModel == "RGB")
                        {
                            float r = ReadBigEndianFloat(br);
                            float g = ReadBigEndianFloat(br);
                            float b = ReadBigEndianFloat(br);

                            colors.Add(new Color(r, g, b, 1f));
                        }
                        else if (colorModel == "CMYK")
                        {
                            float c = ReadBigEndianFloat(br);
                            float m = ReadBigEndianFloat(br);
                            float y = ReadBigEndianFloat(br);
                            float k = ReadBigEndianFloat(br);

                            float r = (1f - c) * (1f - k);
                            float g = (1f - m) * (1f - k);
                            float b = (1f - y) * (1f - k);

                            colors.Add(new Color(r, g, b, 1f));
                        }
                        else if (colorModel == "GRAY")
                        {
                            float gray = ReadBigEndianFloat(br);
                            colors.Add(new Color(gray, gray, gray, 1f));
                        }
                    }

                    fs.Position = blockStartPos + blockLength;
                }
            }

            return colors;
        }

        public static List<Color> ParsePal(string path)
        {
            if (!File.Exists(path)) return new List<Color>();

            byte[] sigBytes = new byte[4];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= 4)
                {
                    fs.Read(sigBytes, 0, 4);
                }
            }

            string sig = System.Text.Encoding.ASCII.GetString(sigBytes);
            if (sig == "RIFF")
            {
                return ParseRiffPal(path);
            }

            return ParseJascPal(path);
        }

        private static List<Color> ParseJascPal(string path)
        {
            List<Color> colors = new List<Color>();
            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 3 || !lines[0].ToUpper().Contains("JASC-PAL"))
            {
                throw new Exception("Invalid JASC PAL file header.");
            }

            int countLineIndex = 2;
            while (countLineIndex < lines.Length && string.IsNullOrEmpty(lines[countLineIndex].Trim()))
            {
                countLineIndex++;
            }

            if (countLineIndex >= lines.Length || !int.TryParse(lines[countLineIndex].Trim(), out int targetColorCount))
            {
                throw new Exception("Could not parse color count from JASC PAL file.");
            }

            for (int i = countLineIndex + 1; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                string[] tokens = trimmed.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length >= 3 &&
                    int.TryParse(tokens[0], out int r) &&
                    int.TryParse(tokens[1], out int g) &&
                    int.TryParse(tokens[2], out int b))
                {
                    colors.Add(new Color(r / 255f, g / 255f, b / 255f, 1f));
                }

                if (colors.Count >= targetColorCount) break;
            }

            return colors;
        }

        private static List<Color> ParseRiffPal(string path)
        {
            List<Color> colors = new List<Color>();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.ReadBytes(4);
                br.ReadUInt32();

                string type = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
                if (type != "PAL ") throw new Exception("RIFF container type is not 'PAL '.");

                while (fs.Position < fs.Length - 8)
                {
                    string chunkID = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
                    uint chunkSize = br.ReadUInt32();
                    long chunkStart = fs.Position;

                    if (chunkID == "data")
                    {
                        br.ReadUInt16();
                        ushort numEntries = br.ReadUInt16();

                        for (int i = 0; i < numEntries; i++)
                        {
                            if (fs.Position >= fs.Length) break;

                            byte r = br.ReadByte();
                            byte g = br.ReadByte();
                            byte b = br.ReadByte();
                            br.ReadByte();

                            colors.Add(new Color(r / 255f, g / 255f, b / 255f, 1f));
                        }
                        break;
                    }
                    else if (chunkID == "LIST")
                    {
                        br.ReadBytes(4);
                        continue;
                    }

                    uint paddedSize = chunkSize % 2 == 1 ? chunkSize + 1 : chunkSize;
                    fs.Position = chunkStart + paddedSize;
                }
            }

            return colors;
        }

        private static ushort ReadBigEndianUInt16(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private static uint ReadBigEndianUInt32(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static float ReadBigEndianFloat(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}