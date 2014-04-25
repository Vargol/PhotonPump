using System;
using System.IO;
using SunflowSharp.Core;
using SunflowSharp.Image;
using SunflowSharp.Systems;

namespace SunflowSharp.Core.Display
{

    public class FileDisplay : IDisplay
    {
        protected BitmapWriter writer;
        protected string filename;

		public FileDisplay(bool saveImage) : this(saveImage ? "output.png" : ".none") { return; }

        public FileDisplay(string filename)
        {
            this.filename = filename == null ? "output.png" : filename;
			string extension = FileUtils.getExtension(filename);
			writer = PluginRegistry.bitmapWriterPlugins.createObject(extension);

        }

        public virtual void imageBegin(int w, int h, int bucketSize)
        {
			if (writer == null)
				return;
			
			try {
				writer.openFile(filename);
				writer.writeHeader(w, h, bucketSize);
			} catch (IOException e) {
				UI.printError(UI.Module.IMG, "I/O error occured while preparing image for display: {0}", e.Message);
			}
		}

        public virtual void imagePrepare(int x, int y, int w, int h, int id)
        {
			// does nothing for files
        }

		public virtual void imageUpdate(int x, int y, int w, int h, Color[] data, float[] alpha)
        {
			if (writer == null)
				return;
			
			try {
				writer.writeTile(x, y, w, h, data, alpha);
			} catch (IOException e) {
				UI.printError(UI.Module.IMG, "I/O error occured while writing image tile [({0},{1}) {2}{3}] image for display: {4}", x, y, w, h, e.Message);	
			}
		}

		public virtual void imageFill(int x, int y, int w, int h, Color c, float alpha)
        {
			if (writer == null)
				return;
			
			Color[] colorTile = new Color[w * h];
			float[] alphaTile = new float[w * h];
			for (int i = 0; i < colorTile.Length; i++) {
				colorTile[i] = c;
				alphaTile[i] = alpha;
			}
			
			imageUpdate(x, y, w, h, colorTile, alphaTile);
		}

        public virtual void imageEnd()
        {
			if (writer == null)
				return;
			
			try {
				writer.closeFile();
			} catch (IOException e) {
				UI.printError(UI.Module.IMG, "I/O error occured while closing the display: {0}", e.Message);
			}
        }
    }
}