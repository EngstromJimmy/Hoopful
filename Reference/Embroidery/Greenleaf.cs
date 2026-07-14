using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Greenleaf
{
	class ArchiveLibrary
	{
		[DllImport("al21mfc.dll")]
		public static extern IntPtr newALGlDecompressor(
			uint level);

		[DllImport("al21mfc.dll")]
		public static extern IntPtr newALWinMemory(
			string BufferName,
			byte[] UserBuffer,
			int UserBufferSize); 

		[DllImport("al21mfc.dll")]
		public static extern int ALDecompress(
			IntPtr hDecomp,
			IntPtr hInput,
			IntPtr hOutput,
			int iLength);

		[DllImport("al21mfc.dll")]
		public static extern int ALMemoryBaseGetBufferSize(
			IntPtr hBuffer);
        
		[DllImport("al21mfc.dll")]
		public static extern IntPtr ALWinMemoryGetBuffer(
			IntPtr hBuffer);

		[DllImport("al21mfc.dll")]
		public static extern void deleteALStorage(
			IntPtr hBuffer); 

//		static void Main(string[] args)
//		{
//			byte[] compressed = {0xF,0xA,0xC,0xE,0xD,0xA,0x7,0xA}; // fake data 
//			byte [] decompressed = Decompress(compressed);
//		}

		public static byte[] DecompressBytes(byte[] compressed )
		{
			return Decompress(compressed);
		}

		static byte[] Decompress(byte[] compressedData)
		{
			int    compressedDataLength   = 0;
			IntPtr hDecompressor          = IntPtr.Zero;
			IntPtr hInput                 = IntPtr.Zero;
			IntPtr hOutput                = IntPtr.Zero;
			int    decompressedDataLength = 0;
			byte[] decompressedData       = null;

			try
			{
				compressedDataLength = compressedData.Length;

				hDecompressor = newALGlDecompressor(4 /*AL_GREENLEAF_LEVEL_4*/);
				hInput = newALWinMemory("Section1", compressedData, compressedDataLength);
				hOutput = newALWinMemory("Output", null, 0);

				if (0 == ALDecompress(hDecompressor, hInput, hOutput, compressedDataLength))
				{
					decompressedDataLength = ALMemoryBaseGetBufferSize(hOutput);
					decompressedData = new byte[decompressedDataLength];

					IntPtr pDecompressedBuffer = ALWinMemoryGetBuffer(hOutput);
					Marshal.Copy(pDecompressedBuffer, decompressedData, 0, decompressedDataLength);
				}
			}
			finally
			{
				if (hInput != IntPtr.Zero)
				{
					deleteALStorage(hInput);
				}

				if (hOutput != IntPtr.Zero)
				{
					deleteALStorage(hOutput);
				}

				if (hDecompressor != IntPtr.Zero)
				{
					// Free?
				}
			}

			return decompressedData;
		}
	}
}
