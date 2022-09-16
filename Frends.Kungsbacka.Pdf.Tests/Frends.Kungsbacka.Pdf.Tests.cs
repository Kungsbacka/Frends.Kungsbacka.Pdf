using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

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
            Assert.AreEqual(6, result.Attachments.Count());
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
        /// Test AddImageAsNewPage
        /// </summary>
        [Test]
        public void AddRotatedImageAsNewPageTest()
        {
            var input = new AddImageInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WithAttachments),
                Image = TestHelper.GetRotatedImage()
            };
            var options = new AddImageOptions()
            {
                Caption = "Right side up!"
            };
            var result = PdfTasks.AddImageAsNewPage(input, options);
            TestHelper.SaveResult("add-rotated-image-as-new-page-test-result.pdf", result.PdfDocument);
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
        //Need to have the wkhtmltopdf folder copied over to C:\Program Files\wkhtmltox\bin for these tests to work
        [Test]
		public void ConvertHtmlToPdfHtmlOnly()
		{
			var input = new ConvertHtmlToPdfInput
			{
				Html = TestHelper.ConvertHtmlToPdfHtml()
			};

			var result = PdfTasks.ConvertHtmlToPdf(input);

			TestHelper.SaveResult("ConvertHtmlToPdfHtmlOnly-test-result.pdf", result.PdfDocument);
		}

		[Test]
        public void ConvertHtmlToPdfHtmlOnlyLandscape()
		{
			var input = new ConvertHtmlToPdfInput
			{
				Html = TestHelper.ConvertHtmlToPdfHtml(),
				Orientation = "Landscape"
			};

			var result = PdfTasks.ConvertHtmlToPdf(input);

			TestHelper.SaveResult("ConvertHtmlToPdfHtmlOnlyLandscape-test-result.pdf", result.PdfDocument);
		}

		[Test]
		public void ConvertHtmlToPdfHtmlOnlyB5PageSize()
		{
			var input = new ConvertHtmlToPdfInput
			{
				Html = TestHelper.ConvertHtmlToPdfHtml(),
				PageSize = "B5"
			};

			var result = PdfTasks.ConvertHtmlToPdf(input);

			TestHelper.SaveResult("ConvertHtmlToPdfHtmlOnlyB5PageSize-test-result.pdf", result.PdfDocument);
		}
	}
}
