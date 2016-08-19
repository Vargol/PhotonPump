using System;
using System.IO;
using System.IO.Compression;
using System.Text; 
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Systems;
using SunflowSharp.Systems.Ui;

namespace SunflowSharp.Image.Writers
{
    /**
     * This display outputs a tiled OpenEXR file with RGB information.
     */
	public class EXRBitmapWriter : BitmapWriter
    {
		private const byte HALF = 1;
		private const byte FLOAT = 2;
		private const byte ZERO = 0;
		private const int HALF_SIZE = 2;
		private const int FLOAT_SIZE = 4;

		private const int OE_MAGIC = 20000630;
		private const int OE_EXR_VERSION = 2;
		private const int OE_TILED_FLAG = 0x00000200;

		private const int NO_COMPRESSION = 0;
		private const int RLE_COMPRESSION = 1;
        private const int ZIP_COMPRESSION = 3;

		private const int RLE_MIN_RUN = 3;
		private const int RLE_MAX_RUN = 127;

        private string filename;
        private BinaryWriter file;
        private long[,] tileOffsets;
        private long tileOffsetsPosition;
        private int tilesX;
        private int tilesY;
        private int tileSize;
        private int compression;
        private byte channelType;
        private int channelSize;
        private byte[] tmpbuf;
        private byte[] comprbuf;

		public EXRBitmapWriter() {
			// default settings
//			configure("compression", "zip");
			configure("channeltype", "half");
			configure("compression", "zip");
//			configure("channeltype", "float");
		}
		
		public override void configure(string option, string value) {
			if (option.Equals("compression")) {
				if (value.Equals("none"))
					compression = NO_COMPRESSION;
				else if (value.Equals("rle"))
					compression = RLE_COMPRESSION;
				else if (value.Equals("zip"))
					compression = ZIP_COMPRESSION;
				else {
					UI.printWarning(UI.Module.IMG, "EXR - Compression type was not recognized - defaulting to zip");
					compression = ZIP_COMPRESSION;
				}
			} else if (option.Equals("channeltype")) {
				if (value.Equals("float")) {
					channelType = FLOAT;
					channelSize = FLOAT_SIZE;
				} else if (value.Equals("half")) {
					channelType = HALF;
					channelSize = HALF_SIZE;
				} else {
					UI.printWarning(UI.Module.DISP, "EXR - Channel type was not recognized - defaulting to float");
					channelType = FLOAT;
					channelSize = FLOAT_SIZE;
				}
			}
		}
		
		public override void openFile(String filename) {
			this.filename = filename == null ? "output.exr" : filename;
		}

		public override void writeHeader(int width, int height, int tileSize) {
			file = new BinaryWriter(File.Open(filename, FileMode.Create));

			if (tileSize <= 0)
				throw new Exception("Can't use OpenEXR bitmap writer without buckets.");
			writeRGBAHeader(width, height, tileSize);
		}
		
		public override void writeTile(int x, int y, int w, int h, Color[] color, float[] alpha) {
			lock(file)
			{
				int tx = x / tileSize;
				int ty = y / tileSize;
				writeEXRTile(tx, ty, w, h, color, alpha);
			}
		}
		
		public override void closeFile() {
			writeTileOffsets();
			file.Close();
		}
		
		private void writeRGBAHeader(int w, int h, int tileSize) {
			byte[] chanOut = { 0, channelType, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1,
				0, 0, 0 };
			
			file.Write(ByteUtil.get4Bytes(OE_MAGIC));
			
			file.Write(ByteUtil.get4Bytes(OE_EXR_VERSION | OE_TILED_FLAG));
			
			file.Write(Encoding.Default.GetBytes("channels"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("chlist"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(73));
			file.Write(Encoding.Default.GetBytes("R"));
			file.Write(chanOut);
			file.Write(Encoding.Default.GetBytes("G"));
			file.Write(chanOut);
			 file.Write(Encoding.Default.GetBytes("B"));
			file.Write(chanOut);
			file.Write(Encoding.Default.GetBytes("A"));
			file.Write(chanOut);
			file.Write(ZERO);
			
			// compression
			file.Write(Encoding.Default.GetBytes("compression"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("compression"));
			file.Write(ZERO);
			file.Write((byte)1);
			file.Write(ByteUtil.get4BytesInv(compression));
			
			// datawindow =~ image size
			file.Write(Encoding.Default.GetBytes("dataWindow"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("box2i"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(16));
			file.Write(ByteUtil.get4Bytes(0));
			file.Write(ByteUtil.get4Bytes(0));
			file.Write(ByteUtil.get4Bytes(w - 1));
			file.Write(ByteUtil.get4Bytes(h - 1));
			
			// dispwindow -> look at openexr.com for more info
			file.Write(Encoding.Default.GetBytes("displayWindow"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("box2i"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(16));
			file.Write(ByteUtil.get4Bytes(0));
			file.Write(ByteUtil.get4Bytes(0));
			file.Write(ByteUtil.get4Bytes(w - 1));
			file.Write(ByteUtil.get4Bytes(h - 1));
			
			/*
* lines in increasing y order = 0 decreasing would be 1
*/
			file.Write(Encoding.Default.GetBytes("lineOrder"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("lineOrder"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(1));
			file.Write((byte)2);
			
			file.Write(Encoding.Default.GetBytes("pixelAspectRatio"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("float"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(4));
			file.Write (ByteUtil.get4Bytes (BitConverter.ToInt32 (BitConverter.GetBytes (1.0f), 0)));
			
			// meaningless to a flat (2D) image
			file.Write(Encoding.Default.GetBytes("screenWindowCenter"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("v2f"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(8));
			file.Write (ByteUtil.get4Bytes (BitConverter.ToInt32 (BitConverter.GetBytes (0.0f), 0)));
			file.Write (ByteUtil.get4Bytes (BitConverter.ToInt32 (BitConverter.GetBytes (0.0f), 0)));

			// meaningless to a flat (2D) image
			file.Write(Encoding.Default.GetBytes("screenWindowWidth"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("float"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(4));
			file.Write (ByteUtil.get4Bytes (BitConverter.ToInt32 (BitConverter.GetBytes (1.0f), 0)));

			this.tileSize = tileSize;
			
			tilesX = ((w + tileSize - 1) / tileSize);
			tilesY = ((h + tileSize - 1) / tileSize);
			
			/*
* twice the space for the compressing buffer, as for ex. the compressor
* can actually increase the size of the data :) If that happens though,
* it is not saved into the file, but discarded
*/
			tmpbuf = new byte[tileSize * tileSize * channelSize * 4];
			comprbuf = new byte[tileSize * tileSize * channelSize * 4 * 2];
			
			tileOffsets = new long[tilesX,tilesY];
			
			file.Write(Encoding.Default.GetBytes("tiles"));
			file.Write(ZERO);
			file.Write(Encoding.Default.GetBytes("tiledesc"));
			file.Write(ZERO);
			file.Write(ByteUtil.get4Bytes(9));
			
			file.Write(ByteUtil.get4Bytes(tileSize));
			file.Write(ByteUtil.get4Bytes(tileSize));
			
			// ONE_LEVEL tiles, ROUNDING_MODE = not important
			file.Write(ZERO);
			
			// an attribute with a name of 0 to end the list
			file.Write(ZERO);
			
			// save a pointer to where the tileOffsets are stored and write dummy
			// fillers for now
			tileOffsetsPosition = file.BaseStream.Position;
			writeTileOffsets();
		}
		
		private void writeTileOffsets() {
			file.Seek((int)tileOffsetsPosition, SeekOrigin.Begin);
			for (int ty = 0; ty < tilesY; ty++)
				for (int tx = 0; tx < tilesX; tx++)
					file.Write(ByteUtil.get8Bytes(tileOffsets[tx,ty]));
		}
		
		private void writeEXRTile(int tileX, int tileY, int w, int h, Color[] tile, float[] alpha) {
			byte[] rgb;
			
			// setting comprSize to max integer so without compression things
			// don't go awry
			int pixptr = 0, writeSize = 0, comprSize = Int32.MaxValue;
			int tileRangeX = (tileSize < w) ? tileSize : w;
			int tileRangeY = (tileSize < h) ? tileSize : h;
			int channelBase = tileRangeX * channelSize;
			
			// lets see if the alignment matches, you can comment this out if
			// need be
			if ((tileSize != tileRangeX) && (tileX == 0))
				Console.Write(" bad X alignment ");
			if ((tileSize != tileRangeY) && (tileY == 0))
				Console.Write(" bad Y alignment ");
			
			tileOffsets[tileX,tileY] = file.BaseStream.Position;
			
			// the tile header: tile's x&y coordinate, levels x&y coordinate and
			// tilesize
			file.Write(ByteUtil.get4Bytes(tileX));
			file.Write(ByteUtil.get4Bytes(tileY));
			file.Write(ByteUtil.get4Bytes(0));
			file.Write(ByteUtil.get4Bytes(0));
			
			// just in case
			Array.Clear(tmpbuf, 0, tmpbuf.Length);
			
			for (int ty = 0; ty < tileRangeY; ty++) {
				for (int tx = 0; tx < tileRangeX; tx++) {
					float[] rgbf = tile[tx + ty * tileRangeX].getRGB();
					if (channelType == FLOAT) {
						rgb = BitConverter.GetBytes(alpha[tx + ty * tileRangeX]);
						tmpbuf[pixptr + 0] = rgb[0];
						tmpbuf[pixptr + 1] = rgb[1];
						tmpbuf[pixptr + 2] = rgb[2];
						tmpbuf[pixptr + 3] = rgb[3];
					} else if (channelType == HALF) {
						rgb = ByteUtil.get2Bytes(ByteUtil.floatToHalf(alpha[tx + ty * tileRangeX]));
						tmpbuf[pixptr + 0] = rgb[0];
						tmpbuf[pixptr + 1] = rgb[1];
					}
					for (int component = 1; component <= 3; component++) {
						if (channelType == FLOAT) {
							rgb = BitConverter.GetBytes(rgbf[3 - component]);
							tmpbuf[(channelBase * component) + pixptr + 0] = rgb[0];
							tmpbuf[(channelBase * component) + pixptr + 1] = rgb[1];
							tmpbuf[(channelBase * component) + pixptr + 2] = rgb[2];
							tmpbuf[(channelBase * component) + pixptr + 3] = rgb[3];
						} else if (channelType == HALF) {
							rgb = ByteUtil.get2Bytes(ByteUtil.floatToHalf(rgbf[3 - component]));
							tmpbuf[(channelBase * component) + pixptr + 0] = rgb[0];
							tmpbuf[(channelBase * component) + pixptr + 1] = rgb[1];
						}
					}
					pixptr += channelSize;
				}
				pixptr += (tileRangeX * channelSize * 3);
			}
			
			writeSize = tileRangeX * tileRangeY * channelSize * 4;
			
			if (compression != NO_COMPRESSION)
				comprSize = compress(compression, tmpbuf, writeSize, comprbuf);
			
			// lastly, write the size of the tile and the tile itself
			// (compressed or not)
			if (comprSize < writeSize) {
				file.Write(ByteUtil.get4Bytes(comprSize));
				file.Write(comprbuf, 0, comprSize);
			} else {
				file.Write(ByteUtil.get4Bytes(writeSize));
				file.Write(tmpbuf, 0, writeSize);
			}
		}
		
		private static int compress(int tp, byte[] inBytes, int inSize, byte[] outBytes) {
			if (inSize == 0)
				return 0;
			
			int t1 = 0, t2 = (inSize + 1) / 2;
			int inPtr = 0;
			byte[] tmp = new byte[inSize];
			
			// zip and rle treat the data first, in the same way so I'm not
			// repeating the code
			if ((tp == ZIP_COMPRESSION) || (tp == RLE_COMPRESSION)) {
				// reorder the pixel data ~ straight from ImfZipCompressor.cpp :)
				while (true) {
					if (inPtr < inSize)
						tmp[t1++] = inBytes[inPtr++];
					else
						break;
					
					if (inPtr < inSize)
						tmp[t2++] = inBytes[inPtr++];
					else
						break;
				}
				
				// Predictor ~ straight from ImfZipCompressor.cpp :)
				t1 = 1;
				int p = tmp[t1 - 1];
				while (t1 < inSize) {
					int d = tmp[t1] - p + (128 + 256);
					p = tmp[t1];
					tmp[t1] = (byte) d;
					t1++;
				}
			}
			
			// We'll just jump from here to the wanted compress/decompress stuff if
			// need be
			switch (tp) {
			case ZIP_COMPRESSION:


				using (MemoryStream output = new MemoryStream())
				{
					using (DeflateStream gzip = new DeflateStream(output, CompressionMode.Compress))
					{
						//						using (StreamWriter writer = new StreamWriter(gzip, System.Text.Encoding.UTF8))
						using (StreamWriter writer = new StreamWriter(gzip))
						{
							writer.Write(tmp);           
						}
					}
					
					outBytes = output.ToArray();
				}
				return outBytes.Length;
			case RLE_COMPRESSION:
				return rleCompress(tmp, inSize, outBytes);
			default:
				return -1;
			}
		}
		
		private static int rleCompress(byte[] inBytes, int inLen, byte[] outBytes) {
			int runStart = 0, runEnd = 1, outWrite = 0;
			while (runStart < inLen) {
				while (runEnd < inLen && inBytes[runStart] == inBytes[runEnd] && (runEnd - runStart - 1) < RLE_MAX_RUN)
					runEnd++;
				if (runEnd - runStart >= RLE_MIN_RUN) {
					// Compressable run
					outBytes[outWrite++] = (byte) ((runEnd - runStart) - 1);
					outBytes[outWrite++] = inBytes[runStart];
					runStart = runEnd;
				} else {
					// Uncompressable run
					while (runEnd < inLen && (((runEnd + 1) >= inLen || inBytes[runEnd] != inBytes[runEnd + 1]) || ((runEnd + 2) >= inLen || inBytes[runEnd + 1] != inBytes[runEnd + 2])) && (runEnd - runStart) < RLE_MAX_RUN)
						runEnd++;
					outBytes[outWrite++] = (byte) (runStart - runEnd);
					while (runStart < runEnd)
						outBytes[outWrite++] = inBytes[runStart++];
				}
				runEnd++;
			}
			return outWrite;
		}
    }
}