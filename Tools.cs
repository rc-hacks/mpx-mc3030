using System;
using System.IO;

namespace Mpx
{
    class Tools
    {
        public static string FormatBuffer(byte[] buffer)
        {
            var stringWriter = new StringWriter();
            DumpBuffer(stringWriter, buffer, 0, buffer.Length, buffer.Length, false);
            return stringWriter.ToString();
        }

        public static void DumpBuffer(byte[] buffer)
        {
            DumpBuffer(Console.Out, buffer, 0, buffer.Length);
        }

        public static void DumpBuffer(byte[] buffer, int offset, int count)
        {
            DumpBuffer(Console.Out, buffer, offset, count);
        }

        public static void DumpBuffer(TextWriter textWriter, byte[] buffer, int offset, int count, int bytesPerRow = 16, bool showAscii = true)
        {
            for (int row = 0; row < count; row += bytesPerRow)
            {
                if (showAscii)
                {
                    textWriter.Write($"{row:X2}: ");
                }

                for (int col = 0; col < bytesPerRow; col++)
                {
                    int position = row + col;
                    if (position < count)
                    {
                        textWriter.Write($"{buffer[offset + position]:X2}");
                        textWriter.Write(position + 1 < count && col < 15 && (col + 1) % 4 == 0 ? '-' : ' ');
                    }
                    else
                    {
                        textWriter.Write("   ");
                    }
                }

                if (showAscii)
                {
                    textWriter.Write(' ');

                    for (int col = 0; col < bytesPerRow; col++)
                    {
                        int position = row + col;
                        if (position < count)
                        {
                            var b = buffer[offset + position];
                            textWriter.Write(b >= 0x20 ? (char)b : '.');
                        }
                    }

                    textWriter.WriteLine();
                }
            }
        }
    }

    static class ArrayHelper
    {
        public static T[] Subset<T>(this T[] array, int startIndex)
        {
            var subset = new T[array.Length - startIndex];
            Array.Copy(array, startIndex, subset, 0, array.Length - startIndex);
            return subset;
        }
    }
}
