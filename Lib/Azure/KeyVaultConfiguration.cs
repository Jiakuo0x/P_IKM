using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Azure;

public class KeyVaultConfiguration
{
    public string DocuSignKeyVaultUrl { get; set; } = string.Empty;
    public string DocuSignSecretName { get;set ; } = string.Empty;
    public string BestSignKeyVaultUrl { get; set; } = string.Empty;
    public string BestSignSecretName { get; set; } = string.Empty;
}
