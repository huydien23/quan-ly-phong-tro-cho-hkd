using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using QuanLyPhongTro.GUI; 

namespace QuanLyPhongTro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Set Vietnamese culture for the entire application
            var culture = new CultureInfo("vi-VN");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // AntdUI locale - Vietnamese
            AntdUI.Localization.Provider = new VietnameseLocalization();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLogin());
        }
    }
}