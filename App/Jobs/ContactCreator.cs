﻿using Database.Enums;
using Database.Models;
using Lib.DocuSign;
using Services;

namespace Jobs;

public class ContactCreator : BackgroundService
{
    private readonly ILogger<ContactCreator> _logger;
    private readonly DocuSignService _docuSign;
    private readonly BestSignService _bestSign;
    private readonly TaskService _taskService;
    private readonly TemplateMappingService _templateMappingService;
    private readonly DocumentService _documentService;

    public ContactCreator(
        ILogger<ContactCreator> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;

        var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        _docuSign = serviceProvider.GetRequiredService<DocuSignService>();
        _bestSign = serviceProvider.GetRequiredService<BestSignService>();
        _taskService = serviceProvider.GetRequiredService<TaskService>();
        _templateMappingService = serviceProvider.GetRequiredService<TemplateMappingService>();
        _documentService = serviceProvider.GetRequiredService<DocumentService>();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                await DoWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContractCreator");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
    protected async Task DoWork()
    {
        var tasks = _taskService.GetTasksByStep(TaskStep.ContractCreating);
        foreach (var task in tasks)
        {
            try
            {
                CreateContractModel createContractModel = new CreateContractModel { Task = task };

                createContractModel.Envelope = await _docuSign.GetEnvelopeAsync(task.DocuSignEnvelopeId);
                createContractModel.EnvelopeFormData = await _docuSign.GetEnvelopeFormDataAsync(task.DocuSignEnvelopeId);
                createContractModel.TemplateMapping = MatchTemplateMapping(createContractModel);

                var contract = await CreateContract(createContractModel);

                _taskService.UpdateTaskContractId(task.Id, contract.ContractId);
                _taskService.ChangeStep(task.Id, TaskStep.ContractCreated);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }

    protected TemplateMapping MatchTemplateMapping(CreateContractModel createContractModel)
    {
        var envelopeType = createContractModel.Envelope
            .CustomFields.ListCustomFields.SingleOrDefault(i => i.Name == "eStamp Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'eStamp Type'.");

        var templateMapping = _templateMappingService.GetMappingByDocuSignId(envelopeType.Value);

        return templateMapping;
    }

    protected async Task<CreateContractSuccessModel> CreateContract(CreateContractModel createContractModel)
    {
        var sender = CreateContractSender(createContractModel);
        var roles = CreateContractRoles(createContractModel);
        var documents = await CreateContractDocuments(createContractModel);

        var apiResponse = await _bestSign.Post<CreateContractSuccessModel>($"/api/templates/send-contracts-sync-v2", new
        {
            sender = sender,
            templateId = createContractModel.TemplateMapping!.BestSignTemplateId,
            roles = roles,
            documents = documents,
        });

        return apiResponse;

    }

    # region CreateContractDocuments
    protected async Task<object> CreateContractDocuments(CreateContractModel createContractModel)
    {
        List<Object> result = new();

        Dictionary<string, object>? mainContract = null;
        List<Object> attachments = new();
        foreach (var document in createContractModel.Envelope.EnvelopeDocuments)
        {
            if (document.Name == "Summary") continue;

            var docFile = await _docuSign.DownloadDocument(createContractModel.Envelope.EnvelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);

            var appendingSignLables = await AppendingSignLables(createContractModel, document.DocumentId);

            var item = new Dictionary<string, object>();
            if (mainContract is null && appendingSignLables is not null)
            {
                item.Add("documentId", createContractModel.TemplateMapping!.BestSignConfiguration.DocumentId);
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("contractConfig", new
                {
                    contractTitle = $"{document.Name}.pdf",
                });
                item.Add("appendingSignLabels", appendingSignLables);
                item.Add("content", docContent);
                mainContract = item;
                result.Add(item);
            }
            else if (mainContract is not null && appendingSignLables is not null)
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("contractConfig", new
                {
                    contractTitle = $"{document.Name}.pdf",
                });
                item.Add("appendingSignLables", appendingSignLables);
                item.Add("content", docContent);
                result.Add(item);
            }
            else
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("content", docContent);
                attachments.Add(item);
            }
        }
        if (mainContract is null) throw new Exception("System Error: Not found the main contract.");
        mainContract.Add("attachments", attachments);
        return result;
    }

    protected async Task<List<Object>?> AppendingSignLables(CreateContractModel createContractModel, string documentId)
    {
        List<Object> result = new();

        var docTabs = await _docuSign.GetDocumentTabs(createContractModel.Envelope.EnvelopeId, documentId);
        var aStamp = docTabs.SignHereTabs?.SingleOrDefault(i => i.TabLabel == "A Stamp Here");
        if (aStamp != null)
        {
            result.Add(new
            {
                x = GetXPosition(aStamp.XPosition),
                y = GetYPosition(aStamp.YPosition),
                pageNumber = int.Parse(aStamp.PageNumber),
                roleName = "RoleA",
                type = "SEAL",
            });
        }

        var bStamp = docTabs.SignHereTabs?.SingleOrDefault(i => i.TabLabel == "B Stamp Here");
        if (bStamp != null)
        {
            result.Add(new
            {
                x = GetXPosition(bStamp.XPosition),
                y = GetYPosition(bStamp.YPosition),
                pageNumber = int.Parse(bStamp.PageNumber),
                roleName = "RoleB",
                type = "SEAL",
            });
        }
        if (aStamp == null && bStamp == null)
            return null;
        return result;
    }
    protected double GetXPosition(string xPosition)
    {
        var x = double.Parse("0." + xPosition);
        return x;
    }

    protected double GetYPosition(string yPosition)
    {
        var y = double.Parse("0." + yPosition);
        return y;
    }
    #endregion

    protected object CreateContractSender(CreateContractModel createContractModel)
    {
        var result = new
        {
            account = createContractModel.Envelope.Sender.Email,
            enterpriseName = createContractModel.TemplateMapping!.BestSignConfiguration.EnterpriseName,
            bizName = createContractModel.TemplateMapping!.BestSignConfiguration.BusinessLine,
        };

        return result;
    }

    protected object CreateContractRoles(CreateContractModel createContractModel)
    {
        List<Object> result = new();
        var formData = createContractModel.EnvelopeFormData.FormData;

        var stampKeeper = formData.FirstOrDefault(i => i.Name == "StampKeeper");
        var signingCompany = formData.FirstOrDefault(i => i.Name == "Signing Company");
        if (stampKeeper is null) throw new Exception("Stamp Keeper is required field.");
        if (signingCompany is null) throw new Exception("Signing Company is required field.");

        result.Add(new
        {
            userInfo = new
            {
                userAccount = stampKeeper.Value,
                enterpriseName = signingCompany.ListSelectedValue,
            },
            roleName = "RoleA",
            receiverType = "SIGNER",
            userType = "ENTERPRISE",
        });

        var supplierContracter = formData.FirstOrDefault(i => i.Name == "Supplier Contacter");
        var supplierContract = formData.FirstOrDefault(i => i.Name == "Supplier Contact");
        if (supplierContracter is not null)
        {
            result.Add(new
            {
                userInfo = new
                {
                    userAccount = supplierContract!.Value,
                    enterpriseName = supplierContracter.Value,
                },
                roleName = "RoleB",
                receiverType = "SIGNER",
                userType = "ENTERPRISE",
            });
        }

        return result;
    }
}

public class CreateContractModel
{
    public ElectronicSignatureTask Task { get; set; } = null!;
    public TemplateMapping? TemplateMapping { get; set; }
    public Envelope Envelope { get; set; } = null!;
    public EnvelopeFormData EnvelopeFormData { get; set; } = null!;
}

public class CreateContractSuccessModel
{
    public string ContractId { get; set; } = null!;
}