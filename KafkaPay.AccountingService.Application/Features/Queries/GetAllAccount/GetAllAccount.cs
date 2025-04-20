using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.AccountingService.Application.DTOS;
using KafkaPay.Shared.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.AccountingService.Application.Features.Queries.GetAllAccount
{
    public record class GetAllAccountQuery() : IRequest<List<AccountDto>>
    {
    };

    public class GetAllAccountQueryHandler : IRequestHandler<GetAllAccountQuery, List<AccountDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllAccountQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<AccountDto>> Handle(GetAllAccountQuery request, CancellationToken cancellationToken)
        {
            return _context.Accounts.Include(e => e.User)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    Balance = a.Balance,
                    Currency = a.Currency,
                    UserId = a.UserId,
                    UserName = a.User.Username
                })
                .ToList();
        }
    }
    
}
