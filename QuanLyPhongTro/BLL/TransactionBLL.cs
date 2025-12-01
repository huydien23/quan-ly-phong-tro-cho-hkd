using System;
using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class TransactionBLL
    {
        private readonly TransactionDAL _transactionDAL;

        public TransactionBLL()
        {
            _transactionDAL = new TransactionDAL();
        }

        #region Queries

        public List<TransactionDTO> GetAll(DateTime? fromDate = null, DateTime? toDate = null, string type = null)
        {
            return _transactionDAL.GetAll(fromDate, toDate, type);
        }

        public DataTable GetAllAsDataTable(int? month = null, int? year = null, string type = null)
        {
            return _transactionDAL.GetAllAsDataTable(month, year, type);
        }

        public TransactionDTO GetById(int id)
        {
            return _transactionDAL.GetById(id);
        }

        public List<TransactionCategoryDTO> GetCategories(string type = null)
        {
            return _transactionDAL.GetCategories(type);
        }

        #endregion

        #region Statistics

        public TransactionSummary GetMonthlySummary(int month, int year)
        {
            return _transactionDAL.GetSummary(month, year);
        }

        public decimal GetYearlyIncome(int year)
        {
            return _transactionDAL.GetTotalIncomeByYear(year);
        }

        public decimal GetYearlyExpense(int year)
        {
            return _transactionDAL.GetTotalExpenseByYear(year);
        }

        public decimal GetYearlyProfit(int year)
        {
            return GetYearlyIncome(year) - GetYearlyExpense(year);
        }

        #endregion

        #region Commands

        public OperationResult AddTransaction(TransactionDTO trans)
        {
            // Validation
            if (trans.TransactionDate == default)
                return OperationResult.Fail("Vui lòng chọn ngày giao dịch");

            if (string.IsNullOrWhiteSpace(trans.TransactionType))
                return OperationResult.Fail("Vui lòng chọn loại (Thu/Chi)");

            if (trans.Amount <= 0)
                return OperationResult.Fail("Số tiền phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(trans.CategoryCode))
                return OperationResult.Fail("Vui lòng chọn loại thu/chi");

            // Set CreatedBy
            trans.CreatedBy = CurrentUser.Username;

            int newId = _transactionDAL.Insert(trans);
            return newId > 0
                ? OperationResult.Success($"Thêm phiếu {trans.TransactionType.ToLower()} thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult UpdateTransaction(TransactionDTO trans)
        {
            if (trans.TransactionId <= 0)
                return OperationResult.Fail("Giao dịch không hợp lệ");

            if (trans.Amount <= 0)
                return OperationResult.Fail("Số tiền phải lớn hơn 0");

            return _transactionDAL.Update(trans)
                ? OperationResult.Success("Cập nhật thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult DeleteTransaction(int id)
        {
            var trans = _transactionDAL.GetById(id);
            if (trans == null)
                return OperationResult.Fail("Không tìm thấy giao dịch");

            return _transactionDAL.Delete(id)
                ? OperationResult.Success("Xóa thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        #endregion

        #region Auto Create from Invoice

        /// <summary>
        /// Tự động tạo phiếu thu khi đánh dấu hóa đơn đã thu
        /// </summary>
        public OperationResult CreateFromInvoice(int invoiceId, decimal amount, string roomName, string customerName, string paymentMethod)
        {
            var trans = new TransactionDTO
            {
                TransactionDate = DateTime.Today,
                TransactionType = "Thu",
                CategoryCode = "TIEN_PHONG",
                CategoryName = "Thu tiền phòng",
                Amount = amount,
                InvoiceId = invoiceId,
                Description = $"Thu tiền phòng {roomName} - {customerName}",
                PaymentMethod = paymentMethod ?? "TienMat",
                ReceivedBy = CurrentUser.FullName,
                CreatedBy = CurrentUser.Username
            };

            int newId = _transactionDAL.Insert(trans);
            return newId > 0
                ? OperationResult.Success()
                : OperationResult.Fail("Không thể tạo phiếu thu");
        }

        #endregion
    }
}
