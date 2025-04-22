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

namespace KafkaPay.TransferService.Application.Features.Queries.GetTransactionHistory
{
    public record GetTransactionHistoryQuery() : IRequest<List<TransactionDTO>>
    {
        public Guid AccountId { get; set; }
    };

    public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, List<TransactionDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetTransactionHistoryQueryHandler(IApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TransactionDTO>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _context.TnxTransactions
                                            .Include(e=>e.Status)
                                            .Where(t => t.FromAccountId == request.AccountId ||
                                            t.ToAccountId == request.AccountId)
                                            .OrderByDescending(t => t.Timestamp)
                                            .ToListAsync(cancellationToken);


            return  _mapper.Map<List<TransactionDTO>>(transactions);  
        }
    }
}
