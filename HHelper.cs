﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HazeronAdviser
{
    class HHelper
    {
        /// <summary>
        /// Converts a byte array to a Hexadecimal string.
        /// </summary>
        /// <param name="singleByte">Byte to be converted.</param>
        static public string ToHex(byte singleByte) // Based on http://stackoverflow.com/a/10048895
        {
            char[] hex = new char[2];

            byte b;

            b = ((byte)(singleByte >> 4));
            hex[0] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

            b = ((byte)(singleByte & 0x0F));
            hex[1] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

            return new string(hex);
        }
        /// <summary>
        /// Converts a byte array to a Hexadecimal string.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        static public string ToHex(byte[] bytes)
        {
            List<string> hexs = new List<string>();
            foreach (byte singleByte in bytes)
                hexs.Add(ToHex(singleByte));
            return string.Join("-", hexs.ToArray());
        }

        /// <summary>
        /// Converts four bytes from a byte array to a int32.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        static public int ToInt32(byte[] bytes, int startIndex)
        {
            byte[] subBytes = HHelper.SubArray(bytes, startIndex, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(subBytes);
            return BitConverter.ToInt32(subBytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a string, using BigEndianUnicode.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        /// <param name="length">Number of bytes to convert.</param>
        static public string ToBigEndianUnicodeString(byte[] bytes, int startIndex, int length)
        {
            byte[] subBytes = HHelper.SubArray(bytes, startIndex, length);
            //subBytes = Helper.ConcatinateArray(new byte[] { 0xFE, 0xFF }, subBytes);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(subBytes);
            string text = Encoding.BigEndianUnicode.GetString(subBytes); // UTF-16 BigEndian to string.
            return text;
        }

        /// <summary>
        /// Returns selected part of a byte array.
        /// </summary>
        /// <param name="bytes">Full byte array.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        static public byte[] SubArray(byte[] bytes, int startIndex)
        {
            if (startIndex == 0)
                return bytes;
            return SubArray(bytes, startIndex, bytes.Length - startIndex);
        }
        /// <summary>
        /// Returns selected part of a byte array.
        /// </summary>
        /// <param name="bytes">Full byte array.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        /// <param name="length">Number of bytes to return.</param>
        static public byte[] SubArray(byte[] bytes, int startIndex, int length)
        {
            byte[] rv = new byte[length];
            System.Buffer.BlockCopy(bytes, startIndex, rv, 0, length);
            return rv;
            //return new List<byte>(bytes).GetRange(startIndex, length).ToArray(); // Another ways of doing it.
        }

        /// <summary>
        /// Combine multiple arrays into one.
        /// One after the other.
        /// </summary>
        static public byte[] ConcatinateArray(byte[] array1, byte[] array2)
        {
            byte[] rv = new byte[array1.Length + array2.Length];
            System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
            return rv;
        }
        /// <summary>
        /// Combine multiple arrays into one.
        /// One after the other.
        /// </summary>
        static public byte[] ConcatinateArray(byte[] array1, byte[] array2, byte[] array3)
        {
            byte[] rv = new byte[array1.Length + array2.Length + array3.Length];
            System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
            System.Buffer.BlockCopy(array3, 0, rv, array1.Length + array2.Length, array3.Length);
            return rv;
        }

        /// <summary>
        /// Returns a string that is void of HTML tags.
        /// Attempts to add newlines where needed.
        /// </summary>
        /// <param name="input">HTML string.</param>
        static public string CleanText(string input) // Removes the html code tags.
        {
            if (input.Contains("<") && input.Contains(">"))
            {
                int tagStart, tagEnd;
                string processed = "";
                while (input.Contains("<") && input.Contains(">"))
                {
                    tagStart = input.IndexOf('<');
                    tagEnd = input.IndexOf('>') - tagStart;
                    processed += input.Remove(tagStart);
                    string tag = input.Substring(tagStart + 1, tagEnd - 1);
                    input = input.Substring(tagStart + tagEnd + 1);
                    switch (tag.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0])
                    {
                        case "br":
                        case "div":
                        case "/div":
                        case "/td":
                        case "/tr":
                            processed += Environment.NewLine;
                            break;
                        //case "b":
                        //case "/b":
                        //case "td":
                        //case "tr":
                        //    break;
                    }
                }
                return processed.Trim().Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine).Replace("&nbsp;", " "); // Trim for good measure and remove triple NewLine.
            }
            else
                return input.Trim();
        }
    }
}