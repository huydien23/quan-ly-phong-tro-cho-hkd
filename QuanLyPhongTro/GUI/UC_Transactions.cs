using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.BLL;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.GUI
{
    public class UC_Transactions : UserControl
    {
        private AntdUI.Table table;
        private AntdUI.Select cboMonth, cboYear, cboType;
        private System.Windows.Forms.Label lblIncome, lblExpense, lblBalance;
        private readonly TransactionBLL _transactionBLL;
        private int selectedTransactionId = -1;

        public UC_Transactions()
        {
            _transactionBLL = new TransactionBLL();
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Table (thêm TRƯỚC)
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Chưa có giao dịch nào";
            table.Columns.Add(new Column("TransactionId", "Mã", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("TransactionDate", "Ngày", ColumnAlign.Center) { Width = "90" });
            table.Columns.Add(new Column("TypeDisplay", "Loại", ColumnAlign.Center) { Width = "60" });
            table.Columns.Add(new Column("CategoryName", "Phân loại", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("Amount", "Số tiền", ColumnAlign.Right) { Width = "120" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("Description", "Diễn giải", ColumnAlign.Left) { Width = "200" });
            table.Columns.Add(new Column("ReceivedBy", "Người nhận", ColumnAlign.Left) { Width = "120" });
            table.CellClick += (s, e) => {
                if (e.Record is DataRowView drv)
                    selectedTransactionId = Convert.ToInt32(drv["TransactionId"]);
                else if (e.Record is DataRow dr)
                    selectedTransactionId = Convert.ToInt32(dr["TransactionId"]);
            };
            this.Controls.Add(table);

            // 2. Summary Panel (thêm SAU table)
            var pnlSummary = new AntdUI.Panel();
            pnlSummary.Dock = DockStyle.Top;
            pnlSummary.Height = 80;
            pnlSummary.BackColor = Color.White;
            pnlSummary.Padding = new Padding(15);

            // Income Card
            var cardIncome = CreateSummaryCard("THU", "0 đ", AppColors.Green, out lblIncome);
            cardIncome.Location = new Point(15, 10);
            pnlSummary.Controls.Add(cardIncome);

            // Expense Card
            var cardExpense = CreateSummaryCard("CHI", "0 đ", AppColors.Red, out lblExpense);
            cardExpense.Location = new Point(235, 10);
            pnlSummary.Controls.Add(cardExpense);

            // Balance Card
            var cardBalance = CreateSummaryCard("CÒN LẠI", "0 đ", AppColors.Blue600, out lblBalance);
            cardBalance.Location = new Point(455, 10);
            pnlSummary.Controls.Add(cardBalance);

            this.Controls.Add(pnlSummary);

            // 3. Toolbar (thêm SAU summary)
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;
            panelTop.Padding = new Padding(15);

            // Filters
            cboMonth = new AntdUI.Select();
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(new SelectItem($"Tháng {i}", i));
            cboMonth.SelectedIndex = DateTime.Now.Month - 1;
            cboMonth.Location = new Point(15, 12);
            cboMonth.Size = new Size(100, 36);
            cboMonth.SelectedIndexChanged += (s, e) => LoadData();

            cboYear = new AntdUI.Select();
            for (int y = DateTime.Now.Year - 2; y <= DateTime.Now.Year + 1; y++)
                cboYear.Items.Add(new SelectItem(y.ToString(), y));
            cboYear.SelectedIndex = 2;
            cboYear.Location = new Point(125, 12);
            cboYear.Size = new Size(80, 36);
            cboYear.SelectedIndexChanged += (s, e) => LoadData();

            cboType = new AntdUI.Select();
            cboType.Items.Add(new SelectItem("Tất cả", ""));
            cboType.Items.Add(new SelectItem("Thu", "Thu"));
            cboType.Items.Add(new SelectItem("Chi", "Chi"));
            cboType.SelectedIndex = 0;
            cboType.Location = new Point(215, 12);
            cboType.Size = new Size(90, 36);
            cboType.SelectedIndexChanged += (s, e) => LoadData();

            // Buttons
            var btnAddIncome = new AntdUI.Button();
            btnAddIncome.Text = "+ Phiếu Thu";
            btnAddIncome.Type = TTypeMini.Primary;
            btnAddIncome.BackColor = AppColors.Green;
            btnAddIncome.Location = new Point(330, 12);
            btnAddIncome.Size = new Size(110, 36);
            btnAddIncome.Click += (s, e) => ShowTransactionForm("Thu");

            var btnAddExpense = new AntdUI.Button();
            btnAddExpense.Text = "+ Phiếu Chi";
            btnAddExpense.Type = TTypeMini.Primary;
            btnAddExpense.BackColor = AppColors.Red;
            btnAddExpense.Location = new Point(450, 12);
            btnAddExpense.Size = new Size(110, 36);
            btnAddExpense.Click += (s, e) => ShowTransactionForm("Chi");

            var btnEdit = new AntdUI.Button();
            btnEdit.Text = "Sửa";
            btnEdit.Location = new Point(570, 12);
            btnEdit.Size = new Size(70, 36);
            btnEdit.Click += (s, e) => EditSelected();

            var btnDelete = new AntdUI.Button();
            btnDelete.Text = "Xóa";
            btnDelete.ForeColor = AppColors.Red;
            btnDelete.Location = new Point(650, 12);
            btnDelete.Size = new Size(70, 36);
            btnDelete.Click += (s, e) => DeleteSelected();

            var btnExport = new AntdUI.Button();
            btnExport.Text = "Xuất Excel";
            btnExport.Location = new Point(730, 12);
            btnExport.Size = new Size(100, 36);
            btnExport.Click += (s, e) => ExportToExcel();

            panelTop.Controls.Add(cboMonth);
            panelTop.Controls.Add(cboYear);
            panelTop.Controls.Add(cboType);
            panelTop.Controls.Add(btnAddIncome);
            panelTop.Controls.Add(btnAddExpense);
            panelTop.Controls.Add(btnEdit);
            panelTop.Controls.Add(btnDelete);
            panelTop.Controls.Add(btnExport);
            this.Controls.Add(panelTop);
        }

        private AntdUI.Panel CreateSummaryCard(string title, string value, Color color, out System.Windows.Forms.Label lblValue)
        {
            var card = new AntdUI.Panel();
            card.Size = new Size(200, 60);
            card.Radius = 8;
            card.Back = Color.FromArgb(250, 252, 255);

            var lblTitle = new System.Windows.Forms.Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9);
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(15, 8);
            lblTitle.Size = new Size(170, 18);
            card.Controls.Add(lblTitle);

            lblValue = new System.Windows.Forms.Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblValue.ForeColor = color;
            lblValue.Location = new Point(15, 28);
            lblValue.Size = new Size(170, 28);
            card.Controls.Add(lblValue);

            return card;
        }

        private void LoadData()
        {
            try
            {
                int month = cboMonth.SelectedIndex + 1;
                int year = DateTime.Now.Year - 2 + cboYear.SelectedIndex;
                string type = cboType.SelectedIndex == 0 ? null : (cboType.SelectedIndex == 1 ? "Thu" : "Chi");

                // Load data
                table.DataSource = _transactionBLL.GetAllAsDataTable(month, year, type);

                // Load summary
                var summary = _transactionBLL.GetMonthlySummary(month, year);
                lblIncome.Text = $"+{summary.TotalIncome:N0} đ";
                lblExpense.Text = $"-{summary.TotalExpense:N0} đ";
                lblBalance.Text = $"{summary.Balance:N0} đ";
                lblBalance.ForeColor = summary.Balance >= 0 ? AppColors.Green : AppColors.Red;
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void ShowTransactionForm(string type)
        {
            var form = new Form();
            form.Text = type == "Thu" ? "Tạo phiếu thu" : "Tạo phiếu chi";
            form.Size = new Size(450, 480);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            int y = 20;
            Font lblFont = new Font("Segoe UI", 10);

            // Ngày
            var lblDate = new System.Windows.Forms.Label { Text = "Ngày:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var dtpDate = new DateTimePicker { Location = new Point(130, y), Size = new Size(280, 30), Format = DateTimePickerFormat.Short };
            form.Controls.Add(lblDate);
            form.Controls.Add(dtpDate);
            y += 40;

            // Loại
            var lblCat = new System.Windows.Forms.Label { Text = "Phân loại:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var cboCat = new ComboBox { Location = new Point(130, y), Size = new Size(280, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            var categories = _transactionBLL.GetCategories(type);
            foreach (var cat in categories)
                cboCat.Items.Add(new ComboItem(cat.CategoryName, cat.CategoryCode));
            if (cboCat.Items.Count > 0) cboCat.SelectedIndex = 0;
            form.Controls.Add(lblCat);
            form.Controls.Add(cboCat);
            y += 40;

            // Số tiền
            var lblAmount = new System.Windows.Forms.Label { Text = "Số tiền:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var txtAmount = new AntdUI.InputNumber { Location = new Point(130, y), Size = new Size(280, 36), Minimum = 0, Maximum = 999999999 };
            form.Controls.Add(lblAmount);
            form.Controls.Add(txtAmount);
            y += 45;

            // Diễn giải
            var lblDesc = new System.Windows.Forms.Label { Text = "Diễn giải:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var txtDesc = new AntdUI.Input { Location = new Point(130, y), Size = new Size(280, 36) };
            form.Controls.Add(lblDesc);
            form.Controls.Add(txtDesc);
            y += 45;

            // Phương thức
            var lblMethod = new System.Windows.Forms.Label { Text = "Phương thức:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var cboMethod = new ComboBox { Location = new Point(130, y), Size = new Size(280, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboMethod.Items.Add(new ComboItem("Tiền mặt", "TienMat"));
            cboMethod.Items.Add(new ComboItem("Chuyển khoản", "ChuyenKhoan"));
            cboMethod.Items.Add(new ComboItem("MoMo", "MoMo"));
            cboMethod.SelectedIndex = 0;
            form.Controls.Add(lblMethod);
            form.Controls.Add(cboMethod);
            y += 40;

            // Người nhận/chi
            var lblReceiver = new System.Windows.Forms.Label { Text = type == "Thu" ? "Người thu:" : "Người chi:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var txtReceiver = new AntdUI.Input { Location = new Point(130, y), Size = new Size(280, 36), Text = CurrentUser.FullName };
            form.Controls.Add(lblReceiver);
            form.Controls.Add(txtReceiver);
            y += 45;

            // Ghi chú
            var lblNote = new System.Windows.Forms.Label { Text = "Ghi chú:", Font = lblFont, Location = new Point(20, y), Size = new Size(100, 25) };
            var txtNote = new AntdUI.Input { Location = new Point(130, y), Size = new Size(280, 60), Multiline = true };
            form.Controls.Add(lblNote);
            form.Controls.Add(txtNote);
            y += 70;

            // Buttons
            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = type == "Thu" ? AppColors.Green : AppColors.Red, Location = new Point(130, y), Size = new Size(130, 40) };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Location = new Point(270, y), Size = new Size(80, 40) };

            btnSave.Click += (s, e) => {
                if (cboCat.SelectedItem == null)
                {
                    AntdUI.Message.warn(form, "Vui lòng chọn phân loại!");
                    return;
                }

                var cat = (ComboItem)cboCat.SelectedItem;
                var method = (ComboItem)cboMethod.SelectedItem;

                var trans = new TransactionDTO
                {
                    TransactionDate = dtpDate.Value,
                    TransactionType = type,
                    CategoryCode = cat.Value,
                    CategoryName = cat.Text,
                    Amount = (decimal)txtAmount.Value,
                    Description = txtDesc.Text,
                    PaymentMethod = method.Value,
                    ReceivedBy = txtReceiver.Text,
                    Note = txtNote.Text
                };

                var result = _transactionBLL.AddTransaction(trans);
                if (result.IsSuccess)
                {
                    AntdUI.Message.success(form, result.Message);
                    form.Close();
                    LoadData();
                }
                else
                {
                    AntdUI.Message.error(form, result.Message);
                }
            };

            btnCancel.Click += (s, e) => form.Close();

            form.Controls.Add(btnSave);
            form.Controls.Add(btnCancel);

            form.ShowDialog();
        }

        private void EditSelected()
        {
            if (selectedTransactionId <= 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Vui lòng chọn giao dịch cần sửa!");
                return;
            }
            // TODO: Implement edit form
            AntdUI.Message.info(this.FindForm(), "Chức năng đang phát triển");
        }

        private void DeleteSelected()
        {
            if (selectedTransactionId <= 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Vui lòng chọn giao dịch cần xóa!");
                return;
            }

            if (VNDialog.Confirm("Xóa giao dịch này?"))
            {
                var result = _transactionBLL.DeleteTransaction(selectedTransactionId);
                if (result.IsSuccess)
                {
                    AntdUI.Message.success(this.FindForm(), result.Message);
                    LoadData();
                    selectedTransactionId = -1;
                }
                else
                {
                    AntdUI.Message.error(this.FindForm(), result.Message);
                }
            }
        }

        private void ExportToExcel()
        {
            AntdUI.Message.info(this.FindForm(), "Chức năng xuất Excel đang phát triển");
        }

        // Helper class for ComboBox
        private class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public ComboItem(string text, string value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }
    }
}
