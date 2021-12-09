using System;
using System.ComponentModel;
using System.Threading;
using iText.IO.Source;
using iText.Kernel.Pdf;

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
                try
                {
                    PdfTools.AddImageAsNewPage(
                        pdf.Document,
                        attachedImage.Data,
                        options.Caption?.Replace("[FILENAME]", attachedImage.Name)
                    );
                }
                catch (InvalidImageDataException)
                {
                    // We ignore this exception to make it easier
                    // to use this task without knowing the type of
                    // all embedded files.
                }
                PdfTools.RemoveAttachment(pdf.Document, attachedImage.Name);
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
                Attachments = PdfTools.ExtractAttachments(pdf.Document, pattern)
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
                _document = new PdfDocument(new PdfReader(source, new ReaderProperties()), new PdfWriter(_outputStream));
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
