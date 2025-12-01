using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class ContractBLL
    {
        private readonly ContractDAL _contractDAL;
        private readonly RoomDAL _roomDAL;
        private readonly CustomerDAL _customerDAL;

        public ContractBLL()
        {
            _contractDAL = new ContractDAL();
            _roomDAL = new RoomDAL();
            _customerDAL = new CustomerDAL();
        }

        #region Queries

        public List<ContractDTO> GetAll()
        {
            return _contractDAL.GetAll();
        }

        public ContractDTO GetById(int id)
        {
            return _contractDAL.GetById(id);
        }

        public ContractDTO GetActiveByRoomId(int roomId)
        {
            return _contractDAL.GetActiveByRoomId(roomId);
        }

        public DataTable GetActiveContracts()
        {
            return _contractDAL.GetActiveContracts();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Tạo hợp đồng mới với Transaction để đảm bảo tính nhất quán
        /// </summary>
        public OperationResult CreateContract(ContractDTO contract)
        {
            // Validation
            if (contract.RoomId <= 0)
                return OperationResult.Fail("Vui lòng chọn phòng");

            if (contract.CustomerId <= 0)
                return OperationResult.Fail("Vui lòng chọn khách thuê");

            if (contract.StartDate == default)
                return OperationResult.Fail("Vui lòng chọn ngày bắt đầu");

            if (contract.Deposit < 0)
                return OperationResult.Fail("Tiền cọc không hợp lệ");

            // Kiểm tra phòng có đang thuê không
            var room = _roomDAL.GetById(contract.RoomId);
            if (room == null)
                return OperationResult.Fail("Không tìm thấy phòng");

            if (room.Status == "DangThue")
                return OperationResult.Fail("Phòng này đang có người thuê!");

            // Sử dụng Transaction để đảm bảo cả 2 thao tác đều thành công
            using (var db = new DatabaseHelper())
            {
                try
                {
                    db.BeginTransaction();

                    // 1. Tạo hợp đồng
                    contract.IsActive = true;
                    if (contract.MonthlyRent <= 0)
                        contract.MonthlyRent = room.Price;

                    int contractId = _contractDAL.InsertWithTransaction(db, contract);
                    if (contractId <= 0)
                    {
                        db.Rollback();
                        return OperationResult.Fail("Không thể tạo hợp đồng");
                    }

                    // 2. Cập nhật trạng thái phòng
                    const string updateRoomSql = "UPDATE Rooms SET Status = @Status WHERE Id = @Id";
                    db.ExecuteNonQueryWithTransaction(updateRoomSql,
                        DatabaseHelper.CreateParameter("@Status", "DangThue"),
                        DatabaseHelper.CreateParameter("@Id", contract.RoomId));

                    db.Commit();
                    return OperationResult.Success("Tạo hợp đồng thành công");
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    return OperationResult.Fail("Lỗi: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Kết thúc hợp đồng
        /// </summary>
        public OperationResult EndContract(int contractId)
        {
            var contract = _contractDAL.GetById(contractId);
            if (contract == null)
                return OperationResult.Fail("Không tìm thấy hợp đồng");

            if (!contract.IsActive)
                return OperationResult.Fail("Hợp đồng đã kết thúc trước đó");

            using (var db = new DatabaseHelper())
            {
                try
                {
                    db.BeginTransaction();

                    // 1. Deactivate hợp đồng
                    _contractDAL.DeactivateWithTransaction(db, contractId);

                    // 2. Cập nhật trạng thái phòng về trống
                    const string updateRoomSql = "UPDATE Rooms SET Status = @Status WHERE Id = @Id";
                    db.ExecuteNonQueryWithTransaction(updateRoomSql,
                        DatabaseHelper.CreateParameter("@Status", "Trong"),
                        DatabaseHelper.CreateParameter("@Id", contract.RoomId));

                    db.Commit();
                    return OperationResult.Success("Kết thúc hợp đồng thành công");
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    return OperationResult.Fail("Lỗi: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin hợp đồng
        /// </summary>
        public OperationResult UpdateContract(ContractDTO contract)
        {
            if (contract.Id <= 0)
                return OperationResult.Fail("Hợp đồng không hợp lệ");

            return _contractDAL.Update(contract)
                ? OperationResult.Success("Cập nhật hợp đồng thành công")
                : OperationResult.Fail("Có lỗi xảy ra khi cập nhật");
        }

        #endregion
    }
}