using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class Classification
    {
        private readonly GdalDataAccess _dal = new();

        public (Bitmap result, int[] classes) KMeansClassification(ImageInfo info, int numClusters = 4)
        {
            int[] bandIndices = Enumerable.Range(1, info.BandCount).ToArray();
            int scaleFactor = info.Width * info.Height > 10000000 ? 4 : info.Width * info.Height > 5000000 ? 2 : 1;
            int sampleWidth = info.Width / scaleFactor;
            int sampleHeight = info.Height / scaleFactor;
            float[][] bands = _dal.ReadMultiBandData(info.FilePath, bandIndices, 0, 0, sampleWidth, sampleHeight);
            int numPixels = sampleWidth * sampleHeight;
            int numBands = info.BandCount;

            bool[] validMask = new bool[numPixels];
            int validCount = 0;
            for (int i = 0; i < numPixels; i++)
            {
                validMask[i] = IsValidPixel(bands, i, info, numBands);
                if (validMask[i]) validCount++;
            }
            if (validCount < numClusters)
                throw new InvalidOperationException("有效像素不足，无法进行分类");

            float[][] validPixels = new float[validCount][];
            int vi = 0;
            for (int i = 0; i < numPixels; i++)
            {
                if (!validMask[i]) continue;
                validPixels[vi] = new float[numBands];
                for (int j = 0; j < numBands; j++) validPixels[vi][j] = bands[j][i];
                vi++;
            }

            float[][] normalizedPixels = MinMaxNormalize(validPixels, validCount, numBands);
            Random random = new();
            float[][] centroids = InitializeCentroids(normalizedPixels, numClusters, numBands, random);
            int[] validClasses = new int[validCount];

            for (int iter = 0; iter < 50; iter++)
            {
                AssignClasses(normalizedPixels, centroids, validClasses);
                float[][] newCentroids = UpdateCentroids(normalizedPixels, validClasses, numClusters, numBands, centroids, random);
                if (CentroidsConverged(centroids, newCentroids)) break;
                centroids = newCentroids;
            }

            int[] sampleClasses = new int[numPixels];
            vi = 0;
            for (int i = 0; i < numPixels; i++)
                sampleClasses[i] = validMask[i] ? validClasses[vi++] : -1;

            int[] fullSizeClasses = ScaleUpClasses(sampleClasses, sampleWidth, sampleHeight, info.Width, info.Height);
            return (RenderClassification(fullSizeClasses, info.Width, info.Height), fullSizeClasses);
        }

        private bool IsValidPixel(float[][] bands, int idx, ImageInfo info, int numBands)
        {
            for (int b = 0; b < numBands; b++)
                if (BitmapHelper.IsNoData(bands[b][idx], info.Bands[b].NoDataValue)) return false;
            return true;
        }

        private float[][] MinMaxNormalize(float[][] pixels, int numPixels, int numBands)
        {
            float[] mins = new float[numBands], maxs = new float[numBands];
            for (int b = 0; b < numBands; b++) { mins[b] = float.MaxValue; maxs[b] = float.MinValue; }
            for (int i = 0; i < numPixels; i++)
                for (int b = 0; b < numBands; b++)
                { mins[b] = Math.Min(mins[b], pixels[i][b]); maxs[b] = Math.Max(maxs[b], pixels[i][b]); }
            float[][] result = new float[numPixels][];
            for (int i = 0; i < numPixels; i++)
            {
                result[i] = new float[numBands];
                for (int b = 0; b < numBands; b++)
                {
                    float range = maxs[b] - mins[b];
                    if (range < 1e-6f) range = 1;
                    result[i][b] = (pixels[i][b] - mins[b]) / range;
                }
            }
            return result;
        }

        private float[][] InitializeCentroids(float[][] pixels, int numClusters, int numBands, Random random)
        {
            float[][] centroids = new float[numClusters][];
            HashSet<int> used = new();
            for (int i = 0; i < numClusters; i++)
            {
                int idx;
                do idx = random.Next(pixels.Length); while (used.Contains(idx));
                used.Add(idx);
                centroids[i] = (float[])pixels[idx].Clone();
            }
            return centroids;
        }

        private void AssignClasses(float[][] pixels, float[][] centroids, int[] classes)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                float minDist = float.MaxValue;
                int bestClass = 0;
                for (int c = 0; c < centroids.Length; c++)
                {
                    float dist = 0;
                    for (int b = 0; b < pixels[i].Length; b++)
                    {
                        float diff = pixels[i][b] - centroids[c][b];
                        dist += diff * diff;
                    }
                    if (!float.IsNaN(dist) && dist < minDist) { minDist = dist; bestClass = c; }
                }
                classes[i] = bestClass;
            }
        }

        private float[][] UpdateCentroids(float[][] pixels, int[] classes, int numClusters, int numBands, float[][] oldCentroids, Random random)
        {
            float[][] centroids = new float[numClusters][];
            int[] counts = new int[numClusters];
            for (int i = 0; i < numClusters; i++) centroids[i] = new float[numBands];
            for (int i = 0; i < pixels.Length; i++)
            {
                int c = classes[i];
                counts[c]++;
                for (int b = 0; b < numBands; b++) centroids[c][b] += pixels[i][b];
            }
            for (int c = 0; c < numClusters; c++)
            {
                if (counts[c] > 0)
                    for (int b = 0; b < numBands; b++) centroids[c][b] /= counts[c];
                else
                    centroids[c] = (float[])pixels[random.Next(pixels.Length)].Clone();
            }
            return centroids;
        }

        private bool CentroidsConverged(float[][] oldCentroids, float[][] newCentroids)
        {
            for (int i = 0; i < oldCentroids.Length; i++)
                for (int j = 0; j < oldCentroids[i].Length; j++)
                    if (Math.Abs(oldCentroids[i][j] - newCentroids[i][j]) > 0.001f) return false;
            return true;
        }

        private int[] ScaleUpClasses(int[] classes, int sampleWidth, int sampleHeight, int fullWidth, int fullHeight)
        {
            int[] fullSizeClasses = new int[fullWidth * fullHeight];
            for (int y = 0; y < fullHeight; y++)
                for (int x = 0; x < fullWidth; x++)
                {
                    int srcX = Math.Min(x * sampleWidth / fullWidth, sampleWidth - 1);
                    int srcY = Math.Min(y * sampleHeight / fullHeight, sampleHeight - 1);
                    fullSizeClasses[y * fullWidth + x] = classes[srcY * sampleWidth + srcX];
                }
            return fullSizeClasses;
        }

        private Bitmap RenderClassification(int[] classes, int width, int height)
        {
            Color[] classColors = { Color.Blue, Color.Green, Color.Red, Color.Yellow };
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
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
                        if (classes[idx] < 0) { row[x * 3] = 0; row[x * 3 + 1] = 0; row[x * 3 + 2] = 0; continue; }
                        Color color = classColors[classes[idx] % classColors.Length];
                        row[x * 3] = color.B; row[x * 3 + 1] = color.G; row[x * 3 + 2] = color.R;
                    }
                }
            }
            bitmap.UnlockBits(dstData);
            return bitmap;
        }
    }
}
