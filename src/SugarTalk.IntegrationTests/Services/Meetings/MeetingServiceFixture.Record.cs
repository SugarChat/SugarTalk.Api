using Xunit;
using System;
using System.Collections.Generic;
using Autofac;
using Shouldly;
using NSubstitute;
using System.Linq;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using SugarTalk.Messages.Dto;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smarties.Messages.DTO.Account;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Enums.Meeting.Speak;
using SugarTalk.Messages.Enums.Meeting.Summary;
using UserAccountDto = SugarTalk.Messages.Dto.Users.UserAccountDto;

namespace SugarTalk.IntegrationTests.Services.Meetings;

public partial class MeetingServiceFixture
{
    [Fact]
    public async Task CanGetMeetingRecord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-1),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url2"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord4 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, otherUser.Id);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);
        await _meetingUtil.AddMeetingRecord(meetingRecord4);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting1.Id);
            meetingRecordDto.MeetingNumber.ShouldBe(meeting1.MeetingNumber);
            meetingRecordDto.MeetingCreator.ShouldBe(testCurrentUser.UserName);
        });
    }

    [Fact]
    public async Task CanGetMeetingRecordByKeyWord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000111000",
            Title = "111會議",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000222000",
            Title = "222會議",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "000333000",
            Title = "333會議",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-1),
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url2"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord4 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, 1);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);
        await _meetingUtil.AddMeetingRecord(meetingRecord4);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "user1",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(1);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting2.Id);
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "0111",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting1.Id);
        });

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                Keyword = "222會議",
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(1);
            var meetingRecordDto = response.Data.Records[0];
            meetingRecordDto.MeetingId.ShouldBe(meeting2.Id);
        });
    }

    [Fact]
    public async Task ShouldNoRecord()
    {
        var testCurrentUser = new TestCurrentUser();
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id??-1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url3"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            RecordNumber = Guid.NewGuid().ToString(),
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, otherUser.Id);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task ShouldMeetingCreatorIsDifferent()
    {
        var otherUser = await _accountUtil.AddUserAccount("user1", "123456").ConfigureAwait(false);
        var testCurrentUser = new TestCurrentUser();
        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "1",
            MeetingMasterUserId = testCurrentUser.Id ?? -1
        };

        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "2",
            MeetingMasterUserId = otherUser.Id
        };

        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "3",
            MeetingMasterUserId = otherUser.Id
        };

        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1.Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2.Id,
            Url = "mock url3"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting3.Id,
            Url = "mock url4"
        };

        await _meetingUtil.AddMeeting(meeting1).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting2).ConfigureAwait(false);
        await _meetingUtil.AddMeeting(meeting3).ConfigureAwait(false);

        await _meetingUtil.AddMeetingUserSession(1, meeting1.Id, 1);
        await _meetingUtil.AddMeetingUserSession(2, meeting2.Id, 1);
        await _meetingUtil.AddMeetingUserSession(3, meeting3.Id, otherUser.Id);

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);

        await RunWithUnitOfWork<IMediator, IRepository>(async (mediator, repository) =>
        {
            var getCurrentUserMeetingRecordRequest = new GetCurrentUserMeetingRecordRequest
            {
                PageSetting = new PageSetting
                {
                    PageSize = 100,
                    Page = 1
                }
            };
            var response = await mediator.RequestAsync<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>(getCurrentUserMeetingRecordRequest).ConfigureAwait(false);
            response.Data.Count.ShouldBe(2);
            var creatorList = response.Data.Records.Select(x => x.MeetingCreator).ToList();
            creatorList.ShouldContain(otherUser.UserName);
            creatorList.ShouldContain(testCurrentUser.UserName);
        });
    }

    [Fact]
    public async Task CanGetEgressIdWhenStartMeetingRecording()
    {
        const string egressId = "123456";
        
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        
        var meeting = await _meetingUtil.ScheduleMeeting();
        
        var meetingRecord1 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting1Id,
            CreatedDate = DateTimeOffset.Now.AddDays(-2),
            Url = "mock url1"
        };

        var meetingRecord2 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting2Id,
            Url = "mock url3"
        };

        var meetingRecord3 = new MeetingRecord
        {
            Id = Guid.NewGuid(),
            MeetingId = meeting.Data.Id,
            Url = "mock url4"
        };

        await _meetingUtil.AddMeetingRecord(meetingRecord1);
        await _meetingUtil.AddMeetingRecord(meetingRecord2);
        await _meetingUtil.AddMeetingRecord(meetingRecord3);
        
        await Run<IMediator, IRepository, IClock>(async (mediator, repository, clock) =>
        {
            var response = await mediator.SendAsync<StartMeetingRecordingCommand, StartMeetingRecordingResponse>(
                new StartMeetingRecordingCommand
                {
                    MeetingId = meeting.Data.Id
                });
            
            response.EgressId.ShouldBe(egressId);
            
            var meetingRecords = await repository.Query<MeetingRecord>(x => x.MeetingId == meeting.Data.Id).ToListAsync();
            meetingRecords.Count(x => x.RecordNumber == $"ZNZX-{clock.Now.Year}{clock.Now.Month}{clock.Now.Day}{2.ToString().PadLeft(6, '0')}").ShouldBe(1);
        }, builder =>
        {
            var liveKitClient = Substitute.For<ILiveKitClient>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitClient.StartTrackCompositeEgressAsync(Arg.Any<StartTrackCompositeEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StartEgressResponseDto { EgressId = egressId });

            liveKitServerUtilService.GenerateTokenForRecordMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(liveKitClient);
            builder.RegisterInstance(liveKitServerUtilService);

            MockClock(builder, DateTimeOffset.Now);
        });
    }
    
    [Theory]
    [InlineData("e581c3fb-8ed0-4b8f-82fc-2453b84d403e", "08db98a9-6a57-48dc-86e5-31bc1c3a8e92", "AI Meeting", "1234567890", "https:/ai meetings/1234567890", "AI Meeting 會議機要", "AI Meeting 會議sugarTalk", "AI Meeting 會議history")]
    [InlineData("e19489c5-25d8-42e0-a30a-b5bb814cd7de", "08db98b4-3a60-40ac-8d81-c162c97916be", "Solar Meeting", "0123456789", "https:/solar meetings/0123456789", "Solar Meeting 會議機要", "Solar Meeting 會議sugarTalk", "Solar Meeting 會議history")]
    [InlineData("7de02c6d-198d-4c8f-81c0-b3ebd5041b9a", "08db994a-adac-4591-8181-3cc86099f525", "BI Meeting", "9012345678", "https:/bi meetings/9012345678", "BI Meeting 會議機要", "BI Meeting 會議sugarTalk", "BI Meeting 會議history")]
    public async Task CanGetMeetingRecordDetailsData(string recordId, string meetingId, string meetingTitile,
        string meetingNumber, string meetingUrl, string meetingSummary, string meetingContent1, string meetingContent2)
    {
        var request = new GetMeetingRecordDetailsRequest()
        {
            Id = Guid.Parse(recordId)
        };
        
        var meetingInfo = new Meeting
        {
            Id = Guid.Parse(meetingId),
            Title = meetingTitile,
            MeetingNumber = meetingNumber,
            StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EndDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AppointmentType = MeetingAppointmentType.Appointment
        };
        
        var meetingRecord = new MeetingRecord
        {
            Id = Guid.Parse(recordId),
            MeetingId = Guid.Parse(meetingId),
            Url = meetingUrl,
            CreatedDate = DateTimeOffset.Now,
        };
        
        var meetingRecordSummary = new MeetingSummary()
        {
            Id = 1,
            RecordId = Guid.Parse(recordId),
            MeetingNumber = meetingNumber,
            SpeakIds = "1",
            OriginText = "测试",
            Status = SummaryStatus.Pending,
            TargetLanguage = TranslationLanguage.ZhCn,
            CreatedDate = DateTimeOffset.Now,
            Summary = meetingSummary
        };
        
        var meetingRecordDetails = new List<MeetingSpeakDetail>
        {
            new MeetingSpeakDetail
            {
                Id = 1,
                MeetingRecordId = Guid.Parse(recordId),
                UserId = 1,
                SpeakStartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                SpeakContent = meetingContent1,
                SpeakStatus = SpeakStatus.Speaking,
                CreatedDate = DateTimeOffset.Now,
                TrackId = "1",
                EgressId = "1",
                MeetingNumber = meetingNumber,
                FilePath = "http://localhost:5000/api/v1/meeting/record/download/1"
            },
            new MeetingSpeakDetail
            {
                Id = 2,
                MeetingRecordId = Guid.Parse(recordId),
                UserId = 2,
                SpeakStartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                SpeakContent = meetingContent2,
                SpeakStatus = SpeakStatus.Speaking,
                CreatedDate = DateTimeOffset.Now,
                TrackId = "2",
                EgressId = "2",
                MeetingNumber = meetingNumber,
                FilePath = "http://localhost:5000/api/v1/meeting/record/download/2"
            }
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingInfo);
            await repository.InsertAsync(meetingRecord);
            await repository.InsertAsync(meetingRecordSummary);
            await repository.InsertAllAsync(meetingRecordDetails);
        });

        await Run<IMediator>(async mediator =>
        {
            var result = await mediator.RequestAsync<GetMeetingRecordDetailsRequest, GetMeetingRecordDetailsResponse>(request).ConfigureAwait(false);

            result.Data.ShouldNotBeNull();
            result.Data.MeetingTitle.ShouldBe(meetingTitile);
            result.Data.MeetingNumber.ShouldBe(meetingNumber);
            result.Data.MeetingStartDate.ShouldBe(meetingInfo.StartDate);
            result.Data.MeetingEndDate.ShouldBe(meetingInfo.EndDate);
            result.Data.Url.ShouldBe(meetingUrl);
            result.Data.Summary.ShouldBe(meetingSummary);
            
            result.Data.MeetingRecordDetail.ShouldNotBeNull();
            
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1).ShouldNotBeNull();
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1)?.SpeakContent.ShouldBe(meetingContent1);
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 1)?.SpeakStartTime
                .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
            
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2).ShouldNotBeNull();
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2)?.SpeakContent.ShouldBe(meetingContent2);
            result.Data.MeetingRecordDetail.FirstOrDefault(x => x.UserId == 2)?.SpeakStartTime
                .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
        });
    }
}