using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace WinFormsClient
{
    public class HtmlHelper
    {
        public static string GetText(Stream stream, Encoding encoding)
        {
            var streamReader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
            var html = streamReader.ReadToEnd();
            return html;
        }

        public static string GetDocumentText(WebBrowser wb)
        {
            if (!string.IsNullOrEmpty(wb.Document.Encoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
            }
            else if (!string.IsNullOrEmpty(wb.Document.DefaultEncoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.DefaultEncoding));
            }
            else
            {
                return GetText(wb.DocumentStream, Encoding.UTF8);
            }
        }
    }
}
