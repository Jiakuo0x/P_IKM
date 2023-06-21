using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

PdfReader reader = new PdfReader("Reference-IWAY Standard 6.0 - 通用章节-CN.pdf.pdf");

PdfStamper stamper = new PdfStamper(reader, new FileStream("output.pdf", FileMode.Create));

// 获取签名字段名称列表
AcroFields fields = stamper.AcroFields;
List<string> signatureFields = new List<string>(fields.GetSignatureNames());

// 遍历签名字段列表
foreach (string fieldName in signatureFields)
{
    // 删除签名字段
    fields.ClearSignatureField(fieldName);
}

stamper.Close();
reader.Close();