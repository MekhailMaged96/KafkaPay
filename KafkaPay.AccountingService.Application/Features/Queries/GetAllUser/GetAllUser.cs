using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Application.DTOS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KafkaPay.AccountingService.Application.Features.Queries.GetAllUser
{
    public record class GetAllUserQuery() : IRequest<List<UserDto>>
    {
    };

    public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, List<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetAllUserQueryHandler> _logger;

        public GetAllUserQueryHandler(IApplicationDbContext context,ILogger<GetAllUserQueryHandler> logger )
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<UserDto>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reterive All User");

            return _context.Users
                .Select(a => new UserDto
                {
                    Id = a.Id,
                    Email = a.Email,
                    UserName = a.Username
                })
                .ToList();
        }
    }
}
