using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.HRMv2.Dto
{
    public class UpdateUserInfoDto
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public string IdCard { get; set; }
        public DateTime? IssuedOn { get; set; }
        public string IssuedBy { get; set; }
        public string PlaceOfPermanent { get; set; }
        public string Address { get; set; }
        public string BankAccountNumber { get; set; }
        public string TaxCode { get; set; }
        public long? BankId { get; set; }
    }

    public class GetInfoToUPDateProfile : UpdateUserInfoDto
    {
        public string RequestStatusName { get; set; }
    }
}
