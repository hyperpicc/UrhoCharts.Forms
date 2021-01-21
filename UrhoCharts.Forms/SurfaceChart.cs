using Xamarin.Forms;


namespace UrhoCharts.Forms
{
    public class SurfaceChart
    {
        public int XSize { get; set; }

        public int YSize { get; set; }

        public byte[] ZData { get; set; }

        public Color BackgroundColor { get; set; }

        public bool IsValid
            => (XSize > 0) && (YSize > 0) && (ZData != null) && (ZData?.Length > 0);
 
        public SurfaceChart()
        {
            XSize           = 0;
            YSize           = 0;
            ZData           = null;
            BackgroundColor = Color.Transparent;
        }
    }
}
