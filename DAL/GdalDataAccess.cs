using OSGeo.GDAL;
using RemoteSensingProcessor.Model;

namespace RemoteSensingProcessor.DAL
{
    public class GdalDataAccess
    {
        public ImageInfo OpenImage(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("影像文件不存在", filePath);

            using Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly);
            if (dataset == null)
            {
                string lastError = Gdal.GetLastErrorMsg();
                throw new Exception($"无法打开影像文件: {lastError}");
            }

            ImageInfo info = new()
            {
                Width = dataset.RasterXSize,
                Height = dataset.RasterYSize,
                BandCount = dataset.RasterCount,
                Projection = dataset.GetProjection(),
                FilePath = filePath,
                Bands = new RasterBand[dataset.RasterCount]
            };

            double[] transform = new double[6];
            dataset.GetGeoTransform(transform);
            info.GeoTransform = GeoTransform.FromArray(transform);

            for (int i = 0; i < dataset.RasterCount; i++)
            {
                using Band band = dataset.GetRasterBand(i + 1);
                info.Bands[i] = new RasterBand
                {
                    Index = i + 1,
                    Name = string.IsNullOrEmpty(band.GetDescription()) ? $"Band {i + 1}" : band.GetDescription(),
                    DataType = band.DataType
                };
                
                double noDataValue = 0;
                int hasNoData = 0;
                try
                {
                    band.GetNoDataValue(out noDataValue, out hasNoData);
                    info.Bands[i].NoDataValue = hasNoData != 0 ? noDataValue : double.NaN;
                }
                catch
                {
                    info.Bands[i].NoDataValue = double.NaN;
                }
                
                double[] minMax = new double[2];
                try
                {
                    band.ComputeRasterMinMax(minMax, 0);
                    info.Bands[i].MinValue = minMax[0];
                    info.Bands[i].MaxValue = minMax[1];
                }
                catch
                {
                    info.Bands[i].MinValue = 0;
                    info.Bands[i].MaxValue = 255;
                }
                
                try
                {
                    info.Bands[i].Histogram = ComputeHistogram(band);
                }
                catch
                {
                    info.Bands[i].Histogram = new int[256];
                }
            }

            info.DataType = dataset.GetRasterBand(1).DataType;
            return info;
        }
        
        private int[] ComputeHistogram(Band band)
        {
            int[] histogram = new int[256];
            int width = band.XSize;
            int height = band.YSize;
            int blockSize = 256;
            
            for (int y = 0; y < height; y += blockSize)
            {
                int rows = Math.Min(blockSize, height - y);
                float[] buffer = new float[width * rows];
                band.ReadRaster(0, y, width, rows, buffer, width, rows, 0, 0);
                
                for (int i = 0; i < buffer.Length; i++)
                {
                    int value = (int)Math.Clamp(buffer[i], 0, 255);
                    histogram[value]++;
                }
            }
            
            return histogram;
        }

        public float[] ReadBandData(string filePath, int bandIndex, int startX, int startY, int width, int height)
        {
            using Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly);
            using Band band = dataset.GetRasterBand(bandIndex);
            
            float[] buffer = new float[width * height];
            band.ReadRaster(startX, startY, width, height, buffer, width, height, 0, 0);
            return buffer;
        }
        
        public float[][] ReadMultiBandData(string filePath, int[] bandIndices, int startX, int startY, int width, int height)
        {
            float[][] result = new float[bandIndices.Length][];
            
            using Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly);
            for (int i = 0; i < bandIndices.Length; i++)
            {
                using Band band = dataset.GetRasterBand(bandIndices[i]);
                result[i] = new float[width * height];
                band.ReadRaster(startX, startY, width, height, result[i], width, height, 0, 0);
            }
            
            return result;
        }

        /// <summary>
        /// 按步长在全图范围内均匀采样，避免只读取左上角导致分类结果缺失。
        /// </summary>
        public float[][] ReadMultiBandDataSubsampled(string filePath, int[] bandIndices, int fullWidth, int fullHeight, int scaleFactor)
        {
            int sampleWidth = (fullWidth + scaleFactor - 1) / scaleFactor;
            int sampleHeight = (fullHeight + scaleFactor - 1) / scaleFactor;
            float[][] result = new float[bandIndices.Length][];
            for (int i = 0; i < bandIndices.Length; i++)
                result[i] = new float[sampleWidth * sampleHeight];

            using Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly);
            float[] rowBuffer = new float[fullWidth];

            for (int i = 0; i < bandIndices.Length; i++)
            {
                using Band band = dataset.GetRasterBand(bandIndices[i]);
                for (int sy = 0; sy < sampleHeight; sy++)
                {
                    int y = Math.Min(sy * scaleFactor, fullHeight - 1);
                    band.ReadRaster(0, y, fullWidth, 1, rowBuffer, fullWidth, 1, 0, 0);
                    int rowOffset = sy * sampleWidth;
                    for (int sx = 0; sx < sampleWidth; sx++)
                    {
                        int x = Math.Min(sx * scaleFactor, fullWidth - 1);
                        result[i][rowOffset + sx] = rowBuffer[x];
                    }
                }
            }

            return result;
        }
        
        public void SaveImage(string filePath, float[] data, int width, int height, GeoTransform transform, string projection)
        {
            Driver driver = Gdal.GetDriverByName("GTiff");
            using Dataset dataset = driver.Create(filePath, width, height, 1, DataType.GDT_Float32, 
                new[] { "COMPRESS=LZW" });
            
            dataset.SetGeoTransform(transform.ToArray());
            dataset.SetProjection(projection);
            
            using Band band = dataset.GetRasterBand(1);
            band.WriteRaster(0, 0, width, height, data, width, height, 0, 0);
            
            band.SetNoDataValue(float.NaN);
        }
        
        public void SaveImage(string filePath, float[][] data, int width, int height, GeoTransform transform, string projection)
        {
            Driver driver = Gdal.GetDriverByName("GTiff");
            using Dataset dataset = driver.Create(filePath, width, height, data.Length, DataType.GDT_Float32,
                new[] { "COMPRESS=LZW" });
            
            dataset.SetGeoTransform(transform.ToArray());
            dataset.SetProjection(projection);
            
            for (int i = 0; i < data.Length; i++)
            {
                using Band band = dataset.GetRasterBand(i + 1);
                band.WriteRaster(0, 0, width, height, data[i], width, height, 0, 0);
                band.SetNoDataValue(float.NaN);
            }
        }
        
        public void SaveImage(string filePath, byte[] data, int width, int height, GeoTransform transform, string projection)
        {
            Driver driver = Gdal.GetDriverByName("GTiff");
            using Dataset dataset = driver.Create(filePath, width, height, 1, DataType.GDT_Byte,
                new[] { "COMPRESS=LZW" });
            
            dataset.SetGeoTransform(transform.ToArray());
            dataset.SetProjection(projection);
            
            using Band band = dataset.GetRasterBand(1);
            band.WriteRaster(0, 0, width, height, data, width, height, 0, 0);
        }
        
        public void SaveImage(string filePath, byte[][] data, int width, int height, GeoTransform transform, string projection)
        {
            Driver driver = Gdal.GetDriverByName("GTiff");
            using Dataset dataset = driver.Create(filePath, width, height, data.Length, DataType.GDT_Byte,
                new[] { "COMPRESS=LZW" });
            
            dataset.SetGeoTransform(transform.ToArray());
            dataset.SetProjection(projection);
            
            for (int i = 0; i < data.Length; i++)
            {
                using Band band = dataset.GetRasterBand(i + 1);
                band.WriteRaster(0, 0, width, height, data[i], width, height, 0, 0);
            }
        }
        
        public (float[] data, GeoTransform transform, string projection) CropImage(string filePath, int startX, int startY, int width, int height)
        {
            using Dataset dataset = Gdal.Open(filePath, Access.GA_ReadOnly);
            
            double[] srcTransform = new double[6];
            dataset.GetGeoTransform(srcTransform);
            
            GeoTransform newTransform = new()
            {
                OriginX = srcTransform[0] + startX * srcTransform[1],
                PixelWidth = srcTransform[1],
                RotationX = srcTransform[2],
                OriginY = srcTransform[3] + startY * srcTransform[5],
                RotationY = srcTransform[4],
                PixelHeight = srcTransform[5]
            };
            
            float[] data = new float[width * height];
            using Band band = dataset.GetRasterBand(1);
            band.ReadRaster(startX, startY, width, height, data, width, height, 0, 0);
            
            return (data, newTransform, dataset.GetProjection());
        }
        
        public enum ResampleMethod
        {
            NearestNeighbor,
            Bilinear,
            Bicubic
        }
    }
}
