using DocuSign.eSign.Api;
using DocuSign.eSign.Model;

namespace Lib.DocuSign;

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

    public async Task AddComment(string comment, string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        List<CommentPublish> publishers = new List<CommentPublish>();
        publishers.Add(new CommentPublish(Text:comment));
        CommentsPublish commentsPublish = new CommentsPublish(publishers);
        await envelopesApi.CreateEnvelopeCommentsAsync(_options.Value.AccountId, envelopeId, commentsPublish);
    }

    public async Task AddComments(string[] comments, string envelopeId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        List<CommentPublish> publishers = new List<CommentPublish>();
        foreach(var comment in comments)
        {
            publishers.Add(new CommentPublish(Text: comment));
        }
        CommentsPublish commentsPublish = new CommentsPublish(publishers);
        await envelopesApi.CreateEnvelopeCommentsAsync(_options.Value.AccountId, envelopeId, commentsPublish);
    }

    public async Task<Stream> DownloadDocument(string envelopeId, string documentId)
    {
        var client = _clientManager.GetClient();
        EnvelopesApi envelopesApi = new(client);
        var result = await envelopesApi.GetDocumentAsync(_options.Value.AccountId, envelopeId, documentId);
        return result;
    }
}
