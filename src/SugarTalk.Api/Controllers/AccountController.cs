using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SugarTalk.Messages.Requests.Account;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Constants;

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
    
    [Authorize]
    [Route("upload/photo"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadPhotoResponse))]
    public async Task<IActionResult> UploadPhotoAsync([FromForm] IFormFile file)
    {
        var ms = new MemoryStream();

        await file.CopyToAsync(ms).ConfigureAwait(false);

        var fileContent = ms.ToArray();

        var request = new UploadPhotoCommand
        {
            FileName = file.FileName, FileContent = fileContent
        };
        
        var response = await _mediator.SendAsync<UploadPhotoCommand, UploadPhotoResponse>(request);
        
        return Ok(response);
    }

    [Authorize]
    [Route("apikey/permission"), HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddApiKeyPermissionResponse))]
    public async Task<IActionResult> AddApiKeyPermissionAsync([FromBody] AddApiKeyPermissionCommand command)
    {
        if (!SecurityStore.Permissions.AllPermissions.Contains(command.PermissionName))
            return BadRequest("Unknown permission.");

        var response = await _mediator
            .SendAsync<AddApiKeyPermissionCommand, AddApiKeyPermissionResponse>(command)
            .ConfigureAwait(false);

        return StatusCode((int)response.Code, response);
    }
}
