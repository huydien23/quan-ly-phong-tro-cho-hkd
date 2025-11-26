using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Services : UserControl
    {
        private AntdUI.Table table;
        private AntdUI.Select cboMonth;
        private AntdUI.Select cboYear;
        private int currentMonth;
        private int currentYear;

        public UC_Services()
        {
            this.BackColor = AppColors.Blue50;
            currentMonth = DateTime.Now.Month;
            currentYear = DateTime.Now.Year;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Table (thêm TRƯỚC)
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Không có phòng đang thuê";
            table.Columns.Add(new Column("RoomId", "ID", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "80" });
            table.Columns.Add(new Column("CustomerName", "Khách thuê", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("ElecOld", "Điện cũ", ColumnAlign.Center) { Width = "90" });
            table.Columns.Add(new Column("ElecNew", "Điện mới", ColumnAlign.Center) { Width = "90" }); 
            table.Columns.Add(new Column("WaterOld", "Nước cũ", ColumnAlign.Center) { Width = "90" });
            table.Columns.Add(new Column("WaterNew", "Nước mới", ColumnAlign.Center) { Width = "90" });
            table.Columns.Add(new Column("ElecUsage", "Tiêu thụ điện", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("WaterUsage", "Tiêu thụ nước", ColumnAlign.Center) { Width = "100" });
            this.Controls.Add(table);

            // 2. Header (thêm SAU)
            var pnlHeader = new AntdUI.Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 60;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Padding = new Padding(15);

            var lblTitle = new AntdUI.Label();
            lblTitle.Text = "GHI ĐIỆN NƯỚC";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = AppColors.Blue900;
            lblTitle.Location = new Point(15, 15);
            lblTitle.Size = new Size(150, 30);

            // Chọn tháng
            cboMonth = new AntdUI.Select();
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(new SelectItem($"Tháng {i}", i));
            cboMonth.SelectedIndex = currentMonth - 1;
            cboMonth.Location = new Point(180, 12);
            cboMonth.Size = new Size(100, 36);
            cboMonth.SelectedIndexChanged += (s, e) => { currentMonth = Convert.ToInt32(((SelectItem)cboMonth.SelectedValue).Tag); LoadData(); };

            // Chọn năm
            cboYear = new AntdUI.Select();
            for (int y = DateTime.Now.Year - 2; y <= DateTime.Now.Year + 1; y++) cboYear.Items.Add(new SelectItem(y.ToString(), y));
            cboYear.SelectedIndex = 2;
            cboYear.Location = new Point(290, 12);
            cboYear.Size = new Size(80, 36);
            cboYear.SelectedIndexChanged += (s, e) => { currentYear = Convert.ToInt32(((SelectItem)cboYear.SelectedValue).Tag); LoadData(); };

            var btnEdit = new AntdUI.Button();
            btnEdit.Text = "Nhập chỉ số";
            btnEdit.Type = TTypeMini.Primary;
            btnEdit.BackColor = AppColors.Blue600;
            btnEdit.Location = new Point(390, 12);
            btnEdit.Size = new Size(110, 36);
            btnEdit.Click += (s, e) => ShowEditForm();

            var btnCreateInvoice = new AntdUI.Button();
            btnCreateInvoice.Text = "Tạo hóa đơn";
            btnCreateInvoice.Type = TTypeMini.Primary;
            btnCreateInvoice.BackColor = AppColors.Green;
            btnCreateInvoice.Location = new Point(510, 12);
            btnCreateInvoice.Size = new Size(110, 36);
            btnCreateInvoice.Click += (s, e) => CreateInvoices();

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(cboMonth);
            pnlHeader.Controls.Add(cboYear);
            pnlHeader.Controls.Add(btnEdit);
            pnlHeader.Controls.Add(btnCreateInvoice);
            this.Controls.Add(pnlHeader);
        }

        private void LoadData()
        {
            try
            {
                // Lấy các phòng đang thuê và chỉ số điện nước
                string sql = @"
                    SELECT r.RoomId, r.RoomName, cu.FullName as CustomerName,
                           ISNULL(s.ElecOld, 0) as ElecOld, ISNULL(s.ElecNew, 0) as ElecNew,
                           ISNULL(s.WaterOld, 0) as WaterOld, ISNULL(s.WaterNew, 0) as WaterNew,
                           ISNULL(s.ElecNew - s.ElecOld, 0) as ElecUsage,
                           ISNULL(s.WaterNew - s.WaterOld, 0) as WaterUsage,
                           c.ContractId
                    FROM Contracts c
                    INNER JOIN Rooms r ON c.RoomId = r.RoomId
                    INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    LEFT JOIN (
                        SELECT * FROM Invoices WHERE Month = @month AND Year = @year
                    ) s ON c.ContractId = s.ContractId
                    WHERE c.IsActive = 1
                    ORDER BY r.RoomName";
                
                var dt = DatabaseHelper.ExecuteQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@month", currentMonth),
                    new System.Data.SqlClient.SqlParameter("@year", currentYear));
                table.DataSource = dt;
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void ShowEditForm()
        {
            if (table.SelectedIndex < 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Chọn phòng cần nhập chỉ số!");
                return;
            }

            var dt = table.DataSource as DataTable;
            var row = dt.Rows[table.SelectedIndex];
            int roomId = Convert.ToInt32(row["RoomId"]);
            string roomName = row["RoomName"].ToString();
            int contractId = Convert.ToInt32(row["ContractId"]);

            var form = new Form();
            form.Text = $"Nhập chỉ số - {roomName} - Tháng {currentMonth}/{currentYear}";
            form.Size = new Size(400, 350);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 6 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Điện cũ:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var numElecOld = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = Convert.ToDecimal(row["ElecOld"]) };
            layout.Controls.Add(numElecOld, 1, 0);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Điện mới:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var numElecNew = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = Convert.ToDecimal(row["ElecNew"]) };
            layout.Controls.Add(numElecNew, 1, 1);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Nước cũ:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            var numWaterOld = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = Convert.ToDecimal(row["WaterOld"]) };
            layout.Controls.Add(numWaterOld, 1, 2);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Nước mới:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            var numWaterNew = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = Convert.ToDecimal(row["WaterNew"]) };
            layout.Controls.Add(numWaterNew, 1, 3);

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Dock = DockStyle.Fill };
            btnSave.Click += (s, e) => {
                try
                {
                    // Kiểm tra đã có record chưa
                    var existing = DatabaseHelper.ExecuteScalar(
                        "SELECT InvoiceId FROM Invoices WHERE ContractId = @cid AND Month = @m AND Year = @y",
                        new System.Data.SqlClient.SqlParameter("@cid", contractId),
                        new System.Data.SqlClient.SqlParameter("@m", currentMonth),
                        new System.Data.SqlClient.SqlParameter("@y", currentYear));

                    if (existing != null)
                    {
                        // Update
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE Invoices SET ElecOld=@eo, ElecNew=@en, WaterOld=@wo, WaterNew=@wn WHERE InvoiceId=@id",
                            new System.Data.SqlClient.SqlParameter("@eo", numElecOld.Value),
                            new System.Data.SqlClient.SqlParameter("@en", numElecNew.Value),
                            new System.Data.SqlClient.SqlParameter("@wo", numWaterOld.Value),
                            new System.Data.SqlClient.SqlParameter("@wn", numWaterNew.Value),
                            new System.Data.SqlClient.SqlParameter("@id", existing));
                    }
                    else
                    {
                        // Insert
                        DatabaseHelper.ExecuteNonQuery(
                            @"INSERT INTO Invoices (ContractId, Month, Year, ElecOld, ElecNew, WaterOld, WaterNew, Status) 
                              VALUES (@cid, @m, @y, @eo, @en, @wo, @wn, 'ChuaThu')",
                            new System.Data.SqlClient.SqlParameter("@cid", contractId),
                            new System.Data.SqlClient.SqlParameter("@m", currentMonth),
                            new System.Data.SqlClient.SqlParameter("@y", currentYear),
                            new System.Data.SqlClient.SqlParameter("@eo", numElecOld.Value),
                            new System.Data.SqlClient.SqlParameter("@en", numElecNew.Value),
                            new System.Data.SqlClient.SqlParameter("@wo", numWaterOld.Value),
                            new System.Data.SqlClient.SqlParameter("@wn", numWaterNew.Value));
                    }
                    AntdUI.Message.success(form, "Đã lưu!");
                    form.Close();
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };
            layout.Controls.Add(btnSave, 1, 5);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void CreateInvoices()
        {
            try
            {
                // Lấy giá điện nước từ Settings
                var priceElec = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'GiaDien'");
                var priceWater = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'GiaNuoc'");
                decimal elecPrice = priceElec != null ? Convert.ToDecimal(priceElec) : 4000;
                decimal waterPrice = priceWater != null ? Convert.ToDecimal(priceWater) : 10000;

                // Cập nhật giá và tổng tiền cho tất cả hóa đơn tháng này
                string sql = @"
                    UPDATE i SET 
                        ElecPrice = @ep,
                        WaterPrice = @wp,
                        RoomPrice = c.MonthlyRent,
                        TotalAmount = c.MonthlyRent + (ISNULL(i.ElecNew,0) - ISNULL(i.ElecOld,0)) * @ep + (ISNULL(i.WaterNew,0) - ISNULL(i.WaterOld,0)) * @wp
                    FROM Invoices i
                    INNER JOIN Contracts c ON i.ContractId = c.ContractId
                    WHERE i.Month = @m AND i.Year = @y";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new System.Data.SqlClient.SqlParameter("@ep", elecPrice),
                    new System.Data.SqlClient.SqlParameter("@wp", waterPrice),
                    new System.Data.SqlClient.SqlParameter("@m", currentMonth),
                    new System.Data.SqlClient.SqlParameter("@y", currentYear));

                AntdUI.Message.success(this.FindForm(), $"Đã tạo hóa đơn tháng {currentMonth}/{currentYear}!");
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }
    }
}