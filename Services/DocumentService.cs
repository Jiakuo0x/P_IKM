using DocuSign.eSign.Model;

namespace Services;

/// <summary>
/// Document service
/// </summary>
public class DocumentService
{
    /// <summary>
    /// Remove the signature from the PDF file and return the new PDF file stream.
    /// </summary>
    /// <param name="document">The originial PDF file stream</param>
    /// <returns>The PDF file stream without the signature</returns>
    /// <exception cref="Exception">The uploaded document is not a PDF file</exception>
    public byte[] DecryptDocument(Stream document)
    {
        Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
        pdf.LoadFromStream(document);
        Spire.Pdf.Widget.PdfFormWidget widgets = (pdf.Form as Spire.Pdf.Widget.PdfFormWidget)
            ?? throw new Exception("System Error: DocuSing document is not a PDF file.");

        for (int i = 0; i < widgets!.FieldsWidget.List.Count; i++)
        {
            Spire.Pdf.Widget.PdfFieldWidget widget = (widgets.FieldsWidget[i] as Spire.Pdf.Widget.PdfFieldWidget)
                ?? throw new Exception("System Error: DocuSing document is not a PDF file.");

            if (widget is Spire.Pdf.Widget.PdfSignatureFieldWidget)
            {
                widgets.FieldsWidget.RemoveAt(i);
            }
        }
        var stream = new MemoryStream();
        pdf.SaveToStream(stream);

        var result = stream.ToArray();
        stream.Dispose();

        return result;
    }
}
