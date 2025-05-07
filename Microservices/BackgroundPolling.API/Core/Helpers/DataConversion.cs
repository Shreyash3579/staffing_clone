using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BackgroundPolling.API.Core.Helpers
{
    public static class DataConversion
    {
        public static DataTable GetDataTableFromObjects<T>(IList<T> objectList) where T : class
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(
                    new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType)
                    ?? info.PropertyType));
            }

            foreach (T entity in objectList)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public static string ExportDataTableToExcel(DataTable dataTable, string fileName, string sheetName, ClosedXML.Excel.XLWorkbook workbook)
        {
            if(workbook == null)
            {
                workbook = new ClosedXML.Excel.XLWorkbook();
            }
            var worksheet = workbook.Worksheets.Add(dataTable, sheetName);
            worksheet.Columns().AdjustToContents();

            var path = AppDomain.CurrentDomain.BaseDirectory;

            var filePath = $"{path}/{fileName}.xlsx";

            workbook.SaveAs(filePath);
            return filePath;
        }

        public static StringBuilder GetMailBodyFromDataTable(DataTable dataTable, string tableHeading)
        {
            var body = new StringBuilder();
            body.Clear();
            if (!string.IsNullOrEmpty(tableHeading))
            {
                body.Append(
                "<div style='font: bold 10pt Arial; margin-bottom: 20px; padding-bottom: 4px; border-bottom: 2px solid #FF6600'>");
                body.Append("<span style='color: #CC0000;'>&gt;&gt;&nbsp;</span>");
                body.Append(tableHeading);
                body.Append("</div>");
            }

            //Create Table 
            body.Append("<table width='100%' border='1' style='border-collapse: collapse;font: 10pt Arial'>");
            //add table headers
            body.Append(GetDataTableHeaders(dataTable));

            if (dataTable.Rows.Count == 0)
            {
                body.Append("<tr>");
                body.Append("<td colspan='7' style='text-align:center;'>No mismatch rows found</td>");
                body.Append("</tr>");
            }

            var index = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                body.Append("<tr>"); // Start a new table row for each DataRow
                body.Append(GetTableCell((index + 1).ToString()));
                foreach (DataColumn col in dataTable.Columns)
                {
                    string cellValue = row[col].ToString();

                    //check if the lastUpdatedDate is DateTime.MinValue or 1/1/0001 12:00:00AM
                    if (col.DataType == typeof(DateTime) && (DateTime.TryParse(cellValue, out DateTime dateValue) && dateValue == DateTime.MinValue))
                    {
                        // Handle DateTime.MinValue by setting it to null or any desired value
                        cellValue = "N/A"; // Set it as "N/A"
                    }

                    // Check if cellValue is empty (or null) and replace it with a hyphen
                    if (string.IsNullOrWhiteSpace(cellValue) || cellValue == "0")
                    {
                        body.Append(GetTableCell(" - "));
                    }
                    else
                    {
                        body.Append(GetTableCell(cellValue));
                    }
                }
                index = index + 1;
                body.Append("</tr>"); // Close the table row
            }
            //Add a line 
            body.Append("</table>");
            body.Append("<br/>");
            return body;
        }

        public static string GetDataTableHeaders(DataTable dataTable, bool addSerialNumberCol = true)
        {
            var sb = new StringBuilder();
            sb.Append("<tr>");

            if (addSerialNumberCol)
            {
                sb.Append("<th style='min-width:70px' align ='left'>" + "S. No" + "</th>");
            }

            foreach (var columnHeader in dataTable.Columns)
            {
                sb.Append("<th style='min-width:70px' align ='left'>" + columnHeader + "</th>");
            }

            sb.Append("</th></tr>");
            return sb.ToString();
        }

        public static string GetTableCell(string cellData)
        {
            return ("<td style='min-width:70px' align ='left'>" + cellData + "</td>");
        }
    }
}
