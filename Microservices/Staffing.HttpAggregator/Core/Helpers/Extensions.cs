using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Staffing.HttpAggregator.Core.Helpers
{
    public static class Extensions
    {
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text)) return String.Empty;
            return string.Concat(
                text.Normalize(NormalizationForm.FormD)
                    .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                 UnicodeCategory.NonSpacingMark)
            ).Normalize(NormalizationForm.FormC);
        }

        public static string PadNumbers(this string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        public static DateTime GetMondayOfWeek(this DateTime date)
        {
            date = date == DateTime.MinValue ? DateTime.Today.Date : date.Date;
            
            var mondayOfStartingWeek = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
            if (date.DayOfWeek == 0)
            {
                mondayOfStartingWeek = date.AddDays(-7).AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
            }
            if (date.Date.CompareTo(mondayOfStartingWeek.Date) < 0)
            {
                mondayOfStartingWeek = date.Date;
            }
            return mondayOfStartingWeek;
        }
    }
}
