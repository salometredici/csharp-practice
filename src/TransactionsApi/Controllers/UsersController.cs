using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transactions.Application.Users;
using Transactions.Domain.Users;

namespace Transactions.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var command = new RegisterCommand(request);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(RegisterCommand), result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var command = new LoginCommand(request);
            return Ok(await _mediator.Send(command));
        }
    }
}
