namespace Lib.Azure;

/// <summary>
/// The configuration of Azure Key Vault is specified in appsettings.json
/// </summary>
public class KeyVaultConfiguration
{
    /// <summary>
    /// Azure Key Vault url for DocuSign
    /// </summary>
    public string DocuSignKeyVaultUrl { get; set; } = string.Empty;

    /// <summary>
    /// Azure Key Vault secret name for DocuSign
    /// </summary>
    public string DocuSignSecretName { get;set ; } = string.Empty;

    /// <summary>
    /// Azure Key Vault url for Bestsign
    /// </summary>
    public string BestSignKeyVaultUrl { get; set; } = string.Empty;

    /// <summary>
    /// Azure Key Vault secret name for Bestsign
    /// </summary>
    public string BestSignSecretName { get; set; } = string.Empty;
}
