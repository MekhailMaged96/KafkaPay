using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer
{
    public record CompleteTransferCommand(Guid TransactionId) : IRequest<bool>;

    public class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CompleteTransferCommandHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory; // Inject IHttpClientFactory

        // Constructor
        public CompleteTransferCommandHandler(IApplicationDbContext context, ILogger<CompleteTransferCommandHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory; // Assign the IHttpClientFactory
        }

        public async Task<bool> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CompleteTransferCommand for TransactionId: {TransactionId}", request.TransactionId);

            var txn = await _context.TnxTransactions
                .Include(t => t.ToAccount).ThenInclude(e => e.User)
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

            if (txn == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found", request.TransactionId);
                return false;
            }

            if (txn.StatusId != (int)TnxTransactionStatusEnum.Pending)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} is not in Pending status. Current StatusId: {StatusId}", request.TransactionId, txn.StatusId);
                return false;
            }

            var oldBalance = txn.ToAccount.Balance;
            txn.ToAccount.Balance += txn.Amount;
            txn.StatusId = (int)TnxTransactionStatusEnum.Completed;

            _logger.LogInformation("Transaction completed. Added {Amount} to account. Old balance: {OldBalance}, New balance: {NewBalance}",
                txn.Amount, oldBalance, txn.ToAccount.Balance);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Changes saved to database for TransactionId: {TransactionId}", request.TransactionId);


            await SendEmailAsync(txn.ToAccount?.User?.Username, cancellationToken);


            return true;
        }

        private async Task<bool> SendEmailAsync(string userName, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = "https://localhost:7175/api/user/SendEmail"; // Replace with your actual SendEmail API endpoint
            var requestData = new
            {
                name = userName
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(apiUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                  //  _logger.LogInformation("Successfully sent email to user {UserName}", userName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email to user {UserName}. StatusCode: {StatusCode}",
                        userName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to user {UserName}", userName);
                return false;
            }
        }
    }
}
