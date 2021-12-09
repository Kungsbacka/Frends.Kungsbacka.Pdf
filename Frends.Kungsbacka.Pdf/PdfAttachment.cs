namespace Frends.Kungsbacka.Pdf
{
    /// <summary>
    /// Represents an attached document in a Pdf file.
    /// It can be either an embedded file (EF) or an associated file (AF)
    /// </summary>
    public class PdfAttachment
    {
        /// <summary>
        /// Name of the attached file (ex.: image.jpg)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// File extension (ex.: .jpg)
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// File contents
        /// </summary>
        public byte[] Data { get; set; }
    }
}
