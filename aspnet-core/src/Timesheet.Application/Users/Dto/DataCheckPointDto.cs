using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Users.Dto
{
    public class DataCheckPointDto
    {
        public string CheckPointName { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Branch { get; set; }
        public Usertype? Type { get; set; }
        public string TypeName { get; set; }
        public int WorkingTime { get; set; }
        public string LevelName { get; set; }
        public string ProjectNames { get; set; }
        public long ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerEmail { get; set; }
        public string ReviewerIsActive { get; set; }

        public override string ToString()
        {
            return $"{{ UserId = {UserId}, FullName = {FullName}, EmailAddress = {EmailAddress}, Branch = {Branch}, Type = {Type}, TypeName = {TypeName}, LevelName = {LevelName}, ReviewerId = {ReviewerId}, ReviewerName = {ReviewerName}, ReviewerEmail = {ReviewerEmail} }}";
        }
    }
}
