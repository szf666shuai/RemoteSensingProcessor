namespace RemoteSensingProcessor.Model
{
    public class GeoTransform
    {
        public double OriginX { get; set; }
        public double PixelWidth { get; set; }
        public double RotationX { get; set; }
        public double OriginY { get; set; }
        public double RotationY { get; set; }
        public double PixelHeight { get; set; }
        
        public double[] ToArray()
        {
            return new[] { OriginX, PixelWidth, RotationX, OriginY, RotationY, PixelHeight };
        }
        
        public static GeoTransform FromArray(double[] transform)
        {
            if (transform == null || transform.Length != 6)
                throw new ArgumentException("GeoTransform 数组必须包含6个元素");
            
            return new GeoTransform
            {
                OriginX = transform[0],
                PixelWidth = transform[1],
                RotationX = transform[2],
                OriginY = transform[3],
                RotationY = transform[4],
                PixelHeight = transform[5]
            };
        }
    }
}
