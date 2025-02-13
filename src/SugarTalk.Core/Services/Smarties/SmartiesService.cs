using System;
using Serilog;
using System.Linq;
using System.Text;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SugarTalk.Core.Services.Aws;
using SugarTalk.Core.Services.Http;
using SugarTalk.Core.Domain.Meeting;
using Microsoft.IdentityModel.Tokens;
using SugarTalk.Core.Services.Ffmpeg;
using SugarTalk.Core.Services.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Dto.SpeechMatics;
using SugarTalk.Messages.Commands.SpeechMatics;

namespace SugarTalk.Core.Services.Smarties;

public interface ISmartiesService : IScopedDependency
{
    Task HandleTranscriptionCallbackAsync(TranscriptionCallBackCommand command, CancellationToken cancellationToken);
}

public class SmartiesService : ISmartiesService
{
    private readonly IAwsS3Service _awsS3Service;
    private readonly IFfmpegService _ffmpegService;
    private readonly IOpenAiService _openAiService;
    private readonly IMeetingService _meetingService;
    private readonly IMeetingDataProvider _meetingDataProvider;
    private readonly ISmartiesDataProvider _smartiesDataProvider;

    public SmartiesService(IAwsS3Service awsS3Service, IFfmpegService ffmpegService, IOpenAiService openAiService, IMeetingService meetingService, IMeetingDataProvider meetingDataProvider, ISmartiesDataProvider smartiesDataProvider, ISugarTalkHttpClientFactory sugarTalkHttpClientFactory)
    {
        _awsS3Service = awsS3Service;
        _ffmpegService = ffmpegService;
        _openAiService = openAiService;
        _meetingService = meetingService;
        _meetingDataProvider = meetingDataProvider;
        _smartiesDataProvider = smartiesDataProvider;
    }

    public async Task HandleTranscriptionCallbackAsync(TranscriptionCallBackCommand command, CancellationToken cancellationToken)
    {
        Log.Information("Sugartalk speech matics call back: {@comman}", command);
        
        if (command == null || command.Results.IsNullOrEmpty() || command.Job == null || command.Job.Id.IsNullOrEmpty()) return;
        
        var originalSpeakInfos = StructureDiarizationResults(command.Results);
        
        Log.Information("SpeechMatics callback originalSpeakInfos: {@originalSpeakInfos}", originalSpeakInfos);
        
        var speechMaticsRecord = await _smartiesDataProvider.GetSpeechMaticsRecordAsync(command.Job.Id, cancellationToken).ConfigureAwait(false);
        
        Log.Information("SpeechMatics record: {@speechMaticsRecord}", speechMaticsRecord);
        
        var originalSpeakDetails = await _meetingDataProvider.GetMeetingSpeakDetailsAsync(
            meetingNumber: speechMaticsRecord.MeetingNumber, recordId: speechMaticsRecord.MeetingRecordId, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        Log.Information("SpeechMatics originalSpeakDetails: {@originalSpeakDetails}", originalSpeakDetails);
        
        var meetingRecord = (await _meetingDataProvider.GetMeetingRecordsAsync(speechMaticsRecord.MeetingRecordId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        Log.Information("SpeechMatics meetingRecord: {@meetingRecord}", meetingRecord);
        
        var speakDetails = new List<MeetingSpeakDetail>();
        
        foreach (var speakInfo in originalSpeakInfos)
        {
            var speakDetail = originalSpeakDetails.OrderBy(x => Math.Abs(x.SpeakStartTime - speakInfo.StartTime)).FirstOrDefault();

            Log.Information("The smallest relative time difference speak detail: {@speakDetail}", speakDetail);
            
            var replaceSameSpeaker = originalSpeakInfos.Where(x => x.Speaker.Equals(speakInfo.Speaker)).Select(x =>
            {
                if (speakDetail == null) return null;
                
                x.Speaker = speakDetail?.Username;
                
                return new MeetingSpeakDetail
                {
                    UserId = speakDetail.UserId,
                    TrackId = speakDetail.TrackId,
                    Username = speakDetail.Username,
                    CreatedDate = speakDetail.CreatedDate,
                    SpeakStatus = speakDetail.SpeakStatus,
                    SpeakEndTime = Convert.ToInt64(x.EndTime * 1000),
                    MeetingNumber = speakDetail.MeetingNumber,
                    MeetingRecordId = speakDetail.MeetingRecordId,
                    SpeakStartTime = Convert.ToInt64(x.StartTime * 1000),
                    FileTranscriptionStatus = speakDetail.FileTranscriptionStatus,
                };
            }).ToList();
            
            Log.Information("SpeechMatics replaceSameSpeaker: {@replaceSameSpeaker}", replaceSameSpeaker);
            
            speakDetails.AddRange(replaceSameSpeaker);
            
            if (speakDetails.Count == originalSpeakInfos.Count)
                break;
        }

        Log.Information("New speak details: {@speakDetails}", speakDetails);
        
        var recordUrl = await _awsS3Service.GeneratePresignedUrlAsync(meetingRecord?.Url ?? "", 30).ConfigureAwait(false);
        
        var localhostUrl = await _meetingService.DownloadWithRetryAsync(recordUrl, cancellationToken: cancellationToken).ConfigureAwait(false);

        var audio = await _ffmpegService.VideoToAudioConverterAsync(localhostUrl, cancellationToken).ConfigureAwait(false);
        
        speakDetails = await TranscriptionSpeakInfoAsync(speakDetails, audio, meetingRecord, cancellationToken).ConfigureAwait(false);
        
        Log.Information("Transcription new speak details: {@speakDetails}", speakDetails);
        
        await _meetingDataProvider.UpdateMeetingSpeakDetailsAsync(speakDetails, true, cancellationToken).ConfigureAwait(false);
        
        await _meetingDataProvider.DeleteMeetingSpeekDetailsAsync(originalSpeakDetails, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<MeetingSpeakDetail>> TranscriptionSpeakInfoAsync(
        List<MeetingSpeakDetail> originalSpeakDetails, byte[] audioContent, MeetingRecord meetingRecord, CancellationToken cancellationToken)
    {
        var audioBytes =  await _ffmpegService.ConvertFileFormatAsync(audioContent, TranscriptionFileType.Mp3, cancellationToken).ConfigureAwait(false);
        
        foreach (var speakDetail in originalSpeakDetails)
        {
            Log.Information("Start time of speak in video: {SpeakStartTimeVideo}, End time of speak in video: {SpeakEndTimeVideo}", speakDetail.SpeakStartTime, speakDetail.SpeakEndTime);

            try
            {
                if (speakDetail.SpeakStartTime != 0 && speakDetail.SpeakEndTime != 0)
                    speakDetail.OriginalContent = await _openAiService.TranscriptionAsync(
                        audioBytes, TranscriptionLanguage.Chinese, Convert.ToInt64(speakDetail.SpeakStartTime), Convert.ToInt64(speakDetail.SpeakEndTime),
                        TranscriptionFileType.Mp3, TranscriptionResponseFormat.Text, cancellationToken: cancellationToken).ConfigureAwait(false);
                else
                    speakDetail.OriginalContent = "";
            }
            catch (Exception ex)
            {
                Log.Information("transcription error: {ErrorMessage}", ex.Message);
            }
            finally
            {
                speakDetail.SpeakStartTime += meetingRecord.CreatedDate.ToUnixTimeMilliseconds();
                speakDetail.SpeakEndTime += meetingRecord.CreatedDate.ToUnixTimeMilliseconds();
            }
        }
        
        return originalSpeakDetails;
    }

    private List<SpeechMaticsSpeakInfoDto> StructureDiarizationResults(List<SpeechMaticsResultDto> results)
    {
        string currentSpeaker = null;
        var startTime = 0.0;
        var endTime = 0.0;
        var speakInfos = new List<SpeechMaticsSpeakInfoDto>();

        foreach (var result in results.Where(result => !result.Alternatives.IsNullOrEmpty()))
        {
            if (currentSpeaker == null)
            {
                currentSpeaker = result.Alternatives[0].Speaker;
                startTime = result.StartTime;
                endTime = result.EndTime;
                continue;
            }

            if (result.Alternatives[0].Speaker.Equals(currentSpeaker))
            {
                endTime = result.EndTime;
            }
            else
            {
                speakInfos.Add(new SpeechMaticsSpeakInfoDto { EndTime = endTime, StartTime = startTime, Speaker = currentSpeaker });
                currentSpeaker = result.Alternatives[0].Speaker;
                startTime = result.StartTime;
                endTime = result.EndTime;
            }
        }

        speakInfos.Add(new SpeechMaticsSpeakInfoDto { EndTime = endTime, StartTime = startTime, Speaker = currentSpeaker });

        Log.Information("Structure diarization results : {@speakInfos}", speakInfos);
        
        return speakInfos;
    }
}