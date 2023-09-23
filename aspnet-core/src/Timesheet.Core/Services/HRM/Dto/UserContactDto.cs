using System;

namespace Timesheet.Services.HRM.Dto
{
    public class UserContactDto
    {
        public long ContractId { get; set; }
        public DateTime? DOB { get; set; }
        public string IdCard { get; set; }
        public DateTime? IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ChildCompany { get; set; }
        public string AddressCompany { get; set; }
        public DateTime? ContractIssuedAt { get; set; }
        public double ContractSalary { get; set; }
        public string Job { get; set; }
    }
}