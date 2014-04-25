// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System.IO;

namespace SunflowSharp.Image.Writers
{
	public class HDRBitmapWriter : BitmapWriter {
		private string filename;
		private int width, height;
		private int[] data;
		
		public override void configure(string option, string value) {
		}
		
		public override void openFile(string filename) {
			this.filename = filename;
		}
		
		public override void writeHeader(int width, int height, int tileSize) {
			this.width = width;
			this.height = height;
			data = new int[width * height];
		}
		
		public override void writeTile(int x, int y, int w, int h, Color[] color, float[] alpha) {
			int[] tileData = ColorEncoder.encodeRGBE(color);
			for (int j = 0, index = 0, pixel = x + y * width; j < h; j++, pixel += width - w)
				for (int i = 0; i < w; i++, index++, pixel++)
					data[pixel] = tileData[index];
		}
		
		public override void closeFile() {


			FileStream f = new FileStream(filename, FileMode.OpenOrCreate);
			byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("#?RGBE\n");
			f.Write(buffer, 0, buffer.Length);
			//f.write("#?RGBE\n".getBytes());
			buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("FORMAT=32-bit_rle_rgbe\n\n");
			f.Write(buffer, 0, buffer.Length);
			//f.write("FORMAT=32-bit_rle_rgbe\n\n".getBytes());
			buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("-Y " + height + " +X " + width + "\n");
			f.Write(buffer, 0, buffer.Length);
			//f.write(("-Y " + height + " +X " + width + "\n").getBytes());
			for (int y = height - 1; y >= 0; y--)
			{
				for (int x = 0; x < width; x++)
				{
					int rgbe = data[(y * width) + x];
					f.WriteByte((byte)(rgbe >> 24));
					f.WriteByte((byte)(rgbe >> 16));
					f.WriteByte((byte)(rgbe >> 8));
					f.WriteByte((byte)rgbe);
				}
			}
			f.Close();

		}
	}
}
