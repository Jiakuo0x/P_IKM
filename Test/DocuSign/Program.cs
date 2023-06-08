using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

var client = ProdClient.GetClient();

EnvelopesApi envelopesApi = new(client);
var formData = envelopesApi.GetFormData(ProdClient.AccountId, "f988b590-5872-4cdf-83d3-c81fd4fa0453");
var stampCompany = formData.FormData.Where(i => i.Name == "ApplicantCompany");
Console.ReadLine();