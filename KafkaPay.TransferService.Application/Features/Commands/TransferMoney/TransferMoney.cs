using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Domain.Enums;
using KafkaPay.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.TransferService.Application.Features.Commands.TransferMoney
{
    public record TransferMoneyCommand(Guid FromAccountId, Guid ToAccountId, decimal Amount) : IRequest<Guid> { };

    public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TransferMoneyCommandHandler(IApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                StatusId = (int)TnxTransactionStatusEnum.Pending,
                Timestamp = DateTime.UtcNow,
                TransactionCode = "T"
            };

            _context.TnxTransactions.Add(txn);

            await _context.SaveChangesAsync(cancellationToken);


            var initiationEvent = new TransactionInitiatedEvent(
                              txn.Id,
                              txn.FromAccountId,
                              txn.ToAccountId,
                              txn.Amount,
                              txn.Timestamp);

            var outboxMessage = new OutBoxMessage(
                typeof(TransactionInitiatedEvent).FullName!,
                initiationEvent,
                DateTime.UtcNow);

            _context.OutBoxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return txn.Id;
        }
    }

}
