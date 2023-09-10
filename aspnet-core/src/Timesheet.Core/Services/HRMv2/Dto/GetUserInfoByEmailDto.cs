using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.HRMv2.Dto
{
    public class GetUserInfoByEmailDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public Sex Sex { get; set; }
        public string StatusName { get; set; }
        public string UserTypeName { get; set; }
        public List<string> SkillNames { get; set; }
        public List<string> Teams { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public string IdCard { get; set; }
        public DateTime? IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string PlaceOfPermanent { get; set; }
        public string Address { get; set; }
        public string BankAccountNumber { get; set; }
        public float RemainLeaveDay { get; set; }
        public string TaxCode { get; set; }
        public InsuranceStatus InsuranceStatus { get; set; }
        public string InsuranceStatusName { get; set; }
        public string Branch { get; set; }
        public string Level { get; set; }
        public string JobPosition { get; set; }
        public string Bank { get; set; }

        public long? BankId { get; set; }
    }
    public class ItemInfoDto
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public bool IsDefault { get; set; }
    }
    public class ResultUpdateInfo
    {
        public bool IsSucess { get; set; }
        public string ResultMessage { get; set; }
    }
}
