using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DynamicFilter
{
    public class ExpressionFilter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public ComparisonOperator Comparison { get; set; }

        [JsonIgnore]
        public string ActualPropertyName { get; set; }
        [JsonIgnore]
        public object ActualValue { get; set; }
        [JsonIgnore]
        public Type PropertyType { get; set; }
    }

    public enum ComparisonOperator
    {
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        NotEqual,
        Contains, //for strings  
        StartsWith, //for strings  
        EndsWith, //for strings  
        In // for list item
    }
}
