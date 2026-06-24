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

        public Bitmap ApplyCannyEdge(Bitmap bitmap, float lowThreshold = 50f, float highThreshold = 150f)
        {
            Bitmap prepared = Ensure24bpp(bitmap, out bool disposePrepared);
            try { return ApplyCannyEdgeCore(prepared, lowThreshold, highThreshold); }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private Bitmap ApplyCannyEdgeCore(Bitmap bitmap, float lowThreshold, float highThreshold)
        {
            int width = bitmap.Width, height = bitmap.Height;
            float[] gray = ExtractGrayscale(bitmap);
            float[] blurred = GaussianBlur5x5(gray, width, height);

            float[] magnitude = new float[width * height];
            float[] direction = new float[width * height];
            ComputeGradients(blurred, width, height, magnitude, direction);

            float[] suppressed = NonMaxSuppression(magnitude, direction, width, height);
            byte[] edges = HysteresisThreshold(suppressed, width, height, lowThreshold, highThreshold);

            Bitmap result = new(width, height, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* dstPtr = (byte*)dstData.Scan0;
                int dstStride = dstData.Stride;
                Parallel.For(0, height, y =>
                {
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < width; x++)
                    {
                        byte v = edges[y * width + x];
                        dstRow[x * 3] = v;
                        dstRow[x * 3 + 1] = v;
                        dstRow[x * 3 + 2] = v;
                    }
                    BitmapHelper.ClearRowPadding(dstRow, width, 3, dstStride);
                });
            }
            result.UnlockBits(dstData);
            return result;
        }

        private static float[] ExtractGrayscale(Bitmap bitmap)
        {
            int width = bitmap.Width, height = bitmap.Height;
            float[] gray = new float[width * height];
            var srcData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                int stride = srcData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* row = srcPtr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        byte b = row[x * 3], g = row[x * 3 + 1], r = row[x * 3 + 2];
                        gray[y * width + x] = 0.299f * r + 0.587f * g + 0.114f * b;
                    }
                }
            }
            bitmap.UnlockBits(srcData);
            return gray;
        }

        private static float[] GaussianBlur5x5(float[] src, int width, int height)
        {
            float[,] kernel =
            {
                { 1, 4, 7, 4, 1 },
                { 4, 16, 26, 16, 4 },
                { 7, 26, 41, 26, 7 },
                { 4, 16, 26, 16, 4 },
                { 1, 4, 7, 4, 1 }
            };
            const float norm = 1f / 273f;
            float[] dst = new float[width * height];
            int half = 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sum = 0;
                    for (int ky = -half; ky <= half; ky++)
                    {
                        for (int kx = -half; kx <= half; kx++)
                        {
                            int px = Math.Clamp(x + kx, 0, width - 1);
                            int py = Math.Clamp(y + ky, 0, height - 1);
                            sum += src[py * width + px] * kernel[ky + half, kx + half];
                        }
                    }
                    dst[y * width + x] = sum * norm;
                }
            }
            return dst;
        }

        private static void ComputeGradients(float[] src, int width, int height, float[] magnitude, float[] direction)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float gx = -src[Clamp(y - 1, height) * width + Clamp(x - 1, width)]
                        - 2 * src[y * width + Clamp(x - 1, width)]
                        - src[Clamp(y + 1, height) * width + Clamp(x - 1, width)]
                        + src[Clamp(y - 1, height) * width + Clamp(x + 1, width)]
                        + 2 * src[y * width + Clamp(x + 1, width)]
                        + src[Clamp(y + 1, height) * width + Clamp(x + 1, width)];
                    float gy = -src[Clamp(y - 1, height) * width + Clamp(x - 1, width)]
                        - 2 * src[Clamp(y - 1, height) * width + x]
                        - src[Clamp(y - 1, height) * width + Clamp(x + 1, width)]
                        + src[Clamp(y + 1, height) * width + Clamp(x - 1, width)]
                        + 2 * src[Clamp(y + 1, height) * width + x]
                        + src[Clamp(y + 1, height) * width + Clamp(x + 1, width)];
                    int idx = y * width + x;
                    magnitude[idx] = MathF.Sqrt(gx * gx + gy * gy);
                    direction[idx] = MathF.Atan2(gy, gx);
                }
            }

            static int Clamp(int v, int max) => Math.Clamp(v, 0, max - 1);
        }

        private static float[] NonMaxSuppression(float[] magnitude, float[] direction, int width, int height)
        {
            float[] result = new float[width * height];
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int idx = y * width + x;
                    float angle = direction[idx] * 180f / MathF.PI;
                    if (angle < 0) angle += 180;

                    float q, r;
                    if (angle < 22.5f || angle >= 157.5f)
                    {
                        q = magnitude[idx + 1];
                        r = magnitude[idx - 1];
                    }
                    else if (angle < 67.5f)
                    {
                        q = magnitude[(y + 1) * width + (x + 1)];
                        r = magnitude[(y - 1) * width + (x - 1)];
                    }
                    else if (angle < 112.5f)
                    {
                        q = magnitude[(y + 1) * width + x];
                        r = magnitude[(y - 1) * width + x];
                    }
                    else
                    {
                        q = magnitude[(y + 1) * width + (x - 1)];
                        r = magnitude[(y - 1) * width + (x + 1)];
                    }

                    result[idx] = magnitude[idx] >= q && magnitude[idx] >= r ? magnitude[idx] : 0;
                }
            }
            return result;
        }

        private static byte[] HysteresisThreshold(float[] src, int width, int height, float low, float high)
        {
            byte[] result = new byte[width * height];
            var strong = new bool[width * height];
            var weak = new bool[width * height];

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] >= high) strong[i] = true;
                else if (src[i] >= low) weak[i] = true;
            }

            var queue = new Queue<(int x, int y)>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    if (strong[idx])
                    {
                        result[idx] = 255;
                        queue.Enqueue((x, y));
                    }
                }
            }

            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + dx[d], ny = cy + dy[d];
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    int nidx = ny * width + nx;
                    if (weak[nidx] && result[nidx] == 0)
                    {
                        result[nidx] = 255;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
            return result;
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
