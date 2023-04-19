using DocuSign.eSign.Client;

namespace Lib.DocuSign;

public class ClientManager
{
    private byte[]? _privateKey { get; set; }

    private readonly IOptions<Configuration> _options;

    public ClientManager(IOptions<Configuration> options)
    {
        _options = options;
    }

    public DocuSignClient GetClient()
    {
        var client = new DocuSignClient(_options.Value.ApiBase);

        if (_privateKey is null)
            _privateKey = File.ReadAllBytes(_options.Value.PrivateKeyPath);

        client.RequestJWTUserToken(
                _options.Value.ClientId,
                _options.Value.UserId,
                _options.Value.AuthServer,
                _privateKey,
                 1);

        return client;
    }
}