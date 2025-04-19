using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.TransferService.Application.Features.Commands.TransferMoney
{
    public record TransferMoneyCommand(Guid FromAccountId, Guid ToAccountId, decimal Amount) : IRequest<Guid> { };

    public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public TransferMoneyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
        {
            var from = await _context.Accounts.FindAsync(request.FromAccountId);
            var to = await _context.Accounts.FindAsync(request.ToAccountId);

            if (from == null || to == null)
                throw new InvalidOperationException("Invalid account(s).");

            if (from.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance.");

            // Deduct from the sender's account
            from.Balance -= request.Amount;

            // Create a pending transaction - Do NOT credit 'to' account yet
            var txn = new TnxTransaction
            {
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                Status = TnxTransactionStatus.Pending,
                Timestamp = DateTime.UtcNow
            };

            _context.TnxTransactions.Add(txn);
            await _context.SaveChangesAsync(cancellationToken);

            return txn.Id;
        }
    }

}
