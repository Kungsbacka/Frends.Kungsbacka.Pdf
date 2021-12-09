using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
