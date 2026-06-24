using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    internal static class IndexRenderer
    {
        public static Bitmap Render(float[] data, int width, int height, float min, float max, IndexDisplayMode mode, IndexColorScheme scheme)
        {
            return mode == IndexDisplayMode.Grayscale
                ? RenderGrayscale(data, width, height, min, max)
                : scheme switch
                {
                    IndexColorScheme.Vegetation => RenderVegetationPseudocolor(data, width, height, min, max),
                    IndexColorScheme.Water => RenderWaterPseudocolor(data, width, height, min, max),
                    IndexColorScheme.Builtup => RenderBuiltupPseudocolor(data, width, height, min, max),
                    _ => RenderGrayscale(data, width, height, min, max)
                };
        }

        public static Bitmap RenderGrayscale(float[] data, int width, int height, float min, float max)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            float range = max - min;
            if (range < 1e-6f) range = 1;

            unsafe
            {
                byte* ptr = (byte*)dstData.Scan0;
                int stride = dstData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        float value = data[idx];
                        if (float.IsNaN(value) || float.IsInfinity(value))
                        {
                            row[x * 3] = 0;
                            row[x * 3 + 1] = 0;
                            row[x * 3 + 2] = 0;
                            continue;
                        }

                        byte gray = (byte)Math.Clamp((value - min) / range * 255f, 0, 255);
                        row[x * 3] = gray;
                        row[x * 3 + 1] = gray;
                        row[x * 3 + 2] = gray;
                    }
                    BitmapHelper.ClearRowPadding(row, width, BitmapHelper.RgbBytesPerPixel, stride);
                }
            }

            bitmap.UnlockBits(dstData);
            return bitmap;
        }

        private static Bitmap RenderVegetationPseudocolor(float[] data, int width, int height, float min, float max)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            float range = max - min;
            if (range < 1e-6f) range = 1;

            unsafe
            {
                byte* ptr = (byte*)dstData.Scan0;
                int stride = dstData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        float value = data[idx];
                        if (float.IsNaN(value) || float.IsInfinity(value))
                        {
                            row[x * 3] = 0;
                            row[x * 3 + 1] = 0;
                            row[x * 3 + 2] = 0;
                            continue;
                        }

                        float normalized = Math.Clamp((value - min) / range, 0, 1);
                        byte r, g, b;
                        if (normalized <= 0.5f)
                        {
                            float t = normalized * 2;
                            r = (byte)(60 + 195 * t);
                            g = (byte)(80 + 175 * t);
                            b = (byte)(200 + 55 * t);
                        }
                        else
                        {
                            float t = (normalized - 0.5f) * 2;
                            r = (byte)(255 - 205 * t);
                            g = (byte)(255 - 20 * t);
                            b = (byte)(255 - 200 * t);
                        }

                        row[x * 3] = b;
                        row[x * 3 + 1] = g;
                        row[x * 3 + 2] = r;
                    }
                    BitmapHelper.ClearRowPadding(row, width, BitmapHelper.RgbBytesPerPixel, stride);
                }
            }

            bitmap.UnlockBits(dstData);
            return bitmap;
        }

        private static Bitmap RenderWaterPseudocolor(float[] data, int width, int height, float min, float max)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            float range = max - min;
            if (range < 1e-6f) range = 1;

            unsafe
            {
                byte* ptr = (byte*)dstData.Scan0;
                int stride = dstData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        float value = data[idx];
                        if (float.IsNaN(value) || float.IsInfinity(value))
                        {
                            row[x * 3] = 0;
                            row[x * 3 + 1] = 0;
                            row[x * 3 + 2] = 0;
                            continue;
                        }

                        float normalized = Math.Clamp((value - min) / range, 0, 1);
                        row[x * 3] = 255;
                        row[x * 3 + 1] = (byte)(255 * (1 - normalized * 0.5f));
                        row[x * 3 + 2] = (byte)(255 * (1 - normalized));
                    }
                    BitmapHelper.ClearRowPadding(row, width, BitmapHelper.RgbBytesPerPixel, stride);
                }
            }

            bitmap.UnlockBits(dstData);
            return bitmap;
        }

        private static Bitmap RenderBuiltupPseudocolor(float[] data, int width, int height, float min, float max)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            float range = max - min;
            if (range < 1e-6f) range = 1;

            unsafe
            {
                byte* ptr = (byte*)dstData.Scan0;
                int stride = dstData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        float value = data[idx];
                        if (float.IsNaN(value) || float.IsInfinity(value))
                        {
                            row[x * 3] = 0;
                            row[x * 3 + 1] = 0;
                            row[x * 3 + 2] = 0;
                            continue;
                        }

                        float normalized = Math.Clamp((value - min) / range, 0, 1);
                        row[x * 3] = (byte)(50 * normalized);
                        row[x * 3 + 1] = (byte)(100 * normalized);
                        row[x * 3 + 2] = (byte)(255 * normalized);
                    }
                    BitmapHelper.ClearRowPadding(row, width, BitmapHelper.RgbBytesPerPixel, stride);
                }
            }

            bitmap.UnlockBits(dstData);
            return bitmap;
        }
    }

    internal enum IndexColorScheme
    {
        Vegetation,
        Water,
        Builtup
    }
}
