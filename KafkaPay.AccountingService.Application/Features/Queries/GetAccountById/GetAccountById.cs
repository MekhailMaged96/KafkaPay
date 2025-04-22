using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Application.DTOS;
using KafkaPay.Shared.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.AccountingService.Application.Features.Queries.GetAccountById
{
    public record GetAccountByIdQuery () : IRequest<AccountDto> {
    
        public Guid AccountId { get; set; }
    };
   
    public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAccountByIdQueryHandler(IApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account == null)
                return new AccountDto();

            var transactions = await _context.TnxTransactions.Include(e=>e.Status)
                .Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id)
                .OrderByDescending(e=>e.Created)
                .ToListAsync(cancellationToken);

            var accountDto = _mapper.Map<AccountDto>(account);

            accountDto.Transactions = _mapper.Map<List<TransactionDTO>>(transactions);

            return accountDto;
        }
    }


}
