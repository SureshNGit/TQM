using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
    public class OverallReportModelView
    {
        public List<YCTestModel> yctestlist { get; set; }
        public List<YCTestSummaryModel> yctestsummarylist { get; set; }
    }
}
