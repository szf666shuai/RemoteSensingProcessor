using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class ObjectIndices
    {
        private readonly GdalDataAccess _dal = new();

        public Bitmap CalculateNDWI(ImageInfo info, int greenBandIndex, int nirBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[] ndwi = ComputeRatioIndex(info, greenBandIndex, nirBandIndex);
            var (min, max) = BitmapHelper.GetValidPercentileRange(ndwi);
            return IndexRenderer.Render(ndwi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Water);
        }

        public Bitmap CalculateMNDWI(ImageInfo info, int greenBandIndex, int swirBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[] mndwi = ComputeRatioIndex(info, greenBandIndex, swirBandIndex);
            var (min, max) = BitmapHelper.GetValidPercentileRange(mndwi);
            return IndexRenderer.Render(mndwi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Water);
        }

        public Bitmap CalculateNDBI(ImageInfo info, int swirBandIndex, int nirBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[] ndbi = ComputeRatioIndex(info, swirBandIndex, nirBandIndex);
            var (min, max) = BitmapHelper.GetValidPercentileRange(ndbi);
            return IndexRenderer.Render(ndbi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Builtup);
        }

        private float[] ComputeRatioIndex(ImageInfo info, int bandA, int bandB)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { bandA, bandB }, 0, 0, info.Width, info.Height);
            double[] noData = { info.Bands[bandA - 1].NoDataValue, info.Bands[bandB - 1].NoDataValue };

            float[] result = new float[info.Width * info.Height];
            for (int i = 0; i < result.Length; i++)
            {
                if (BitmapHelper.IsAnyBandNoData(bands, i, noData))
                {
                    result[i] = float.NaN;
                    continue;
                }
                result[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            return result;
        }
    }
}
