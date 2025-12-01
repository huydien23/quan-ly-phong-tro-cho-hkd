using System;

namespace QuanLyPhongTro.DTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public int CustomerId { get { return Id; } set { Id = value; } }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string CCCD { get; set; }
        public string IdentityCard { get { return CCCD; } set { CCCD = value; } } // Alias
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Job { get; set; }
        public string Workplace { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}