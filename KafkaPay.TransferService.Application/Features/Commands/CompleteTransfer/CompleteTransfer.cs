using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer
{
    public record CompleteTransferCommand(Guid TransactionId) : IRequest<bool>;

    public class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CompleteTransferCommandHandler> _logger;

        public CompleteTransferCommandHandler(IApplicationDbContext context, ILogger<CompleteTransferCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CompleteTransferCommand for TransactionId: {TransactionId}", request.TransactionId);

            var txn = await _context.TnxTransactions
                .Include(t => t.ToAccount)
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

            return true;
        }
    }
}
