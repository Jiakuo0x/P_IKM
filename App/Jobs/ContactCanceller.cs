using DocuSign.eSign.Model;
using Data.Models;
using Data.Enums;

namespace App.Jobs
{
    public class ContactCanceller : BackgroundService
    {
        private readonly ILogger<DocuSignReader> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<Lib.DocuSign.Configuration> _docuSignOptions;
        public ContactCanceller(
            ILogger<DocuSignReader> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<Lib.DocuSign.Configuration> docuSignOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _docuSignOptions = docuSignOptions;
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
                    _logger.LogError(ex, "Error in ContactCanceller");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
            }
        }
        protected async Task DoWork()
        {
            var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
            var bestSignApiClient = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<Lib.BestSign.ApiClient>();

            var tasks = dbContext.Set<ElectronicSignatureTask>()
                .Where(i => i.CurrentStep == TaskStep.ContractCancelling).ToList();

            foreach (var task in tasks)
            {
                var apiResponse = await bestSignApiClient.Post<Lib.BestSign.ApiResponse>($"/api/contracts/{task.BestSignContractId}/revoke", new
                {
                    revokeReason = "The system has cancelled the contract because the relevant envelope of DocuSign has been cancelled",
                });
                if (apiResponse.Code == "0")
                {
                    task.CurrentStep =TaskStep.ContractCancelled;
                }
            }
            dbContext.SaveChanges();
        }
    }
}
