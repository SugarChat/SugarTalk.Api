using System;

namespace SugarTalk.Messages.Dtos.Users;

public class RoleDto
{
    public int Id { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime ModifiedOn { get; set; }
    
    public Guid Uuid { get; set; }
    
    public string Name { get; set; }
}