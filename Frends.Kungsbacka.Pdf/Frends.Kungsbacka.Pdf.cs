using Frends.Kungsbacka.Pdf.HtmlToPdf;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;

namespace Frends.Kungsbacka.Pdf
{
	/// <summary>
	/// Implements Pdf related tasks
	/// </summary>
	public static class PdfTasks
    {
        /// <summary>
        /// Adds an image to a new page after the last page.
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <param name="options">Optional parameters</param>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult AddImageAsNewPage([PropertyTab] AddImageInput input, [PropertyTab] AddImageOptions options)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            if (input.Image is null)
            {
                throw new ArgumentNullException(nameof(input.Image));
            }
            var pdf = new Pdf(input.PdfDocument);
            PdfTools.AddImageAsNewPage(
                pdf.Document, 
                input.Image,
                options?.Caption
            );
            var output = new PdfDocumentResult
            {
                PdfDocument = pdf.ToArray()
            };
            return output;
        }

        /// <summary>
        /// Extracts and removes images embedded as files (EF) in a Pdf document
        /// and adds them back to new pages (one per image) after the last page.
        /// If a Pdf contains embedded files that are not images, they will be ignored.
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <param name="options">Optional parameters</param>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult ConvertEmbeddedImagesToPages([PropertyTab] PdfDocumentInput input, [PropertyTab] ConvertEmbeddedOptions options)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            // Since AddImageAsNewPage will just do nothing and return if
            // something other than an image file is supplied, we can just
            // use wildcard here if no filter i supplied.
            string pattern = options?.Filter ?? "*";
            var pdf = new Pdf(input.PdfDocument);
            foreach (PdfAttachment attachedImage in PdfTools.ExtractAttachments(pdf.Document, pattern))
            {
				var success = PdfTools.AddImageAsNewPage(
						pdf.Document,
						attachedImage.Data,
						options?.Caption?.Replace("[FILENAME]", attachedImage.Name));

				if (success)
                {
                    PdfTools.RemoveAttachment(pdf.Document, attachedImage.Name);
                }
            }
            var output = new PdfDocumentResult
            {
                PdfDocument = pdf.ToArray()
            };
            return output;
        }

        /// <summary>
        /// Merges two Pdf documents into one
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <param name="options">Optional parameters</param>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult MergeEmbeddedPdfDocuments([PropertyTab] PdfDocumentInput input, [PropertyTab] PdfCommonOptions options)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            var pdf = new Pdf(input.PdfDocument);
            // Extension is currently the only method we use to try to find embedded
            // Pdf documents.
            string pattern = options?.Filter;
            if (string.IsNullOrEmpty(pattern))
            {
                pattern = "*.pdf";
            }
            else if (!pattern.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                pattern += ".pdf";
            }
            foreach (PdfAttachment attachedFile in PdfTools.ExtractAttachments(pdf.Document, pattern))
            {
                // Cannot use Pdf helper class here since you cannot copy from a PdfDocument
                // that has a PdfWriter (https://stackoverflow.com/a/58434289)
                IRandomAccessSource source = new RandomAccessSourceFactory().CreateSource(attachedFile.Data);
                var pdfToEmbed = new PdfDocument(new PdfReader(source, new ReaderProperties()));
                PdfTools.MergePdfs(pdf.Document, pdfToEmbed);
            }
            var output = new PdfDocumentResult
            {
                PdfDocument = pdf.ToArray()
            };
            return output;
        }

        /// <summary>
        /// Extracts embedded files from a Pdf document. The attachments are returned
        /// as PdfAttachment objects with the following members: Name (file name),
        /// Extension (file extension), Data (file as byte array)
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <param name="options">Optional parameters</param>
        /// <returns>AttachmentResult {IEnumerable&lt;PdfAttachment&gt; Attachments}</returns>
        public static ExtractAttachmentsResult ExtractAttachments([PropertyTab] PdfDocumentInput input, [PropertyTab] PdfCommonOptions options)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            string pattern = options?.Filter ?? "*";
            var pdf = new Pdf(input.PdfDocument);
            var output = new ExtractAttachmentsResult
            {
                Attachments = PdfTools.ExtractAttachments(pdf.Document, pattern, options.ExtractOepPrefix)
            };
            return output;
        }

        /// <summary>
        /// Removes both embedded (EF) and associated (AF) files from a Pdf document.
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <param name="options">Optional parameters</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult RemoveAttachments([PropertyTab] PdfDocumentInput input, [PropertyTab] PdfCommonOptions options)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            string pattern = options?.Filter ?? "*";
            var pdf = new Pdf(input.PdfDocument);
            foreach(string attachmentName in PdfTools.GetAttachmentNames(pdf.Document, pattern))
            {
                PdfTools.RemoveAttachment(pdf.Document, attachmentName);
            }
            return new PdfDocumentResult
            {
                PdfDocument = pdf.ToArray()
            };
        }

        /// <summary>
        /// Adds footer text to all pages in a Pdf document.
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult AddFooter([PropertyTab] AddFooterInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.PdfDocument is null)
            {
                throw new ArgumentNullException(nameof(input.PdfDocument));
            }
            var pdf = new Pdf(input.PdfDocument);
            PdfTools.AddFooter(pdf.Document, input.Text);
            return new PdfDocumentResult
            {
                PdfDocument = pdf.ToArray()
            };
        }

        /// <summary>
        /// Converts html string to pdf document.
        /// </summary>
        /// <param name="input">Mandatory parameters</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
        public static PdfDocumentResult ConvertHtmlToPdf([PropertyTab] ConvertHtmlToPdfInput input,
                                                         [PropertyTab] WkHtmlMarginOptions marginOptions,
                                                         [PropertyTab] WkHtmlFooterOptions footerOptions)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (string.IsNullOrEmpty(input.Html))
            {
                throw new ArgumentNullException(nameof(input.Html));
            }

            var converter = new HtmlToPdfConverter(input, marginOptions, footerOptions);

            var pdf = converter.RenderHtmlAsPdf(input.Html);

            return new PdfDocumentResult
            {
                PdfDocument = pdf
            };
        }

		/// <summary>
		/// Extracts textstring from PDF:s
		/// </summary>
		/// <param name="pdfBytes">Mandatory parameters</param>
		/// <param name="regexPattern">Mandatory parameters</param>
		/// <returns></returns>
		/// <returns>string></returns>
		public static string ExtractTextByRegex(byte[] pdfBytes, string regexPattern)
		{
			using (var memoryStream = new MemoryStream(pdfBytes))
			{
				var pdfDocument = new PdfDocument(new PdfReader(memoryStream));

				var numberOfPages = pdfDocument.GetNumberOfPages();
				var stringBuilder = new StringBuilder();

				for (int index = 1; index <= numberOfPages; ++index)
				{
					var extractionStrategy = new RegexBasedLocationExtractionStrategy(regexPattern);
					new PdfCanvasProcessor(extractionStrategy).ProcessPageContent(pdfDocument.GetPage(index));

					foreach (IPdfTextLocation resultantLocation in extractionStrategy.GetResultantLocations())
					{
						string text = resultantLocation.GetText();
						if (!string.IsNullOrEmpty(text))
							stringBuilder.Append(text);
					}
				}
				return stringBuilder.ToString();
			}
		}

		
		/// <summary>
		/// Merges several PDF:s together
		/// </summary>
		/// <param name="input">Mandatory parameters</param>
		/// <returns>PdfDocumentResult {byte[] PdfDocument}</returns>
		public static PdfDocumentResult MergePdfs([PropertyTab] List<PdfDocumentInput> input)
		{
			if (input is null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			if (input.Any(x => x is null))
			{
				throw new ArgumentNullException(nameof(input));
			}

			byte[] result = null;

			using (var outputStream = new MemoryStream())
			{
				using (var pdfWriter = new PdfWriter(outputStream))
				using (var mergedPdf = new PdfDocument(pdfWriter))
				{
					var pdfMerger = new PdfMerger(mergedPdf);

					foreach (var pdfBytes in input)
					{
						using var tempStream = new MemoryStream(pdfBytes.PdfDocument);
						using var reader = new PdfReader(tempStream);
						using var sourcePdf = new PdfDocument(reader);

						pdfMerger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
					}
				}

				result = outputStream.ToArray();
			}

			var output = new PdfDocumentResult
			{
				PdfDocument = result
			};

			return output;
		}
		private class Pdf
        {
            private readonly PdfDocument _document;
            private readonly ByteArrayOutputStream _outputStream;
            private bool _isClosed;

            public PdfDocument Document
            {
                get
                {
                    if (_isClosed)
                    {
                        throw new InvalidOperationException("Cannot access a closed document.");
                    }
                    return _document;
                }
            }

            public Pdf(byte[] pdfBytes)
            {
                IRandomAccessSource source = new RandomAccessSourceFactory().CreateSource(pdfBytes);
                _outputStream = new ByteArrayOutputStream();
                var pdfReader = new PdfReader(source, new ReaderProperties());
                // Avoid "PdfReader is not opened with owner password" errors
                pdfReader.SetUnethicalReading(true);
                _document = new PdfDocument(pdfReader, new PdfWriter(_outputStream));
            }

            public byte[] ToArray()
            {
                if (_isClosed)
                {
                    throw new InvalidOperationException("Cannot call ToArray() on a closed document.");
                }
                Document.Close();
                _outputStream.Close();
                _isClosed = true;
                return _outputStream.ToArray();
            }
        }
    }
}
