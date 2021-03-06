﻿using System.Numerics;
using System.Runtime.CompilerServices;

namespace Channels
{
    // Move to text library?
    internal class CommonVectors
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector<byte> GetVector(byte vectorByte)
        {
            // Vector<byte> .ctor is a bit fussy to get working; however this always seems to work
            // https://github.com/dotnet/coreclr/issues/7459#issuecomment-253965670
            return Vector.AsVectorByte(new Vector<ulong>(vectorByte * 0x0101010101010101ul));
        }
    }
}
