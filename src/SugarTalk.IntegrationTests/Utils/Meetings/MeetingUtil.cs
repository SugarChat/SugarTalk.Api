using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using System.Net;
using NSubstitute;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using LiveKit_CSharp.Services.Meeting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Settings.Caching;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Requests.Meetings;
using Xunit;

namespace SugarTalk.IntegrationTests.Utils.Meetings;

public class MeetingUtil : TestUtil
{
    public MeetingUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<ScheduleMeetingResponse> ScheduleMeeting(
        string title = null, string timezone = null, string securityCode = null,
        DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, DateTimeOffset? utilDate = null,
        MeetingAppointmentType appointmentType = MeetingAppointmentType.Quick,
        MeetingRepeatType repeatType = MeetingRepeatType.None, bool isMuted = false, bool isRecorded = false)
    {
        return await Run<IMediator, ScheduleMeetingResponse>(async (mediator) =>
        {
            var response = await mediator.SendAsync<ScheduleMeetingCommand, ScheduleMeetingResponse>(
                new ScheduleMeetingCommand
                {
                    Title = title,
                    TimeZone = timezone,
                    SecurityCode = securityCode,
                    StartDate = startDate ?? DateTimeOffset.Now.AddHours(-1),
                    EndDate = endDate ?? DateTimeOffset.Now.AddDays(2),
                    UtilDate = utilDate,
                    AppointmentType = appointmentType,
                    RepeatType = repeatType,
                    IsMuted = isMuted,
                    IsRecorded = isRecorded
                });

            return response;
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();
            liveKitServerUtilService.CreateMeetingAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new CreateMeetingFromLiveKitResponseDto { RoomInfo = new LiveKitRoom() });

            liveKitServerUtilService.GenerateTokenForCreateMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    public async Task<MeetingDto> JoinMeeting(string meetingNumber, bool isMuted = false)
    {
        return await Run<IMediator, MeetingDto>(async (mediator) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = meetingNumber,
                IsMuted = isMuted
            });

            return response.Data.Meeting;
        }, builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();
            
            liveKitServerUtilService.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitServerUtilService);
        });
    }

    public async Task AddMeeting(Meeting meeting)
    {
        await RunWithUnitOfWork<IRepository>(async (repository) =>
        {
            await repository.InsertAsync(meeting, CancellationToken.None).ConfigureAwait(false);
        });
    }
    
    public async Task<Meeting> GetMeeting(string meetingNumber)
    {
        return await Run<IRepository, Meeting>(async (repository) =>
        {
            return await repository.QueryNoTracking<Meeting>()
                .Where(x => x.MeetingNumber == meetingNumber)
                .SingleAsync(CancellationToken.None);
        });
    }

    public async Task AddMeetingUserSession(int id, Guid meetingId, int userId, bool isMuted = false, bool isSharingScreen = false)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new MeetingUserSession
            {
                Id = id,
                UserId = userId,
                IsMuted = isMuted,
                MeetingId = meetingId,
                IsSharingScreen = isSharingScreen
            }, CancellationToken.None);
        });
    }
    
    public async Task AddMeetingUserSetting(Guid id, int userId, Guid meetingId,
        SpeechTargetLanguageType targetLanguageType, CantoneseToneType? cantoneseToneType,
        DateTimeOffset? lastModifiedDate = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new MeetingUserSetting
            {
                Id = id,
                UserId = userId,
                MeetingId = meetingId,
                TargetLanguageType = targetLanguageType,
                CantoneseToneType = cantoneseToneType ?? CantoneseToneType.HiuGaaiNeural,
                LastModifiedDate = lastModifiedDate ?? DateTimeOffset.Now
            }, CancellationToken.None);
        });
    }

    public async Task EndMeeting(string meetingNumber)
    {
        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<EndMeetingCommand, EndMeetingResponse>(new EndMeetingCommand
            {
                MeetingNumber = meetingNumber
            });
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
        });
    }

    public async Task<KickOutMeetingByUserIdResponse> KickOutUserByUserIdAsync(Guid meetingId, int kickOutUserId,
        int MasterUserId, string meetingNumber)
    {
        return await Run<IMediator, KickOutMeetingByUserIdResponse>(async mediator =>
        {
            return await mediator.SendAsync<KickOutMeetingByUserIdCommand, KickOutMeetingByUserIdResponse>(
                new KickOutMeetingByUserIdCommand
                {
                    KickOutUserId = kickOutUserId,
                    MeetingId = meetingId
                });
        }, builder =>
        {
            var services = Substitute.For<ILiveKitServerUtilService>();
            var openAiService = Substitute.For<IOpenAiService>();

            services.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("11231312312312312313223");
            builder.RegisterInstance(services);
            builder.RegisterInstance(openAiService);
        });
    }

    public async Task<VerifyMeetingUserPermissionResponse> VerifyMeetingUserPermissionAsync(
        VerifyMeetingUserPermissionCommand verifyMeetingUserPermissionCommand)
    {
        return await Run<IMediator, VerifyMeetingUserPermissionResponse>(async mediator =>
        {
            return await
                mediator.SendAsync<VerifyMeetingUserPermissionCommand, VerifyMeetingUserPermissionResponse>(
                    verifyMeetingUserPermissionCommand);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
        });
    }

    public async Task<GetMeetingByNumberResponse> GetMeetingAsync(string meetingNumber)
    {
        return await Run<IMediator, GetMeetingByNumberResponse>(async mediator =>
        {
            return await mediator.RequestAsync<GetMeetingByNumberRequest, GetMeetingByNumberResponse>(
                new GetMeetingByNumberRequest
                {
                    MeetingNumber = meetingNumber
                });
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
        });
    }

    public async Task<MeetingUserSession> GetUserSessionAsync(int userId, Guid meetingId)
    {
        return await Run<IRepository, MeetingUserSession>(async repo =>
        {
            return await repo.FirstOrDefaultAsync<MeetingUserSession>(x =>
                x.UserId == userId && x.MeetingId == meetingId);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
        });
    }
    
    public async Task<MeetingDto> JoinMeetingByUserAsync(UserAccount user, string meetingNumber, bool isMuted = false)
    {
        return await Run<IMediator, MeetingDto>(async (mediator) =>
        {
            var response = await mediator.SendAsync<JoinMeetingCommand, JoinMeetingResponse>(new JoinMeetingCommand
            {
                MeetingNumber = meetingNumber,
                IsMuted = isMuted
            });
            return response.Data.Meeting;
        }, async builder =>
        {
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var accountDataProvider = Substitute.For<IAccountDataProvider>();
            var openAiService = Substitute.For<IOpenAiService>();
            
            accountDataProvider.GetUserAccountAsync(Arg.Any<int>()).Returns(new UserAccountDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Uuid = user.Uuid,
                IsActive = user.IsActive,
                Issuer = user.Issuer,
                ThirdPartyUserId = user.ThirdPartyUserId,
                CreatedOn = user.CreatedOn,
                ModifiedOn = user.ModifiedOn,
            });
            builder.RegisterInstance(liveKitServerUtilService);
            builder.RegisterInstance(accountDataProvider);
            builder.RegisterInstance(openAiService);
        });
    }

    public async Task AddMeetingRecord(MeetingRecord record)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(record).ConfigureAwait(false);
        });
    }

    public async Task<MeetingRecord> GenerateMeetingRecordAsync(MeetingDto meetingDto, string url = null)
    {
        return new MeetingRecord()
        {
            MeetingId = meetingDto.Id,
            CreatedDate = DateTimeOffset.Now,
            RecordType = MeetingRecordType.OnRecord,
            Url = url ?? "mock url"
        };
    }

    public async Task AddMeetingRecordAsync(MeetingRecord meetingRecord)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingRecord, CancellationToken.None);
        });
    }

    public async Task<List<MeetingRecord>> GetMeetingRecordsByMeetingIdAsync(Guid meetingId)
    {
        return await Run<IRepository, List<MeetingRecord>>(async repository =>
        {
            return await repository.QueryNoTracking<MeetingRecord>().Where(x => x.MeetingId == meetingId).ToListAsync();
        });
    }

    public async Task<MeetingRecord> GetMeetingRecordByMeetingRecordIdAsync(Guid meetingRecordId)
    {
        return await Run<IRepository, MeetingRecord>(async repository =>
        {
            return await repository.QueryNoTracking<MeetingRecord>()
                .FirstOrDefaultAsync(x => x.Id == meetingRecordId);
        });
    }

    public async Task<MeetingRecord> GetMeetingRecordByMeetingIdAsync(Guid meetingId)
    {
        return await Run<IRepository, MeetingRecord>(async repository =>
        {
            return await repository.QueryNoTracking<MeetingRecord>(x => x.MeetingId == meetingId)
                .OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        });
    }

    public async Task<StorageMeetingRecordVideoResponse> StorageMeetingRecordVideoByMeetingIdAsync(Guid meetingId, Guid meetingRecordId, string egressId = null)
    {
        return await Run<IMediator, StorageMeetingRecordVideoResponse>(async mediator =>
        {
            return await mediator.SendAsync<StorageMeetingRecordVideoCommand, StorageMeetingRecordVideoResponse>(
                new StorageMeetingRecordVideoCommand
                {
                    MeetingId = meetingId,
                    MeetingRecordId = meetingRecordId,
                    EgressId = egressId ?? "mock egressId"
                });
        }, builder =>
        {
            var meetingService = Substitute.For<IMeetingService>();
            var liveKitservices = Substitute.For<ILiveKitServerUtilService>();
            var liveKitClient = Substitute.For<ILiveKitClient>();
            
            meetingService
                .StorageMeetingRecordVideoAsync(Arg.Any<StorageMeetingRecordVideoCommand>(),
                    Arg.Any<CancellationToken>()).Returns(new StorageMeetingRecordVideoResponse
                {
                    Code = HttpStatusCode.OK,
                    Msg = "Ok"
                });
            
            liveKitservices.GenerateTokenForJoinMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("11231312312312312313223");
            
            liveKitClient.StopEgressAsync(Arg.Any<StopEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StopEgressResponseDto()
                {
                    EgressId = "mock egressId",
                    EndedAt = "mock endedat",
                    File = new FileDetails { Location = "mock url" },
                    Status = "mock status",
                    Error = "mock error",
                });

            liveKitClient.GetEgressInfoListAsync(Arg.Any<GetEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new GetEgressInfoListResponseDto()
                {
                    EgressItems = new List<EgressItemDto>
                    {
                        new EgressItemDto
                        {
                            EgressId = "mock egressId",
                            EndedAt = "mock endedAt",
                            Status = "EGRESS_COMPLETE",
                            File = new FileDetails { Location = "mock url" }
                        }
                    }
                });
            
            builder.RegisterInstance(liveKitservices);
            builder.RegisterInstance(liveKitClient);
            builder.RegisterInstance(meetingService);
        });
    }
}