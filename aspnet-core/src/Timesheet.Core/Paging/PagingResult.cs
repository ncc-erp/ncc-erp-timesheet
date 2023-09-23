using System;
using System.Collections.Generic;
using System.Text;

namespace Ncc.Paging
{
    public class PagingResult<T>
    {
        public long TotalItems { get; set; }
        public List<T> Items { get; set; }
    }
}
