using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class UserPunishmentImportDto
    {
        public int Row { get; set; }
        public string Email { get; set; }
        public UserPunishmentType Type { get; set; }
        public string OriginalTypeValue { get; set; } 

        public int Count { get; set; }
        public int Money { get; set; }
        public DateTime DateAt { get; set; }
        public string OriginalDateAtValue { get; set; }
        public bool IsValidDate { get; set; } = true;
        public string UserNote { get; set; }
        public string NoteReply { get; set; }
        public List<string> ParsingErrors { get; set; } = new List<string>();

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Email);
        }
    }
}
