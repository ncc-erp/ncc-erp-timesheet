using Ncc.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceManagement.Tests.Seeders
{
    public interface ISeeder
    {
        void Seed(TimesheetDbContext context);
    }
}
