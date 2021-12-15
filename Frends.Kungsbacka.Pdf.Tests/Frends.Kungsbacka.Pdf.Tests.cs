using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Frends.Kungsbacka.Pdf.Tests
{
    [TestFixture]
    class TestClass
    {
        /// <summary>
        /// Test ConvertEmbeddedImagesToPages
        /// </summary>
        [Test]
        public void ConvertEmbeddedImagesToPagesTest()
        {
            var input = new PdfDocumentInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments)
            };
            var options = new ConvertEmbeddedOptions()
            {
                Filter = "*"
            };
            var result = PdfTasks.ConvertEmbeddedImagesToPages(input, options);
            TestHelper.SaveResult("convert-embedded-images-to-pages-test-result.pdf", result.PdfDocument);
        }

        /// <summary>
        /// Test RemoveAttachments
        /// </summary>
        [Test]
        public void RemoveAttachmentsTest()
        {
            var input = new PdfDocumentInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments)
            };
            var options = new PdfCommonOptions()
            {
                Filter = "*"
            };
            var result = PdfTasks.RemoveAttachments(input, options);
            TestHelper.SaveResult("remove-attachments-test-result.pdf", result.PdfDocument);
        }

        /// <summary>
        /// Test ExtractAttachments
        /// </summary>
        [Test]
        public void ExtractAttachmentsTest()
        {
            var input = new PdfDocumentInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments)
            };
            var options = new PdfCommonOptions()
            {
                Filter = "*"
            };
            var result = PdfTasks.ExtractAttachments(input, options);
            Assert.AreEqual(5, result.Attachments.Count());
        }

        /// <summary>
        /// Test AddImageAsNewPage
        /// </summary>
        [Test]  
        public void AddImageAsNewPageTest()
        {
            var input = new AddImageInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments),
                Image = TestHelper.GetImage()
            };
            var options = new AddImageOptions()
            {
                Caption = "Ocean"
            };
            var result = PdfTasks.AddImageAsNewPage(input, options);
            TestHelper.SaveResult("add-image-as-new-page-test-result.pdf", result.PdfDocument);
        }

        /// <summary>
        /// Test MergeEmbeddedPdfDocuments
        /// </summary>
        [Test]
        public void MergeEmbeddedPdfDocumentsTest()
        {
            var input = new PdfDocumentInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments)
            };
            var options = new PdfCommonOptions()
            {
                Filter = "*.pdf"
            };
            var result = PdfTasks.MergeEmbeddedPdfDocuments(input, options);
            TestHelper.SaveResult("merge-embedded-pdf-documents-test-result.pdf", result.PdfDocument);
        }

        /// <summary>
        /// Test AddFooter
        /// </summary>
        [Test]
        public void AddFooterTest()
        {
            var input = new AddFooterInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments),
                Text = "This is a multiline footer.\nAnother line of text..."
            };
            var result = PdfTasks.AddFooter(input);
            TestHelper.SaveResult("add-footer-test-result.pdf", result.PdfDocument);
        }


        [Test]
        public void CustomTest()
        {
            var pdf = System.IO.File.ReadAllBytes(@"C:\Temp\arende.pdf");

            var r1 = PdfTasks.ExtractAttachments(new PdfDocumentInput() { PdfDocument = pdf }, new PdfCommonOptions() { Filter = "Signeringsunderlag.pdf" });

            var r2 = PdfTasks.ConvertEmbeddedImagesToPages(new PdfDocumentInput() { PdfDocument = r1.Attachments.First().Data }, null);

            var r3 = PdfTasks.MergeEmbeddedPdfDocuments(new PdfDocumentInput() { PdfDocument = r2.PdfDocument }, null);

            var r4 = PdfTasks.RemoveAttachments(new PdfDocumentInput() { PdfDocument = r3.PdfDocument }, new PdfCommonOptions() { Filter = "*.pdf" });

            System.IO.File.WriteAllBytes(@"C:\Temp\arende-e.pdf", r4.PdfDocument);
        }
    }
}
