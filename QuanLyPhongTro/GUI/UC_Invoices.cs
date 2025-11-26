using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Invoices : UserControl
    {
        private AntdUI.Table table;
        private AntdUI.Select cboStatus;
        private DataRow selectedInvoice;

        public UC_Invoices()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Table (thêm TRƯỚC)
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Chưa có hóa đơn nào";
            table.Columns.Add(new Column("InvoiceId", "Mã", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("CustomerName", "Khách thuê", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("Period", "Kỳ", ColumnAlign.Center) { Width = "80" });
            table.Columns.Add(new Column("RoomPrice", "Tiền phòng", ColumnAlign.Right) { Width = "100" });
            table.Columns.Add(new Column("ElecFee", "Tiền điện", ColumnAlign.Right) { Width = "100" });
            table.Columns.Add(new Column("WaterFee", "Tiền nước", ColumnAlign.Right) { Width = "100" });
            table.Columns.Add(new Column("TotalAmount", "Tổng cộng", ColumnAlign.Right) { Width = "120" });
            table.Columns.Add(new Column("Status", "Trạng thái", ColumnAlign.Center) { Width = "100" });
            this.Controls.Add(table);

            // 2. Toolbar (thêm SAU)
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;
            panelTop.Padding = new Padding(15);

            cboStatus = new AntdUI.Select();
            cboStatus.Items.Add(new SelectItem("Tất cả", "All"));
            cboStatus.Items.Add(new SelectItem("Chưa thu", "ChuaThu"));
            cboStatus.Items.Add(new SelectItem("Đã thu", "DaThu"));
            cboStatus.SelectedIndex = 0;
            cboStatus.Location = new Point(15, 12);
            cboStatus.Size = new Size(150, 36);
            cboStatus.SelectedIndexChanged += (s, e) => LoadData();

            var btnPaid = new AntdUI.Button();
            btnPaid.Text = "Đánh dấu Đã thu";
            btnPaid.Type = TTypeMini.Primary;
            btnPaid.BackColor = AppColors.Green;
            btnPaid.Location = new Point(180, 12);
            btnPaid.Size = new Size(140, 36);
            btnPaid.Click += (s, e) => MarkAsPaid();

            var btnPrint = new AntdUI.Button();
            btnPrint.Text = "In hóa đơn";
            btnPrint.Location = new Point(335, 12);
            btnPrint.Size = new Size(100, 36);
            btnPrint.Click += (s, e) => PrintInvoice();

            var btnDetail = new AntdUI.Button();
            btnDetail.Text = "Xem chi tiết";
            btnDetail.Location = new Point(450, 12);
            btnDetail.Size = new Size(100, 36);
            btnDetail.Click += (s, e) => ShowDetail();

            var btnExport = new AntdUI.Button();
            btnExport.Text = " Xuất Excel";
            btnExport.Location = new Point(565, 12);
            btnExport.Size = new Size(110, 36);
            btnExport.Click += (s, e) => ExportToExcel();

            var btnReport = new AntdUI.Button();
            btnReport.Text = " Báo cáo";
            btnReport.Location = new Point(685, 12);
            btnReport.Size = new Size(100, 36);
            btnReport.Click += (s, e) => GenerateReport();

            panelTop.Controls.Add(cboStatus);
            panelTop.Controls.Add(btnPaid);
            panelTop.Controls.Add(btnPrint);
            panelTop.Controls.Add(btnDetail);
            panelTop.Controls.Add(btnExport);
            panelTop.Controls.Add(btnReport);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            try
            {
                string statusFilter = "";
                if (cboStatus.SelectedIndex > 0)
                {
                    string status = cboStatus.SelectedIndex == 1 ? "ChuaThu" : "DaThu";
                    statusFilter = $" AND i.Status = '{status}'";
                }

                string sql = $@"
                    SELECT i.InvoiceId, r.RoomName, cu.FullName as CustomerName,
                           CONCAT(i.Month, '/', i.Year) as Period,
                           ISNULL(i.RoomPrice, 0) as RoomPrice,
                           (ISNULL(i.ElecNew,0) - ISNULL(i.ElecOld,0)) * ISNULL(i.ElecPrice,0) as ElecFee,
                           (ISNULL(i.WaterNew,0) - ISNULL(i.WaterOld,0)) * ISNULL(i.WaterPrice,0) as WaterFee,
                           ISNULL(i.TotalAmount, 0) as TotalAmount,
                           CASE WHEN i.Status = 'DaThu' THEN N'Đã thu' ELSE N'Chưa thu' END as Status,
                           i.Month, i.Year, i.ElecOld, i.ElecNew, i.WaterOld, i.WaterNew,
                           i.ElecPrice, i.WaterPrice, i.ContractId, i.Status as StatusCode
                    FROM Invoices i
                    INNER JOIN Contracts c ON i.ContractId = c.ContractId
                    INNER JOIN Rooms r ON c.RoomId = r.RoomId
                    INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    WHERE 1=1 {statusFilter}
                    ORDER BY i.Year DESC, i.Month DESC, r.RoomName";

                table.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void MarkAsPaid()
        {
            if (table.SelectedIndex < 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Chọn hóa đơn cần đánh dấu!");
                return;
            }

            var dt = table.DataSource as DataTable;
            var row = dt.Rows[table.SelectedIndex];
            
            if (row["StatusCode"].ToString() == "DaThu")
            {
                AntdUI.Message.warn(this.FindForm(), "Hóa đơn này đã thu tiền rồi!");
                return;
            }

            int invoiceId = Convert.ToInt32(row["InvoiceId"]);
            string roomName = row["RoomName"].ToString();
            decimal total = Convert.ToDecimal(row["TotalAmount"]);

            if (VNDialog.Confirm($"Đánh dấu đã thu {total:N0} VNĐ cho phòng {roomName}?"))
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Invoices SET Status = 'DaThu', PaidDate = GETDATE() WHERE InvoiceId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", invoiceId));
                    AntdUI.Message.success(this.FindForm(), "Đã cập nhật!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
                }
            }
        }

        private void ShowDetail()
        {
            if (table.SelectedIndex < 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Chọn hóa đơn cần xem!");
                return;
            }

            var dt = table.DataSource as DataTable;
            var row = dt.Rows[table.SelectedIndex];

            string detail = $@"
HÓA ĐƠN TIỀN PHÒNG
==================
Phòng: {row["RoomName"]}
Khách: {row["CustomerName"]}
Kỳ: Tháng {row["Month"]}/{row["Year"]}

CHI TIẾT:
---------
Tiền phòng: {Convert.ToDecimal(row["RoomPrice"]):N0} VNĐ

Điện: {row["ElecOld"]} → {row["ElecNew"]} = {Convert.ToInt32(row["ElecNew"]) - Convert.ToInt32(row["ElecOld"])} kWh
      × {Convert.ToDecimal(row["ElecPrice"]):N0} = {Convert.ToDecimal(row["ElecFee"]):N0} VNĐ

Nước: {row["WaterOld"]} → {row["WaterNew"]} = {Convert.ToInt32(row["WaterNew"]) - Convert.ToInt32(row["WaterOld"])} khối
      × {Convert.ToDecimal(row["WaterPrice"]):N0} = {Convert.ToDecimal(row["WaterFee"]):N0} VNĐ

==================
TỔNG CỘNG: {Convert.ToDecimal(row["TotalAmount"]):N0} VNĐ
Trạng thái: {row["Status"]}
";
            MessageBox.Show(detail, "Chi tiết hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PrintInvoice()
        {
            if (table.SelectedIndex < 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Chọn hóa đơn cần in!");
                return;
            }

            var dt = table.DataSource as DataTable;
            selectedInvoice = dt.Rows[table.SelectedIndex];

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += PrintPage;

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = pd;
            preview.ShowDialog();
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            var row = selectedInvoice;
            int y = 50;

            // Header
            g.DrawString("HÓA ĐƠN TIỀN PHÒNG", new Font("Arial", 16, FontStyle.Bold), Brushes.Black, 200, y);
            y += 40;

            g.DrawString($"Phòng: {row["RoomName"]}", new Font("Arial", 12), Brushes.Black, 50, y); y += 25;
            g.DrawString($"Khách thuê: {row["CustomerName"]}", new Font("Arial", 12), Brushes.Black, 50, y); y += 25;
            g.DrawString($"Kỳ thanh toán: Tháng {row["Month"]}/{row["Year"]}", new Font("Arial", 12), Brushes.Black, 50, y); y += 35;

            // Line
            g.DrawLine(Pens.Black, 50, y, 550, y); y += 10;

            // Details
            g.DrawString($"Tiền phòng:", new Font("Arial", 11), Brushes.Black, 50, y);
            g.DrawString($"{Convert.ToDecimal(row["RoomPrice"]):N0} VNĐ", new Font("Arial", 11), Brushes.Black, 400, y); y += 25;

            int elecUsage = Convert.ToInt32(row["ElecNew"]) - Convert.ToInt32(row["ElecOld"]);
            g.DrawString($"Tiền điện ({elecUsage} kWh × {Convert.ToDecimal(row["ElecPrice"]):N0}):", new Font("Arial", 11), Brushes.Black, 50, y);
            g.DrawString($"{Convert.ToDecimal(row["ElecFee"]):N0} VNĐ", new Font("Arial", 11), Brushes.Black, 400, y); y += 25;

            int waterUsage = Convert.ToInt32(row["WaterNew"]) - Convert.ToInt32(row["WaterOld"]);
            g.DrawString($"Tiền nước ({waterUsage} khối × {Convert.ToDecimal(row["WaterPrice"]):N0}):", new Font("Arial", 11), Brushes.Black, 50, y);
            g.DrawString($"{Convert.ToDecimal(row["WaterFee"]):N0} VNĐ", new Font("Arial", 11), Brushes.Black, 400, y); y += 35;

            // Line
            g.DrawLine(Pens.Black, 50, y, 550, y); y += 10;

            // Total
            g.DrawString("TỔNG CỘNG:", new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 50, y);
            g.DrawString($"{Convert.ToDecimal(row["TotalAmount"]):N0} VNĐ", new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 380, y); y += 40;

            // Footer
            g.DrawString($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", new Font("Arial", 10), Brushes.Gray, 50, y);
        }

        private void ExportToExcel()
        {
            var dt = table.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Không có dữ liệu để xuất!");
                return;
            }

            // Create export DataTable with friendly column names
            var exportDt = new DataTable();
            exportDt.Columns.Add("Mã HĐ");
            exportDt.Columns.Add("Phòng");
            exportDt.Columns.Add("Khách thuê");
            exportDt.Columns.Add("Kỳ");
            exportDt.Columns.Add("Tiền phòng");
            exportDt.Columns.Add("Tiền điện");
            exportDt.Columns.Add("Tiền nước");
            exportDt.Columns.Add("Tổng cộng");
            exportDt.Columns.Add("Trạng thái");

            foreach (DataRow row in dt.Rows)
            {
                exportDt.Rows.Add(
                    row["InvoiceId"],
                    row["RoomName"],
                    row["CustomerName"],
                    row["Period"],
                    row["RoomPrice"],
                    row["ElecFee"],
                    row["WaterFee"],
                    row["TotalAmount"],
                    row["Status"]
                );
            }

            ExportHelper.ExportToCsv(exportDt, "HoaDon");
        }

        private void GenerateReport()
        {
            try
            {
                // Get summary data
                var summary = DatabaseHelper.ExecuteQuery($@"
                    SELECT 
                        (SELECT COUNT(*) FROM Invoices WHERE Year = {DateTime.Now.Year}) as TotalInvoices,
                        (SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE Year = {DateTime.Now.Year} AND Status = 'DaThu') as TotalCollected,
                        (SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE Year = {DateTime.Now.Year} AND Status = 'ChuaThu') as TotalPending,
                        (SELECT COUNT(*) FROM Invoices WHERE Status = 'ChuaThu' AND Year = {DateTime.Now.Year}) as PendingCount
                ");

                if (summary.Rows.Count == 0) return;
                var s = summary.Rows[0];

                // Get monthly breakdown
                var monthly = DatabaseHelper.ExecuteQuery($@"
                    SELECT Month, 
                           SUM(CASE WHEN Status = 'DaThu' THEN TotalAmount ELSE 0 END) as Collected,
                           SUM(CASE WHEN Status = 'ChuaThu' THEN TotalAmount ELSE 0 END) as Pending
                    FROM Invoices 
                    WHERE Year = {DateTime.Now.Year}
                    GROUP BY Month
                    ORDER BY Month");

                string content = $@"
<h2>Tổng quan năm {DateTime.Now.Year}</h2>
<table>
    <tr><td><strong>Tổng số hóa đơn:</strong></td><td>{s["TotalInvoices"]}</td></tr>
    <tr><td><strong>Đã thu:</strong></td><td style='color:green'>{Convert.ToDecimal(s["TotalCollected"]):N0} VNĐ</td></tr>
    <tr><td><strong>Chưa thu:</strong></td><td style='color:red'>{Convert.ToDecimal(s["TotalPending"]):N0} VNĐ ({s["PendingCount"]} hóa đơn)</td></tr>
</table>

<h2>Chi tiết theo tháng</h2>
<table>
    <tr><th>Tháng</th><th>Đã thu</th><th>Chưa thu</th></tr>";

                foreach (DataRow row in monthly.Rows)
                {
                    content += $@"
    <tr>
        <td>Tháng {row["Month"]}</td>
        <td style='color:green'>{Convert.ToDecimal(row["Collected"]):N0}</td>
        <td style='color:red'>{Convert.ToDecimal(row["Pending"]):N0}</td>
    </tr>";
                }

                content += "</table>";

                ExportHelper.ExportToHtml($"BÁO CÁO DOANH THU NĂM {DateTime.Now.Year}", content, "BaoCaoDoanhThu");
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi tạo báo cáo: " + ex.Message);
            }
        }
    }
}