using System;
using System.ComponentModel;
using System.Threading.Tasks;
using iText.IO.Source;
using iText.Kernel.Pdf;
using Newtonsoft.Json;
using PugPdf.Core;
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
                bool success = true;
                try
                {
                    PdfTools.AddImageAsNewPage(
                        pdf.Document,
                        attachedImage.Data,
                        options?.Caption?.Replace("[FILENAME]", attachedImage.Name)
                    );
                }
                catch (InvalidImageDataException)
                {
                    success = false;
                    // We ignore this exception to make it easier
                    // to use this task without knowing the type of
                    // all embedded files.
                }
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
        public static async Task<PdfDocumentResult> ConvertHtmlToPdf([PropertyTab] ConvertHtmlToPdfInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (string.IsNullOrEmpty(input.Html))
            {
                throw new ArgumentNullException(nameof(input.Html));
            }

            var renderer = new HtmlToPdf();
            renderer.PrintOptions.Title = input.Title;

            if (!string.IsNullOrEmpty(input.PdfHeader))
            {
                renderer.PrintOptions.Header = JsonConvert.DeserializeObject<PdfHeader>(input.PdfHeader);
            }

            if (!string.IsNullOrEmpty(input.PdfFooter))
            {
                renderer.PrintOptions.Footer = JsonConvert.DeserializeObject<PdfFooter>(input.PdfFooter);
            }

            if (!string.IsNullOrEmpty(input.Orientation) && Enum.TryParse(input.Orientation, out PdfOrientation orientation))
            {
                renderer.PrintOptions.Orientation = orientation;
            }

            if (!string.IsNullOrEmpty(input.PageSize) && Enum.TryParse(input.PageSize, out PdfPageSize pageSize))
            {
                renderer.PrintOptions.PageSize = pageSize;
            }

            var pdf = await renderer.RenderHtmlAsPdfAsync(input.Html);

            return new PdfDocumentResult
            {
                PdfDocument = pdf.BinaryData
            };
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
