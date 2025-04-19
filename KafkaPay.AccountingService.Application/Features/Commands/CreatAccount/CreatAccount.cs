using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Entities;
using MediatR;

namespace KafkaPay.AccountingService.Application.Features.Commands.CreatAccount
{
    public record CreateAccountCommand(Guid UserId, decimal InitialBalance, string Currency) : IRequest<Guid>;

    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateAccountCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                UserId = request.UserId,
                Balance = request.InitialBalance,
                Currency = request.Currency
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            return account.Id;
        }
    }
}
