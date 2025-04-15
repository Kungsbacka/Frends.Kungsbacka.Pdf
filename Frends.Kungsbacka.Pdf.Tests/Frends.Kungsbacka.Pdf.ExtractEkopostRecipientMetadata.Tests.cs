using NUnit.Framework;
using System.Linq;
namespace Frends.Kungsbacka.Pdf.Tests
{
	public class ExtractEkopostRecipientMetadataTests
	{
		[Test]
		public void ExtractEkopostRecipientMetadataAndDocumentByRegex_NoMatches_ReturnsNull()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments);
			var pattern = @"\{name:""DummyPattern""";

			// Act
			var result = PdfTasks.ExtractEkopostRecipientMetadataAndDocumentByRegex(pdfBytes, pattern);

			// Assert
			Assert.IsEmpty(result);
		}

		[Test]
		public void ExtractEkopostRecipientMetadataAndDocumentByRegex_SingleMatch_ReturnsOneResult()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments);
			var pattern = "This is a test document used for unit testing of PDF";

			// Act
			var result = PdfTasks.ExtractEkopostRecipientMetadataAndDocumentByRegex(pdfBytes, pattern);

			// Assert
			Assert.NotNull(result);

			var list = result.ToList();
			Assert.AreEqual(1, list.Count);

			Assert.AreEqual(pattern, list[0].Metadata);
		}

		[Test]
		public void ExtractEkopostRecipientMetadataAndDocumentByRegex_MultipleMatches_ReturnsMultipleResults()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments);
			var pattern = @"(?i)\b(test)\b";

			// Act
			var result = PdfTasks.ExtractEkopostRecipientMetadataAndDocumentByRegex(pdfBytes, pattern);

			// Assert
			Assert.NotNull(result);
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);

			StringAssert.Contains("test", list[0].Metadata);
			StringAssert.Contains("Test", list[1].Metadata);
		}
		[Test]
		public void ExtractEkopostRecipientMetadataAndDocumentByRegex_ShouldFindDocumentsAndMetadata()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.WíthEkopostMetadata);
			var pattern = @"\{(.*?)\}";

			// Act
			var result = PdfTasks.ExtractEkopostRecipientMetadataAndDocumentByRegex(pdfBytes, pattern);

			// Assert
			Assert.IsNotEmpty(result);
			Assert.AreEqual(3, result.Count());

			var first = result.ToList()[0];
			Assert.AreEqual("{name:\"Test\", merge:\"11111111-1111\", subject:\"Test\"}", first.Metadata);
			Assert.AreEqual(3, TestHelper.BytesToPdf(first.Document).GetNumberOfPages());

			var second = result.ToList()[1];
			Assert.AreEqual("{name:\"Test\", merge:\"22222222-2222\", subject:\"Test\"}", second.Metadata);
			Assert.AreEqual(2, TestHelper.BytesToPdf(second.Document).GetNumberOfPages());

			var third = result.ToList()[2];
			Assert.AreEqual("{name:\"Test\", merge:\"33333333-3333\", subject:\"Test\"}", third.Metadata);
			Assert.AreEqual(1, TestHelper.BytesToPdf(third.Document).GetNumberOfPages());
		}
		[Test]
		public void ExtractTextByRegex_MultipleMatch_ReturnsConcatenatedResult()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments);
			var pattern = "test";

			// Act
			var result = PdfTasks.ExtractTextByRegex(pdfBytes, pattern);

			// Assert
			Assert.NotNull(result);
			StringAssert.Contains("testtest", result);
		}
		[Test]
		public void ExtractTextByRegex_SingleMatch_ReturnsMatchingResult()
		{
			// Arrange
			var pdfBytes = TestHelper.GetTestDocument(TestHelper.TestDocumentTypes.NoAttachments);
			var pattern = "Watermarks";

			// Act
			var result = PdfTasks.ExtractTextByRegex(pdfBytes, pattern);

			// Assert
			Assert.NotNull(result);
			StringAssert.Contains(pattern, result);
		}
	}
}