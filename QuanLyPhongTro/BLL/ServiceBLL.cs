using System.Collections.Generic;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class ServiceBLL
    {
        private readonly ServiceDAL _serviceDAL;

        public ServiceBLL()
        {
            _serviceDAL = new ServiceDAL();
        }

        #region Queries

        public List<ServiceDTO> GetAllServices(bool activeOnly = true)
        {
            return _serviceDAL.GetAll(activeOnly);
        }

        public ServiceDTO GetById(int id)
        {
            return _serviceDAL.GetById(id);
        }

        public ServiceDTO GetByCode(string code)
        {
            return _serviceDAL.GetByCode(code);
        }

        public List<RoomServiceDTO> GetRoomServices(int roomId)
        {
            return _serviceDAL.GetRoomServices(roomId);
        }

        // Helpers
        public decimal GetElectricPrice() => _serviceDAL.GetElectricPrice();
        public decimal GetWaterPrice() => _serviceDAL.GetWaterPrice();
        public bool IsWifiFree() => _serviceDAL.IsServiceFree("WIFI");
        public bool IsTrashFree() => _serviceDAL.IsServiceFree("RAC");
        public bool IsParkingFree() => _serviceDAL.IsServiceFree("XE_MAY");

        #endregion

        #region Commands

        public OperationResult UpdateService(ServiceDTO service)
        {
            if (string.IsNullOrWhiteSpace(service.ServiceName))
                return OperationResult.Fail("Tên dịch vụ không được để trống");

            if (service.UnitPrice < 0)
                return OperationResult.Fail("Đơn giá không hợp lệ");

            return _serviceDAL.Update(service)
                ? OperationResult.Success("Cập nhật dịch vụ thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult UpdateServicePrice(string code, decimal price, bool isFree)
        {
            if (price < 0)
                return OperationResult.Fail("Đơn giá không hợp lệ");

            return _serviceDAL.UpdatePrice(code, price, isFree)
                ? OperationResult.Success("Cập nhật giá thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult SaveRoomService(RoomServiceDTO rs)
        {
            return _serviceDAL.SaveRoomService(rs)
                ? OperationResult.Success("Lưu thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        #endregion
    }
}
