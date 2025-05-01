using KafkaPay.AccountingService.Application.DTOS;
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
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator,ILogger<UserController> logger)
        {
            _mediator = mediator;
           _logger = logger;
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

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(UserDTO user)
        {

            await Task.Delay(100); 

            _logger.LogInformation($"Email successfully sent to user {user.name}");

            var response = new
            {
                Success = true,
                Message = $"Email sent successfully to user { user.name }",
                Data = new { Name = user.name }
            };

            return Ok(response);
        }
    }
}
