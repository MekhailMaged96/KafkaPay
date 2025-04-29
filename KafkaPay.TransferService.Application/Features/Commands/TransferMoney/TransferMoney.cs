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
using Microsoft.Extensions.Logging;

namespace KafkaPay.TransferService.Application.Features.Commands.TransferMoney
{
    public record TransferMoneyCommand(Guid FromAccountId, Guid ToAccountId, decimal Amount) : IRequest<Guid> { };

    public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TransferMoneyCommandHandler> _logger;

        public TransferMoneyCommandHandler(IApplicationDbContext context,IMapper mapper,ILogger<TransferMoneyCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
           _logger = logger;
        }

        public async Task<Guid> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting transfer from account {FromAccountId} to account {ToAccountId} for amount {Amount}.", request.FromAccountId, request.ToAccountId, request.Amount);

            var from = await _context.Accounts.FindAsync(request.FromAccountId);
            var to = await _context.Accounts.FindAsync(request.ToAccountId);

            if (from == null || to == null)
            {
                _logger.LogError("Invalid account(s) provided: FromAccountId = {FromAccountId}, ToAccountId = {ToAccountId}.", request.FromAccountId, request.ToAccountId);
                throw new InvalidOperationException("Invalid account(s).");

            }

            if (from.Balance < request.Amount)
            {
                _logger.LogError("Insufficient balance in account {FromAccountId}. Balance: {Balance}, Requested Transfer: {Amount}.", request.FromAccountId, from.Balance, request.Amount);
                throw new InvalidOperationException("Insufficient balance.");
            }

            // Deduct from the sender's account
            from.Balance -= request.Amount;

            _logger.LogInformation("Transferred {Amount} from account {FromAccountId} to account {ToAccountId}. New Balance: {Balance}.", request.Amount, request.FromAccountId, request.ToAccountId, from.Balance);

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


            txn.AddDomainEvent(new TransactionInitiEvent()
            {
                TransactionId = txn.Id,
                FromAccountId = txn.FromAccountId,
                ToAccountId = txn.ToAccountId,
                Amount = txn.Amount,
                Timestamp = txn.Timestamp
            });


            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Transaction event for TransactionId {TransactionId} has been added.", txn.Id);

            return txn.Id;
        }
    }

}
