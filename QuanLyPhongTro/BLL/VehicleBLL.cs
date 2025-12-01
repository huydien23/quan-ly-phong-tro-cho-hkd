using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class VehicleBLL
    {
        private readonly VehicleDAL _vehicleDAL;

        public VehicleBLL()
        {
            _vehicleDAL = new VehicleDAL();
        }

        #region Queries

        public List<VehicleDTO> GetAll()
        {
            return _vehicleDAL.GetAll();
        }

        public List<VehicleDTO> GetByCustomer(int customerId)
        {
            return _vehicleDAL.GetByCustomer(customerId);
        }

        public VehicleDTO GetById(int id)
        {
            return _vehicleDAL.GetById(id);
        }

        public DataTable GetVehiclesWithRoom()
        {
            return _vehicleDAL.GetVehiclesWithRoom();
        }

        public int GetVehicleCountByCustomer(int customerId)
        {
            return _vehicleDAL.CountByCustomer(customerId);
        }

        #endregion

        #region Commands

        public OperationResult AddVehicle(VehicleDTO vehicle)
        {
            if (vehicle.CustomerId <= 0)
                return OperationResult.Fail("Vui lòng chọn chủ xe");

            if (string.IsNullOrWhiteSpace(vehicle.VehicleType))
                return OperationResult.Fail("Vui lòng chọn loại xe");

            // Xe máy/ô tô phải có biển số
            if (vehicle.VehicleType != "XeDap" && string.IsNullOrWhiteSpace(vehicle.LicensePlate))
                return OperationResult.Fail("Vui lòng nhập biển số xe");

            vehicle.LicensePlate = vehicle.LicensePlate?.Trim().ToUpper();
            vehicle.Brand = vehicle.Brand?.Trim();
            vehicle.Color = vehicle.Color?.Trim();

            int newId = _vehicleDAL.Insert(vehicle);
            return newId > 0
                ? OperationResult.Success("Thêm xe thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult UpdateVehicle(VehicleDTO vehicle)
        {
            if (vehicle.VehicleId <= 0)
                return OperationResult.Fail("Xe không hợp lệ");

            return _vehicleDAL.Update(vehicle)
                ? OperationResult.Success("Cập nhật xe thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult DeleteVehicle(int id)
        {
            return _vehicleDAL.Delete(id)
                ? OperationResult.Success("Xóa xe thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        #endregion
    }
}
