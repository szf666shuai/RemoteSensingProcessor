using RemoteSensingProcessor.UI;

namespace RemoteSensingProcessor
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // GDAL 全局初始化，必须在使用任何 GDAL 功能前调用
            OSGeo.GDAL.Gdal.AllRegister();
            
            // 启动主窗体
            Application.Run(new MainForm());
        }
    }
}
