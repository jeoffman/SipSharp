using SipSharp.Sip.Interfaces;
using System;
using System.Linq;

namespace SipSharp.Sip
{
    internal class SipUtilities
    {
    }

    public static class Utils
    {
        public static int IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            int positions = -1;
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                {
                    positions = i;
                    break;
                }
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }
    }
}