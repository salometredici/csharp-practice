using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transactions.Application.Transactions;
using Transactions.Domain.Transfers;

namespace Transactions.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? sourceAccountId)
        {
            var query = new TransactionsSearchQuery(from, to, sourceAccountId);
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferToAccountAsync([FromBody] TransferRequest request)
        {
            var command = new TransferCommand(request);
            return Ok(await _mediator.Send(command));
        }
    }
}
