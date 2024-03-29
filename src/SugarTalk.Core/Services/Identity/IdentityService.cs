﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.Identity;

public class IdentityService : IIdentityService
{
    private readonly IRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IAccountDataProvider _accountDataProvider;

    public IdentityService(IRepository repository, ICurrentUser currentUser, IAccountDataProvider accountDataProvider)
    {
        _repository = repository;
        _currentUser = currentUser;
        _accountDataProvider = accountDataProvider;
    }
    
    public async Task<bool> IsInRolesAsync(int userId, string[] rolesArray, CancellationToken cancellationToken)
    {
        var user = 
            await _accountDataProvider.GetUserAccountAsync(userId, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        var roles = user.Roles.Select(x => x.Name).ToList();

        foreach (var role in rolesArray)
        {
            if (!roles.Contains(role))
            {
                return false;
            }
        }
        
        return true;
    }

    public async Task<UserAccountDto> GetCurrentUserAsync(bool throwWhenNotFound = false, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;

        if (userId != null)
            return await _accountDataProvider.GetUserAccountAsync(userId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (throwWhenNotFound)
            throw new UnauthorizedAccessException();

        return null;
    }

    public async Task AllocateUserToRoleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        var user =
            await _accountDataProvider.GetUserAccountAsync(userId, includeRoles:true, 
                cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (user == null) return;
        
        var userRole = 
            await _repository.SingleOrDefaultAsync<RoleUser>(x => 
                x.UserId == user.Id && x.RoleId == roleId, cancellationToken).ConfigureAwait(false);

        if (userRole != null) return;
        {
            await _repository.InsertAsync(new RoleUser
            {
                RoleId = roleId,
                UserId = user.Id,
                Uuid = Guid.NewGuid()
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}