using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using Lib.DocuSign;
using Microsoft.Extensions.Options;
using System.Numerics;

namespace Services;

/// <summary>
/// DocuSign service
/// </summary>
public class DocuSignService
{
    private readonly IOptions<Configuration> _options;
    private readonly ClientManager _clientManager;
    public DocuSignService(
        IOptions<Configuration> options,
        ClientManager clientManager)
    {
        _options = options;
        _clientManager = clientManager;
    }

    /// <summary>
    /// Match envelopes within the last 30 days in DocuSign.
    /// </summary>
    /// <returns>Envelopes</returns>
    public async Task<EnvelopesInformation> MatchEnvelopes()
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var envelopes = await envelopesApi.ListStatusChangesAsync(_options.Value.AccountId, new EnvelopesApi.ListStatusChangesOptions
        {
            fromDate = DateTime.Now.Date.AddDays(-30).ToShortDateString(),
            include = "custom_fields,documents,recipients",
        });
        return envelopes;
    }

    /// <summary>
    /// Get envelope information
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <returns>Envelope</returns>
    public async Task<Envelope> GetEnvelopeAsync(string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var envelope = await envelopesApi.GetEnvelopeAsync(_options.Value.AccountId, envelopeId, new EnvelopesApi.GetEnvelopeOptions
        {
            include = "custom_fields,documents,recipients,tabs",
        });
        return envelope;
    }

    /// <summary>
    /// Update the custom field "Latest Status" in DocuSign envelope
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <param name="comment">Comment</param>
    /// <returns>The task of the function</returns>
    public async Task UpdateComment(string envelopeId, string comment)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var envelope = await envelopesApi.GetEnvelopeAsync(_options.Value.AccountId, envelopeId, new EnvelopesApi.GetEnvelopeOptions
        {
            include = "custom_fields",
        });
        var customField = envelope.CustomFields.TextCustomFields.FirstOrDefault(i => i.Name == "Latest Status");
        if (customField is not null)
        {
            customField.Value = $"{comment} - [{DateTime.Now:g}]";
        }
        var result = await envelopesApi.UpdateCustomFieldsAsync(_options.Value.AccountId, envelopeId, envelope.CustomFields);
    }

    /// <summary>
    /// Get envelope form data
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <returns>The task of the function</returns>
    public async Task<EnvelopeFormData> GetEnvelopeFormDataAsync(string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetFormDataAsync(_options.Value.AccountId, envelopeId);
        return result;
    }

    /// <summary>
    /// Get the tabs in the document
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <param name="documentId">Document ID</param>
    /// <returns>Tabs</returns>
    public async Task<Tabs> GetDocumentTabs(string envelopeId, string documentId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetDocumentTabsAsync(_options.Value.AccountId, envelopeId, documentId);
        return result;
    }

    /// <summary>
    /// Download the document file in the envelope
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <param name="documentId">Document ID</param>
    /// <returns>File stream</returns>
    public async Task<Stream> DownloadDocument(string envelopeId, string documentId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetDocumentAsync(_options.Value.AccountId, envelopeId, documentId);
        return result;
    }

    /// <summary>
    /// Append documents to the envelope
    /// </summary>
    /// <param name="envelopeId">EnvelopeId</param>
    /// <param name="documents">Documents</param>
    /// <returns>The task of the function</returns>
    public async Task AppendDocuments(string envelopeId, List<Document> documents)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);

        await envelopesApi.UpdateDocumentsAsync(_options.Value.AccountId, envelopeId, new EnvelopeDefinition
        {
            Documents = documents,
        });
    }

    /// <summary>
    /// Remove the recipient who is configured as a listener in the envelope
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <returns>The task of the function</returns>
    public async Task RemoveListener(string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var recipients = await envelopesApi.ListRecipientsAsync(_options.Value.AccountId, envelopeId);
        var listener = recipients.Signers.SingleOrDefault(i => i.Email == _options.Value.ListenEmail && i.RoutingOrder == recipients.CurrentRoutingOrder);
        if (listener is null) return;

        var response = await envelopesApi.DeleteRecipientAsync(_options.Value.AccountId, envelopeId, listener.RecipientId);
    }

    /// <summary>
    /// Check whether the envelope currently needs to be processed
    /// </summary>
    /// <param name="envelope">Envelope of DocuSign</param>
    /// <returns>Is pending</returns>
    public bool EnvelopeIsPending(Envelope envelope)
    {
        var recipients = envelope.Recipients;
        if (recipients is null) return false;

        var listener = recipients.Signers.SingleOrDefault(i => i.Email == _options.Value.ListenEmail && i.RoutingOrder == recipients.CurrentRoutingOrder);
        if (listener is null) return false;

        return true;
    }

    /// <summary>
    /// Voided the envelope
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    /// <param name="reason">Reason</param>
    /// <returns>The task of the function</returns>
    public async Task VoidedEnvelope(string envelopeId, string reason)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);

        var response = await envelopesApi.UpdateAsync(_options.Value.AccountId, envelopeId, new Envelope
        {
            Status = "voided",
            VoidedReason = reason,
        });
    }
}
