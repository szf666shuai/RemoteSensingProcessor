namespace RemoteSensingProcessor.Model
{
    public class RasterBand
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public OSGeo.GDAL.DataType DataType { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double NoDataValue { get; set; }
        public int[] Histogram { get; set; } = Array.Empty<int>();
    }
}
