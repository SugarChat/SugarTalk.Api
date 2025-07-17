

using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Commands.Tencent;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Services.Tencent;

public interface ITencentService : IScopedDependency
{
    Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<StopCloudRecordingResponse> StopCloudRecordingAsync(StopCloudRecordingCommand command, CancellationToken cancellationToken);
    
    Task<UpdateCloudRecordingResponse> UpdateCloudRecordingAsync(UpdateCloudRecordingCommand command, CancellationToken cancellationToken);
}

public class TencentService : ITencentService
{
    private readonly IMapper _mapper;
    private readonly TencentClient _tencentClient;
    
    public TencentService(IMapper mapper, TencentClient tencentClient)
    {
        _mapper = mapper;
        _tencentClient = tencentClient;
    }

    public async Task<StartCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        return await _tencentClient.CreateCloudRecordingAsync(_mapper.Map<CreateCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
    }

    public async Task<StopCloudRecordingResponse> StopCloudRecordingAsync(StopCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        return await _tencentClient.StopCloudRecordingAsync(_mapper.Map<DeleteCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
    }

    public async Task<UpdateCloudRecordingResponse> UpdateCloudRecordingAsync(UpdateCloudRecordingCommand command, CancellationToken cancellationToken)
    {
        return await _tencentClient.ModifyCloudRecordingAsync(_mapper.Map<ModifyCloudRecordingRequest>(command), cancellationToken).ConfigureAwait(false);
    }
}