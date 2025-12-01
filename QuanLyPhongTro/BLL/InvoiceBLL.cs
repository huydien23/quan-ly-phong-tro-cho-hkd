using System;
using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class InvoiceBLL
    {
        private readonly InvoiceDAL _invoiceDAL;
        private readonly ContractDAL _contractDAL;
        private readonly SettingDAL _settingDAL;
        private readonly RoomDAL _roomDAL;

        public InvoiceBLL()
        {
            _invoiceDAL = new InvoiceDAL();
            _contractDAL = new ContractDAL();
            _settingDAL = new SettingDAL();
            _roomDAL = new RoomDAL();
        }

        #region Queries

        public List<InvoiceDTO> GetAll()
        {
            return _invoiceDAL.GetAll();
        }

        public InvoiceDTO GetById(int id)
        {
            return _invoiceDAL.GetById(id);
        }

        public DataTable GetUnpaidInvoices()
        {
            return _invoiceDAL.GetUnpaidInvoices();
        }

        /// <summary>
        /// Lấy danh sách phòng đang thuê để ghi chỉ số điện nước
        /// </summary>
        public DataTable GetRoomsForRecording()
        {
            const string sql = @"
                SELECT r.Id AS RoomId, r.RoomName, 
                       ISNULL((SELECT TOP 1 ElecNew FROM Invoices i 
                               INNER JOIN Contracts c ON i.ContractId = c.Id 
                               WHERE c.RoomId = r.Id ORDER BY i.Id DESC), 0) AS ElecOld,
                       ISNULL((SELECT TOP 1 WaterNew FROM Invoices i 
                               INNER JOIN Contracts c ON i.ContractId = c.Id 
                               WHERE c.RoomId = r.Id ORDER BY i.Id DESC), 0) AS WaterOld
                FROM Rooms r 
                WHERE r.Status = 'DangThue'
                ORDER BY r.RoomName";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        #endregion

        #region Statistics

        public decimal GetTotalRevenueByYear(int year)
        {
            return _invoiceDAL.GetTotalRevenueByYear(year);
        }

        public decimal GetTotalRevenueByMonth(int month, int year)
        {
            return _invoiceDAL.GetTotalRevenueByMonth(month, year);
        }

        public int GetUnpaidCount()
        {
            return _invoiceDAL.GetUnpaidCount();
        }

        public DataTable GetMonthlyRevenueByYear(int year)
        {
            return _invoiceDAL.GetMonthlyRevenueByYear(year);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Tạo hóa đơn cho phòng
        /// </summary>
        public OperationResult CreateInvoice(int roomId, int elecNew, int waterNew, int month, int year)
        {
            // Validation
            if (elecNew < 0 || waterNew < 0)
                return OperationResult.Fail("Chỉ số điện nước không hợp lệ");

            // Lấy hợp đồng đang active của phòng
            var contract = _contractDAL.GetActiveByRoomId(roomId);
            if (contract == null)
                return OperationResult.Fail("Không tìm thấy hợp đồng cho phòng này!");

            // Kiểm tra đã có hóa đơn tháng này chưa
            if (_invoiceDAL.InvoiceExistsForMonth(contract.Id, month, year))
                return OperationResult.Fail($"Đã có hóa đơn tháng {month}/{year} cho phòng này!");

            // Lấy chỉ số cũ từ hóa đơn trước
            var lastInvoice = _invoiceDAL.GetLastInvoiceByRoom(roomId);
            int elecOld = lastInvoice?.ElecNew ?? 0;
            int waterOld = lastInvoice?.WaterNew ?? 0;

            // Validate chỉ số mới >= chỉ số cũ
            if (elecNew < elecOld)
                return OperationResult.Fail($"Chỉ số điện mới ({elecNew}) không thể nhỏ hơn chỉ số cũ ({elecOld})");

            if (waterNew < waterOld)
                return OperationResult.Fail($"Chỉ số nước mới ({waterNew}) không thể nhỏ hơn chỉ số cũ ({waterOld})");

            // Lấy đơn giá từ Settings
            decimal elecPrice = _settingDAL.GetElectricPrice();
            decimal waterPrice = _settingDAL.GetWaterPrice();

            // Lấy giá phòng
            var room = _roomDAL.GetById(roomId);
            decimal roomPrice = room?.Price ?? contract.MonthlyRent;

            // Tính toán
            int elecUsage = elecNew - elecOld;
            int waterUsage = waterNew - waterOld;
            decimal elecAmount = elecUsage * elecPrice;
            decimal waterAmount = waterUsage * waterPrice;
            decimal totalAmount = roomPrice + elecAmount + waterAmount;

            // Tạo hóa đơn
            var invoice = new InvoiceDTO
            {
                ContractId = contract.Id,
                Month = month,
                Year = year,
                ElecOld = elecOld,
                ElecNew = elecNew,
                ElecPrice = elecPrice,
                WaterOld = waterOld,
                WaterNew = waterNew,
                WaterPrice = waterPrice,
                RoomPrice = roomPrice,
                TotalAmount = totalAmount,
                Status = "ChuaThanhToan"
            };

            int newId = _invoiceDAL.Insert(invoice);
            return newId > 0
                ? OperationResult.Success($"Tạo hóa đơn thành công. Tổng tiền: {totalAmount:N0} VNĐ")
                : OperationResult.Fail("Có lỗi xảy ra khi tạo hóa đơn");
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán
        /// </summary>
        public OperationResult MarkAsPaid(int invoiceId)
        {
            var invoice = _invoiceDAL.GetById(invoiceId);
            if (invoice == null)
                return OperationResult.Fail("Không tìm thấy hóa đơn");

            if (invoice.Status == "DaThanhToan")
                return OperationResult.Fail("Hóa đơn đã được thanh toán trước đó");

            return _invoiceDAL.MarkAsPaid(invoiceId)
                ? OperationResult.Success("Đã đánh dấu thanh toán thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        /// <summary>
        /// Xóa hóa đơn
        /// </summary>
        public OperationResult DeleteInvoice(int invoiceId)
        {
            var invoice = _invoiceDAL.GetById(invoiceId);
            if (invoice == null)
                return OperationResult.Fail("Không tìm thấy hóa đơn");

            if (invoice.Status == "DaThanhToan")
                return OperationResult.Fail("Không thể xóa hóa đơn đã thanh toán");

            return _invoiceDAL.Delete(invoiceId)
                ? OperationResult.Success("Xóa hóa đơn thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi xóa");
        }

        #endregion

        #region Tax Calculation

        /// <summary>
        /// Tính thuế cho hộ kinh doanh theo quy định
        /// </summary>
        public TaxResult CalculateTax(int year)
        {
            decimal yearlyRevenue = _invoiceDAL.GetTotalRevenueByYear(year);
            return TaxCalculator.Calculate(yearlyRevenue);
        }

        #endregion
    }

    /// <summary>
    /// Tính thuế cho hộ kinh doanh theo Thông tư 40/2021/TT-BTC
    /// </summary>
    public static class TaxCalculator
    {
        // Ngưỡng doanh thu chịu thuế: 100 triệu/năm
        private const decimal THRESHOLD = 100_000_000m;
        
        // Thuế suất GTGT: 5% cho dịch vụ lưu trú
        private const decimal VAT_RATE = 0.05m;
        
        // Thuế suất TNCN: 1.5% cho hộ kinh doanh cho thuê tài sản
        private const decimal PIT_RATE = 0.015m;

        public static TaxResult Calculate(decimal yearlyRevenue)
        {
            var result = new TaxResult
            {
                YearlyRevenue = yearlyRevenue,
                Threshold = THRESHOLD
            };

            if (yearlyRevenue <= THRESHOLD)
            {
                // Dưới ngưỡng - miễn thuế
                result.IsTaxable = false;
                result.VATAmount = 0;
                result.PITAmount = 0;
                result.TotalTax = 0;
                result.Note = "Doanh thu dưới 100 triệu/năm - Không phải nộp thuế";
            }
            else
            {
                // Trên ngưỡng - tính thuế trên toàn bộ doanh thu
                result.IsTaxable = true;
                result.VATAmount = yearlyRevenue * VAT_RATE;
                result.PITAmount = yearlyRevenue * PIT_RATE;
                result.TotalTax = result.VATAmount + result.PITAmount;
                result.Note = $"Thuế GTGT (5%): {result.VATAmount:N0} + Thuế TNCN (1.5%): {result.PITAmount:N0}";
            }

            return result;
        }
    }

    public class TaxResult
    {
        public decimal YearlyRevenue { get; set; }
        public decimal Threshold { get; set; }
        public bool IsTaxable { get; set; }
        public decimal VATAmount { get; set; }
        public decimal PITAmount { get; set; }
        public decimal TotalTax { get; set; }
        public string Note { get; set; }
    }
}