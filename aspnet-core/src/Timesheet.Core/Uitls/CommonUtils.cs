using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Uitls
{
    public class CommonUtils
    {
        public static string GenerateRandomPassword(byte length)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);

        }

        public static string AddMoreHourToHHmm(string HHmm, double hour)
        {
            if (HHmm.Contains(":"))
            {
                var t = HHmm.Split(':');
                double minute = double.Parse(t[0]) * 60 + double.Parse(t[1]) + hour * 60;

                return (int) (minute / 60) + ":" + minute % 60;

            }
            return HHmm;

        }

        public static double SubtractHHmm(string HHmm1, string HHmm2)
        {
            if (string.IsNullOrEmpty(HHmm1) || string.IsNullOrEmpty(HHmm2))
                return 0;
            if( HHmm1.Contains(":") &&  HHmm2.Contains(":"))
            {
                var t1 = HHmm1.Split(':');
                var t2 = HHmm2.Split(':');
                return double.Parse(t1[0]) * 60 + double.Parse(t1[1]) - (double.Parse(t2[0]) * 60 + double.Parse(t2[1]));
            }
            return 0;
        }
        public static double GetEmployeeWorkingHours(string HHmm1, string HHmm2)
        {
            if (string.IsNullOrEmpty(HHmm1) || string.IsNullOrEmpty(HHmm2))
                return 0;
            if (HHmm1.Contains(":") && HHmm2.Contains(":"))
            {
                var t1 = HHmm1.Split(':');
                var t2 = HHmm2.Split(':');
                if (double.Parse(t2[0]) > 12)
                {
                    return double.Parse(t1[0]) * 60 + double.Parse(t1[1]) - (double.Parse(t2[0]) * 60 + double.Parse(t2[1]));
                }
                return double.Parse(t1[0]) * 60 + double.Parse(t1[1]) - (double.Parse(t2[0]) * 60 + double.Parse(t2[1])) - 60;
            }
            return 0;
        }

        public string ConvertHourToHHmm(double hour)
        {
            return TimeSpan.FromHours(hour).ToString(@"hh\:mm");
        }

        public static string ConvertHourToHHmm(int workingMinute)
        {
            int hours = workingMinute / 60;
            int minute = workingMinute % 60;
            if (minute == 0)
            {
                return hours >= 10 ? hours.ToString() + "h" : "0" + hours + "h";
            }
            else
            {
                string result = hours + ":" + (minute >= 10 ? minute.ToString() : "0" + minute);
                if (hours >= 10)
                {
                    return result;
                }
                else
                {
                    return "0" + result;
                }
            }
        }

        public static string RequestTypeToString(RequestType requestType)
        {
            return Enum.GetName(typeof(RequestType), requestType);
        }

        public static string RequestTypeToString(RequestType requestType, string OffTypeName)
        {
            if (requestType == RequestType.Off)
            {
                return OffTypeName;
            }
            return Enum.GetName(typeof(RequestType), requestType);
        }

        public static string BranchName(Branch? branch)
        {
            if (!branch.HasValue)
            {
                return "NoBranch";
            }
            switch (branch)
            {
                case Branch.DaNang:
                    return "ĐN";
                case Branch.HaNoi:
                    return "HN";
                case Branch.HoChiMinh:
                    return "HCM";
                case Branch.Vinh:
                    return "Vinh";
            }
            return Enum.GetName(typeof(Branch), branch);
        }

        public static string UserTypeName(Usertype? type)
        {
            if (!type.HasValue)
            {
                return "NoType";
            }
            switch (type)
            {
                case Usertype.Staff:
                    return "Staff";
                case Usertype.Internship:
                    return "TTS";
                case Usertype.Collaborators:
                    return "CTV";
            }
            return Enum.GetName(typeof(Usertype), type);
        }

        public static string RequestStatusName(RequestStatus status)
        {
            return Enum.GetName(typeof(RequestStatus), status);
        }

        public static string UserLevelName(UserLevel? level)
        {
            return Enum.GetName(typeof(UserLevel), level);
        }


        public static Dictionary<UserLevel, string> UserLevelDetail ()
        {
            return new Dictionary<UserLevel, string>()
            {
              { UserLevel.Intern_0,"- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_1, "- Hỗ trợ 1.000.000 VNĐ <br>- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_2, "- Hỗ trợ 2.000.000 VNĐ <br>- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_3, "- Hỗ trợ 4.000.000 VNĐ <br>- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.FresherMinus, "- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Fresher, "- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.FresherPlus, "- Hỗ trợ ăn trưa 800.000 VNĐ <br>- Hỗ trợ gửi xe 100.000 VNĐ <br> Tất cả hỗ trợ tính trên giờ làm việc thực tế" }
            };

        }

        public static string TypeOfWorkName(TypeOfWork typeOfWork)
        {
           if (typeOfWork == TypeOfWork.NormalWorkingHours)
            {
                return "Normal Working";
            }
            return "OT";

        }

        public static string ChargeName(bool isCharge)
        {
            return isCharge? "Charged" : "No Charge";

        }

        public static string ProjectUserWorkType(bool isTemp)
        {
            return isTemp ? "Temp" : "Official";
        }

        public static double ConvertMinuteToHour(int minute)
        {
            return (double)minute / 60;
        }
        public static string LevelName(UserLevel? userLevel)
        {
            switch (userLevel)
            {
                case UserLevel.Intern_0:
                    return "I0";
                case UserLevel.Intern_1:
                    return "I1";
                case UserLevel.Intern_2:
                    return "I2";
                case UserLevel.Intern_3:
                    return "I3";
                case UserLevel.FresherMinus:
                    return "F-";
                case UserLevel.Fresher:
                    return "F";
                case UserLevel.FresherPlus:
                    return "F+";
                case UserLevel.MiddleMinus:
                    return "M-";
                case UserLevel.Middle:
                    return "M";
                case UserLevel.MiddlePlus:
                    return "M+";
                case UserLevel.JuniorMinus:
                    return "J-";
                case UserLevel.Junior:
                    return "J";
                case UserLevel.JuniorPlus:
                    return "J+";
                case UserLevel.SeniorMinus:
                    return "S-";
                case UserLevel.Senior:
                    return "S";
                case UserLevel.SeniorPlus:
                    return "P";
                default:
                    return "";
            }
        }
        public static string GetReviewDetailDisplay(bool isStart, bool isStop, UserLevel? beginLevel, UserLevel? nextLevel, UserLevel? level)
        {
            string display = "";
            if (isStart)
            {
                display += $"{CommonUtils.LevelName(beginLevel)}";
            }
            if (isStop)
            {
                display += level.HasValue && display != "" ? " -> " : "";
                display += level.HasValue ? CommonUtils.LevelName(level) : "";
                return display;
            }
            if (nextLevel.HasValue)
            {
                display += nextLevel.HasValue && display != "" ? " -> " : "";
                display += CommonUtils.LevelName(nextLevel);
            }
            return display;
        }

        public static UserLevel GetUserLevelByLevelCode(string levelCode)
        {
            return Enum.Parse<UserLevel>(levelCode);
        }
        public static Usertype GetUserTypeByTypeFromHrmV2(Usertype typeFromHrmV2)
        {
            var userTypeMapers = new Usertype[] { Usertype.Internship, Usertype.Collaborators, Usertype.Staff, Usertype.Staff };
            return userTypeMapers[(int)typeFromHrmV2];
        }
        public static Sex GetSexBySexFromHrmV2(Sex sexFromHrmV2)
        {
            var sexMapers = new Sex[] { Sex.Male, Sex.Male, Sex.Female };
            return sexMapers[(int)sexFromHrmV2];
        }

        public static List<string> SeparateMessage(string[] arrMessage, int maxlength, string separteStr)
        {
            int totalLength = 0;
            var sb = new StringBuilder();
            var resultList = new List<string>();

            for(int i = 0; i < arrMessage.Length; i++)
            {
                var item = arrMessage[i];
                if (totalLength + item.Length < maxlength)
                {
                    sb.Append(item);
                    sb.Append(separteStr);
                    totalLength += item.Length + separteStr.Length;
                }
                else
                {
                    resultList.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(item);
                    sb.Append(separteStr);
                    totalLength = item.Length + separteStr.Length;
                }
                if (i == arrMessage.Length - 1)
                {
                    resultList.Add(sb.ToString());
                }
            }

            return resultList;
        }

        public static List<string> SeparateMessage(string message, int maxlength, string separteStr)
        {
            if (message.Length <= maxlength)
            {
                return new List<string> { message };
            }

            var arr = message.Split(separteStr);

            return SeparateMessage(arr, maxlength, separteStr);
        }

        public static string GetUserNameByEmail(string email)
        {
            return email.Split("@")[0];
        }
        public static string GetDiscordTagUser(string email)
        {
            return "${" + GetUserNameByEmail(email) + "}";
        }

        public static string GetNotifyKomuNoCheckInAndNoCheckOutPunish(string email, double registerTime, string trackerTime, float percentageConfig)
        {
            return $"{CommonUtils.GetDiscordTagUser(email)} " +
                        $"NO CHECK IN **AND** NO CHECK OUT " +
                        $"- register working hour: {DateTimeUtils.ConvertMinuteToHour(registerTime)}" +
                        $"- tracker time: {trackerTime}, " +
                        $" (tracker **KHÔNG** đủ : {Math.Round(percentageConfig * registerTime / 60, 2)} h) => 100K";
        }

        public static string GetNotifyKomuNoCheckInAndNoCheckOutNoPunish(string email, double registerTime, string trackerTime, float percentageConfig)
        {
            return $"{CommonUtils.GetDiscordTagUser(email)} " +
                        $"NO CHECK IN **AND** NO CHECK OUT " +
                        $"- register working hour: {DateTimeUtils.ConvertMinuteToHour(registerTime)}" +
                        $"- tracker time: {trackerTime}, " +
                        $" (tracker **ĐỦ** : {Math.Round(percentageConfig * registerTime / 60, 2)} h => thay check out) => 50K";
        }

        public static string GetNotifyKomuNoCheckOutPunish(string email)
        {
            return $"{CommonUtils.GetDiscordTagUser(email)} " +
                        $"NO CHECK OUT => 50K";
        }
        public static string GetErrorMessageReviewIntern(UserLevel? oldLevel, UserLevel? currenLevel)
        {
            if(!oldLevel.HasValue || !currenLevel.HasValue)
            {
                return "không có level";
            }
            if (currenLevel > UserLevel.Intern_3)
                return "không phải Intern";
            return $"Old level ({LevelName(oldLevel)}) khác Current level ({LevelName(currenLevel)})";
        }
        public static List<GetBGJobsDescription> BackgroundJobDescription(string jobAgr)
        {
        
            var listDes = new List<GetBGJobsDescription>()
            {
                new GetBGJobsDescription()
                {
                    SubJobType = "WorkingTimeBackgroundJob",
                    Description= "Nhân viên đăng kí thời gian làm việc"
                },
                new GetBGJobsDescription()
                {
                    SubJobType = "EmailBackgroundJob",
                    Description= "Gửi mail"
                },


            };
            return listDes;
        }
    }
}
public class GetBGJobsDescription
{
    public string SubJobType { get; set; }
    public string Description { get; set; }

}
