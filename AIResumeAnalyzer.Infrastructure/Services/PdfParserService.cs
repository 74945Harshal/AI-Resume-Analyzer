using AIResumeAnalyzer.Application.Common.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

public class PdfParserService : IPdfParserService
{
    public async Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        // Read stream into memory first so iText7 can seek
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return await Task.Run(() =>
        {
            var sb = new StringBuilder();

            using var reader = new PdfReader(memoryStream);
            using var pdfDoc = new PdfDocument(reader);

            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var strategy = new LocationTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                sb.AppendLine(text);
            }

            return sb.ToString().Trim();
        }, cancellationToken);
    }
}
