using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEST
{
    class Program
    {

        static void Main(string[] args)
        {
            string text = "1212";
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var crc64 = CRC64.Create("CRC64_XZ");
            var crc64Value = crc64.Compute(bytes);
            Console.WriteLine("0x" + crc64Value.ToString("X2"));
        }

        #region CRC64
        public class CRC64
        {
            readonly ulong[] table;
            readonly ulong polynomial;
            readonly ulong initial;
            readonly ulong finalXor;
            readonly bool inputReflected;
            readonly bool resultReflected;
            readonly ulong castMask = ulong.MaxValue;
            readonly ulong msbMask = 0x8000000000000000;

            public static CRC64 Create(string algorName)
            {
                switch (algorName)
                {
                    case "CRC64_ECMA_182":
                        return new CRC64(
                            polynomial: 0x42f0e1eba9ea3693,
                            initial: ulong.MinValue,
                            finalXor: ulong.MinValue,
                            inputReflected: false,
                            resultReflected: false);
                    case "CRC64_GO_ISO":
                        return new CRC64(
                            polynomial: 0x000000000000001b,
                            initial: ulong.MaxValue,
                            finalXor: ulong.MaxValue,
                            inputReflected: true,
                            resultReflected: true
                            );
                    case "CRC64_XZ":
                        return new CRC64(
                            polynomial: 0x42f0e1eba9ea3693,
                            initial: ulong.MaxValue,
                            finalXor: ulong.MaxValue,
                            inputReflected: true,
                            resultReflected: true
                            );
                    default:
                        throw new KeyNotFoundException();
                }
            }

            public CRC64(ulong polynomial, ulong initial, ulong finalXor, bool inputReflected, bool resultReflected)
            {
                this.polynomial = polynomial;
                this.initial = initial;
                this.finalXor = finalXor;
                this.inputReflected = inputReflected;
                this.resultReflected = resultReflected;
                table = CreateTable();
            }

            public ulong[] CreateTable()
            {
                ulong[] crcTable = new ulong[256];
                for (uint divident = 0; divident < 256; divident++)
                {
                    ulong currByte = divident;
                    currByte = (currByte << 56) & castMask;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if ((currByte & msbMask) > 0)
                        {
                            currByte <<= 1;
                            currByte ^= polynomial;
                        }
                        else
                        {
                            currByte <<= 1;
                        }
                    }
                    crcTable[divident] = currByte & castMask;
                }
                return crcTable;
            }

            public ulong Compute(string text)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                return Compute(bytes);
            }

            public ulong Compute(byte[] bytes)
            {
                ulong crc = initial;
                for (int i = 0; i < bytes.Length; i++)
                {
                    ulong curByte = (ulong)bytes[i] & 0xFF;
                    if (inputReflected)
                    {
                        curByte = Reflect8(curByte);
                    }
                    crc = (crc ^ (curByte << 56)) & castMask;
                    ulong pos = (crc >> 56) & 0xFF;
                    crc = (crc << 8) & castMask;
                    crc = (crc ^ table[pos]) & castMask;
                }
                if (resultReflected)
                {
                    crc = Reflect(crc);
                }
                crc = (crc ^ finalXor) & castMask;
                return crc;
            }

            ulong Reflect8(ulong val)
            {
                ulong resByte = 0;
                for (int i = 0; i < 8; i++)
                {
                    if ((val & (ulong)(1 << i)) != 0)
                    {
                        resByte |= ((ulong)(1 << (7 - i)) & 0xFF);
                    }
                }
                return resByte;
            }

            ulong Reflect(ulong value)
            {
                ulong temp = value;
                ulong result = 0;
                for (int i = 63; i >= 0; i--)
                {
                    ulong a = (temp & 1) << i;
                    result |= a;
                    temp = temp >> 1;
                }
                return result;
            }
        }
        #endregion
    }
}
