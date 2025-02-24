using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Messages.Dto.Smarties;
using Smarties.Messages.Requests.Ask;
using SugarTalk.Core.Settings.Smarties;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISmartiesClient : IScopedDependency
{
    Task<AskGptResponse> PerformQueryAsync(AskGptRequestDto request, CancellationToken cancellationToken);
    
    Task<GetEchoAvatarUserToneResponse> GetEchoAvatarVoiceSettingAsync(GetEchoAvatarVoiceSettingRequestDto request, CancellationToken cancellationToken);

    Task<GetStaffDepartmentHierarchyTreeResponse> GetStaffDepartmentHierarchyTreeAsync(GetStaffDepartmentHierarchyTreeRequest request, CancellationToken cancellationToken);

    Task<GetStaffsResponse> GetStaffsRequestAsync(GetStaffsRequestDto request, CancellationToken cancellationToken);
    
    Task<CreateSpeechMaticsJobResponseDto> CreateSpeechMaticsJobAsync(CreateSpeechMaticsJobCommandDto command, CancellationToken cancellationToken);
}

public class SmartiesClient : ISmartiesClient
{
    private readonly SmartiesSettings _smartiesSettings;
    private readonly Dictionary<string, string> _headers;
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public SmartiesClient(ISugarTalkHttpClientFactory httpClientFactory, SmartiesSettings smartiesSettings)
    {
        _smartiesSettings = smartiesSettings;
        _httpClientFactory = httpClientFactory;
        
        _headers = new Dictionary<string, string>
        {
            { "X-API-KEY", _smartiesSettings.ApiKey }
        };
    }

    public async Task<GetEchoAvatarUserToneResponse> GetEchoAvatarVoiceSettingAsync(GetEchoAvatarVoiceSettingRequestDto request, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.GetAsync<GetEchoAvatarUserToneResponse>(
            $"{_smartiesSettings.BaseUrl}/api/EchoAvatar/voice/setting?UserName={request.UserName}&VoiceUuid={request.VoiceUuid}&LanguageType={request.LanguageType}", cancellationToken, headers: _headers).ConfigureAwait(false);
    }
    
    public async Task<AskGptResponse> PerformQueryAsync(AskGptRequestDto request, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.PostAsJsonAsync<AskGptResponse>(
            $"{_smartiesSettings.BaseUrl}/api/Ask/general/query", request, cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<GetStaffDepartmentHierarchyTreeResponse> GetStaffDepartmentHierarchyTreeAsync(GetStaffDepartmentHierarchyTreeRequest request, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.GetAsync<GetStaffDepartmentHierarchyTreeResponse>(
            $"{_smartiesSettings.BaseUrl}/api/Foundation/department/staff/hierarchy/tree?StaffIdSource={request.StaffIdSource}&HierarchyDepth={request.HierarchyDepth}&HierarchyStaffRange={request.HierarchyStaffRange}", cancellationToken, headers: _headers).ConfigureAwait(false);
    }

    public async Task<GetStaffsResponse> GetStaffsRequestAsync(GetStaffsRequestDto request, CancellationToken cancellationToken)
    {
        var userIds = "";

        if (request.UserIds is { Count: > 0 })
        {
            foreach (var userId in request.UserIds)
            {
                userIds += $"&UserIds={userId}";    
            }
        }
        
        return await _httpClientFactory.GetAsync<GetStaffsResponse>(
            $"{_smartiesSettings.BaseUrl}/api/Foundation/staffs?IsActive={request.IsActive}{userIds}", cancellationToken, headers: _headers).ConfigureAwait(false);
	}
    
    public async Task<CreateSpeechMaticsJobResponseDto> CreateSpeechMaticsJobAsync(CreateSpeechMaticsJobCommandDto command, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            { "Url", command.Url },
            { "Key", command.Key },
            { "Language", command.Language },
            { "SourceSystem", command.SourceSystem.ToString() },
            { "RecordName", command.RecordName}
        };
        
        var files = new Dictionary<string, (byte[], string)> { { "file", (command.RecordContent, command.RecordName) } };
        
        return await _httpClientFactory.PostAsMultipartAsync<CreateSpeechMaticsJobResponseDto>(
            $"{_smartiesSettings.BaseUrl}/api/SpeechMatics/create/job", parameters, files, cancellationToken, headers: _headers).ConfigureAwait(false);
    }
}