using System;
using System.Net;

namespace Saber.Core.Extensions
{
    public static class Web
    {
        public static uint ToInt(this IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ToIpAddress(this uint address)
        {
            byte[] bytes = BitConverter.GetBytes(address);

            // flip little-endian to big-endian(network order)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return new IPAddress(bytes).ToString();
        }
    }
}
