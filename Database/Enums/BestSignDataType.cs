namespace Database.Enums;

/// <summary>
/// The data type of Bestsign
/// </summary>
public enum BestSignDataType
{
    /// <summary>
    /// Unknown for exception
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The description field in the contract document
    /// </summary>
    DescriptionFields = 10,

    /// <summary>
    /// The company name of Party A in Bestsign
    /// </summary>
    RoleACompanyName = 20,

    /// <summary>
    /// The account of Party A in Bestsign
    /// </summary>
    RoleAAccount = 21,

    /// <summary>
    /// The company name of Party B in Bestsign
    /// </summary>
    RoleBCompanyName = 30,

    /// <summary>
    /// The account of Party B in Bestsign
    /// </summary>
    RoleBAccount = 31,

    /// <summary>
    /// The account of contract sender
    /// </summary>
    SenderAccount = 40,

    /// <summary>
    /// The enterprise name of contract sender
    /// </summary>
    SenderEnterpriseName = 41,

    /// <summary>
    /// The business line of contract sender
    /// </summary>
    SenderBusinessLine = 42,

    /// <summary>
    /// The signing location of Party A
    /// </summary>
    AStampHere = 50,

    /// <summary>
    /// The signing location of Party B
    /// </summary>
    BStampHere = 60,

    /// <summary>
    /// A mark indicating whether to use an electronic signature
    /// </summary>
    Tab_eStampRequire = 200,
}
