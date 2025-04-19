using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.TransferService.Application.Features.Commands.FailTransfer
{
    public record FailTransferCommand(Guid TransactionId) : IRequest<bool>;

    public class FailTransferCommandHandler : IRequestHandler<FailTransferCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public FailTransferCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(FailTransferCommand request, CancellationToken cancellationToken)
        {
            var txn = await _context.TnxTransactions
                .Include(t => t.FromAccount)
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId);

            if (txn == null || txn.Status != TnxTransactionStatus.Pending)
                return false;

            // Refund the source account
            txn.FromAccount.Balance += txn.Amount;

            // Mark the transaction as failed
            txn.Status = TnxTransactionStatus.Failed;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
