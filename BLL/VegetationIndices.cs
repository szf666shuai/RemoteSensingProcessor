using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class VegetationIndices
    {
        private readonly GdalDataAccess _dal = new();
        
        public Bitmap CalculateNDVI(ImageInfo info, int nirBandIndex, int redBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath, 
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] ndvi = new float[info.Width * info.Height];
            for (int i = 0; i < ndvi.Length; i++)
            {
                ndvi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            
            return RenderIndex(ndvi, info.Width, info.Height, -1, 1);
        }
        
        public Bitmap CalculateEVI(ImageInfo info, int nirBandIndex, int redBandIndex, int blueBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex, blueBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] evi = new float[info.Width * info.Height];
            for (int i = 0; i < evi.Length; i++)
            {
                float denominator = bands[0][i] + 6 * bands[1][i] - 7.5f * bands[2][i] + 1;
                evi[i] = 2.5f * (bands[0][i] - bands[1][i]) / Math.Max(denominator, 1e-6f);
            }
            
            return RenderIndex(evi, info.Width, info.Height, -1, 1);
        }
        
        public Bitmap CalculateSAVI(ImageInfo info, int nirBandIndex, int redBandIndex, float L = 0.5f)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] savi = new float[info.Width * info.Height];
            for (int i = 0; i < savi.Length; i++)
            {
                savi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + L) * (1 + L);
            }
            
            return RenderIndex(savi, info.Width, info.Height, -1, 1);
        }
        
        public Bitmap CalculateMSAVI(ImageInfo info, int nirBandIndex, int redBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { nirBandIndex, redBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] msavi = new float[info.Width * info.Height];
            for (int i = 0; i < msavi.Length; i++)
            {
                float sqrtTerm = (float)Math.Sqrt(Math.Pow(2 * bands[0][i] + 1, 2) - 8 * (bands[0][i] - bands[1][i]));
                msavi[i] = 0.5f * (2 * bands[0][i] + 1 - sqrtTerm);
            }
            
            return RenderIndex(msavi, info.Width, info.Height, -1, 1);
        }
        
        private Bitmap RenderIndex(float[] data, int width, int height, float min, float max)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
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
                        float normalized = (data[idx] - min) / (max - min);
                        
                        if (normalized < 0) normalized = 0;
                        if (normalized > 1) normalized = 1;
                        
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
                }
            }
            
            bitmap.UnlockBits(dstData);
            return bitmap;
        }
    }
}
