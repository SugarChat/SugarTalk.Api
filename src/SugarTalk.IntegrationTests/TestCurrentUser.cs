using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.IntegrationTests;

public class TestCurrentUser
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public ThirdPartyFrom AuthType { get; } = ThirdPartyFrom.Google;

    public string DisplayName { get; set; } = "TestName";
}