using KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer;
using KafkaPay.TransferService.Application.Features.Commands.FailTransfer;
using KafkaPay.TransferService.Application.Features.Commands.TransferMoney;
using KafkaPay.TransferService.Application.Features.Queries.GetTransactionHistory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KafkaPay.TransferService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransferController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateTransfer([FromBody] TransferMoneyCommand command)
        {
            var transactionId = await _mediator.Send(command);
            return Ok(new { TransactionId = transactionId });
        }

    
        [HttpPost("complete/{transactionId:guid}")]
        public async Task<IActionResult> CompleteTransfer(Guid transactionId)
        {
            var result = await _mediator.Send(new CompleteTransferCommand(transactionId));
            return result ? Ok("Transfer completed.") : BadRequest("Invalid or already completed transaction.");
        }

     
        [HttpPost("fail/{transactionId:guid}")]
        public async Task<IActionResult> FailTransfer(Guid transactionId)
        {
            var result = await _mediator.Send(new FailTransferCommand(transactionId));
            return result ? Ok("Transfer failed and refunded.") : BadRequest("Invalid or already processed transaction.");
        }


        [HttpGet("{accountId}/history")]
        public async Task<IActionResult> History(Guid accountId)
        {
            var result = await _mediator.Send(new GetTransactionHistoryQuery { AccountId = accountId });
            return Ok(result);
        }
    }
}
