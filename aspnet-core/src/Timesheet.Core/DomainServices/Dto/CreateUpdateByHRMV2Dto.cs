using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class CreateUpdateByHRMV2Dto 
    {
        public Sex Sex { get; set; }
        public Usertype Type { get; set; }
        public string EmailAddress { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string BranchCode { get; set; }
        public string LevelCode { get; set; }
        public string PositionCode { get; set; }
        public DateTime WorkingStartDate { get; set; }
        public UserLevel Level
        {
            get
            {
                var userLevel = CommonUtils.GetUserLevelByLevelCode(LevelCode);
                if(userLevel > UserLevel.SeniorPlus || userLevel < UserLevel.Intern_0)
                {
                    userLevel = UserType == Usertype.Internship ? UserLevel.Intern_0 : UserLevel.FresherMinus;
                }
                return userLevel;
            }
        }
        public Usertype UserType => CommonUtils.GetUserTypeByTypeFromHrmV2(Type);
        public Sex GetSex => CommonUtils.GetSexBySexFromHrmV2(Sex);
    }

}
