using System.Collections.Generic;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class CustomerBLL
    {
        private readonly CustomerDAL _customerDAL;

        public CustomerBLL()
        {
            _customerDAL = new CustomerDAL();
        }

        #region Queries

        public List<CustomerDTO> GetAll()
        {
            return _customerDAL.GetAll();
        }

        public CustomerDTO GetById(int id)
        {
            return _customerDAL.GetById(id);
        }

        public List<CustomerDTO> Search(string keyword)
        {
            return _customerDAL.Search(keyword);
        }

        #endregion

        #region Commands

        public OperationResult AddCustomer(CustomerDTO customer)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(customer.FullName))
                return OperationResult.Fail("Họ tên không được để trống");

            if (string.IsNullOrWhiteSpace(customer.CCCD))
                return OperationResult.Fail("CCCD không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Phone))
                return OperationResult.Fail("Số điện thoại không được để trống");

            // Trim data
            customer.FullName = customer.FullName.Trim();
            customer.CCCD = customer.CCCD.Trim();
            customer.Phone = customer.Phone.Trim();
            customer.Address = customer.Address?.Trim();
            customer.Email = customer.Email?.Trim();

            // Check CCCD trùng
            if (_customerDAL.CheckCCCDExists(customer.CCCD))
                return OperationResult.Fail("CCCD đã tồn tại trong hệ thống!");

            int newId = _customerDAL.Insert(customer);
            return newId > 0
                ? OperationResult.Success("Thêm khách hàng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi thêm khách hàng");
        }

        public OperationResult UpdateCustomer(CustomerDTO customer)
        {
            if (customer.Id <= 0)
                return OperationResult.Fail("Khách hàng không hợp lệ");

            if (string.IsNullOrWhiteSpace(customer.FullName))
                return OperationResult.Fail("Họ tên không được để trống");

            if (string.IsNullOrWhiteSpace(customer.CCCD))
                return OperationResult.Fail("CCCD không được để trống");

            // Check CCCD trùng (trừ chính mình)
            if (_customerDAL.CheckCCCDExists(customer.CCCD, customer.Id))
                return OperationResult.Fail("CCCD đã tồn tại trong hệ thống!");

            return _customerDAL.Update(customer)
                ? OperationResult.Success("Cập nhật khách hàng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi cập nhật");
        }

        public OperationResult DeleteCustomer(int id)
        {
            // TODO: Kiểm tra xem khách hàng có hợp đồng đang active không
            return _customerDAL.Delete(id)
                ? OperationResult.Success("Xóa khách hàng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi xóa");
        }

        #endregion
    }
}
