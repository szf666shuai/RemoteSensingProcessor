namespace RemoteSensingProcessor.Model
{
    public class ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BandCount { get; set; }
        public string Projection { get; set; } = string.Empty;
        public GeoTransform GeoTransform { get; set; } = new();
        public double ResolutionX => GeoTransform.PixelWidth;
        public double ResolutionY => Math.Abs(GeoTransform.PixelHeight);
        public string FilePath { get; set; } = string.Empty;
        public OSGeo.GDAL.DataType DataType { get; set; }
        public RasterBand[] Bands { get; set; } = Array.Empty<RasterBand>();
        
        public (double X, double Y) GetCornerCoordinate(int row, int col)
        {
            double x = GeoTransform.OriginX + col * GeoTransform.PixelWidth + row * GeoTransform.RotationX;
            double y = GeoTransform.OriginY + col * GeoTransform.RotationY + row * GeoTransform.PixelHeight;
            return (x, y);
        }
        
        public (double X, double Y)[] GetAllCorners()
        {
            return new[]
            {
                GetCornerCoordinate(0, 0),
                GetCornerCoordinate(0, Width),
                GetCornerCoordinate(Height, Width),
                GetCornerCoordinate(Height, 0)
            };
        }
    }
}
