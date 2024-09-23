using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietQRHelper
{
    public static class Utils
    {
        /// <summary>
        /// Add simple polyfill for TextEncoder
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Byte[] StringToUint8Array(this string content)
        {
            if (typeof(Encoding).IsAssignableFrom(typeof(Encoding)))
            {
                return Encoding.UTF8.GetBytes(content);
            }

            // Polyfill for TextEncoder
            // This is a simplified implementation of the TextEncoder API
            // Source: https://gist.github.com/Yaffle/5458286
            List<byte> octets = new List<byte>();
            int length = content.Length;
            int i = 0;
            while (i < length)
            {
                int codePoint = char.ConvertToUtf32(content, i);
                int c = 0;
                int bits = 0;
                if (codePoint <= 0x0000007F)
                {
                    c = 0;
                    bits = 0x00;
                }
                else if (codePoint <= 0x000007FF)
                {
                    c = 6;
                    bits = 0xC0;
                }
                else if (codePoint <= 0x0000FFFF)
                {
                    c = 12;
                    bits = 0xE0;
                }
                else if (codePoint <= 0x001FFFFF)
                {
                    c = 18;
                    bits = 0xF0;
                }
                octets.Add((byte)(bits | (codePoint >> c)));
                c -= 6;
                while (c >= 0)
                {
                    octets.Add((byte)(0x80 | ((codePoint >> c) & 0x3F)));
                    c -= 6;
                }
                i += (codePoint >= 0x10000) ? 2 : 1;
            }
            return octets.ToArray();
        }
    }
}
