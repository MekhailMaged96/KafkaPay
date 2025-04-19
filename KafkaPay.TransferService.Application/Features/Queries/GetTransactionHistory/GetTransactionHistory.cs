using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.TransferService.Application.Features.Queries.GetTransactionHistory
{
    public record GetTransactionHistoryQuery() : IRequest<List<TnxTransaction>>
    {
        public Guid AccountId { get; set; }
    };

    public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, List<TnxTransaction>>
    {
        private readonly IApplicationDbContext _context;

        public GetTransactionHistoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TnxTransaction>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _context.TnxTransactions
                .Where(t => t.FromAccountId == request.AccountId || t.ToAccountId == request.AccountId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }
    }
}
