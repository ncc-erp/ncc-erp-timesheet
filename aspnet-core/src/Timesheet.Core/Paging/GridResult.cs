using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Paging
{
    public class GridResult<T> where T : class
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; }

        public GridResult(IReadOnlyList<T> items, int total)
        {
            Items = items;
            TotalCount = total;
        }
    }
}
