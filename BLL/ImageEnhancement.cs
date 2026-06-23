using System.Drawing.Imaging;

namespace RemoteSensingProcessor.BLL
{
    public class ImageEnhancement
    {
        public Bitmap ApplyMeanFilter(Bitmap bitmap) =>
            ApplySeparableFilter(bitmap, new float[] { 1 / 3f, 1 / 3f, 1 / 3f });

        public Bitmap ApplyGaussianFilter(Bitmap bitmap) =>
            ApplySeparableFilter(bitmap, new float[] { 1 / 4f, 2 / 4f, 1 / 4f });

        public Bitmap ApplyLaplacianSharpening(Bitmap bitmap) =>
            ApplyConvolution(bitmap, new float[,]
            {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            });

        public Bitmap ApplySobelHorizontal(Bitmap bitmap) =>
            ApplyConvolution(bitmap, new float[,]
            {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 }
            });

        public Bitmap ApplySobelVertical(Bitmap bitmap) =>
            ApplyConvolution(bitmap, new float[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            });

        public Bitmap ApplySobelEdge(Bitmap bitmap)
        {
            Bitmap prepared = Ensure24bpp(bitmap, out bool disposePrepared);
            try { return ApplySobelEdgeCore(prepared); }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private Bitmap ApplySobelEdgeCore(Bitmap bitmap)
        {
            int width = bitmap.Width, height = bitmap.Height;
            Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride, dstStride = dstData.Stride;
                Parallel.For(0, height, y =>
                {
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < width; x++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            float gx = -GetChannel(srcPtr, srcStride, width, height, x - 1, y - 1, c) - 2 * GetChannel(srcPtr, srcStride, width, height, x - 1, y, c) - GetChannel(srcPtr, srcStride, width, height, x - 1, y + 1, c)
                                + GetChannel(srcPtr, srcStride, width, height, x + 1, y - 1, c) + 2 * GetChannel(srcPtr, srcStride, width, height, x + 1, y, c) + GetChannel(srcPtr, srcStride, width, height, x + 1, y + 1, c);
                            float gy = -GetChannel(srcPtr, srcStride, width, height, x - 1, y - 1, c) - 2 * GetChannel(srcPtr, srcStride, width, height, x, y - 1, c) - GetChannel(srcPtr, srcStride, width, height, x + 1, y - 1, c)
                                + GetChannel(srcPtr, srcStride, width, height, x - 1, y + 1, c) + 2 * GetChannel(srcPtr, srcStride, width, height, x, y + 1, c) + GetChannel(srcPtr, srcStride, width, height, x + 1, y + 1, c);
                            dstRow[x * 3 + c] = (byte)Math.Clamp(Math.Sqrt(gx * gx + gy * gy), 0, 255);
                        }
                    }
                    BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                });
            }
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        private static unsafe byte GetChannel(byte* srcPtr, int stride, int width, int height, int x, int y, int channel)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return 0;
            return ((byte*)(srcPtr + y * stride))[x * 3 + channel];
        }

        private Bitmap ApplySeparableFilter(Bitmap bitmap, float[] kernel1D)
        {
            Bitmap prepared = Ensure24bpp(bitmap, out bool disposePrepared);
            try
            {
                Bitmap horizontal = ApplyHorizontalPass(prepared, kernel1D);
                Bitmap result = ApplyVerticalPass(horizontal, kernel1D);
                horizontal.Dispose();
                return result;
            }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private Bitmap ApplyHorizontalPass(Bitmap bitmap, float[] kernel)
        {
            int width = bitmap.Width, height = bitmap.Height, half = kernel.Length / 2;
            Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride, dstStride = dstData.Stride;
                Parallel.For(0, height, y =>
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < width; x++)
                        for (int c = 0; c < 3; c++)
                        {
                            float sum = 0, weightSum = 0;
                            for (int k = -half; k <= half; k++)
                            {
                                int px = x + k;
                                if (px < 0 || px >= width) continue;
                                float w = kernel[k + half];
                                sum += srcRow[px * 3 + c] * w;
                                weightSum += w;
                            }
                            dstRow[x * 3 + c] = (byte)Math.Clamp(weightSum > 0 ? sum / weightSum : sum, 0, 255);
                        }
                    BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                });
            }
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        private Bitmap ApplyVerticalPass(Bitmap bitmap, float[] kernel)
        {
            int width = bitmap.Width, height = bitmap.Height, half = kernel.Length / 2;
            Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride, dstStride = dstData.Stride;
                Parallel.For(0, height, y =>
                {
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < width; x++)
                        for (int c = 0; c < 3; c++)
                        {
                            float sum = 0, weightSum = 0;
                            for (int k = -half; k <= half; k++)
                            {
                                int py = y + k;
                                if (py < 0 || py >= height) continue;
                                float w = kernel[k + half];
                                byte* srcRow = srcPtr + py * srcStride;
                                sum += srcRow[x * 3 + c] * w;
                                weightSum += w;
                            }
                            dstRow[x * 3 + c] = (byte)Math.Clamp(weightSum > 0 ? sum / weightSum : sum, 0, 255);
                        }
                    BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                });
            }
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        private Bitmap ApplyConvolution(Bitmap bitmap, float[,] kernel)
        {
            Bitmap prepared = Ensure24bpp(bitmap, out bool disposePrepared);
            try { return ApplyConvolutionCore(prepared, kernel); }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private Bitmap ApplyConvolutionCore(Bitmap bitmap, float[,] kernel)
        {
            int kernelSize = kernel.GetLength(0), halfKernel = kernelSize / 2;
            float kernelSum = 0;
            for (int ky = 0; ky < kernelSize; ky++)
                for (int kx = 0; kx < kernelSize; kx++)
                    kernelSum += kernel[ky, kx];
            bool preserveBrightness = Math.Abs(kernelSum - 1f) < 0.01f;
            bool zeroCentered = Math.Abs(kernelSum) < 0.01f;
            int width = bitmap.Width, height = bitmap.Height;
            Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride, dstStride = dstData.Stride;
                Parallel.For(0, height, y =>
                {
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < width; x++)
                        for (int c = 0; c < 3; c++)
                        {
                            float sum = 0, weightSum = 0;
                            for (int ky = -halfKernel; ky <= halfKernel; ky++)
                                for (int kx = -halfKernel; kx <= halfKernel; kx++)
                                {
                                    int py = y + ky, px = x + kx;
                                    if (py >= 0 && py < height && px >= 0 && px < width)
                                    {
                                        float weight = kernel[ky + halfKernel, kx + halfKernel];
                                        byte* srcRow = srcPtr + py * srcStride;
                                        sum += srcRow[px * 3 + c] * weight;
                                        weightSum += weight;
                                    }
                                }
                            float output = preserveBrightness && weightSum > 0 && Math.Abs(weightSum - 1f) > 0.01f ? sum / weightSum
                                : zeroCentered ? sum + 128f : sum;
                            dstRow[x * 3 + c] = (byte)Math.Clamp(output, 0, 255);
                        }
                    BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                });
            }
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        public Bitmap DensitySlice(Bitmap bitmap, int numClasses)
        {
            Bitmap prepared = Ensure24bpp(bitmap, out bool disposePrepared);
            try
            {
                int width = prepared.Width, height = prepared.Height;
                Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
                var srcData = prepared.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                int classSize = Math.Max(1, 256 / numClasses);
                Color[] colors = GenerateClassColors(numClasses);
                unsafe
                {
                    byte* srcPtr = (byte*)srcData.Scan0;
                    byte* dstPtr = (byte*)dstData.Scan0;
                    int srcStride = srcData.Stride, dstStride = dstData.Stride;
                    Parallel.For(0, height, y =>
                    {
                        byte* srcRow = srcPtr + y * srcStride;
                        byte* dstRow = dstPtr + y * dstStride;
                        for (int x = 0; x < width; x++)
                        {
                            byte value = (byte)((srcRow[x * 3] + srcRow[x * 3 + 1] + srcRow[x * 3 + 2]) / 3);
                            int classIdx = Math.Min(value / classSize, numClasses - 1);
                            Color color = colors[classIdx];
                            dstRow[x * 3] = color.B;
                            dstRow[x * 3 + 1] = color.G;
                            dstRow[x * 3 + 2] = color.R;
                        }
                        BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                    });
                }
                prepared.UnlockBits(srcData);
                result.UnlockBits(dstData);
                return result;
            }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private static Bitmap Ensure24bpp(Bitmap bitmap, out bool shouldDispose)
        {
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) { shouldDispose = false; return bitmap; }
            shouldDispose = true;
            return BitmapHelper.CloneAs24bppRgb(bitmap);
        }

        private Color[] GenerateClassColors(int numClasses)
        {
            Color[] colors = new Color[numClasses];
            if (numClasses >= 1) colors[0] = Color.Blue;
            if (numClasses >= 2) colors[1] = Color.Cyan;
            if (numClasses >= 3) colors[2] = Color.Green;
            if (numClasses >= 4) colors[3] = Color.Yellow;
            if (numClasses >= 5) colors[4] = Color.Orange;
            if (numClasses >= 6) colors[5] = Color.Red;
            for (int i = 6; i < numClasses; i++)
                colors[i] = ColorFromHsv(i / (float)numClasses * 360, 0.8f, 0.8f);
            return colors;
        }

        private Color ColorFromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value *= 255;
            int v = Convert.ToInt32(value), p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
            return hi switch
            {
                0 => Color.FromArgb(v, t, p), 1 => Color.FromArgb(q, v, p), 2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v), 4 => Color.FromArgb(t, p, v), _ => Color.FromArgb(v, p, q)
            };
        }
    }
}
