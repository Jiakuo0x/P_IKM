﻿using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using Lib.DocuSign;
using Microsoft.Extensions.Options;

namespace Services;

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

    public async Task AddComment(string envelopeId, string comment)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);

        List<CommentPublish> publishers = new List<CommentPublish>();
        publishers.Add(new CommentPublish(Text: comment, Id: Guid.NewGuid().ToString(), ThreadId: Guid.NewGuid().ToString()));

        CommentsPublish commentsPublish = new CommentsPublish(publishers);
        await envelopesApi.CreateEnvelopeCommentsAsync(_options.Value.AccountId, envelopeId, commentsPublish);
    }

    public async Task<EnvelopeFormData> GetEnvelopeFormDataAsync(string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetFormDataAsync(_options.Value.AccountId, envelopeId);
        return result;
    }

    public async Task<Tabs> GetDocumentTabs(string envelopeId, string documentId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetDocumentTabsAsync(_options.Value.AccountId, envelopeId, documentId);
        return result;
    }
    public async Task<Stream> DownloadDocument(string envelopeId, string documentId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetDocumentAsync(_options.Value.AccountId, envelopeId, documentId);
        return result;
    }

    public async Task AppendDocuments(string envelopeId, List<Document> documents)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);

        await envelopesApi.UpdateDocumentsAsync(_options.Value.AccountId, envelopeId, new EnvelopeDefinition
        {
            Documents = documents,
        });
    }

    public async Task RemoveListener(string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var recipients = await envelopesApi.ListRecipientsAsync(_options.Value.AccountId, envelopeId);
        var listener = recipients.Signers.SingleOrDefault(i => i.Email == _options.Value.ListenEmail && i.RoutingOrder == recipients.CurrentRoutingOrder);
        if (listener is null) return;

        await envelopesApi.DeleteRecipientAsync(_options.Value.AccountId, envelopeId, listener.RecipientId);
    }
}
