using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frends.Kungsbacka.Pdf
{
    /// <summary>
    /// Input for tasks that only takes a Pdf document as a required parameter
    /// </summary>
    public class PdfDocumentInput
    {
        /// <summary>
        /// Pdf document
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public byte[] PdfDocument { get; set; }
    }

    /// <summary>
    /// Optional parameters
    /// </summary>
    public class PdfCommonOptions
    {
        /// <summary>
        /// Filter attachments. Multiple filters are supported separated by comma (ex.: *.jpg, *.png).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Filter { get; set; }
        /// <summary>
        /// Extract prefix from PDF description
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public bool ExtractOepPrefix { get; set; }
    }

    /// <summary>
    /// Returned from tasks that returns an altered Pdf document
    /// </summary>
    public class PdfDocumentResult
    {
        /// <summary>
        /// Pdf document
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public byte[] PdfDocument { get; set; }
    }

    /// <summary>
    /// Returned from task ExtractAttachments
    /// </summary>
    public class ExtractAttachmentsResult
    {
        /// <summary>
        /// Pdf attachments
        /// </summary>
        public IEnumerable<PdfAttachment> Attachments { get; set; }
    }

    /// <summary>
    /// Required parameters for task AddImage
    /// </summary>
    public class AddImageInput
    {
        /// <summary>
        /// Pdf document
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public byte[] PdfDocument { get; set; }

        /// <summary>
        /// Image data
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public byte[] Image { get; set; }
    }

    /// <summary>
    /// Optional parameters for task AddImageAsNewPage
    /// </summary>
    public class AddImageOptions
    {
        /// <summary>
        /// Text to display above the image.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Caption { get; set; }
    }

    /// <summary>
    /// Optional parameters for task ConvertEmbeddedImagesToPages
    /// </summary>
    public class ConvertEmbeddedOptions
    {
        /// <summary>
        /// Text to display above image. Use [FILENAME] as a placeholder
        /// that will be replaced by the actual file name.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Caption { get; set; }

        /// <summary>
        /// Filter attachments. Multiple filters are supported separated by comma (ex.: *.jpg, *.png).
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Filter { get; set; }
    }

    /// <summary>
    /// Required parameters for task AddFooter
    /// </summary>
    public class AddFooterInput
    {
        /// <summary>
        /// Pdf document
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public byte[] PdfDocument { get; set; }

        /// <summary>
        /// Footer text
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// Required parameters for task ConvertHtmlToPdfInput
    /// </summary>
    public class ConvertHtmlToPdfInput
    {
        /// <summary>
        /// Html string
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Html { get; set; }

        /// <summary>
        /// Pdfdocument title
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Title { get; set; }

        /// <summary>
        /// Orientation of document.
        /// Available settings: 'Portrait', 'Landscape'
        /// </summary>
        /// 
        [DisplayFormat(DataFormatString = "Text")]
        public string Orientation { get; set; }

        /// <summary>
        /// Pdf pagesize (default A4)
        /// Available settings:
        /// A0,
        /// A1,
        /// A2,
        /// A3,
        /// A4,
        /// A5,
        /// A6,
        /// A7,
        /// A8,
        /// A9,
        /// B0,
        /// B1,
        /// B2,
        /// B3,
        /// B4,
        /// B5,
        /// B6,
        /// B7,
        /// B8,
        /// B9,
        /// B10,
        /// C5E,
        /// Comm10E,
        /// DLE,
        /// Executive,
        /// Folio,
        /// Ledger,
        /// Legal,
        /// Letter,
        /// Tabloid
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string PageSize { get; set; }

        /// <summary>
        /// Path to wkhtmltopdf executable.
        /// The default path is 'C:\Program Files\wkhtmltox\bin\wkhtmltopdf.exe'
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string ExecutablePath { get; set; }
    }
}
