using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;
using System.Drawing.Imaging;

namespace RemoteSensingProcessor.BLL
{
    public class BasicOperations
    {
        private readonly GdalDataAccess _dal = new();
        
        public Bitmap RenderTrueColor(ImageInfo info)
        {
            return RenderBandCombination(info, new[] { 3, 2, 1 });
        }
        
        public Bitmap RenderFalseColor(ImageInfo info)
        {
            return RenderBandCombination(info, new[] { 4, 3, 2 });
        }
        
        public Bitmap RenderBandCombination(ImageInfo info, int[] bandIndices)
        {
            if (bandIndices.Length != 3)
                throw new ArgumentException("波段组合必须包含3个波段");
            
            foreach (int idx in bandIndices)
            {
                if (idx < 1 || idx > info.BandCount)
                    throw new ArgumentOutOfRangeException(nameof(bandIndices), $"波段索引 {idx} 超出范围 [1, {info.BandCount}]");
            }
            
            return CreateColorBitmapWithBlockRead(info, bandIndices);
        }
        
        public Bitmap RenderGrayscale(ImageInfo info, int bandIndex)
        {
            if (bandIndex < 1 || bandIndex > info.BandCount)
                throw new ArgumentOutOfRangeException(nameof(bandIndex), $"波段索引 {bandIndex} 超出范围 [1, {info.BandCount}]");
            
            return CreateGrayscaleBitmapWithBlockRead(info, bandIndex);
        }
        
        private Bitmap CreateColorBitmap(float[][] bands, int width, int height)
        {
            Bitmap bitmap = new(width, height, PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), 
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        row[x * 3] = (byte)Math.Clamp(bands[2][idx], 0, 255);
                        row[x * 3 + 1] = (byte)Math.Clamp(bands[1][idx], 0, 255);
                        row[x * 3 + 2] = (byte)Math.Clamp(bands[0][idx], 0, 255);
                    }
                }
            }
            
            bitmap.UnlockBits(data);
            return bitmap;
        }
        
        private Bitmap CreateGrayscaleBitmap(float[] band, int width, int height)
        {
            Bitmap bitmap = new(width, height, PixelFormat.Format8bppIndexed);
            
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            bitmap.Palette = palette;
            
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                
                for (int y = 0; y < height; y++)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        row[x] = (byte)Math.Clamp(band[idx], 0, 255);
                    }
                }
            }
            
            bitmap.UnlockBits(data);
            return bitmap;
        }
        
        private Bitmap CreateColorBitmapWithBlockRead(ImageInfo info, int[] bandIndices)
        {
            Bitmap bitmap = new(info.Width, info.Height, PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, info.Width, info.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            int blockSize = 256;
            
            // 获取每个波段的范围用于归一化
            float[][] bandRanges = new float[3][];
            for (int i = 0; i < 3; i++)
            {
                int idx = bandIndices[i] - 1;
                bandRanges[i] = new float[] { (float)info.Bands[idx].MinValue, (float)info.Bands[idx].MaxValue };
                float range = bandRanges[i][1] - bandRanges[i][0];
                if (range == 0) range = 1;
                bandRanges[i][0] = range;
            }
            
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                
                for (int y = 0; y < info.Height; y += blockSize)
                {
                    int rows = Math.Min(blockSize, info.Height - y);
                    float[][] bands = _dal.ReadMultiBandData(info.FilePath, bandIndices, 0, y, info.Width, rows);
                    
                    for (int blockY = 0; blockY < rows; blockY++)
                    {
                        byte* row = ptr + (y + blockY) * stride;
                        for (int x = 0; x < info.Width; x++)
                        {
                            int idx = blockY * info.Width + x;
                            if (BitmapHelper.IsNoData(bands[0][idx], info.Bands[bandIndices[0] - 1].NoDataValue) ||
                                BitmapHelper.IsNoData(bands[1][idx], info.Bands[bandIndices[1] - 1].NoDataValue) ||
                                BitmapHelper.IsNoData(bands[2][idx], info.Bands[bandIndices[2] - 1].NoDataValue))
                            {
                                row[x * 3] = 0; row[x * 3 + 1] = 0; row[x * 3 + 2] = 0;
                                continue;
                            }
                            float minR = (float)info.Bands[bandIndices[0] - 1].MinValue;
                            float minG = (float)info.Bands[bandIndices[1] - 1].MinValue;
                            float minB = (float)info.Bands[bandIndices[2] - 1].MinValue;
                            float r = (bands[0][idx] - minR) / bandRanges[0][0];
                            float g = (bands[1][idx] - minG) / bandRanges[1][0];
                            float b = (bands[2][idx] - minB) / bandRanges[2][0];
                            
                            row[x * 3] = (byte)Math.Clamp(b * 255, 0, 255);
                            row[x * 3 + 1] = (byte)Math.Clamp(g * 255, 0, 255);
                            row[x * 3 + 2] = (byte)Math.Clamp(r * 255, 0, 255);
                        }
                    }
                }
            }
            
            bitmap.UnlockBits(data);
            return bitmap;
        }
        
        private Bitmap CreateGrayscaleBitmapWithBlockRead(ImageInfo info, int bandIndex)
        {
            Bitmap bitmap = new(info.Width, info.Height, PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, info.Width, info.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int blockSize = 256;
            int bandIdx = bandIndex - 1;
            float minVal = (float)info.Bands[bandIdx].MinValue;
            float maxVal = (float)info.Bands[bandIdx].MaxValue;
            float range = maxVal - minVal;
            if (range == 0) range = 1;

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;

                for (int y = 0; y < info.Height; y += blockSize)
                {
                    int rows = Math.Min(blockSize, info.Height - y);
                    float[] band = _dal.ReadBandData(info.FilePath, bandIndex, 0, y, info.Width, rows);

                    for (int blockY = 0; blockY < rows; blockY++)
                    {
                        byte* row = ptr + (y + blockY) * stride;
                        for (int x = 0; x < info.Width; x++)
                        {
                            int idx = blockY * info.Width + x;
                            byte gray;
                            if (BitmapHelper.IsNoData(band[idx], info.Bands[bandIdx].NoDataValue))
                                gray = 0;
                            else
                            {
                                float normalized = (band[idx] - minVal) / range;
                                gray = (byte)Math.Clamp(normalized * 255, 0, 255);
                            }
                            row[x * 3] = gray;
                            row[x * 3 + 1] = gray;
                            row[x * 3 + 2] = gray;
                        }
                        BitmapHelper.ClearRowPadding(row, info.Width, BitmapHelper.RgbBytesPerPixel, stride);
                    }
                }
            }

            bitmap.UnlockBits(data);
            return bitmap;
        }
        
        public Bitmap ApplyLinearStretchFromImage(ImageInfo info, float percent = 0.02f)
        {
            if (info.BandCount >= 3)
                return ApplyLinearStretchFromBands(info, new[] { 3, 2, 1 }, percent);
            return ApplyLinearStretchFromGrayscale(info, 1, percent);
        }

        private Bitmap ApplyLinearStretchFromBands(ImageInfo info, int[] bandIndices, float percent)
        {
            float[] lowers = new float[3], uppers = new float[3];
            for (int i = 0; i < 3; i++)
                ComputeBandPercentiles(info, bandIndices[i], percent, out lowers[i], out uppers[i]);
            return RenderColorWithLinearStretch(info, bandIndices, lowers, uppers);
        }

        private Bitmap ApplyLinearStretchFromGrayscale(ImageInfo info, int bandIndex, float percent)
        {
            ComputeBandPercentiles(info, bandIndex, percent, out float lower, out float upper);
            return RenderGrayscaleWithLinearStretch(info, bandIndex, lower, upper);
        }

        private void ComputeBandPercentiles(ImageInfo info, int bandIndex, float percent, out float lower, out float upper)
        {
            int bandIdx = bandIndex - 1;
            int totalPixels = info.Width * info.Height;
            int step = Math.Max(1, totalPixels / Math.Min(300000, totalPixels));
            var samples = new List<float>();
            for (int y = 0; y < info.Height; y += 256)
            {
                int rows = Math.Min(256, info.Height - y);
                float[] band = _dal.ReadBandData(info.FilePath, bandIndex, 0, y, info.Width, rows);
                for (int blockY = 0; blockY < rows; blockY++)
                    for (int x = 0; x < info.Width; x += step)
                    {
                        float value = band[blockY * info.Width + x];
                        if (!BitmapHelper.IsNoData(value, info.Bands[bandIdx].NoDataValue))
                            samples.Add(value);
                    }
            }
            if (samples.Count == 0)
            {
                lower = (float)info.Bands[bandIdx].MinValue;
                upper = (float)info.Bands[bandIdx].MaxValue;
            }
            else
            {
                samples.Sort();
                float[] sorted = samples.ToArray();
                lower = BitmapHelper.Percentile(sorted, percent);
                upper = BitmapHelper.Percentile(sorted, 1f - percent);
            }
            if (upper - lower < 1e-6f) { lower = (float)info.Bands[bandIdx].MinValue; upper = (float)info.Bands[bandIdx].MaxValue; }
            if (upper - lower < 1e-6f) upper = lower + 1f;
        }

        private Bitmap RenderColorWithLinearStretch(ImageInfo info, int[] bandIndices, float[] lowers, float[] uppers)
        {
            Bitmap bitmap = new(info.Width, info.Height, PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, info.Width, info.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            float[] ranges = { Math.Max(uppers[0] - lowers[0], 1e-6f), Math.Max(uppers[1] - lowers[1], 1e-6f), Math.Max(uppers[2] - lowers[2], 1e-6f) };
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                for (int y = 0; y < info.Height; y += 256)
                {
                    int rows = Math.Min(256, info.Height - y);
                    float[][] bands = _dal.ReadMultiBandData(info.FilePath, bandIndices, 0, y, info.Width, rows);
                    for (int blockY = 0; blockY < rows; blockY++)
                    {
                        byte* row = ptr + (y + blockY) * stride;
                        for (int x = 0; x < info.Width; x++)
                        {
                            int idx = blockY * info.Width + x;
                            if (BitmapHelper.IsNoData(bands[0][idx], info.Bands[bandIndices[0] - 1].NoDataValue) ||
                                BitmapHelper.IsNoData(bands[1][idx], info.Bands[bandIndices[1] - 1].NoDataValue) ||
                                BitmapHelper.IsNoData(bands[2][idx], info.Bands[bandIndices[2] - 1].NoDataValue))
                            { row[x * 3] = 0; row[x * 3 + 1] = 0; row[x * 3 + 2] = 0; continue; }
                            row[x * 3] = (byte)Math.Clamp((bands[2][idx] - lowers[2]) / ranges[2] * 255, 0, 255);
                            row[x * 3 + 1] = (byte)Math.Clamp((bands[1][idx] - lowers[1]) / ranges[1] * 255, 0, 255);
                            row[x * 3 + 2] = (byte)Math.Clamp((bands[0][idx] - lowers[0]) / ranges[0] * 255, 0, 255);
                        }
                    }
                }
            }
            bitmap.UnlockBits(data);
            return bitmap;
        }

        private Bitmap RenderGrayscaleWithLinearStretch(ImageInfo info, int bandIndex, float lower, float upper)
        {
            Bitmap bitmap = new(info.Width, info.Height, PixelFormat.Format24bppRgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, info.Width, info.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int bandIdx = bandIndex - 1;
            float range = Math.Max(upper - lower, 1e-6f);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                for (int y = 0; y < info.Height; y += 256)
                {
                    int rows = Math.Min(256, info.Height - y);
                    float[] band = _dal.ReadBandData(info.FilePath, bandIndex, 0, y, info.Width, rows);
                    for (int blockY = 0; blockY < rows; blockY++)
                    {
                        byte* row = ptr + (y + blockY) * stride;
                        for (int x = 0; x < info.Width; x++)
                        {
                            int idx = blockY * info.Width + x;
                            if (BitmapHelper.IsNoData(band[idx], info.Bands[bandIdx].NoDataValue))
                            { row[x * 3] = 0; row[x * 3 + 1] = 0; row[x * 3 + 2] = 0; continue; }
                            byte gray = (byte)Math.Clamp((band[idx] - lower) / range * 255, 0, 255);
                            row[x * 3] = gray; row[x * 3 + 1] = gray; row[x * 3 + 2] = gray;
                        }
                    }
                }
            }
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public Bitmap ApplyLinearStretch(Bitmap bitmap, float percent = 0.02f)
        {
            byte[][] luts = CalculateChannelLuts(bitmap, percent);
            return StretchBitmapWithLuts(bitmap, luts);
        }

        private byte[][] CalculateChannelLuts(Bitmap bitmap, float percent)
        {
            int[][] histograms = { new int[256], new int[256], new int[256] };
            int totalPixels = bitmap.Width * bitmap.Height;
            int sampleStep = totalPixels > 10000000 ? 10 : totalPixels > 5000000 ? 5 : 1;
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                for (int y = 0; y < bitmap.Height; y += sampleStep)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < bitmap.Width; x += sampleStep)
                    {
                        histograms[0][row[x * 3]]++;
                        histograms[1][row[x * 3 + 1]]++;
                        histograms[2][row[x * 3 + 2]]++;
                    }
                }
            }
            bitmap.UnlockBits(data);
            return new[] { BuildLut(histograms[0], percent), BuildLut(histograms[1], percent), BuildLut(histograms[2], percent) };
        }

        private static byte[] BuildLut(int[] histogram, float percent)
        {
            int total = histogram.Sum();
            if (total == 0) return Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            int lower = 0, upper = 255, accumulated = 0;
            for (int i = 0; i < 256; i++) { accumulated += histogram[i]; if ((float)accumulated / total >= percent) { lower = i; break; } }
            accumulated = 0;
            for (int i = 255; i >= 0; i--) { accumulated += histogram[i]; if ((float)accumulated / total >= percent) { upper = i; break; } }
            if (upper <= lower) upper = Math.Min(lower + 1, 255);
            int range = upper - lower;
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++) lut[i] = (byte)Math.Clamp((i - lower) * 255f / range, 0, 255);
            return lut;
        }

        private Bitmap StretchBitmapWithLuts(Bitmap bitmap, byte[][] luts)
        {
            Bitmap prepared = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? bitmap : BitmapHelper.CloneAs24bppRgb(bitmap);
            bool disposePrepared = !ReferenceEquals(prepared, bitmap);
            try
            {
                Bitmap result = new(prepared.Width, prepared.Height, PixelFormat.Format24bppRgb);
                var srcData = prepared.LockBits(new Rectangle(0, 0, prepared.Width, prepared.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var dstData = result.LockBits(new Rectangle(0, 0, prepared.Width, prepared.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* srcPtr = (byte*)srcData.Scan0;
                    byte* dstPtr = (byte*)dstData.Scan0;
                    int srcStride = srcData.Stride, dstStride = dstData.Stride;
                    for (int y = 0; y < prepared.Height; y++)
                    {
                        byte* srcRow = srcPtr + y * srcStride;
                        byte* dstRow = dstPtr + y * dstStride;
                        for (int x = 0; x < prepared.Width; x++)
                        {
                            dstRow[x * 3] = luts[0][srcRow[x * 3]];
                            dstRow[x * 3 + 1] = luts[1][srcRow[x * 3 + 1]];
                            dstRow[x * 3 + 2] = luts[2][srcRow[x * 3 + 2]];
                        }
                        BitmapHelper.ClearRowPadding(dstRow, prepared.Width, 3, dstStride);
                    }
                }
                prepared.UnlockBits(srcData);
                result.UnlockBits(dstData);
                return result;
            }
            finally { if (disposePrepared) prepared.Dispose(); }
        }

        private int[] CalculateHistogram(Bitmap bitmap)
        {
            int[] histogram = new int[256];
            int totalPixels = bitmap.Width * bitmap.Height;
            
            // 对大影像进行采样统计，避免卡死
            int sampleStep = 1;
            if (totalPixels > 10000000) // 超过1000万像素，采样10%
            {
                sampleStep = 10;
            }
            else if (totalPixels > 5000000) // 超过500万像素，采样20%
            {
                sampleStep = 5;
            }
            
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;
                int pixelSize = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1;
                
                for (int y = 0; y < bitmap.Height; y += sampleStep)
                {
                    byte* row = ptr + y * stride;
                    for (int x = 0; x < bitmap.Width; x += sampleStep)
                    {
                        byte value = pixelSize == 3 ? (byte)((row[x * 3] + row[x * 3 + 1] + row[x * 3 + 2]) / 3) : row[x];
                        histogram[value]++;
                    }
                }
            }
            
            bitmap.UnlockBits(data);
            return histogram;
        }
        
        private Bitmap StretchBitmap(Bitmap bitmap, int lower, int upper)
        {
            Bitmap result = new(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = result.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int range = Math.Max(upper - lower, 1);
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++) lut[i] = (byte)Math.Clamp((i - lower) * 255f / range, 0, 255);
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride, dstStride = dstData.Stride;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < bitmap.Width; x++)
                        for (int c = 0; c < 3; c++)
                            dstRow[x * 3 + c] = lut[srcRow[x * 3 + c]];
                    BitmapHelper.ClearRowPadding(dstRow, bitmap.Width, 3, dstStride);
                }
            }
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
        
        public Bitmap Grayscale(Bitmap bitmap)
        {
            Bitmap result = new(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
            
            ColorPalette palette = result.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            result.Palette = palette;
            
            var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, result.PixelFormat);
            
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte gray = (byte)(0.299 * srcRow[x * 3] + 0.587 * srcRow[x * 3 + 1] + 0.114 * srcRow[x * 3 + 2]);
                        dstRow[x] = gray;
                    }
                }
            }
            
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
        
        public Bitmap Invert(Bitmap bitmap)
        {
            Bitmap result = new(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                int pixelSize = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1;
                
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int c = 0; c < pixelSize; c++)
                        {
                            dstRow[x * pixelSize + c] = (byte)(255 - srcRow[x * pixelSize + c]);
                        }
                    }
                }
            }
            
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
        
        public Bitmap AdjustBrightnessContrast(Bitmap bitmap, int brightness, int contrast)
        {
            float b = brightness / 255f;
            float c = (contrast + 100) / 100f;
            
            Bitmap result = new(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, result.PixelFormat);
            
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                int pixelSize = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1;
                
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* srcRow = srcPtr + y * srcStride;
                    byte* dstRow = dstPtr + y * dstStride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int ch = 0; ch < pixelSize; ch++)
                        {
                            float value = srcRow[x * pixelSize + ch] / 255f;
                            value = (value - 0.5f) * c + 0.5f + b;
                            dstRow[x * pixelSize + ch] = (byte)Math.Clamp(value * 255, 0, 255);
                        }
                    }
                }
            }
            
            bitmap.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }
    }
}
