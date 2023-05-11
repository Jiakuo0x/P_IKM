using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;

List<FileInfo> fileInfos = new List<FileInfo>();
fileInfos.Add(new FileInfo("Sample1.pdf"));
fileInfos.Add(new FileInfo("Sample2.pdf"));
fileInfos.Add(new FileInfo("Sample3.pdf"));
foreach (var fileInfo in fileInfos)
{
    Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
    var file = File.ReadAllBytes(fileInfo.FullName);
    pdf.LoadFromBytes(file);
    Spire.Pdf.Widget.PdfFormWidget widgets = pdf.Form as Spire.Pdf.Widget.PdfFormWidget;
    for(int i = 0; i < widgets.FieldsWidget.List.Count; i++)
    {
        Spire.Pdf.Widget.PdfFieldWidget widget = widgets.FieldsWidget[i] as Spire.Pdf.Widget.PdfFieldWidget;
        if (widget is Spire.Pdf.Widget.PdfSignatureFieldWidget)
        {
           widgets.FieldsWidget.RemoveAt(i);
        }
    }
    pdf.SaveToFile($"{fileInfo.Name}-no-signature.pdf");
}
Console.ReadKey();
