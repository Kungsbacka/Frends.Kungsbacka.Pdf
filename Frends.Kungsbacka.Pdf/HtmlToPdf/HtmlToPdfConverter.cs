using System;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace Frends.Kungsbacka.Pdf.HtmlToPdf
{
    public class HtmlToPdfConverter
    {
        private readonly IConverter _converter;

        public HtmlToPdfConverter()
        {
            _converter = new BasicConverter(new WkHtmlToPdfDotNet.PdfTools());
        }

        public byte[] Convert(
            ConvertHtmlToPdfInput input,
            WkHtmlMarginOptions marginOptions,
            WkHtmlFooterOptions footerOptions)
        {
            var orientation = Orientation.Portrait;
            var paperSize = PaperKind.A4;

            if (!string.IsNullOrEmpty(input.Orientation))
            {
                orientation = (Orientation)Enum.Parse(typeof(Orientation), input.Orientation, true);
            }

            if (!string.IsNullOrEmpty(input.PageSize))
            {
                paperSize = (PaperKind)Enum.Parse(typeof(PaperKind), input.PageSize, true);
            }

            return
                _converter.Convert(new HtmlToPdfDocument()
                {
                    GlobalSettings = new GlobalSettings()
                    {
                        Margins = new MarginSettings()
                        {
                            Bottom = marginOptions.MarginBottom,
                            Top = marginOptions.MarginTop,
                            Left = marginOptions.MarginLeft,
                            Right = marginOptions.MarginRight,
                            Unit = Unit.Millimeters
                        },
                        Orientation = orientation,
                        PaperSize = paperSize
                    },
                    Objects =
                    {
                        new ObjectSettings()
                        {
                            FooterSettings = new FooterSettings()
                            {
                                Spacing = footerOptions.FooterSpacing,
                                HtmlUrl = footerOptions.FooterHtmlPath,
                                Line = footerOptions.IncludeFooterLine
                            },
                            WebSettings = new WebSettings()
                            {
                                EnableIntelligentShrinking = !input.DisableSmartShrinking
                            },
                            HtmlContent = input.Html
                        }
                    }
                });
        }
    }
}
