using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Helpers
{
    public static class Utilities
    {
        #region "Extension Methods"
        /// <summary>
        /// Returns the date after adding the non-business days (i.e excluding the weekend)
        /// Eg: AddBusinessDays("10-jun-2022", 3) will return 15-Jun-2022 (excludes 2 weekend days and add 3 business days)
        /// </summary>
        /// <param name="current"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DateTime AddBusinessDays(this DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                } while (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday);
            }
            return current;
        }
        #endregion
    }
}
