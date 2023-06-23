using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    
    [Authorize]
    [Route("user"), HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCurrentUserResponse))]
    public async Task<IActionResult> GetCurrentUserAsync([FromQuery] GetCurrentUserRequest request)
    {
        var response = await _mediator.RequestAsync<GetCurrentUserRequest, GetCurrentUserResponse>(request);
        
        return Ok(response);
    }
}
