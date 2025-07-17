using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Settings.TencentCloud;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Trtc.V20190722;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ITencentCloudClient : IScopedDependency
{
    Task<CreateCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken);

    Task<DeleteCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken);
}

public class TencentCloudClient : ITencentCloudClient
{
    private readonly TencentCloudSetting _tencentCloudSetting;

    public TencentCloudClient(TencentCloudSetting tencentCloudSetting)
    {
        _tencentCloudSetting = tencentCloudSetting;
    }
    
    public TrtcClient CreateClient()
    {
        var cred = new Credential
        {
            SecretId = _tencentCloudSetting.SecretId,
            SecretKey = _tencentCloudSetting.SecretKey
        };

        var clientProfile = new ClientProfile
        {
            HttpProfile = new HttpProfile
            {
                Endpoint = _tencentCloudSetting.BaseUrl 
            }
        };

        return new TrtcClient(cred, _tencentCloudSetting.Region, clientProfile);
    }

    public async Task<CreateCloudRecordingResponse> CreateCloudRecordingAsync(CreateCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        return await client.CreateCloudRecording(request).ConfigureAwait(false);
    }
    
    public async Task<DeleteCloudRecordingResponse> StopCloudRecordingAsync(DeleteCloudRecordingRequest request, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        
        return await client.DeleteCloudRecording(request).ConfigureAwait(false);
    }
}