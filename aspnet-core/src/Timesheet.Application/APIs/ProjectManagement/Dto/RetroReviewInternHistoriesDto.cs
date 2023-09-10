using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Timesheet.APIs.ProjectManagement.Dto
{
    public class RetroReviewInternHistoriesDto
    {
        public string Email { get; set; }
        public List<PointDto> PointHistories { get; set; }
        private float? _AveragePoint => PointHistories.Average(s => s.Point);
        public float? AveragePoint => _AveragePoint.HasValue ? MathF.Round(_AveragePoint.Value, 2) : float.NaN;
    }
    public class PointDto
    {
        public float? Point { get; set; }
        public bool isRetro { get; set; }
        public DateTime StartDate { get; set; }
        public string Note { get; set; }
        public string ProjectName { get; set; }
    }
}
