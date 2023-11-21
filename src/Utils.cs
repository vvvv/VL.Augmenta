using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Augmenta
{
    internal static class Utils
    {
        internal static int ReadInt(Span<byte> data, int offset)
        {
            return MemoryMarshal.Cast<byte, int>(data.Slice(offset))[0];
        }

        internal static float ReadFloat(Span<byte> data, int offset)
        {
            return MemoryMarshal.Cast<byte, float>(data.Slice(offset))[0];
        }

        internal static Vector3 ReadVector(Span<byte> data, int offset)
        {
            return MemoryMarshal.Cast<byte, Vector3>(data.Slice(offset))[0];
        }
        internal static Span<Vector3> ReadVectors(Span<byte> data, int offset, int length)
        {
            return MemoryMarshal.Cast<byte, Vector3>(data.Slice(offset, length));
        }
    }
}
