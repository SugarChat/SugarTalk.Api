using AutoMapper;
using SugarTalk.Messages.Commands.Tencent;
using TencentCloud.Trtc.V20190722.Models;

namespace SugarTalk.Core.Mapping;

public class TencentMapping: Profile
{
    public TencentMapping()
    {
        CreateMap<CreateCloudRecordingCommand, CreateCloudRecordingRequest>().ReverseMap();
        CreateMap<StopCloudRecordingCommand, DeleteCloudRecordingRequest>().ReverseMap();
        CreateMap<UpdateCloudRecordingCommand, ModifyCloudRecordingRequest>().ReverseMap();
        CreateMap<StartCloudRecordingResponse, CreateCloudRecordingResponse>().ReverseMap();
        CreateMap<StopCloudRecordingResponse, DeleteCloudRecordingResponse>().ReverseMap();
        CreateMap<UpdateCloudRecordingResponse, ModifyCloudRecordingResponse>().ReverseMap();
    }
}