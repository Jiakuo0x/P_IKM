namespace Database.Enums;

/// <summary>
/// The data type of DocuSign
/// </summary>
public enum DocuSignDataType
{
    /// <summary>
    /// Unknown for exception
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The data from the value of form data
    /// </summary>
    FormData_Value = 10,

    /// <summary>
    /// The data from the list selected value of form data
    /// </summary>
    FormData_ListSelectedValue = 11,

    /// <summary>
    /// The data from the custom field that type of text custom field
    /// </summary>
    TextCustomField = 20,

    /// <summary>
    /// The data from the custom field that type of list custom field
    /// </summary>
    ListCustomField = 21,

    /// <summary>
    /// The data from the email of the applicant, who is the first recipient signer
    /// </summary>
    ApplicantEmail = 30,

    /// <summary>
    /// The data from the email of the Docusign envelope sender
    /// </summary>
    SenderEmail = 40,

    /// <summary>
    /// The data from the checkbox group
    /// </summary>
    CheckboxGroup = 50,
}