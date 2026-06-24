using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class VegetationIndices
    {
        private readonly GdalDataAccess _dal = new();

        public Bitmap CalculateNDVI(ImageInfo info, int nirBandIndex, int redBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[] ndvi = ComputeNdviArray(info, nirBandIndex, redBandIndex);
            var (min, max) = BitmapHelper.GetValidPercentileRange(ndvi);
            return IndexRenderer.Render(ndvi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Vegetation);
        }

        public Bitmap CalculateEVI(ImageInfo info, int nirBandIndex, int redBandIndex, int blueBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex, blueBandIndex }, 0, 0, info.Width, info.Height);
            double[] noData =
            {
                info.Bands[nirBandIndex - 1].NoDataValue,
                info.Bands[redBandIndex - 1].NoDataValue,
                info.Bands[blueBandIndex - 1].NoDataValue
            };

            float[] evi = new float[info.Width * info.Height];
            for (int i = 0; i < evi.Length; i++)
            {
                if (BitmapHelper.IsAnyBandNoData(bands, i, noData))
                {
                    evi[i] = float.NaN;
                    continue;
                }
                float denominator = bands[0][i] + 6 * bands[1][i] - 7.5f * bands[2][i] + 1;
                evi[i] = 2.5f * (bands[0][i] - bands[1][i]) / Math.Max(denominator, 1e-6f);
            }

            var (min, max) = BitmapHelper.GetValidPercentileRange(evi);
            return IndexRenderer.Render(evi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Vegetation);
        }

        public Bitmap CalculateSAVI(ImageInfo info, int nirBandIndex, int redBandIndex, float L = 0.5f, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            double[] noData = { info.Bands[nirBandIndex - 1].NoDataValue, info.Bands[redBandIndex - 1].NoDataValue };

            float[] savi = new float[info.Width * info.Height];
            for (int i = 0; i < savi.Length; i++)
            {
                if (BitmapHelper.IsAnyBandNoData(bands, i, noData))
                {
                    savi[i] = float.NaN;
                    continue;
                }
                savi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + L) * (1 + L);
            }

            var (min, max) = BitmapHelper.GetValidPercentileRange(savi);
            return IndexRenderer.Render(savi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Vegetation);
        }

        public Bitmap CalculateMSAVI(ImageInfo info, int nirBandIndex, int redBandIndex, IndexDisplayMode displayMode = IndexDisplayMode.Pseudocolor)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            double[] noData = { info.Bands[nirBandIndex - 1].NoDataValue, info.Bands[redBandIndex - 1].NoDataValue };

            float[] msavi = new float[info.Width * info.Height];
            for (int i = 0; i < msavi.Length; i++)
            {
                if (BitmapHelper.IsAnyBandNoData(bands, i, noData))
                {
                    msavi[i] = float.NaN;
                    continue;
                }
                float inside = (float)(Math.Pow(2 * bands[0][i] + 1, 2) - 8 * (bands[0][i] - bands[1][i]));
                if (inside < 0)
                {
                    msavi[i] = float.NaN;
                    continue;
                }
                msavi[i] = 0.5f * (2 * bands[0][i] + 1 - (float)Math.Sqrt(inside));
            }

            var (min, max) = BitmapHelper.GetValidPercentileRange(msavi);
            return IndexRenderer.Render(msavi, info.Width, info.Height, min, max, displayMode, IndexColorScheme.Vegetation);
        }

        private float[] ComputeNdviArray(ImageInfo info, int nirBandIndex, int redBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            double[] noData = { info.Bands[nirBandIndex - 1].NoDataValue, info.Bands[redBandIndex - 1].NoDataValue };

            float[] ndvi = new float[info.Width * info.Height];
            for (int i = 0; i < ndvi.Length; i++)
            {
                if (BitmapHelper.IsAnyBandNoData(bands, i, noData))
                {
                    ndvi[i] = float.NaN;
                    continue;
                }
                ndvi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            return ndvi;
        }
    }
}
