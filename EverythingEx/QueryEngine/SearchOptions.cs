using System;

namespace QueryEngine
{
    public class SearchOptions
    {
        public bool MatchCase { get; set; } = false;
        public bool MatchWholeWord { get; set; } = false;
        public bool MatchPath { get; set; } = false;
        public bool EnableRegex { get; set; } = false;
        public FilterCategory ActiveFilter { get; set; } = FilterCategory.Everything;
        public int MaxResults { get; set; } = 0;

        public SearchOptions Clone()
        {
            return new SearchOptions
            {
                MatchCase = this.MatchCase,
                MatchWholeWord = this.MatchWholeWord,
                MatchPath = this.MatchPath,
                EnableRegex = this.EnableRegex,
                ActiveFilter = this.ActiveFilter,
                MaxResults = this.MaxResults
            };
        }
    }
}
