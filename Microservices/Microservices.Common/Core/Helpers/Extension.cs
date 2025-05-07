using System;
using System.Linq;

namespace Microservices.Common.Core.Helpers
{
    public static class Extension
    {
        public static DateTime ConvertToPacificStandardTime(this DateTime date)
        {
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            return TimeZoneInfo.ConvertTime(date, pstZone);
        }

        public static string ConcatenateIfNotExists(this string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target;
            var isSourceContainsTarget = source.Split(',').Contains(target);
            return isSourceContainsTarget ? source : string.Join(",", source, target);
        }

        public static string ToGenericTypeString(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            string genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0,
                genericTypeName.IndexOf('`'));
            string genericArgs = string.Join(",",
                t.GetGenericArguments()
                    .Select(ta => ToGenericTypeString(ta)).ToArray());
            return genericTypeName + "<" + genericArgs + ">";
        }
    }
}
