using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Frends.Kungsbacka.Pdf
{
	internal class MemoryPdfSplitter : PdfSplitter
	{
		private readonly List<MemoryStream> _streams = new List<MemoryStream>();

		internal MemoryPdfSplitter(PdfDocument pdfDoc) : base(pdfDoc) { }

		protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
		{
			var ms = new MemoryStream();
			_streams.Add(ms);
			return new PdfWriter(ms);
		}

		internal List<byte[]> SplitToByteArrays(int pageCount)
		{
			SplitByPageCount(pageCount, new CloseOnReady());
			return _streams.Select(ms => ms.ToArray()).ToList();
		}

		private sealed class CloseOnReady : IDocumentReadyListener
		{
			public void DocumentReady(PdfDocument doc, PageRange range) => doc.Close();
		}
	}
}
