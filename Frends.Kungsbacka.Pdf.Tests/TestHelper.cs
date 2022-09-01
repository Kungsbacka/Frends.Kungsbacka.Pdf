using System;
using System.IO;
using NUnit.Framework;

namespace Frends.Kungsbacka.Pdf.Tests
{
    internal static class TestHelper
    {
        public enum TestDocumentTypes { WithAttachments, NoAttachments };

        public static byte[] GetTestDocument(TestDocumentTypes testDocumentType)
        {
            string fileName;
            switch (testDocumentType)
            {
                case TestDocumentTypes.NoAttachments:
                    fileName = "test-file1.pdf";
                    break;
                case TestDocumentTypes.WithAttachments:
                    fileName = "test-file-with-attachments.pdf";
                    break;
                default:
                    throw new ArgumentException(nameof(testDocumentType));
            }
            return File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "doc", fileName));

        }

        public static void SaveResult(string fileName, byte[] pdfDocument)
        {
            File.WriteAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "doc", fileName), pdfDocument);
        }

        public static byte[] GetImage()
        {
            return File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "doc", "ocean1.jpg"));
        }
        public static byte[] GetRotatedImage()
        {
            return File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "doc", "rotated.jpg"));
        }
        public static string ConvertHtmlToPdfHtml()
        {
            return @"
                    <html>
					<head>
					<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
					<title>Html to PDF test</title>
					<style type='text/css'>
					body {
						font-family: 'minion-pro','Helvetica Neue','Helvetica',Helvetica,Arial,sans-serif;
						font-size: 14px;
					}

					p {
						padding: 2pt;
					}
                    .top-text {
                        color: red;
                        font-weight: bold;
                    }
                    .left {
                        position: absolute;
                        left: 1
                    }
                    .center {
                        text-align: center;
                    }
                    .right {
                        position: absolute;
                        right: 1;
                    }
                    .margin-top {
                        margin-top: 75px;
                    }
                    .margin-bottom {
                        margin-bottom: 75px;
                    }
                    .logo {
                        margin-top: 250px;
                    }
					</style>
					</head>
					<body>
					<div class='top-text margin-bottom center'>Det här är en text längst upp i mitten med röd text och bold</div>

					<p class='center'>Hej!</p>

                    <div class='center margin-top'>
                        <div>Detta är en  text i mitten av dokumentet</div>
                    </div>

                    <div class='left margin-top'>Detta är lite text längst till vänster i dokumentet</div>
                    <div class='right margin-top'>Detta är lite text längst till höger i dokumentet</div>

					<p class='center logo'>
                        <img src='data:image/png;base64,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' />
                    </p>

					</body>
					</html>";
        }
        public static string ConvertHtmlToPdfPdfHeader()
        {
            return @"{
                ""LeftText"": ""Left header"",
                ""CenterText"": ""Center header"",
                ""RightText"": ""Right header"",
                ""DisplayLine"": ""true"",
                ""FontName"": ""Arial"",
                ""FontSize"": ""14.0"",
                ""Spacing"": ""2""
            }";
        }
        public static string ConvertHtmlToPdfPdfFooter()
        {
            return @"{
                ""LeftText"": ""Left footer"",
                ""CenterText"": ""Center footer"",
                ""RightText"": ""Right footer"",
                ""DisplayLine"": ""true"",
                ""FontName"": ""Arial"",
                ""FontSize"": ""13.0"",
                ""Spacing"": ""5""
            }";
        }
    }
}
