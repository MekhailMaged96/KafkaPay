using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.AccountingService.Application.Features.Queries.GetAccountById
{
    public record GetAccountByIdQuery () : IRequest<Account> {
    
        public Guid AccountId { get; set; }
    };
   
    public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Account>
    {
        private readonly IApplicationDbContext _context;

        public GetAccountByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Account> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Accounts
                .Include(x => x.TnxTransactions)
                .FirstOrDefaultAsync(x => x.Id == request.AccountId, cancellationToken);
        }
    }


}
