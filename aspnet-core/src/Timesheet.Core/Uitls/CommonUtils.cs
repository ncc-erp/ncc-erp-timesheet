using System;
using System.Collections.Generic;
using System.Linq;
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

                int hours = (int) (minute / 60);
                int minutes = (int) (minute % 60);

                return $"{hours:D2}:{minutes:D2}";

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
                case Usertype.ProbationaryStaff:
                    return "Probationary Staff";
                case Usertype.Vendor:
                    return "Vendor";
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
              { UserLevel.Intern_0,"- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_1, "- Hỗ trợ 1.000.000 VNĐ <br>- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_2, "- Hỗ trợ 2.000.000 VNĐ <br>- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Intern_3, "- Hỗ trợ 4.000.000 VNĐ <br>- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.FresherMinus, "- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.Fresher, "- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" },
              { UserLevel.FresherPlus, "- Hỗ trợ ăn trưa 1.000.000 VNĐ (500.000 VNĐ được thanh toán bằng token - Số còn lại được thanh toán qua chuyển khoản) <br>- Hỗ trợ gửi xe 100.000 VNĐ <br>Tất cả hỗ trợ tính trên giờ làm việc thực tế" }
            };

        }

        public static Dictionary<ReviewInternStatus, string> ReviewInternStatusString ()
        {
            return new Dictionary<ReviewInternStatus, string>()
            {
                { ReviewInternStatus.Draft, "Draft" },
                { ReviewInternStatus.Reviewed, "Reviewed" },
                { ReviewInternStatus.Approved, "Approved" },
                { ReviewInternStatus.SentEmail, "Sent Email" },
                { ReviewInternStatus.Rejected, "Rejected" },
                { ReviewInternStatus.PmReviewed, "PM Reviewed" },
                { ReviewInternStatus.ReOpen, "ReOpen" }
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
            var userTypeMap = new Dictionary<Usertype, Usertype>
            {
                { Usertype.Staff, Usertype.Staff },
                { Usertype.Internship, Usertype.Internship },
                { Usertype.Collaborators, Usertype.Collaborators },
                { Usertype.ProbationaryStaff, Usertype.ProbationaryStaff },
                { Usertype.Vendor, Usertype.Vendor }
            };
            if (userTypeMap.ContainsKey(typeFromHrmV2))
            {
                return userTypeMap[typeFromHrmV2];
            }
            throw new ArgumentOutOfRangeException(nameof(typeFromHrmV2), $"Invalid user type value from HRM V2: {typeFromHrmV2}");
        }
        public static Dictionary<WhitelistType, string> WhitelistTypeName()
        {
            return new Dictionary<WhitelistType, string>
            {
                { WhitelistType.TrackerTime, "TRACKER_TIME" },
                { WhitelistType.FullyRemote, "FULLY_REMOTE" }
            };
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
        public static List<List<dynamic>> SplitIntoChunks(List<dynamic> details, int batchSize)
        {
            var chunks = new List<List<dynamic>>();
            for (int i = 0; i < details.Count; i += batchSize)
            {
                var chunk = details.Skip(i).Take(batchSize).ToList();
                chunks.Add(chunk);
            }
            return chunks;
        }

        private static string FormatWithBullet(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "<div>Không có</div>";

            var lines = text.Replace("<br>", "\n")
                            .Replace("<br/>", "\n")
                            .Replace("<br />", "\n")
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                if (trimmedLine.StartsWith("-"))
                {
                    trimmedLine = trimmedLine.Substring(1).Trim();
                }
                builder.Append($"<div style='margin-bottom: 4px;'><span style='margin-right: 6px;'>&bull;</span>{trimmedLine}</div>");
            }

            return builder.ToString();
        }

        public static string GenerateReviewInternEmailTemplateHtml(
            bool isReject,
            int month,
            int year,
            string internName,
            string reviewerName,
            string currentLevelStr,
            string newLevelStr,
            float? rateStar,
            string applyDate,
            string supportInfoHtml,
            List<Entities.ReviewInternCapability> capabilities)
        {
            var pointRowsHtml = new StringBuilder();
            foreach (var item in capabilities.Where(x => x.Capability.Type == CapabilityType.Point))
            {
                pointRowsHtml.Append($@"
                <tr>
                    <td style='padding:12px 16px; font-size:14px; color:#111827; border-bottom:1px solid #e5e7eb;'>{item.Capability.Name} (x{item.Coefficient})</td>
                    <td align='right' style='padding:12px 16px; font-size:14px; color:#111827; border-bottom:1px solid #e5e7eb;'>{item.Point}/5</td>
                </tr>");
            }

            var noteBoxesHtml = new StringBuilder();
            var noteCapabilities = capabilities.Where(x => x.Capability.Type == CapabilityType.Note).ToList();

            if (noteCapabilities.Any())
            {
                noteBoxesHtml.Append(@"<table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'><tr>");
                bool isSingleNote = noteCapabilities.Count == 1;

                for (int i = 0; i < noteCapabilities.Count; i++)
                {
                    var item = noteCapabilities[i];
                    string bgColor = i % 2 == 0 ? "#f0fdf4" : "#fff7ed";
                    string borderColor = i % 2 == 0 ? "#bbf7d0" : "#fed7aa";
                    string textColor = i % 2 == 0 ? "#166534" : "#9a3412";
                    string width = isSingleNote ? "100%" : "49%";

                    var formattedNoteHtml = FormatWithBullet(item.Note);

                    noteBoxesHtml.Append($@"
                        <td valign='top' width='{width}' style='background:{bgColor}; border:1px solid {borderColor}; border-radius:10px; padding:16px;'>
                            <div style='font-size:15px; font-weight:bold; color:{textColor}; margin-bottom:10px;'>
                                {item.Capability.Name}
                            </div>
                            <div style='margin:0; color:{textColor}; font-size:14px; line-height:22px;'>
                                {formattedNoteHtml}
                            </div>
                        </td>
                    ");

                    if (!isSingleNote && i % 2 == 0 && i < noteCapabilities.Count - 1)
                    {
                        noteBoxesHtml.Append(@"<td width='2%' style='font-size:1px; line-height:1px; padding:0;'>&nbsp;</td>");
                    }

                    if (!isSingleNote && i % 2 == 1 && i < noteCapabilities.Count - 1)
                    {
                        noteBoxesHtml.Append("</tr><tr><td colspan='3' height='16'></td></tr><tr>");
                    }
                }

                if (!isSingleNote && noteCapabilities.Count % 2 != 0)
                {
                    noteBoxesHtml.Append(@"<td width='2%' style='font-size:1px; line-height:1px; padding:0;'>&nbsp;</td><td valign='top' width='49%'></td>");
                }
                noteBoxesHtml.Append("</tr></table>");
            }

            string currentMonth = month < 10 ? "0" + month.ToString() : month.ToString();
            string headerTitle = isReject ? "Hủy kết quả đánh giá" : "Kết quả đánh giá thực tập sinh";

            string greetingHtml = isReject
                ? $"<div style='font-size:14px; line-height:22px; color:#4b5563; margin-top:8px;'>Bảng thông tin kết quả review trước đó do {reviewerName} thực hiện dưới đây đã bị hủy.</div>"
                : $"<div style='font-size:14px; line-height:22px; color:#4b5563; margin-top:8px;'>Dưới đây là kết quả đánh giá thực tập trong tháng <strong>{currentMonth}/{year}</strong> của bạn.</div>";

            string formattedSupportInfoHtml = FormatWithBullet(supportInfoHtml);

            var mailBody = $@"
                            <!DOCTYPE html>
                            <html lang='vi'>
                            <head>
                              <meta charset='UTF-8' />
                              <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                              <title>Monthly Internship Review</title>
                            </head>
                            <body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#1f2937;'>
                              <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color:#f4f6f8; margin:0; padding:24px 0;'>
                                <tr>
                                  <td align='center'>
                                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='750' style='width:750px; max-width:750px; background-color:#ffffff; border-radius:12px; overflow:hidden; border:1px solid #e5e7eb;'>
                                      <tr>
                                        <td style='background-color:#111827; padding:24px 32px;'>
                                          <div style='font-size:22px; line-height:30px; font-weight:bold; color:#ffffff;'>
                                            {headerTitle}
                                          </div>
                                          <div style='font-size:14px; line-height:22px; color:#d1d5db; margin-top:6px;'>
                                            Đợt đánh giá tháng {currentMonth}/{year}
                                          </div>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:28px 32px 8px 32px;'>
                                          <div style='font-size:16px; line-height:24px; color:#111827;'>
                                            Thân gửi <strong>{internName}</strong>,
                                          </div>
                                          {greetingHtml}
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:16px 32px 8px 32px;'>
                                          <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'>
                                            <tr>
                                              <td width='40%' valign='top' style='padding-right:8px;'>
                                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' height='100%' style='background:#f9fafb; border:1px solid #e5e7eb; border-radius:10px; height: 100%;'>
                                                  <tr>
                                                    <td style='padding:16px;' valign='top'>
                                                      <div style='font-size:12px; color:#6b7280; text-transform:uppercase; letter-spacing:0.5px;'>Reviewer</div>
                                                      <div style='font-size:16px; font-weight:bold; color:#111827; margin-top:6px;'>{reviewerName}</div>
                                                    </td>
                                                  </tr>
                                                </table>
                                              </td>
                                              <td width='35%' valign='top' style='padding-left:4px; padding-right:4px;'>
                                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' height='100%' style='background:#f9fafb; border:1px solid #e5e7eb; border-radius:10px; height: 100%;'>
                                                  <tr>
                                                    <td style='padding:16px;' valign='top'>
                                                      <div style='font-size:12px; color:#6b7280; text-transform:uppercase; letter-spacing:0.5px;'>Level</div>
                                                      <div style='font-size:16px; font-weight:bold; color:#111827; margin-top:6px;'>{currentLevelStr} &rarr; {newLevelStr}</div>
                                                    </td>
                                                  </tr>
                                                </table>
                                              </td>
                                              <td width='25%' valign='top' style='padding-left:8px;'>
                                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' height='100%' style='background:#ecfdf5; border:1px solid #a7f3d0; border-radius:10px; height: 100%;'>
                                                  <tr>
                                                    <td style='padding:16px;' valign='top'>
                                                      <div style='font-size:12px; color:#047857; text-transform:uppercase; letter-spacing:0.5px;'>Điểm đánh giá</div>
                                                      <div style='font-size:16px; font-weight:bold; color:#065f46; margin-top:6px;'>{(rateStar ?? 0).ToString("0.00")} / 5</div>
                                                    </td>
                                                  </tr>
                                                </table>
                                              </td>
                                            </tr>
                                          </table>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:20px 32px 8px 32px;'>
                                          <div style='font-size:16px; font-weight:bold; color:#111827; margin-bottom:12px;'>
                                            Chi tiết đánh giá
                                          </div>
                                          <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='border-collapse:collapse; border:1px solid #e5e7eb; border-radius:10px; overflow:hidden;'>
                                            <tr style='background-color:#f9fafb;'>
                                              <td style='padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Tiêu chí</td>
                                              <td align='right' style='padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Điểm</td>
                                            </tr>
                                            {pointRowsHtml}
                                          </table>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:20px 32px 8px 32px;'>
                                            {noteBoxesHtml}
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:20px 32px 8px 32px;'>
                                          <div style='font-size:16px; font-weight:bold; color:#111827; margin-bottom:12px;'>
                                            Thông tin hỗ trợ
                                          </div>
                                          <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background:#f9fafb; border:1px solid #e5e7eb; border-radius:10px;'>
                                            <tr>
                                              <td style='padding:16px;'>
                                                <div style='font-size:14px; line-height:24px; color:#111827;'>
                                                    {formattedSupportInfoHtml}
                                                </div>
                                              </td>
                                            </tr>
                                          </table>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:20px 32px 8px 32px;'>
                                          <div style='font-size:14px; line-height:22px; color:#4b5563;'>
                                            <strong>Ngày áp dụng:</strong> {applyDate}
                                          </div>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td style='padding:20px 32px 32px 32px;'>
                                          <div style='font-size:13px; line-height:22px; color:#6b7280; border-top:1px solid #e5e7eb; padding-top:16px;'>
                                            Mọi thắc mắc liên quan đến nội dung đánh giá, vui lòng liên hệ trực tiếp với PM để được giải đáp.
                                          </div>
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                              </table>
                            </body>
                            </html>";

            return mailBody;
        }

        public static string GenerateTimesheetEmailTemplateHtml(string headerTitle, string bodyContent)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                </head>
                <body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#1f2937;'>
                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color:#f4f6f8; margin:0; padding:24px 0;'>
                        <tr>
                            <td align='center'>
                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='1200' style='width:1200px; max-width:1200px; background-color:#ffffff; border-radius:12px; overflow:hidden; border:1px solid #e5e7eb;'>
                                    <tr>
                                        <td style='background-color:#111827; padding:24px 32px;'>
                                            <div style='font-size:22px; line-height:30px; font-weight:bold; color:#ffffff;'>
                                                {headerTitle}
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:8px 32px 32px 32px;'>
                                            <div style='margin-top:20px;'>
                                                <div style='margin:0; color:#111827; font-size:14px; line-height:22px;'>
                                                    {bodyContent}
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }

        public static string GenerateTimesheetTableHtml(string tbodyContent)
        {
            return $@"
                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color:#ffffff; border-collapse:separate; border-spacing:0; border:1px solid #e5e7eb; border-radius:10px; overflow:hidden; margin-bottom: 20px; table-layout: fixed;'>
                    <thead>
                        <tr style='background-color:#f9fafb;'>
                            <th align='center' width='10%' style='width: 10%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Date At</th>
                            <th align='center' width='10%' style='width: 10%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Task Name</th>
                            <th align='center' width='45%' style='width: 45%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Note</th>
                            <th align='center' width='10%' style='width: 10%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Working Time</th>
                            <th align='center' width='15%' style='width: 15%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Type Of Work</th>
                            <th align='center' width='10%' style='width: 10%; padding:12px 16px; font-size:13px; font-weight:bold; color:#374151; border-bottom:1px solid #e5e7eb;'>Charged</th>
                        </tr>
                    </thead>
                    <tbody>{tbodyContent}</tbody>
                </table>";
        }
    }
}
public class GetBGJobsDescription
{
    public string SubJobType { get; set; }
    public string Description { get; set; }

}
