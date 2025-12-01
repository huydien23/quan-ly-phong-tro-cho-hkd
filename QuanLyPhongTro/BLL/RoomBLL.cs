using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class RoomBLL
    {
        private readonly RoomDAL _roomDAL;
        private readonly ContractDAL _contractDAL;

        public RoomBLL()
        {
            _roomDAL = new RoomDAL();
            _contractDAL = new ContractDAL();
        }

        #region Queries

        public List<RoomDTO> GetAllRooms()
        {
            return _roomDAL.GetAllRooms();
        }

        public RoomDTO GetById(int id)
        {
            return _roomDAL.GetById(id);
        }

        public DataTable GetRoomsWithTenant()
        {
            return _roomDAL.GetRoomsWithTenant();
        }

        public DataTable GetRoomTypes()
        {
            return _roomDAL.GetRoomTypes();
        }

        #endregion

        #region Commands

        public OperationResult AddRoom(string name, int typeId)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Fail("Tên phòng không được để trống");

            name = name.Trim();

            // Check trùng tên
            if (_roomDAL.CheckRoomNameExists(name))
                return OperationResult.Fail("Tên phòng đã tồn tại!");

            var room = new RoomDTO
            {
                RoomName = name,
                RoomTypeId = typeId,
                Status = "Trong"
            };

            int newId = _roomDAL.Insert(room);
            return newId > 0
                ? OperationResult.Success("Thêm phòng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi thêm phòng");
        }

        public OperationResult UpdateRoom(int id, string name, int typeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Fail("Tên phòng không được để trống");

            name = name.Trim();

            // Check trùng tên (trừ chính nó)
            if (_roomDAL.CheckRoomNameExists(name, id))
                return OperationResult.Fail("Tên phòng đã tồn tại!");

            var room = _roomDAL.GetById(id);
            if (room == null)
                return OperationResult.Fail("Không tìm thấy phòng");

            room.RoomName = name;
            room.RoomTypeId = typeId;

            return _roomDAL.Update(room)
                ? OperationResult.Success("Cập nhật phòng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi cập nhật");
        }

        public OperationResult DeleteRoom(int id)
        {
            // Kiểm tra phòng có đang cho thuê không
            if (_contractDAL.HasActiveContract(id))
                return OperationResult.Fail("Không thể xóa phòng đang cho thuê!");

            return _roomDAL.Delete(id)
                ? OperationResult.Success("Xóa phòng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi xóa phòng");
        }

        public OperationResult UpdateStatus(int roomId, string status)
        {
            return _roomDAL.UpdateStatus(roomId, status)
                ? OperationResult.Success()
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        public OperationResult AddRoomType(string typeName, decimal price, double area)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return OperationResult.Fail("Tên loại phòng không được để trống");

            if (price <= 0)
                return OperationResult.Fail("Giá phòng phải lớn hơn 0");

            int newId = _roomDAL.InsertRoomType(typeName.Trim(), price, area);
            return newId > 0
                ? OperationResult.Success("Thêm loại phòng thành công")
                : OperationResult.Fail("Có lỗi xảy ra");
        }

        #endregion
    }
}