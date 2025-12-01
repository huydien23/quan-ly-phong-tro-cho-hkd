using System;

namespace QuanLyPhongTro.DTO
{
    public class CoOccupantDTO
    {
        public int OccupantId { get; set; }
        public int ContractId { get; set; }
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }  // 'VoChong', 'ConCai', 'BanBe'
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Job { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RelationshipDisplay
        {
            get
            {
                switch (Relationship)
                {
                    case "VoChong": return "Vợ/Chồng";
                    case "ConCai": return "Con cái";
                    case "BanBe": return "Bạn bè";
                    case "AnhChiEm": return "Anh chị em";
                    default: return Relationship;
                }
            }
        }
    }
}
