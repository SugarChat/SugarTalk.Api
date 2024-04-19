using Xunit;
using System;
using Autofac;
using Shouldly;
using NSubstitute;
using System.Linq;
using System.Net;
using System.Text;
using Mediator.Net;
using System.Threading;
using SugarTalk.Core.Data;
using SugarTalk.Messages.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Services.LiveKit;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Dto.Translation;
using SugarTalk.Messages.Commands.Meetings;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Dto.LiveKit.Egress;
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
    
            builder.RegisterInstance(openAiService);
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
            var openAiService = Substitute.For<IOpenAiService>();
            var liveKitClient = Substitute.For<ILiveKitClient>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();

            liveKitClient.StartRoomCompositeEgressAsync(Arg.Any<StartRoomCompositeEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StartEgressResponseDto { EgressId = egressId });

            liveKitServerUtilService.GenerateTokenForRecordMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("token123");
            
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitClient);
            builder.RegisterInstance(liveKitServerUtilService);

            MockClock(builder, DateTimeOffset.Now);
        });
    }
    
    [Theory]
    [InlineData("e581c3fb-8ed0-4b8f-82fc-2453b84d403e", "08db98a9-6a57-48dc-86e5-31bc1c3a8e92", "AI Meeting", "1234567890", "https:/ai meetings/1234567890", "AI Meeting 會議機要", "AI Meeting 會議sugarTalk", "AI Meeting 會議history", false)]
    [InlineData("e581c3fb-8ed0-4b8f-82fc-2453b84d403e", "08db98a9-6a57-48dc-86e5-31bc1c3a8e92", "AI Meeting", "1234567890", "https:/ai meetings/1234567890", "AI Meeting 會議機要", "AI Meeting 會議sugarTalk", "AI Meeting 會議history", true)]
    [InlineData("e19489c5-25d8-42e0-a30a-b5bb814cd7de", "08db98b4-3a60-40ac-8d81-c162c97916be", "Solar Meeting", "0123456789", "https:/solar meetings/0123456789", "Solar Meeting 會議機要", "Solar Meeting 會議sugarTalk", "Solar Meeting 會議history", false)]
    [InlineData("e19489c5-25d8-42e0-a30a-b5bb814cd7de", "08db98b4-3a60-40ac-8d81-c162c97916be", "Solar Meeting", "0123456789", "https:/solar meetings/0123456789", "Solar Meeting 會議機要", "Solar Meeting 會議sugarTalk", "Solar Meeting 會議history", true)]
    [InlineData("7de02c6d-198d-4c8f-81c0-b3ebd5041b9a", "08db994a-adac-4591-8181-3cc86099f525", "BI Meeting", "9012345678", "https:/bi meetings/9012345678", "BI Meeting 會議機要", "BI Meeting 會議sugarTalk", "BI Meeting 會議history", false)]
    [InlineData("7de02c6d-198d-4c8f-81c0-b3ebd5041b9a", "08db994a-adac-4591-8181-3cc86099f525", "BI Meeting", "9012345678", "https:/bi meetings/9012345678", "BI Meeting 會議機要", "BI Meeting 會議sugarTalk", "BI Meeting 會議history", true)]
    public async Task CanGetMeetingRecordDetailsData(string recordId, string meetingId, string meetingTitle,
        string meetingNumber, string meetingUrl, string meetingSummary, string meetingContent1, string meetingContent2, bool isInsertMeetingRecordSummary)
    {
        var request = new GetMeetingRecordDetailsRequest()
        {
            Id = Guid.Parse(recordId)
        };
        
        var meetingInfo = new Meeting
        {
            Id = Guid.Parse(meetingId),
            Title = meetingTitle,
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
                OriginalContent = meetingContent1,
                SpeakStatus = SpeakStatus.Speaking,
                CreatedDate = DateTimeOffset.Now,
                TrackId = "1",
                MeetingNumber = meetingNumber,
                FileTranscriptionStatus = FileTranscriptionStatus.Pending
            },
            new MeetingSpeakDetail
            {
                Id = 2,
                MeetingRecordId = Guid.Parse(recordId),
                UserId = 2,
                SpeakStartTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                OriginalContent = meetingContent2, 
                SmartContent = "I am a test smart content.",
                SpeakEndTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                SpeakStatus = SpeakStatus.Speaking,
                CreatedDate = DateTimeOffset.Now,
                TrackId = "2",
                MeetingNumber = meetingNumber,
                FileTranscriptionStatus = FileTranscriptionStatus.Completed
            }
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingInfo);
            await repository.InsertAsync(meetingRecord);
            if (isInsertMeetingRecordSummary)
            {
                await repository.InsertAsync(meetingRecordSummary);
            }
            await repository.InsertAllAsync(meetingRecordDetails);
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
        });

        await Run<IMediator>(async mediator =>
        {
            var result = await mediator.RequestAsync<GetMeetingRecordDetailsRequest, GetMeetingRecordDetailsResponse>(request).ConfigureAwait(false);

            result.Data.ShouldNotBeNull();
            result.Data.MeetingTitle.ShouldBe(meetingTitle);
            result.Data.MeetingNumber.ShouldBe(meetingNumber);
            result.Data.MeetingStartDate.ShouldBe(meetingInfo.StartDate);
            result.Data.MeetingEndDate.ShouldBe(meetingInfo.EndDate);
            result.Data.Url.ShouldBe(meetingUrl);
            if (isInsertMeetingRecordSummary)
            {
                result.Data.Summary.Summary.ShouldBe(meetingSummary);
            }
            else
            {
                result.Data.Summary.ShouldBeNull();
            }

            result.Data.MeetingRecordDetails.ShouldNotBeNull();
            
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 1).ShouldNotBeNull();
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 1)?.OriginalContent.ShouldBe(meetingContent1);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 1).MeetingNumber.ShouldBe(meetingNumber);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 1).SpeakStatus.ShouldBe(SpeakStatus.Speaking);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 1).TrackId.ShouldBe("1");
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 1).SpeakEndTime.ShouldBeNull();
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 1)?.FileTranscriptionStatus.ShouldBe(FileTranscriptionStatus.Pending);
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 1)?.SpeakStartTime
                .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
            
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 2).ShouldNotBeNull();
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 2)?.OriginalContent.ShouldBe(meetingContent2);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).SmartContent.ShouldBe("I am a test smart content.");
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).MeetingNumber.ShouldBe(meetingNumber);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).SpeakStatus.ShouldBe(SpeakStatus.Speaking);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).TrackId.ShouldBe("2");
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).FileTranscriptionStatus.ShouldBe(FileTranscriptionStatus.Completed);
            result.Data.MeetingRecordDetails.FirstOrDefault(x=>x.UserId == 2).SpeakEndTime
                .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
            result.Data.MeetingRecordDetails.FirstOrDefault(x => x.UserId == 2)?.SpeakStartTime
                .ShouldBe(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
        }, builder =>
        {
            var openAiService = Substitute.For<IOpenAiService>();
            
            builder.RegisterInstance(openAiService);
        });
    }
     
    [Theory]
    [InlineData("mock url", "mock url1", "mock url2" )]
    public async Task CanGetNewMeetingRecordByMeetingRecordId(string url, string url2, string url3)
    {
        
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);

        var testRecord = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, url);
        var testRecord2 = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, url2);
        var testRecord3 = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, url3);
        
        await _meetingUtil.AddMeetingRecordAsync(testRecord);
        await _meetingUtil.AddMeetingRecordAsync(testRecord2);
        await _meetingUtil.AddMeetingRecordAsync(testRecord3);
        
        var meetingRecords = await _meetingUtil.GetMeetingRecordsByMeetingIdAsync(meetingDto.Id);
        var test = meetingRecords.FirstOrDefault(x => x.Url == url3);
        
        var meetingRecord = await _meetingUtil.GetMeetingRecordByMeetingRecordIdAsync(testRecord3.Id);
        
        meetingRecord.CreatedDate.ShouldBe(test.CreatedDate);
        meetingRecord.RecordType.ShouldBe(test.RecordType);
        meetingRecord.Url.ShouldBe(test.Url);
        meetingRecord.MeetingId.ShouldBe(meetingDto.Id);
    }

    [Theory]
    [InlineData("mock url1")]
    [InlineData("mock url2")]
    public async Task CanMeetingRecordShouldBeValue(string url)
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto, url);
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        
        var response = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        response.ShouldNotBeNull();
        response.Url.ShouldBe(url);
        response.Id.ShouldBe(meetingRecord.Id);
    }

    [Fact]
    public async Task CanMeetingRecordResponseShouldBeValue()
    {
        var scheduleMeetingResponse = await _meetingUtil.ScheduleMeeting();
        var meetingDto = await _meetingUtil.JoinMeeting(scheduleMeetingResponse.Data.MeetingNumber);
        var meetingRecord = await _meetingUtil.GenerateMeetingRecordAsync(meetingDto);
        
        await _meetingUtil.AddMeetingRecordAsync(meetingRecord);
        var dbMeetingRecord = await _meetingUtil.GetMeetingRecordByMeetingIdAsync(meetingDto.Id);
        dbMeetingRecord.RecordType.ShouldBe(MeetingRecordType.OnRecord);

        var response = await _meetingUtil.StorageMeetingRecordVideoByMeetingIdAsync(meetingDto.Id, meetingRecord.Id);
        response.Code.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CanDelayStorageRecordVideo()
    {
        var meetingRecordId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        const string audioContent = "0123123测试测试";
        var fileContent = Encoding.UTF8.GetBytes(audioContent);
        var startedAt = DateTimeOffset.Now;
        var endedAt = DateTimeOffset.Now.AddHours(1);
        
        var meeting = new Meeting()
        {
            Id = meetingId,
            CreatedDate = DateTimeOffset.Now,
            MeetingNumber = "123"
        };
        
        var meetingRecord = new MeetingRecord
        {
            Id = meetingRecordId,
            RecordNumber = "6666",
            RecordType = MeetingRecordType.OnRecord,
            CreatedDate = startedAt,
            IsDeleted = false,
            UrlStatus = MeetingRecordUrlStatus.Pending,
            MeetingId = meetingId
        };
        
        var speakDetail1 = new MeetingSpeakDetail()
        {
            Id = 1,
            TrackId = "11",
            MeetingNumber = meeting.MeetingNumber,
            SpeakStatus = SpeakStatus.End,
            MeetingRecordId = meetingRecordId,
            SpeakStartTime = DateTimeOffset.Now.ToUnixTimeSeconds()+1,
            SpeakEndTime = DateTimeOffset.Now.ToUnixTimeSeconds()+3
        };
        
        var speakDetail2 = new MeetingSpeakDetail()
        {
            Id = 2,
            TrackId = "22",
            MeetingNumber = meeting.MeetingNumber,
            SpeakStatus = SpeakStatus.Speaking,
            MeetingRecordId = meetingRecordId,
            SpeakStartTime = DateTimeOffset.Now.ToUnixTimeSeconds()+4,
        };

        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(meetingRecord).ConfigureAwait(false);
            await repository.InsertAsync(meeting).ConfigureAwait(false);
            await repository.InsertAsync(speakDetail1).ConfigureAwait(false);
            await repository.InsertAsync(speakDetail2).ConfigureAwait(false);
        });
        
        await RunWithUnitOfWork<IMediator>(async mediator =>
        {
            await mediator.SendAsync(
                new StorageMeetingRecordVideoCommand
                {
                    MeetingId = meetingId,
                    MeetingRecordId = meetingRecordId,
                    EgressId = "5555",
                });

        },builder =>
        {
            var liveKitClient = Substitute.For<ILiveKitClient>();
            var openAiService = Substitute.For<IOpenAiService>();
            var liveKitServerUtilService = Substitute.For<ILiveKitServerUtilService>();
            var sugarTalkHttpClientFactory = Substitute.For<ISugarTalkHttpClientFactory>();

            liveKitClient.GetEgressInfoListAsync(Arg.Any<GetEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new GetEgressInfoListResponseDto()
                {
                    EgressItems = new List<EgressItemDto>
                    {
                        new EgressItemDto
                        {
                            EgressId = "5555",
                            EndedAt = endedAt.ToString(),
                            StartedAt = startedAt.ToString(),
                            Status = "EGRESS_COMPLETE",
                            File = new FileDetails { Location = "mock url" }
                        }
                    }
                });
            
            liveKitServerUtilService.GenerateTokenForRecordMeeting(Arg.Any<UserAccountDto>(), Arg.Any<string>())
                .Returns("117458");
            
            liveKitClient.StopEgressAsync(Arg.Any<StopEgressRequestDto>(), Arg.Any<CancellationToken>())
                .Returns(new StopEgressResponseDto()
                {
                    EgressId = "mock egressId",
                    EndedAt = "mock end at",
                    File = new FileDetails { Location = "mock url" },
                    Status = "mock status",
                    Error = "mock error",
                });
            
            sugarTalkHttpClientFactory.GetAsync<byte[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(fileContent);

            openAiService.ChatCompletionsAsync(
                    Arg.Any<List<CompletionsRequestMessageDto>>(), 
                    Arg.Any<List<CompletionsRequestFunctionDto>>(), 
                    Arg.Any<CompletionsRequestFunctionCallDto>(), 
                    Arg.Any<OpenAiModel>(), Arg.Any<CompletionResponseFormatDto>(), null, Arg.Any<double>(),
                    Arg.Any<bool>(),  Arg.Any<CancellationToken>())
                .Returns(new CompletionsResponseDto
                {
                    Choices = new List<CompletionsChoiceDto>
                    {
                        new()
                        {
                            Message = new CompletionsChoiceMessageDto
                            {
                                Content =  "{\"optimized_text\": \"I'm smart content\"}"
                            }
                        }
                    }
                });
            
            openAiService.TranscriptionAsync(Arg.Any<byte[]>(), Arg.Any<TranscriptionLanguage?>(),
                Arg.Any<long>(), Arg.Any<long>(), Arg.Any<TranscriptionFileType>(), Arg.Any<TranscriptionResponseFormat>(),
                Arg.Any<CancellationToken>()).Returns(audioContent);
            
            builder.RegisterInstance(liveKitClient);
            builder.RegisterInstance(openAiService);
            builder.RegisterInstance(liveKitServerUtilService);
            builder.RegisterInstance(sugarTalkHttpClientFactory);
        });
        
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            var afterGetRecords = await repository.Query<MeetingRecord>().ToListAsync().ConfigureAwait(false);

            afterGetRecords.ShouldNotBeNull();
            afterGetRecords.FirstOrDefault(x => x.Id == meetingRecordId)?.MeetingId.ShouldBe(meetingRecord.MeetingId);
            afterGetRecords.FirstOrDefault(x => x.Id == meetingRecordId)?.Url.ShouldBe("mock url");
            afterGetRecords.FirstOrDefault(x => x.Id == meetingRecordId)?.RecordType.ShouldBe(MeetingRecordType.EndRecord);
            afterGetRecords.FirstOrDefault(x => x.Id == meetingRecordId)?.UrlStatus.ShouldBe(MeetingRecordUrlStatus.Completed);
          
            var afterGetMeetingDetail = await repository.Query<MeetingSpeakDetail>().ToListAsync().ConfigureAwait(false);
            
            afterGetMeetingDetail.ShouldNotBeNull();
            afterGetMeetingDetail.Count.ShouldBe(2);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 1)?.FileTranscriptionStatus.ShouldBe(FileTranscriptionStatus.Completed);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 2)?.FileTranscriptionStatus.ShouldBe(FileTranscriptionStatus.Completed);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 1)?.OriginalContent.ShouldBe(audioContent);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 2)?.OriginalContent.ShouldBe(audioContent);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 2)?.SpeakEndTime.ShouldNotBeNull();
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 1)?.SpeakStatus.ShouldBe(SpeakStatus.End);
            afterGetMeetingDetail.FirstOrDefault(x => x.Id == 1)?.SmartContent.ShouldBe("I'm smart content");
        });
    }
}