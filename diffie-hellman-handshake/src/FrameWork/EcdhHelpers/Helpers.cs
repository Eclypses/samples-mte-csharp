using System;
using System.Buffers;

namespace PackageCSharpECDHFW
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------
    public class Helpers
    {
        internal static byte[] Rent(int minimumLength) => ArrayPool<byte>.Shared.Rent(minimumLength);

        internal static void Return(ArraySegment<byte> arraySegment)
        {
            Return(arraySegment.Array, arraySegment.Count);
        }
        internal const int ClearAll = -1;
        internal static void Return(byte[] array, int clearSize = ClearAll)
        {
            bool clearWholeArray = clearSize < 0;

            if (!clearWholeArray && clearSize != 0)
            {
#if (NETCOREAPP || NETSTANDARD2_1) && !CP_NO_ZEROMEMORY
                CryptographicOperations.ZeroMemory(array.AsSpan(0, clearSize));
#else
                Array.Clear(array, 0, clearSize);
#endif
            }

            ArrayPool<byte>.Shared.Return(array, clearWholeArray);
        }
    }
}
