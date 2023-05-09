using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("login"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var response = await _mediator.RequestAsync<LoginRequest, LoginResponse>(request);

        return Ok(response);
    }
}
