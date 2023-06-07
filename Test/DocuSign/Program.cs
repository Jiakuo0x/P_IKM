using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

var client = ProdClient.GetClient();

EnvelopesApi envelopesApi = new(client);
var formData = envelopesApi.GetFormData(ProdClient.AccountId, "a5da6346-0dff-40e0-8467-6c17e2337653");
var stampCompany = formData.FormData.Where(i => i.Name == "ApplicantCompany");
Console.ReadLine();