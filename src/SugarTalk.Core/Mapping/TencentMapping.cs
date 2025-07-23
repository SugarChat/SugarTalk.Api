using AutoMapper;
using SugarTalk.Messages.Commands.Tencent;
using TencentCloud.Trtc.V20190722.Models;
using AudioParams = SugarTalk.Messages.Commands.Tencent.AudioParams;
using MixLayout = SugarTalk.Messages.Commands.Tencent.MixLayout;
using MixLayoutParams = SugarTalk.Messages.Commands.Tencent.MixLayoutParams;
using MixTranscodeParams = SugarTalk.Messages.Commands.Tencent.MixTranscodeParams;
using RecordParams = SugarTalk.Messages.Commands.Tencent.RecordParams;
using SubscribeStreamUserIds = SugarTalk.Messages.Commands.Tencent.SubscribeStreamUserIds;
using VideoParams = SugarTalk.Messages.Commands.Tencent.VideoParams;
using WaterMark = SugarTalk.Messages.Commands.Tencent.WaterMark;
using WaterMarkChar = SugarTalk.Messages.Commands.Tencent.WaterMarkChar;
using WaterMarkImage = SugarTalk.Messages.Commands.Tencent.WaterMarkImage;
using WaterMarkTimestamp = SugarTalk.Messages.Commands.Tencent.WaterMarkTimestamp;

namespace SugarTalk.Core.Mapping;

public class TencentMapping: Profile
{
    public TencentMapping()
    {
        CreateMap<CreateCloudRecordingCommand, CreateCloudRecordingRequest>()
            .ForMember(dest => dest.RecordParams, opt => opt.MapFrom(src => src.RecordParams))
            .ForMember(dest => dest.MixTranscodeParams, opt => opt.MapFrom(src => src.MixTranscodeParams))
            .ForMember(dest => dest.MixLayoutParams, opt => opt.MapFrom(src => src.MixLayoutParams))
            .ReverseMap();

        CreateMap<RecordParams, TencentCloud.Trtc.V20190722.Models.RecordParams>()
            .ForMember(dest => dest.SubscribeStreamUserIds, opt => opt.MapFrom(src => src.SubscribeStreamUserIds))
            .ReverseMap();

        CreateMap<SubscribeStreamUserIds, TencentCloud.Trtc.V20190722.Models.SubscribeStreamUserIds>().ReverseMap();

        CreateMap<MixTranscodeParams, TencentCloud.Trtc.V20190722.Models.MixTranscodeParams>()
            .ForMember(dest => dest.AudioParams, opt => opt.MapFrom(src => src.AudioParams))
            .ForMember(dest => dest.VideoParams, opt => opt.MapFrom(src => src.VideoParams))
            .ReverseMap();

        CreateMap<AudioParams, TencentCloud.Trtc.V20190722.Models.AudioParams>().ReverseMap();

        CreateMap<VideoParams, TencentCloud.Trtc.V20190722.Models.VideoParams>().ReverseMap();
        
        
        CreateMap<StopCloudRecordingCommand, DeleteCloudRecordingRequest>().ReverseMap();
        
        CreateMap<UpdateCloudRecordingCommand, ModifyCloudRecordingRequest>()
            .ForMember(dest => dest.MixLayoutParams, opt => opt.MapFrom(src => src.MixLayoutParams))
            .ForMember(dest => dest.SubscribeStreamUserIds, opt => opt.MapFrom(src => src.SubscribeStreamUserIds))
            .ReverseMap();

        CreateMap<MixLayoutParams, TencentCloud.Trtc.V20190722.Models.MixLayoutParams>()
            .ForMember(dest => dest.MixLayoutList, opt => opt.MapFrom(src => src.MixLayoutList))
            .ForMember(dest => dest.WaterMarkList, opt => opt.MapFrom(src => src.WaterMarkList))
            .ReverseMap();

        CreateMap<MixLayout, TencentCloud.Trtc.V20190722.Models.MixLayout>().ReverseMap();

        CreateMap<WaterMark, TencentCloud.Trtc.V20190722.Models.WaterMark>().ReverseMap();

        CreateMap<WaterMarkImage, TencentCloud.Trtc.V20190722.Models.WaterMarkImage>().ReverseMap();

        CreateMap<WaterMarkChar, TencentCloud.Trtc.V20190722.Models.WaterMarkChar>().ReverseMap();

        CreateMap<WaterMarkTimestamp, TencentCloud.Trtc.V20190722.Models.WaterMarkTimestamp>().ReverseMap();
        
        CreateMap<StartCloudRecordingResponse, CreateCloudRecordingResponse>().ReverseMap();
        
        CreateMap<StopCloudRecordingResponse, DeleteCloudRecordingResponse>().ReverseMap();
        
        CreateMap<UpdateCloudRecordingResponse, ModifyCloudRecordingResponse>().ReverseMap();
    }
}