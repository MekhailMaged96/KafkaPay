using KafkaPay.AccountingService.Application.Features.Commands.CreatAccount;
using KafkaPay.AccountingService.Application.Features.Commands.CreateUser;
using KafkaPay.AccountingService.Application.Features.Queries.GetAccountById;
using KafkaPay.AccountingService.Application.Features.Queries.GetAllAccount;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KafkaPay.AccountingService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

   

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("account/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetAccountByIdQuery { AccountId = id });
            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllAccountQuery());
            return Ok(result);
        }
    }
}
