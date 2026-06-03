using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        [Test]
        public void ExtractAttachments_AppendsNumber_WhenRequested()
        {
            var fileNames = new[] { "doc.pdf", "doc.pdf", "other.txt" };
            var pdfBytes = TestHelper.CreatePdfWithAttachment(fileNames);

            var input = new PdfDocumentInput { PdfDocument = pdfBytes };
            var options = new ExtractAttachmentsOptions { Filter = "*", MakeFilenameSafe = true, AppendAttachmentNumber = true };

            var result = PdfTasks.ExtractAttachments(input, options);

            Assert.AreEqual(3, result.Attachments.Count());

            var names = result.Attachments.Select(a => a.Name).ToList();

            // Expect exact file names in order when appending numbers
            var expected = new[] { "doc_bilaga1.pdf", "doc_bilaga2.pdf", "other_bilaga3.txt" };
            CollectionAssert.AreEqual(expected, names);
        }

        [Test]
        public void ExtractAttachments_DoesNotAppend_WhenNotRequested()
        {
            var fileNames = new[] { "doc.pdf", "doc1.pdf" };
            var pdfBytes = TestHelper.CreatePdfWithAttachment(fileNames);

            var input = new PdfDocumentInput { PdfDocument = pdfBytes };
            var options = new ExtractAttachmentsOptions { Filter = "*", MakeFilenameSafe = true, AppendAttachmentNumber = false };

            var result = PdfTasks.ExtractAttachments(input, options);

            var names = result.Attachments.Select(a => a.Name).ToList();

            // Default behavior: allow duplicate display names (two returned attachments should both be "doc.pdf")
            var expected = new[] { "doc.pdf", "doc1.pdf" };
            CollectionAssert.AreEqual(expected, names);
        }

        [Test]
        public void ExtractAttachments_DoesNotAppend_WhenAppendAttachmentNumberNotSet()
        {
            var fileNames = new[] { "doc.pdf", "doc1.pdf" };
            var pdfBytes = TestHelper.CreatePdfWithAttachment(fileNames);

            var input = new PdfDocumentInput { PdfDocument = pdfBytes };
            var options = new ExtractAttachmentsOptions { Filter = "*", MakeFilenameSafe = true};

            var result = PdfTasks.ExtractAttachments(input, options);

            var names = result.Attachments.Select(a => a.Name).ToList();

            // Default behavior: allow duplicate display names
            var expected = new[] { "doc.pdf", "doc1.pdf" };
            CollectionAssert.AreEqual(expected, names);
        }

        [Test]
        public void ExtractAttachments_RemovesDuplicates_WhenFilesAreSameLengthAndHaveSameName()
        {
            var fileNames = new[] { "doc.pdf", "doc.pdf" };
            var pdfBytes = TestHelper.CreatePdfWithAttachment(fileNames);

            var input = new PdfDocumentInput { PdfDocument = pdfBytes };
            var options = new ExtractAttachmentsOptions { Filter = "*", MakeFilenameSafe = true };

            var result = PdfTasks.ExtractAttachments(input, options);

            var names = result.Attachments.Select(a => a.Name).ToList();

            var expected = new[] { "doc.pdf" };
            CollectionAssert.AreEqual(expected, names);
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

			var resultEmbeddedFileNames = TestHelper.GetFileSpecArray(resultPdf).Where(x => x.IsString()).Select(x => System.IO.Path.GetExtension(x.ToString()));

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
            //Arrange
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
            //Arrange
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

        [Test]
        public void ConvertHtmlToPdf_HtmlOnly()
        {
            // arrange
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml()
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            Assert.NotNull(result.PdfDocument);
            Assert.True(result.PdfDocument.Length > 100);

            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            Assert.AreEqual(1, pdf.GetNumberOfPages());
        }


        [Test]
        public void ConvertHtmlToPdf_UsesCorrectEncoding()
        {
            // arrange
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml()
            };
            var margins = new WkHtmlMarginOptions();
            var footer = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, margins, footer);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            // extract all text from page 1
            var text = PdfTextExtractor.GetTextFromPage(pdf.GetFirstPage());

            // now just look for your Swedish string
            StringAssert.Contains(
                "Detta är en text i mitten av dokumentet. Specialtecken: !#¤%&/()=?éèì",
                text);
        }

        [Test]
        public void ConvertHtmlToPdf_SetsDocumentTitleInMetadata()
        {
            // arrange
            var expectedTitle = "My Title";

            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Title = expectedTitle
            };
            var margins = new WkHtmlMarginOptions();
            var footer = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, margins, footer);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);
            var info = pdf.GetDocumentInfo();

            Assert.AreEqual(expectedTitle, pdf.GetDocumentInfo().GetTitle());
        }


        [TestCase(null)]
        [TestCase("")]
        public void ConvertHtmlToPdf_DefaultsToPageSizeA4(string pageSize)
        {
            // arrange
            var tolerance = 1.0f;

            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                PageSize = pageSize
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            Assert.AreEqual(PageSize.A4.GetWidth(), pdf.GetFirstPage().GetPageSize().GetWidth(), tolerance);
            Assert.AreEqual(PageSize.A4.GetHeight(), pdf.GetFirstPage().GetPageSize().GetHeight(), tolerance);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ConvertHtmlToPdf_DefaultsToOrientationPortrait(string orientation)
        {
            // arrange
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Orientation = orientation
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            Assert.Less(pdf.GetFirstPage().GetPageSize().GetWidth(), pdf.GetFirstPage().GetPageSize().GetHeight());
        }

        [Test]
        public void ConvertHtmlToPdf_OnlyLandscape()
        {
            // arrange
            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Orientation = "Landscape"
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            Assert.Less(pdf.GetFirstPage().GetPageSize().GetHeight(), pdf.GetFirstPage().GetPageSize().GetWidth());
        }

        [Test]
        public void ConvertHtmlToPdf_OnlyB5PageSize()
        {
            // arrange
            var tolerance = 1.0f;

            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                PageSize = "B5"
            };
            var marginOptions = new WkHtmlMarginOptions();
            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            Assert.AreEqual(PageSize.B5.GetWidth(), pdf.GetFirstPage().GetPageSize().GetWidth(), tolerance);
            Assert.AreEqual(PageSize.B5.GetHeight(), pdf.GetFirstPage().GetPageSize().GetHeight(), tolerance);
        }

        [Test]
        public void ConvertHtmlToPdf_MarginTop()
        {
            // arrange
            var tolerance = 2f;
            var givenMarginTop = 100; // Default is mm if no unit is specified.
            var expectedMarginTopPt = givenMarginTop * 72f / 25.4f; // A PDF point is 1/72 inch. 1 inch = 25.4 mm.

            var input = new ConvertHtmlToPdfInput
            {
                Html = TestHelper.ConvertHtmlToPdfHtml(),
                Orientation = "Landscape"
            };

            var marginOptions = new WkHtmlMarginOptions
            {
                MarginTop = givenMarginTop
            };

            var footerOptions = new WkHtmlFooterOptions();

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);

            var finder = new TextMarginFinder();
            var processor = new PdfCanvasProcessor(finder);
            processor.ProcessPageContent(pdf.GetPage(1));

            var textRectangle = finder.GetTextRectangle();
            Assert.NotNull(textRectangle);

            float pageHeight = pdf.GetFirstPage().GetPageSize().GetHeight();
            float actualTopMargin = pageHeight - (textRectangle.GetY() + textRectangle.GetHeight());

            Assert.AreEqual(expectedMarginTopPt, actualTopMargin, tolerance);
        }

        [Test]
        public void ConvertHtmlToPdf_IncludeFooterLine()
        {
            // arrange
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

            // act
            var result = PdfTasks.ConvertHtmlToPdf(input, marginOptions, footerOptions);

            // assert
            var pdf = TestHelper.BytesToPdf(result.PdfDocument);
            var bytes = pdf.GetFirstPage().GetContentBytes();
            var content = Encoding.ASCII.GetString(bytes);

            // NOTE: Below is AI code. The alternative was to use a whole lot of extra code,
            //       just to assert that the footer line was drawn.
            // look for a "move-to (m) … line-to (l) … stroke (S)" sequence
            // pattern:      <num> <num> m   <num> <num> l   S
            var regextPattern = @"\d+(\.\d+)?\s+\d+(\.\d+)?\s+m\s+\d+(\.\d+)?\s+\d+(\.\d+)?\s+l\s+S";

            StringAssert.IsMatch(regextPattern, content,
                "Expected to see a stroked horizontal line operator in the PDF content stream.");
        }

        [Test]
		public void ExtractTextByRegexShouldReturnSingleResultWhenOnlyOneMatch()
		{
			string expectedResult = "Test";

			var regex = @"\bTest\b";

			var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText) };

			var result = PdfTasks.ExtractTextByRegex(pdfDocumentInput, regex);

			Assert.AreEqual(result.Length, 4);
			Assert.AreEqual(result, expectedResult);
		}
		[Test]
		public void ExtractTextByRegexShouldReturnAggregatedResultWhenMultipleMatches()
		{
			string expectedResult = "simply dummy textsimply dummy textsimply dummy textsimply dummy text";
             
			var regex = @"\bsimply dummy text\b";

            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText) };

            var result = PdfTasks.ExtractTextByRegex(pdfDocumentInput, regex);

			Assert.AreEqual(result, expectedResult);
		}
		[Test]
		public void ExtractTextByRegexNotFoundRegexShouldReturnEmptyString()
		{
			var regex = @"\bTextThatShouldBeMissing\b";

            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText) };

			var result = PdfTasks.ExtractTextByRegex(pdfDocumentInput, regex);

			Assert.IsEmpty(result);
		}
		[Test]
		public void ExtractTextByRegexInvalidRegexShouldThrowArgumentException()
		{
			var regex = @"\InvalidRegex\";

            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText) };

            Assert.Throws<RegexParseException>(() => PdfTasks.ExtractTextByRegex(pdfDocumentInput, regex));
		}

        [Test]
        public void ExtractAllText_ExtractsCorrectTextFromPDF()
        {
            // Arrange
            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractText) };
            // Act
            var result = PdfTasks.ExtractAllText(pdfDocumentInput);
            // Assert
            Assert.IsNotEmpty(result);
            Assert.AreEqual("Test PDF ", result[0]);
            Assert.AreEqual("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the ", result[1]);
            Assert.AreEqual("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the ", result[13]);
        }

        [Test]
        public void ExtractAllText_ExtractsCorrectTextFromPDFWithMultiplePages()
        {
            // Arrange
            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractTextMultiplePages) };
            // Act
            var result = PdfTasks.ExtractAllText(pdfDocumentInput);

            // Assert
            Assert.IsNotEmpty(result);
            //First page
            Assert.AreEqual("Test file #1", result[0]);
            Assert.AreEqual("This is a test document used for unit testing of PDF", result[1]);
            // Second page
            Assert.AreEqual("Test file #1", result[28]);
            Assert.AreEqual("This is a test document used for unit testing of PDF", result[29]);
        }

        [Test]
        public void ExtractAllText_ThrowsExceptionWhenInputIsNull()
        {
            // Arrange
            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = null };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PdfTasks.ExtractAllText(pdfDocumentInput));
        }


        [Test]
        public void ExtractAllText_ThrowsExceptionWhenInputIsEmpty()
        {
            // Arrange
            var pdfDocumentInput = new PdfDocumentInput { PdfDocument = Array.Empty<byte>() };
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PdfTasks.ExtractAllText(pdfDocumentInput));
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

        [Test]
		public void SplitPDF_SplitsPDFIntoMultiplePages()
		{
			// Arrange
			var input = new PdfDocumentInput { PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractTextMultiplePages) };
            var options = new SplitPdfOptions { PageCount = 1 };

			// Act & Assert
			var result = PdfTasks.SplitPdf(input, options);
			Assert.IsNotEmpty(result);
			Assert.AreEqual(2, result.Count);
		}

		[Test]
		public void SplitPDF_PdfDocumentInputNullInput_ThrowsArgumentNullException ()
		{
			// Arrange
			var input = new PdfDocumentInput { PdfDocument = null };
            var options = new SplitPdfOptions { PageCount = 1 };
			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.SplitPdf(input, options));
		}

        [Test]
		public void SplitPDF_NullInput_ThrowsArgumentNullException ()
		{
			// Arrange
			PdfDocumentInput input = null;
			var options = new SplitPdfOptions { PageCount = 1 };

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.SplitPdf(input, options));
		}

        [Test]
		public void SplitPDF_EmptyInput_ThrowsArgumentNullException ()
		{
			// Arrange
			PdfDocumentInput input = new PdfDocumentInput { PdfDocument = Array.Empty<byte>() };
			var options = new SplitPdfOptions { PageCount = 1 };

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.SplitPdf(input, options));
		}
        [Test]
        public void SplitPDF_SplitsPDFIntoSinglePageDocuments()
        {
            // Arrange
            var input = new PdfDocumentInput
            {
                PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractTextMultiplePages)
            };
			var options = new SplitPdfOptions { PageCount = 1 };

			// Act
			var result = PdfTasks.SplitPdf(input, options);

            // Assert
            Assert.AreEqual(2, result.Count);

            foreach (var splitPdf in result)
            {
                using var stream = new MemoryStream(splitPdf.PdfDocument);
                using var reader = new PdfReader(stream);
                using var pdfDoc = new PdfDocument(reader);

                Assert.AreEqual(1, pdfDoc.GetNumberOfPages());
            }
        }
		[Test]
		public void SplitPDF_SplitsPDFIntoSingleDocumentWithTwoPagesWhenSettingPageCountTwo()
		{
			// Arrange
			var input = new PdfDocumentInput
			{
				PdfDocument = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.ExtractTextMultiplePages)
			};
			var options = new SplitPdfOptions { PageCount = 2 };

			// Act
			var result = PdfTasks.SplitPdf(input, options);

			// Assert
			Assert.AreEqual(1, result.Count);
		}
	}
}
