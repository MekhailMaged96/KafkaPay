using KafkaPay.AccountingService.Application.Features.Commands.CreateUser;
using KafkaPay.AccountingService.Application.Features.Queries.GetAllAccount;
using KafkaPay.AccountingService.Application.Features.Queries.GetAllUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KafkaPay.AccountingService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllUserQuery());
            return Ok(result);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
