using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Frends.Kungsbacka.Pdf.Tests
{
	public class ExtractEkopostRecipientMetadata
	{
		private byte[] CreatePdf(int numberOfPages, string textPrefix)
		{
			using (var ms = new MemoryStream())
			{
				using (var writer = new PdfWriter(ms))
				{
					using (var pdfDoc = new PdfDocument(writer))
					{
						using (var doc = new Document(pdfDoc))
						{
							for (int i = 1; i <= numberOfPages; i++)
							{
								doc.Add(new Paragraph($"{textPrefix} - Page {i}"));
								if (i < numberOfPages)
								{
									// Add a new page
									pdfDoc.AddNewPage();
								}
							}
						}
					}
				}
				return ms.ToArray();
			}
		}

		[Test]
		public void MergePDFs()
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
			var pdfDoc = CreatePdfDocument(result);

			// Assert
			Assert.AreEqual(2, pdfDoc.GetNumberOfPages());

			TestHelper.SaveResult("MergePDFs-test-result.pdf", result.PdfDocument);
		}

		private static PdfDocument CreatePdfDocument(PdfDocumentResult result)
		{
			var memStream = new MemoryStream(result.PdfDocument);
			var pdfReader = new PdfReader(memStream);
			var pdfDoc = new PdfDocument(pdfReader);

			return pdfDoc;
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
			new PdfDocumentInput { PdfDocument = CreatePdf(1, "Doc1") },
			null // This item is null
        };

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => PdfTasks.MergePdfs(input));
		}

		[Test]
		public void MergePdfs_SinglePdfInList_SamePdfReturned()
		{
			// Arrange
			var singlePdf = CreatePdf(2, "SingleDoc");
			var input = new List<PdfDocumentInput>
			{
				new PdfDocumentInput { PdfDocument = singlePdf }
			};

			// Act
			var result = PdfTasks.MergePdfs(input);

			// Assert
			Assert.NotNull(result);
			Assert.NotNull(result.PdfDocument);

			var pdfDoc = CreatePdfDocument(result);
			Assert.AreEqual(2, pdfDoc.GetNumberOfPages());
		}

		[Test]
		public void MergePdfs_MultiplePdfs_ReturnsMergedPdf()
		{
			// Arrange
			var pdf1 = CreatePdf(2, "Doc1");
			var pdf2 = CreatePdf(3, "Doc2");

			var input = new List<PdfDocumentInput>
			{
				new PdfDocumentInput { PdfDocument = pdf1 },
				new PdfDocumentInput { PdfDocument = pdf2 }
			};

			// Act
			var result = PdfTasks.MergePdfs(input);

			// Assert
			Assert.NotNull(result);
			Assert.NotNull(result.PdfDocument);

			var pdfDoc = CreatePdfDocument(result);
			Assert.AreEqual(5, pdfDoc.GetNumberOfPages());
		}
	}
}