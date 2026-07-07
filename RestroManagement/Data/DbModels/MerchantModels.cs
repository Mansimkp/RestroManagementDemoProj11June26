using System;
using System.ComponentModel.DataAnnotations;

namespace RestroManagement.DbModels
{
    public class Merchant
    {
        [Key]
        public int UniqueId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BusinessLicense { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsVerified { get; set; }
    }

    public class Store
    {
        [Key]
        public int UniqueId { get; set; }
        public int MerchantId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string? StoreDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public enum MerchantStaffRole
    {
        POC,
        Admin,
        Staff
    }

    public class MerchantStaff
    {
        [Key]
        public int UniqueId { get; set; }
        public int MerchantId { get; set; }
        public int UserId { get; set; }
        public MerchantStaffRole Role { get; set; }
        public string? Designation { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
