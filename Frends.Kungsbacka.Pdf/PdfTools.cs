using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Frends.Kungsbacka.Pdf
{
    /// <summary>
    /// Functions for manipulating Pdf files
    /// </summary>
    internal static class PdfTools
    {
        /// <summary>
        /// Adds the supplied image as a new page after the last page. If the image
        /// contains Exif data with information about image orientation, the image
        /// is rotated according to that information before it is inserted into
        /// the page.
        /// </summary>
        /// <param name="pdfDocument">Pdf document to add image to</param>
        /// <param name="imageBytes">Image data</param>
        /// <param name="caption">Optional text that gets displayed above the image</param>
        public static bool AddImageAsNewPage(PdfDocument pdfDocument, byte[] imageBytes, string caption = "")
        {
            bool addText = !string.IsNullOrWhiteSpace(caption);
            imageBytes = ImageTools.RotateImage(imageBytes);

            if (imageBytes == null)
            {
                return false;
            }

            ImageData image = ImageDataFactory.Create(imageBytes, true);
            float imageWidth = image.GetWidth();
            float imageHeight = image.GetHeight();
            float margin = 10f;
            PageSize pageSize = imageWidth < imageHeight ? PageSize.A4 : PageSize.A4.Rotate();
            PdfPage page = pdfDocument.AddNewPage(pageSize);
            PdfCanvas canvas = new PdfCanvas(page);
            float pageWidth = page.GetPageSizeWithRotation().GetWidth();
            float pageHeight = page.GetPageSizeWithRotation().GetHeight();
            float textBoxHeight = 0f;

            if (addText)
            {
                textBoxHeight = 20f;
            }

            float maxImageWidth = pageWidth - margin * 2;
            float maxImageHeight = pageHeight - textBoxHeight - margin * 2;
            float ratio = Math.Min(maxImageWidth / imageWidth, maxImageHeight / imageHeight);

            if (ratio < 1f)
            {
                imageWidth *= ratio;
                imageHeight *= ratio;
            }

            Rectangle imageRect = new Rectangle(
                (pageWidth - imageWidth) / 2,
                (pageHeight - imageHeight) / 2 - textBoxHeight / 2,
                imageWidth,
                imageHeight
            );

            if (addText)
            {
                canvas.BeginText();
                canvas.SetFontAndSize(PdfFontFactory.CreateFont(), 11f);
                canvas.SetTextMatrix(margin, pageHeight - textBoxHeight / 2 - margin);
                canvas.ShowText(caption);
                canvas.EndText();
            }

            canvas.AddImageFittedIntoRectangle(image, imageRect, true);

            return true;
        }


        /// <summary>
        /// Merges two Pdf documents into one. Document #2 is placed after
        /// document #2.
        /// </summary>
        /// <param name="pdfDocument1">Pdf document to merge into</param>
        /// <param name="pdfDocument2">Pdf document to merge</param>
        public static PdfDocument MergePdfs(PdfDocument pdfDocument1, PdfDocument pdfDocument2)
        {
            // Make sure the documents can be merged by setting Unethical Reading to true
            // This avoids the error "PdfReader is not opened with owner password"
            pdfDocument1.GetReader().SetUnethicalReading(true);
            pdfDocument2.GetReader().SetUnethicalReading(true);
            PdfMerger merger = new PdfMerger(pdfDocument1);

            merger.Merge(pdfDocument2, 1, pdfDocument2.GetNumberOfPages());

            return pdfDocument1;
        }


        /// <summary>
        /// Extracts embedded files (EF) as attachments from the supplied Pdf.
        /// This method does not extract files from associated files (AF).
        /// Use the pattern parameter to filter out what files to extract.
        /// 
        /// Based on the following answers on Stack Overflow:
        /// <a href="https://stackoverflow.com/a/14951567">answer #1</a>, 
        /// <a href="https://stackoverflow.com/a/6334252">answer #2</a>
        /// </summary>
        /// <param name="pdfDocument">Pdf document to extract from</param>
        /// <param name="pattern">Optional filter for file names</param>
        /// <param name="extractOepPrefix">Optional boolean for extracting oep prefixes from the description</param>
        /// <param name="makeFilenameSafe">Optional boolean for replacing system disallowed file name characters with '_'</param>
        public static IEnumerable<PdfAttachment> ExtractAttachments(PdfDocument pdfDocument, string pattern = "", bool extractOepPrefix = false, bool makeFilenameSafe = true)
        {
            PdfArray fileSpecArray = GetFileSpecArray(pdfDocument);

            if (fileSpecArray == null)
            {
                return Enumerable.Empty<PdfAttachment>();
            }

            Regex regex = null;

            if (!string.IsNullOrWhiteSpace(pattern))
            {
                regex = GetRegexFromPattern(pattern);
            }

            var attachments = new List<PdfAttachment>();
            int size = fileSpecArray.Size();
            if (size % 2 != 0)
            {
                return attachments.AsEnumerable();
            }

            for (int i = 0; i < size; i += 2)
            {
                PdfDictionary fileSpec = fileSpecArray.GetAsDictionary(i + 1);

                if (fileSpec != null)
                {
                    PdfDictionary refs = fileSpec.GetAsDictionary(PdfName.EF);  
                    PdfStream stream = GetStream(refs);
                    string fileName = GetFileName(fileSpec);

                    string oepPrefix = extractOepPrefix ? GetOepFilePrefix(fileSpec, fileName) : string.Empty;

					if (!attachments.Any(x => x.Data.Length == stream.GetBytes().Length && x.Name == fileName))
					{
                        if (regex == null || regex.IsMatch(fileName))
                        {
                            attachments.Add(new PdfAttachment()
                            {
                                Name = fileName,
                                Extension = System.IO.Path.GetExtension(fileName), //Seems to actually allow many chars but exception on some though (ex. '|')
                                Data = stream.GetBytes(),
                                OepPrefix = oepPrefix
                            });
                        }
                    }
                }
            }

            return attachments.AsEnumerable();
        }


        private static string GetOepFilePrefix(PdfDictionary dict, string fileName)
        {
            if (dict.ContainsKey(PdfName.Desc))
            {
                var description = dict.GetAsString(PdfName.Desc).ToString();
                if (string.IsNullOrEmpty(description)) return string.Empty;

                //Prefix comes from oep like this: "GM image.PNG" with GM as the prefix followed by the filename
                //If no prefix is setup in oep, the filename isn´t included in the description which means we can strip the filename from the description to get the prefix
                if (!description.Contains(fileName)) return string.Empty;

                var oepPrefix = description.Replace(fileName, string.Empty);
                if (string.IsNullOrEmpty(oepPrefix)) return string.Empty;

                return oepPrefix.TrimEnd();
            }

            return string.Empty;
        }


        /// <summary>
        /// Similar to ExtractAttachments, but only returns a list
        /// with attachment names.
        /// </summary>
        /// <param name="pdfDocument">Pdf document to get attachment names from</param>
        /// <param name="pattern">Optional filter for file names</param>
        public static IEnumerable<string> GetAttachmentNames(PdfDocument pdfDocument, string pattern = "")
        {
            PdfArray fileSpecArray = GetFileSpecArray(pdfDocument);
            if (fileSpecArray == null)
            {
                return Enumerable.Empty<string>();
            }

            Regex regex = null;
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                regex = GetRegexFromPattern(pattern);
            }

            var fileNames = new List<string>();
            int fileArrSize = fileSpecArray.Size();
            if (fileArrSize % 2 != 0)
            {
                return fileNames.AsEnumerable();
            }

            for (int i = 0; i < fileArrSize; i += 2)
            {
                PdfDictionary fileSpec = fileSpecArray.GetAsDictionary(i + 1);
                string fileName = GetFileName(fileSpec);

                if (fileName != null)
                {
                    if (regex == null || regex.IsMatch(fileName))
                    {
                        fileNames.Add(fileName);
                    }
                }
            }

            return fileNames.AsEnumerable();
        }


        /// <summary>
        /// Removes both embedded and associated files with the supplied name.
        /// 
        /// An embedded file (EF) is a file stream that is embedded into a container
        /// Pdf file, while an ssociated file is a file that is embedded or referenced
        /// from a Pdf file using additional entries defined in ISO 32000-2, 14.13
        /// (<a href="https://www.pdfa.org/wp-content/uploads/2018/10/PDF20_AN002-AF.pdf">source</a>)
        /// </summary>
        /// <param name="pdfDocument">Pdf document to remove from</param>
        /// <param name="name">Name of file to remove (case insensitive)</param>
        public static void RemoveAttachment(PdfDocument pdfDocument, string name)
        {
            PdfArray fileSpecArray = GetFileSpecArray(pdfDocument);

            if (fileSpecArray != null)
            {
                var list = new List<int>();
                int size = fileSpecArray.Size();

                if (size % 2 != 0)
                {
                    return;
                }

                for (int i = 0; i < size; i += 2)
                {
                    PdfDictionary fileSpec = fileSpecArray.GetAsDictionary(i + 1);
                    if (fileSpec != null)
                    {
                        string fileName = GetFileName(fileSpec);
                        if (fileName.Equals(name))
                        {
                            list.Add(i);
                            list.Add(i + 1);
                        }
                    }
                }

                // Sort descending so we don't change indexes when removing.
                list.Sort((a, b) => b.CompareTo(a));
                foreach (int index in list)
                {
                    fileSpecArray.Remove(index);
                }
            }

            PdfDictionary root = pdfDocument.GetCatalog().GetPdfObject();
            PdfArray attachmentArray = root.GetAsArray(PdfName.AF);

            if (attachmentArray != null)
            {
                var list = new List<PdfObject>();

                for (int i = 0; i < attachmentArray.Size(); i++)
                {
                    PdfDictionary fileSpec = attachmentArray.GetAsDictionary(i);

                    if (fileSpec != null)
                    {
                        string fileName = GetFileName(fileSpec);

                        if (fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(attachmentArray.Get(i));
                        }
                    }
                }

                foreach (PdfObject fileSpec in list)
                {
                    attachmentArray.Remove(fileSpec);
                }
            }
        }


        /// <summary>
        /// Adds a footer text to all pages. Multiple lines are delimited by newline (\n)
        /// </summary>
        /// <param name="pdfDocument">Pdf document to add footer to</param>
        /// <param name="footerText">Footer text. Use \n to break up text into multiple lines</param>
        public static void AddFooter(PdfDocument pdfDocument, string footerText)
        {
            var doc = new Document(pdfDocument);
            int numLines = footerText.Count(c => c.Equals('\n'));
            Paragraph footer = new Paragraph(footerText)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.BLACK);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDocument.GetPage(i).GetPageSize();
                float x = pageSize.GetWidth() / 2;
                float y = pageSize.GetBottom() + 15 + (5 * numLines);

                doc.ShowTextAligned(footer, x, y, i, TextAlignment.CENTER, VerticalAlignment.MIDDLE, 0);
            }
        }


        private static PdfArray GetFileSpecArray(PdfDocument pdfDocument)
        {
            var array = pdfDocument
                ?.GetCatalog()
                ?.GetPdfObject()
                ?.GetAsDictionary(PdfName.Names)
                ?.GetAsDictionary(PdfName.EmbeddedFiles)
                ?.GetAsArray(PdfName.Names);

            if (array != null)
            {
                return array;
            }

            // When there are lots of embedded files, they get split up
            // into multiple dictionaries (kids).
            array = pdfDocument
                ?.GetCatalog()
                ?.GetPdfObject()
                ?.GetAsDictionary(PdfName.Names)
                ?.GetAsDictionary(PdfName.EmbeddedFiles)
                ?.GetAsArray(PdfName.Kids); 
            
            if (array != null)
            {
                var combined = new PdfArray();

                foreach (PdfDictionary dict in array)
                {
                    combined.AddAll(dict.GetAsArray(PdfName.Names));
                }

                return combined.Count() == 0 ? null : combined;
            }
            return null;
        }


        /// <summary>
        /// Retrieves the file name from a given PdfDictionary representing a file specification.
        /// If the dictionary contains the 'UF' (Unicode File) key, its value is returned as the file name.
        /// Otherwise, the value of the 'F' (File) key is used. Optionally, the file name can be sanitized
        /// by replacing invalid file name characters with underscores.
        /// </summary>
        /// <param name="dict">PdfDictionary representing a file specification to get the names from</param>
        /// <param name="sanitizeFileName">Optionally replace invalid chars with '_'</param>
        private static string GetFileName(PdfDictionary dict, bool sanitizeFileName = false)
        {
            if (dict == null)
            {
                return null;
            }

            if (dict.ContainsKey(PdfName.UF))
            {
                return dict.GetAsString(PdfName.UF).ToString();
            }
            
            string fileName = dict.GetAsString(PdfName.F).ToString();

            if (sanitizeFileName)
            {
                char[] invalidFileChars = System.IO.Path.GetInvalidFileNameChars();
                fileName = new string(fileName.Select(ch => invalidFileChars.Any(invCh => invCh == ch) ? '_' : ch).ToArray());
            }

            return fileName;
        }


        private static PdfStream GetStream(PdfDictionary dict)
        {
            if (dict == null)
            {
                return null;
            }

            if (dict.ContainsKey(PdfName.UF))
            {
                return dict.GetAsStream(PdfName.UF);
            }

            return dict.GetAsStream(PdfName.F);
        }


        private static Regex GetRegexFromPattern(string pattern)
        {
            string regexPattern = string.Join("|",
                pattern
                    .Split(',')
                    .Select(s => "(" + Regex.Escape(s.Trim()).Replace(@"\*", ".*").Replace(@"\?", ".") + ")")
                    .ToArray()
            );

            return new Regex(
                "^" + regexPattern + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );
        }
    }
}
