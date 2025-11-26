using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLyPhongTro.Core
{
    public static class ExportHelper
    {
        /// <summary>
        /// Export DataTable to CSV (Excel compatible)
        /// </summary>
        public static void ExportToCsv(DataTable dt, string defaultFileName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Xuất dữ liệu"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();

                // Header - use Vietnamese-friendly encoding
                var headers = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                    headers[i] = dt.Columns[i].ColumnName;
                sb.AppendLine(string.Join(",", headers));

                // Data rows
                foreach (DataRow row in dt.Rows)
                {
                    var values = new string[dt.Columns.Count];
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string val = row[i]?.ToString() ?? "";
                        // Escape commas and quotes
                        if (val.Contains(",") || val.Contains("\"") || val.Contains("\n"))
                            val = $"\"{val.Replace("\"", "\"\"")}\"";
                        values[i] = val;
                    }
                    sb.AppendLine(string.Join(",", values));
                }

                // Write with UTF-8 BOM for Excel Vietnamese support
                File.WriteAllText(saveDialog.FileName, sb.ToString(), new UTF8Encoding(true));
                
                if (MessageBox.Show($"Đã xuất thành công!\n{saveDialog.FileName}\n\nMở file ngay?", "Thành công", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Export report to HTML (can print or convert to PDF)
        /// </summary>
        public static void ExportToHtml(string title, string content, string defaultFileName)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmm}.html",
                Title = "Xuất báo cáo"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{title}</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; padding: 20px; }}
        h1 {{ color: #1e3a5f; border-bottom: 2px solid #1e3a5f; padding-bottom: 10px; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #1e3a5f; color: white; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .footer {{ margin-top: 30px; color: #666; font-size: 12px; }}
        @media print {{ body {{ padding: 0; }} }}
    </style>
</head>
<body>
    <h1>{title}</h1>
    {content}
    <div class='footer'>
        Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}<br>
        Phần mềm Quản lý Phòng trọ
    </div>
</body>
</html>";

                File.WriteAllText(saveDialog.FileName, html, Encoding.UTF8);
                
                if (MessageBox.Show($"Đã xuất thành công!\n{saveDialog.FileName}\n\nMở file ngay?", "Thành công",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Convert DataTable to HTML table
        /// </summary>
        public static string DataTableToHtml(DataTable dt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table>");
            
            // Header
            sb.AppendLine("<tr>");
            foreach (DataColumn col in dt.Columns)
                sb.AppendLine($"<th>{col.ColumnName}</th>");
            sb.AppendLine("</tr>");

            // Rows
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (var item in row.ItemArray)
                    sb.AppendLine($"<td>{item}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }
    }
}
