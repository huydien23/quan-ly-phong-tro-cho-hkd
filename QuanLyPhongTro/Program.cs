using System;
using System.Windows.Forms;
using QuanLyPhongTro.GUI; 

namespace QuanLyPhongTro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLogin());
        }
    }
}