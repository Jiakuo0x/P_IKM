using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

var client = DevClient.GetClient();

EnvelopesApi envelopesApi = new(client);
var envelope = envelopesApi.GetEnvelope(DevClient.AccountId, "6deeccca-4ff0-4eb0-a9bb-d9cdb0795a7d");
Console.ReadLine();