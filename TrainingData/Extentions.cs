using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingData
{
    public static class Extentions
    {
        public static string[] AsStringArray(this byte[] chunk)
        {
            string[] array = new string[chunk[0]];
            int arrayjump = 1;
            for (int i = 0; i < array.Length; i++)
            {
                ushort bytelength = BitConverter.ToUInt16(chunk, arrayjump++);
                array[i] = BitConverter.ToString(chunk, ++arrayjump, bytelength);
                arrayjump += bytelength;
            }

            return array;
        }
    }
}
