using System;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class PunishmentSystemImportDto
    {
        public int Row { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UserPunishmentType Type { get; set; }
        public string OriginalTypeValue { get; set; }
        public bool IsValidType { get; set; } = true;
        public int Money { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> ParsingErrors { get; set; } = new List<string>();
        
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(OriginalTypeValue);
        }
    }
}
