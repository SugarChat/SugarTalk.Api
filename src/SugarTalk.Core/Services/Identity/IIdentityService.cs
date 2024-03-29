﻿using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.Identity;

public interface IIdentityService : IScopedDependency
{
    Task<bool> IsInRolesAsync(int userId, string[] rolesArray, CancellationToken cancellationToken);
    
    Task<UserAccountDto> GetCurrentUserAsync(bool throwWhenNotFound = false, CancellationToken cancellationToken = default);
}