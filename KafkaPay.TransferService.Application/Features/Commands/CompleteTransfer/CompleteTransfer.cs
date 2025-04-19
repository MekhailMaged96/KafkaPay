using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer
{
    public record CompleteTransferCommand(Guid TransactionId) : IRequest<bool>;

    public class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public CompleteTransferCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
        {
            var txn = await _context.TnxTransactions
                .Include(t => t.ToAccount)
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId);

            if (txn == null || txn.Status != TnxTransactionStatus.Pending)
                return false;

            txn.ToAccount.Balance += txn.Amount;
            txn.Status = TnxTransactionStatus.Completed;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
