using DocuSign.eSign.Client;

namespace Lib.DocuSign;

public class ClientManager
{
    #region Static Variable
    private DocuSignClient? _client { get; set; }
    private DateTime _clientExpiresAt { get; set; } = DateTime.MinValue;
    private byte[]? _privateKey { get; set; }
    #endregion

    private readonly IOptions<Configuration> _options;

    public ClientManager(IOptions<Configuration> options)
    {
        _options = options;
    }

    public DocuSignClient GetClient()
    {
        if (_client is null)
            _client = new DocuSignClient(_options.Value.ApiBase);

        if (DateTime.Now > _clientExpiresAt)
        {
            if (_privateKey is null)
                _privateKey = File.ReadAllBytes(_options.Value.PrivateKeyPath);

            _client.RequestJWTUserToken(
                _options.Value.ClientId,
                _options.Value.UserId,
                _options.Value.AuthServer,
                _privateKey,
                 1);
            _clientExpiresAt = DateTime.Now.AddMinutes(30);
        }

        return _client;
    }
}