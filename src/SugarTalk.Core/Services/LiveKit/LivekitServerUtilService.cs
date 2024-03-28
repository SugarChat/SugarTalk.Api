using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using LiveKit_CSharp.Services.Meeting;
using Serilog;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Account;
using SugarTalk.Messages.Dto.LiveKit;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Core.Settings.LiveKit;
using SugarTalk.Messages.Dto.LiveKit.Egress;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.LiveKit;

public interface ILiveKitServerUtilService : IScopedDependency
{
    Task<CreateMeetingFromLiveKitResponseDto> CreateMeetingAsync(
        string meetingNumber, string token, int emptyTimeOut = 600, int maxParticipants = 200, CancellationToken cancellationToken = default);

    string GenerateTokenForCreateMeeting(UserAccountDto user, string meetingNumber);
    
    string GenerateTokenForJoinMeeting(UserAccountDto user, string meetingNumber);
    
    string GenerateTokenForRecordMeeting(UserAccountDto user, string meetingNumber);

    string GenerateTokenForGuest(string deviceId, string username, string meetingNumber);

    string GetMeetingInfoPermission(string meetingNumber);
}

public class LiveKitServerUtilService : ILiveKitServerUtilService
{
    private readonly ILiveKitClient _liveKitClient;
    private readonly LiveKitServerSetting _liveKitServerSetting;

    public LiveKitServerUtilService(ILiveKitClient liveKitClient, LiveKitServerSetting liveKitServerSetting)
    {
        _liveKitClient = liveKitClient;
        _liveKitServerSetting = liveKitServerSetting;
    }

    public async Task<CreateMeetingFromLiveKitResponseDto> CreateMeetingAsync(
        string meetingNumber, string token, int emptyTimeOut, int maxParticipants, CancellationToken cancellationToken)
    {
        return new CreateMeetingFromLiveKitResponseDto
        {
            RoomInfo = await _liveKitClient.CreateRoomAsync(token, new CreateLiveKitRoomDto
            {
                MeetingNumber = meetingNumber,
                EmptyTimeOut = emptyTimeOut,
                MaxParticipants = maxParticipants
            }, cancellationToken).ConfigureAwait(false)
        };
    }

    public string GenerateTokenForCreateMeeting(UserAccountDto user, string meetingNumber)
    {
        var generateAccessToken = new GenerateAccessToken();
        
        Log.Information("GenerateTokenForCreateMeeting: meetingNumber: {meetingNumber}, userId: {deviceId}, username: {username}", meetingNumber, user.Id, user.UserName);
        
        return generateAccessToken.CreateMeeting(
            meetingNumber: meetingNumber, apiKey: _liveKitServerSetting.Apikey, apiSecret: _liveKitServerSetting.ApiSecret, 
            userId: user.Id.ToString(), username: user.UserName);
    }

    public string GenerateTokenForJoinMeeting(UserAccountDto user, string meetingNumber)
    {
        var generateAccessToken = new GenerateAccessToken();
        
        Log.Information("GenerateTokenForJoinMeeting: meetingNumber: {meetingNumber}, userId: {deviceId}, username: {username}", meetingNumber, user.Id, user.UserName);
        
        return generateAccessToken.JoinMeeting(
            meetingNumber, _liveKitServerSetting.Apikey, _liveKitServerSetting.ApiSecret, user.UserName, user.UserName);
    }

    public string GenerateTokenForRecordMeeting(UserAccountDto user, string meetingNumber)
    {
        var generateAccessToken = new GenerateAccessToken();

        Log.Information("GenerateTokenForRecordMeeting: meetingNumber: {meetingNumber}, userId: {deviceId}, username: {username}", meetingNumber, user.Id, user.UserName);
        
        return generateAccessToken.RecordMeeting(
            meetingNumber, _liveKitServerSetting.Apikey, _liveKitServerSetting.ApiSecret, user.Id.ToString(), user.UserName);
    }

    public string GenerateTokenForGuest(string deviceId, string username, string meetingNumber)
    {
        var generateAccessToken = new GenerateAccessToken();
        
        Log.Information("GenerateTokenForGuest: meetingNumber: {meetingNumber}, deviceId: {deviceId}, username: {username}", meetingNumber, deviceId, username);
        
        return generateAccessToken.JoinMeeting(
            meetingNumber, _liveKitServerSetting.Apikey, _liveKitServerSetting.ApiSecret, 
            deviceId, username);
    }

    public string GetMeetingInfoPermission(string meetingNumber)
    {
        var generateAccessToken = new GenerateAccessToken();
        
        Log.Information("GenerateTokenForGuest: meetingNumber: {meetingNumber}", meetingNumber);
        
        return generateAccessToken.GetMeetingInfoPermission(
            meetingNumber, _liveKitServerSetting.Apikey, _liveKitServerSetting.ApiSecret);
    }
}