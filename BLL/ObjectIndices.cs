using RemoteSensingProcessor.DAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.BLL
{
    public class ObjectIndices
    {
        private readonly GdalDataAccess _dal = new();
        
        public Bitmap CalculateNDWI(ImageInfo info, int greenBandIndex, int nirBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { greenBandIndex, nirBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] ndwi = new float[info.Width * info.Height];
            for (int i = 0; i < ndwi.Length; i++)
            {
                ndwi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            
            return RenderWaterIndex(ndwi, info.Width, info.Height);
        }
        
        public Bitmap CalculateMNDWI(ImageInfo info, int greenBandIndex, int swirBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { greenBandIndex, swirBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] mndwi = new float[info.Width * info.Height];
            for (int i = 0; i < mndwi.Length; i++)
            {
                mndwi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            
            return RenderWaterIndex(mndwi, info.Width, info.Height);
        }
        
        public Bitmap CalculateNDBI(ImageInfo info, int swirBandIndex, int nirBandIndex)
        {
            float[][] bands = _dal.ReadMultiBandData(info.FilePath,
                new[] { swirBandIndex, nirBandIndex }, 0, 0, info.Width, info.Height);
            
            float[] ndbi = new float[info.Width * info.Height];
            for (int i = 0; i < ndbi.Length; i++)
            {
                ndbi[i] = (bands[0][i] - bands[1][i]) / (bands[0][i] + bands[1][i] + 1e-6f);
            }
            
            return RenderBuiltupIndex(ndbi, info.Width, info.Height);
        }
        
        private Bitmap RenderWaterIndex(float[] data, int width, int height)
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
                        float value = Math.Clamp((data[idx] + 1) / 2, 0, 1);
                        
                        byte r = (byte)(255 * (1 - value));
                        byte g = (byte)(255 * (1 - value * 0.5f));
                        byte b = (byte)(255);
                        
                        row[x * 3] = b;
                        row[x * 3 + 1] = g;
                        row[x * 3 + 2] = r;
                    }
                }
            }
            
            bitmap.UnlockBits(dstData);
            return bitmap;
        }
        
        private Bitmap RenderBuiltupIndex(float[] data, int width, int height)
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
                        float value = Math.Clamp((data[idx] + 1) / 2, 0, 1);
                        
                        byte r = (byte)(255 * value);
                        byte g = (byte)(100 * value);
                        byte b = (byte)(50 * value);
                        
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
