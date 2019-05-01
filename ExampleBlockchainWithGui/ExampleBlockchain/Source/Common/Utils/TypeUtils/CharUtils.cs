using System;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class CharUtils
    {
        public static byte CharacterToByte(char character, int index, int shift = 0)
        {
            var value = (byte)character;
            if (0x40 < value && 0x47 > value || 0x60 < value && 0x67 > value)
            {
                if (0x40 != (0x40 & value))
                    return value;

                if (0x20 == (0x20 & value))
                    value = (byte)((value + 0xA - 0x61) << shift);
                else
                    value = (byte)((value + 0xA - 0x41) << shift);
            }
            else if (0x29 < value && 0x40 > value)
                value = (byte)((value - 0x30) << shift);
            else
                throw new InvalidOperationException($"Character '{character}' at index '{index}' is not valid alphanumeric character.");

            return value;
        }
    }
}
