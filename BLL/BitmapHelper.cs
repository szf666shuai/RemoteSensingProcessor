using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RemoteSensingProcessor.BLL
{
    internal readonly record struct BitmapCapture(byte[] Pixels, int Width, int Height, int Stride);

    internal static class BitmapHelper
    {
        public const int RgbBytesPerPixel = 3;

        public static BitmapCapture Capture24bpp(Bitmap source)
        {
            Bitmap prepared = source.PixelFormat == PixelFormat.Format24bppRgb
                ? source
                : CloneAs24bppRgb(source);
            bool disposePrepared = !ReferenceEquals(prepared, source);

            try
            {
                var rect = new Rectangle(0, 0, prepared.Width, prepared.Height);
                BitmapData data = prepared.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int byteCount = Math.Abs(data.Stride) * prepared.Height;
                    byte[] pixels = new byte[byteCount];
                    Marshal.Copy(data.Scan0, pixels, 0, byteCount);
                    return new BitmapCapture(pixels, prepared.Width, prepared.Height, data.Stride);
                }
                finally
                {
                    prepared.UnlockBits(data);
                }
            }
            finally
            {
                if (disposePrepared)
                    prepared.Dispose();
            }
        }

        public static Bitmap CreateFromCapture(BitmapCapture capture)
        {
            Bitmap bitmap = new(capture.Width, capture.Height, PixelFormat.Format24bppRgb);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, capture.Width, capture.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                Marshal.Copy(capture.Pixels, 0, data.Scan0, capture.Pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            return bitmap;
        }

        public static Bitmap CloneAs24bppRgb(Bitmap source)
        {
            Bitmap result = new(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using Graphics g = Graphics.FromImage(result);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(source, 0, 0, source.Width, source.Height);
            return result;
        }

        public static unsafe void ClearRowPadding(byte* row, int width, int bytesPerPixel, int stride)
        {
            int used = width * bytesPerPixel;
            for (int i = used; i < stride; i++)
                row[i] = 0;
        }

        public static bool IsNoData(float value, double noDataValue)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return true;
            if (double.IsNaN(noDataValue))
                return false;
            return Math.Abs(value - noDataValue) < 1e-4f * Math.Max(1f, Math.Abs((float)noDataValue));
        }

        public static float Percentile(float[] sorted, float percent)
        {
            if (sorted.Length == 0) return 0;
            if (sorted.Length == 1) return sorted[0];
            percent = Math.Clamp(percent, 0f, 1f);
            float position = percent * (sorted.Length - 1);
            int lowerIndex = (int)Math.Floor(position);
            int upperIndex = (int)Math.Ceiling(position);
            if (lowerIndex == upperIndex) return sorted[lowerIndex];
            float weight = position - lowerIndex;
            return sorted[lowerIndex] * (1 - weight) + sorted[upperIndex] * weight;
        }

        public static (float min, float max) GetValidPercentileRange(float[] data, float lowerPercent = 0.02f, float upperPercent = 0.98f)
        {
            int count = 0;
            foreach (float v in data)
            {
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                    count++;
            }
            if (count == 0) return (0f, 1f);

            float[] valid = new float[count];
            int i = 0;
            foreach (float v in data)
            {
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                    valid[i++] = v;
            }
            Array.Sort(valid);
            float min = Percentile(valid, lowerPercent);
            float max = Percentile(valid, upperPercent);
            if (max <= min) max = min + 1e-6f;
            return (min, max);
        }

        public static bool IsAnyBandNoData(float[][] bands, int pixelIndex, double[] noDataValues)
        {
            for (int b = 0; b < bands.Length; b++)
            {
                if (IsNoData(bands[b][pixelIndex], noDataValues[b]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 排除元数据未标记 NoData 的填充区（如 Landsat 条带外全零像素）。
        /// </summary>
        public static bool IsInvalidBackgroundPixel(float[][] bands, int pixelIndex, double[] noDataValues)
            => IsInvalidBackgroundPixelAt(bands, pixelIndex, bands.Length, noDataValues);

        public static bool IsInvalidBackgroundPixelAt(float[][] bands, int pixelIndex, int numBands, double[] noDataValues)
        {
            if (IsAnyBandNoData(bands, pixelIndex, noDataValues))
                return true;

            float sum = 0, max = float.MinValue, min = float.MaxValue;
            for (int b = 0; b < numBands; b++)
            {
                float v = bands[b][pixelIndex];
                if (float.IsNaN(v) || float.IsInfinity(v))
                    return true;
                sum += v;
                max = Math.Max(max, v);
                min = Math.Min(min, v);
            }

            if (max <= 1e-6f)
                return true;

            if (max <= 0 && max - min <= 1e-6f)
                return true;

            if (numBands >= 3 && sum <= 1e-6f)
                return true;

            return false;
        }
    }
}
