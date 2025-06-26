using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
		/// Test ConvertEmbeddedImagesToPagesShouldOnlyContainDocuments
		/// </summary>
		[Test]
		public void ConvertEmbeddedImagesToPagesShouldOnlyContainDocuments()
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
			var resultPdf = TestHelper.BytesToPdf(result.PdfDocument);

			var resultEmbeddedFileNames = TestHelper.GetFileSpecArray(resultPdf).Where(x => x.IsString()).Select(x => Path.GetExtension(x.ToString()));

			var allowedFileTypes = new string[2] { ".pdf", ".docx" };

            Assert.IsTrue(resultEmbeddedFileNames.All(x => allowedFileTypes.Contains(x)));
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
            var options = new ExtractAttachmentsOptions()
            {
                Filter = "*"
            };
            var result = PdfTasks.ExtractAttachments(input, options);
            Assert.AreEqual(6, result.Attachments.Count());
        }

        [Test]
        public void ExtractAttachments_MakeFilenameSafe_True_ReplacesUnsafeChars()
        {
            string UnsafeFilename = "my:unsafe/file*name?|.txt";
            string SafeFilename = "my_unsafe_file_name__.txt";

            byte[] pdfWithUnsafe = TestHelper.CreatePdfWithAttachment(UnsafeFilename);
            var input = new PdfDocumentInput { PdfDocument = pdfWithUnsafe };
            var options = new ExtractAttachmentsOptions
            {
                MakeFilenameSafe = true,
                ExtractOepPrefix = false,
                Filter = "*"
            };

            // Act
            var result = PdfTasks.ExtractAttachments(input, options);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Attachments);
            Assert.IsNotEmpty(result.Attachments);
            var attachment = result.Attachments.FirstOrDefault();
            Assert.NotNull(attachment);
            Assert.AreEqual(SafeFilename, attachment.Name);
        }

        [Test]
        public void ExtractAttachments_MakeFilenameSafe_False_And_Filename_Is_actually_Safe_KeepsOriginalName()
        {
            string UnsafeFilename = "my safe file name.txt";

            byte[] pdfWithUnsafe = TestHelper.CreatePdfWithAttachment(UnsafeFilename);
            var input = new PdfDocumentInput { PdfDocument = pdfWithUnsafe };
            var options = new ExtractAttachmentsOptions
            {
                MakeFilenameSafe = false,
                ExtractOepPrefix = false,
                Filter = "*"
            };

            // Act
            var result = PdfTasks.ExtractAttachments(input, options);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Attachments);
            Assert.IsNotEmpty(result.Attachments);
            var attachment = result.Attachments.FirstOrDefault();
            Assert.NotNull(attachment);
            Assert.AreEqual(UnsafeFilename, attachment.Name);
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
				Html = TestHelper.ConvertHtmlToPdfHtml(),
			};
            var marginOptions = new WkHtmlMarginOptions();
			var footerOptions = new WkHtmlFooterOptions();

            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

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
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

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
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

			TestHelper.SaveResult("ConvertHtmlToPdfHtmlOnlyB5PageSize-test-result.pdf", result.PdfDocument);
		}
        [Test]
        public void ConvertHtmlToPdfHtmlMarginTop()
        {
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Orientation = "Landscape"
            };
            var marginOptions = new WkHtmlMarginOptions
            {
                MarginTop = 100
            };
            var footerOptions = new WkHtmlFooterOptions();

            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            TestHelper.SaveResult("ConvertHtmlToPdfHtmlMarginTop-test-result.pdf", result.PdfDocument);
        }
        [Test]
        public void ConvertHtmlToPdfHtmlIncludeFooterLine()
        {
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Orientation = "Landscape"
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions
            {
                IncludeFooterLine = true
            };

            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            TestHelper.SaveResult("ConvertHtmlToPdfHtmlIncludeFooterLine-test-result.pdf", result.PdfDocument);
        }
        [Test]
		public void ExtractTextByRegexShouldReturnSingleResultWhenOnlyOneMatch()
		{
			string expectedResult = "Test";

			var regex = @"\bTest\b";

			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText);

			var result = PdfTasks.ExtractTextByRegex(pdfBytes, regex);

			Assert.AreEqual(result.Length, 4);
			Assert.AreEqual(result, expectedResult);
		}
		[Test]
		public void ExtractTextByRegexShouldReturnAggregatedResultWhenMultipleMatches()
		{
			string expectedResult = "simply dummy textsimply dummy textsimply dummy textsimply dummy text";
             
			var regex = @"\bsimply dummy text\b";

            var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText);

			var result = PdfTasks.ExtractTextByRegex(pdfBytes, regex);

			Assert.AreEqual(result, expectedResult);
		}
		[Test]
		public void ExtractTextByRegexNotFoundRegexShouldReturnEmptyString()
		{
			var regex = @"\bTextThatShouldBeMissing\b";

			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText);

			var result = PdfTasks.ExtractTextByRegex(pdfBytes, regex);

			Assert.IsEmpty(result);
		}
		[Test]
		public void ExtractTextByRegexInvalidRegexShouldThrowArgumentException()
		{
			var regex = @"\InvalidRegex\";

			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText);

			Assert.Throws<ArgumentException>(() => PdfTasks.ExtractTextByRegex(pdfBytes, regex));
		}

		[Test]
		public void MergePDFs_MultiplePdfDocumentInputShouldBeMergedIntoOnePdfDocumentWithTwoPages()
		{
			// Arrange
			var input = new List<PdfDocumentInput>()
			{
				{
					new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments) }
				},
				{
					new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments) }
				}
			};
			// Act
			var result = PdfTasks.MergePdfs(input);
			var pdfDoc = TestHelper.BytesToPdf(result.PdfDocument);

			// Assert
			Assert.AreEqual(2, pdfDoc.GetNumberOfPages());

			TestHelper.SaveResult("MergePDFs-test-result.pdf", result.PdfDocument);
		}

		[Test]
		public void MergePdfs_NullInput_ThrowsArgumentNullException()
		{
			// Arrange
			List<PdfDocumentInput> input = null;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.MergePdfs(input));
		}

		[Test]
		public void MergePdfs_InputWithNullItem_ThrowsArgumentNullException()
		{
			// Arrange
			var input = new List<PdfDocumentInput>
			{
				new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments) },
				null // This item is null
			};

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.MergePdfs(input));
		}
	}
}
