using Abp.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RetroDetails.Dto
{
    internal class UpdateRetroResultDto
    {
        public long Row { get; set; }
        public string Email { get; set; }
        public string PositionName { get; set; }
        public string Point { get; set; }
        public float? PointF { get
            {
                if (string.IsNullOrEmpty(Point))
                    return null;
                try
                {
                    //return Convert.ToInt64(Point);
                    return float.Parse(Point);
                }
                catch(Exception ex)
                {
                    return null ;
                }
            }
        }
        public string Note { get; set; }
        public bool IsEmpty => Email.IsNullOrEmpty() && PositionName.IsNullOrEmpty() && Point.IsNullOrEmpty() && Note.IsNullOrEmpty();
    }
}
