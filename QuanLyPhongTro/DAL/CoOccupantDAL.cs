using System.Collections.Generic;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class CoOccupantDAL : BaseRepository
    {
        public List<CoOccupantDTO> GetByContract(int contractId)
        {
            const string sql = "SELECT * FROM CoOccupants WHERE ContractId = @ContractId ORDER BY OccupantId";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@ContractId", contractId));
            return MapToList<CoOccupantDTO>(dt);
        }

        public CoOccupantDTO GetById(int id)
        {
            const string sql = "SELECT * FROM CoOccupants WHERE OccupantId = @Id";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<CoOccupantDTO>(dt.Rows[0]) : null;
        }

        public int Insert(CoOccupantDTO occupant)
        {
            const string sql = @"
                INSERT INTO CoOccupants (ContractId, FullName, CCCD, Phone, Relationship, DateOfBirth, Gender, Job, Note)
                VALUES (@ContractId, @FullName, @CCCD, @Phone, @Relationship, @DOB, @Gender, @Job, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@ContractId", occupant.ContractId),
                Param("@FullName", occupant.FullName),
                Param("@CCCD", occupant.CCCD),
                Param("@Phone", occupant.Phone),
                Param("@Relationship", occupant.Relationship),
                Param("@DOB", occupant.DateOfBirth),
                Param("@Gender", occupant.Gender),
                Param("@Job", occupant.Job),
                Param("@Note", occupant.Note));

            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        public bool Update(CoOccupantDTO occupant)
        {
            const string sql = @"
                UPDATE CoOccupants SET 
                    FullName = @FullName, CCCD = @CCCD, Phone = @Phone,
                    Relationship = @Relationship, DateOfBirth = @DOB, 
                    Gender = @Gender, Job = @Job, Note = @Note
                WHERE OccupantId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", occupant.OccupantId),
                Param("@FullName", occupant.FullName),
                Param("@CCCD", occupant.CCCD),
                Param("@Phone", occupant.Phone),
                Param("@Relationship", occupant.Relationship),
                Param("@DOB", occupant.DateOfBirth),
                Param("@Gender", occupant.Gender),
                Param("@Job", occupant.Job),
                Param("@Note", occupant.Note)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM CoOccupants WHERE OccupantId = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        public int CountByContract(int contractId)
        {
            const string sql = "SELECT COUNT(*) FROM CoOccupants WHERE ContractId = @ContractId";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@ContractId", contractId));
            return result != null ? System.Convert.ToInt32(result) : 0;
        }
    }
}
