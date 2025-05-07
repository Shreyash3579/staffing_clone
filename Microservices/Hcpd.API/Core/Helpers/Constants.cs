using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hcpd.API.Core.Helpers
{
    public static class Constants
    {
        public static class ServiceExtensions
        {
            public static string ValidIssuer = "Staffing Authentication API";
            public static string ValidAudience = "APIs accessed by Staffing App";
            public static string StaffingSecretKey = "staffing_secretKey";

        }

        public static class ReviewDocTag
        {
            public static string DOCCODE = "DOCCODE";
            public static string HEADER = "HEADER";
            public static string BODY = "BODY";
            public static string ReviewStatus = "ReviewStatus";
            public static string LastUpdated = "LastUpdated";
            public static string SPCW = "SPCW";
            public static string CPDN = "CPDN";
            public static string CDPP = "CDPP";
            public static string MostImportantMsgText = "MostImportantMsgText";
            public static string DevPrioritiesText = "DevPrioritiesText";
            public static string DevFocusVA = "DevFocusVA";
            public static string DevFocusClient = "DevFocusClient";
            public static string DevFocusTeam = "DevFocusTeam";
        }

        public static class ReviewDocMessage
        {
            public static string cpdnHeading = @"{\rtf1\ansi\ansicpg1252\uc1\deff0{\fonttbl{\f0\fnil\fcharset0\fprq2 Arial;}{\f1\fswiss\fcharset0\fprq2 Verdana;}{\f2\froman\fcharset2\fprq2 Symbol;}}\f1\fs20\ul\cf1 Current development plan - Next six months:\par }";
            public static string cdppHeading = @"{\rtf1\ansi\ansicpg1252\uc1\deff0{\fonttbl{\f0\fnil\fcharset0\fprq2 Arial;}{\f1\fswiss\fcharset0\fprq2 Verdana;}{\f2\froman\fcharset2\fprq2 Symbol;}}\f1\fs20\ul\cf1 Current development plan:\par }";
            public static string spcwHeading = @"{\rtf1\ansi\ansicpg1252\uc1\deff0{\fonttbl{\f0\fnil\fcharset0\fprq2 Arial;}{\f1\fswiss\fcharset0\fprq2 Verdana;}{\f2\froman\fcharset2\fprq2 Symbol;}}\f1\fs20\ul\cf1 Synthesis of performance on case work:\par }";
            public static string noDataAvailableMessage = @"{\rtf1\ansi\ansicpg1252\uc1\deff0{\fonttbl{\f0\fnil\fcharset0\fprq2 Arial;}{\f1\fswiss\fcharset0\fprq2 Verdana;}{\f2\froman\fcharset2\fprq2 Symbol;}}\f1\fs18\i no data available\par }";
            public static string devPrioritiesHeading = @"{\rtf1\ansi\ansicpg1252\uc1\deff0{\fonttbl{\f0\fnil\fcharset0\fprq2 Arial;}{\f1\fswiss\fcharset0\fprq2 Verdana;}{\f2\froman\fcharset2\fprq2 Symbol;}}\f1\fs20\ul\cf1 Skillplan (developmental priorities)\par }";
        }
    }
}
