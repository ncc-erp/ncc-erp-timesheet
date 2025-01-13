using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Constants
{
    public class MezonConstant
    {
        public const string MEZON_NOTI_TYPE = "TIMESHEET";
        public const string MULTILINE_MARKDOWN_CODE_TYPE = "t";
        public const string INLINE_MARKDOWN_CODE_TYPE = "s";
        public const string TRIPLE_BACKTICKS = "```";
        public const string MARKDOWN_PATTERN = @"(```|`)";
        public const string MENTION_PATTERN = @"@([a-zA-Z0-9.]+)";
    }
}
