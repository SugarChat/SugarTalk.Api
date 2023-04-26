using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.Messages.Dto.Users;

public class UserAccountDto
{
    public UserAccountDto()
    {
        Roles = new List<RoleDto>();
    }
    
    public int Id { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime ModifiedOn { get; set; }
    
    public Guid Uuid { get; set; }
    
    public string UserName { get; set; }
    
    public bool IsActive { get; set; }
    
    public string ThirdPartyUserId { get; set; }
    
    public UserAccountIssuer Issuer { get; set; }
    
    public List<RoleDto> Roles { get; set; }
}