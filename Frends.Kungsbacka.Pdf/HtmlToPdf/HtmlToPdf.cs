using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Frends.Kungsbacka.Pdf.HtmlToPdf
{
    //Based on https://github.com/pug-pelle-p/pugpdf
    public class HtmlToPdfConverter
    {
        private const string DefaultExecutablePath = @"C:\Program Files\wkhtmltox\bin\wkhtmltopdf.exe";

        public HtmlToPdfConverter(ConvertHtmlToPdfInput input)
        {
            if (!string.IsNullOrEmpty(input.Orientation))
            {
                Orientation = (PdfOrientation)Enum.Parse(typeof(PdfOrientation), input.Orientation, true);
            }

            if (!string.IsNullOrEmpty(input.PageSize))
            {
                PageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), input.PageSize, true);
            }

            ExecutablePath = string.IsNullOrEmpty(input.ExecutablePath) ? DefaultExecutablePath : input.ExecutablePath;
        }
        public byte[] RenderHtmlAsPdf(string html)
        {
            var data = ConvertToPdf(html, GetSwitches());

            return data;
        }

        private string GetSwitches()
        {
            var switches = string.Empty;

            switches += $"--page-size {PageSize} ";

            switches += $"--orientation {Orientation} ";

            if (!string.IsNullOrEmpty(Title))
                switches += $"--title \"{Title}\" ";

            switches += $"--image-dpi {ImageDPI} ";
            switches += $"--image-quality {ImageQuality} ";
            switches += "--disable-smart-shrinking ";

            return switches;
        }

        private string GetPath()
        {
            var path = ExecutablePath;

            if (!File.Exists(path))
                throw new FileNotFoundException("Executable not found.", path);

            return path;
        }

        private string SpecialCharsEncode(string text)
        {
            var charArray = text.ToCharArray();
            var stringBuilder = new StringBuilder();

            foreach (var ch in charArray)
            {
                var charInt = Convert.ToInt32(ch);

                if (charInt > sbyte.MaxValue)
                    stringBuilder.AppendFormat("&#{0};", charInt);
                else
                    stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }

        private byte[] ConvertToPdf(string html, string switches = "")
        {
            switches = "-q " + switches + " -";
            if (!string.IsNullOrEmpty(html))
            {
                switches += " -";
                html = SpecialCharsEncode(html);
            }

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = GetPath(),
                    Arguments = switches,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };
                process.Start();

                if (!string.IsNullOrEmpty(html))
                {
                    using (var standardInput = process.StandardInput)
                    {
                        standardInput.WriteLine(html);
                    }
                }

                using (var memoryStream = new MemoryStream())
                using (var baseStream = process.StandardOutput.BaseStream)
                {
                    var buffer = new byte[4096];
                    int count;

                    while ((count = baseStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, count);

                    var end = process.StandardError.ReadToEnd();

                    if (memoryStream.Length == 0L)
                        throw new Exception(end);

                    process.WaitForExit();

                    return memoryStream.ToArray();
                }
            }
        }
        public string ExecutablePath { get; set; }
        public PdfPageSize PageSize { get; set; } = PdfPageSize.A4;
        public PdfOrientation Orientation { get; set; } = PdfOrientation.Portrait;
        public string Title { get; set; }
        public int ImageDPI { get; set; } = 600;
        public int ImageQuality { get; set; } = 94;
    }

    public enum PdfOrientation
    {
        Portrait,
        Landscape
    }
    public enum PdfPageSize
    {
        A0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        A9,
        B0,
        B1,
        B2,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        B9,
        B10,
        C5E,
        Comm10E,
        DLE,
        Executive,
        Folio,
        Ledger,
        Legal,
        Letter,
        Tabloid
    }
}