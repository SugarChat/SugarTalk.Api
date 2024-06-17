using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Services.Ffmpeg;

public interface IFfmpegService : IScopedDependency
{
    Task<List<byte[]>> SplitAudioAsync(byte[] audioBytes, long secondsPerAudio, CancellationToken cancellationToken);
    
    Task<List<byte[]>> SpiltAudioAsync(byte[] audioBytes, long startTime, long endTime, CancellationToken cancellationToken);

    Task<byte[]> ConvertFileFormatAsync(byte[] file, TranscriptionFileType fileType, CancellationToken cancellationToken);

    Task<byte[]> VideoToAudioConverterAsync(string url, CancellationToken cancellationToken);
}

public class FfmpegService : IFfmpegService
{
    public async Task<byte[]> ConvertMp4ToWavAsync(byte[] mp4Bytes, CancellationToken cancellationToken)
    {
        var baseFileName = Guid.NewGuid().ToString();
        var inputFileName = $"{baseFileName}.mp4";
        var outputFileName = $"{baseFileName}.wav";

        try
        {
            Log.Information("Converting mp4 to wav, the mp4 length is {Length}", mp4Bytes.Length);

            await File.WriteAllBytesAsync(inputFileName, mp4Bytes, cancellationToken).ConfigureAwait(false);

            if (!File.Exists(inputFileName))
            {
                Log.Information("Converting mp4 to wav, persisted mp4 file failed");
                return Array.Empty<byte>();
            }

            var ffmpegArguments = $"-i {inputFileName} -vn -acodec pcm_s16le {outputFileName}";

            Log.Information("Ffmpeg convert command arguments: {FfmpegArguments}", ffmpegArguments);

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = ffmpegArguments
                };

                proc.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        Log.Information("Converting mp4 to wav, {@Output}", e);
                    }
                };

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(outputFileName))
                return await File.ReadAllBytesAsync(outputFileName, cancellationToken).ConfigureAwait(false);

            Log.Information("Converting mp4 to wav, failed to generate wav");

            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Converting mp4 to wav error occurred");
            return Array.Empty<byte>();
        }
        finally
        {
            Log.Information("Converting mp4 to wav finally deleting files");

            if (File.Exists(inputFileName)) File.Delete(inputFileName);
            if (File.Exists(outputFileName)) File.Delete(outputFileName);
        }
    }
    
    public async Task<byte[]> ConvertMp3ToWavAsync(byte[] mp3Bytes, int? samplingRate = null, CancellationToken cancellationToken = default)
    {
        var baseFileName = Guid.NewGuid().ToString();
        var inputFileName = $"{baseFileName}.mp3";
        var outputFileName = $"{baseFileName}.wav";
    
        try
        {
            Log.Information("Converting mp3 to wav, the mp3 length is {Length}", mp3Bytes.Length);

            await File.WriteAllBytesAsync(inputFileName, mp3Bytes, cancellationToken).ConfigureAwait(false);

            if (!File.Exists(inputFileName))
            {
                Log.Information("Converting mp3 to wav, persisted mp3 file failed");
                return Array.Empty<byte>();
            }

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = samplingRate.HasValue
                        ? $"-i {inputFileName} -vn -acodec pcm_s16le -ar {samplingRate.Value} {outputFileName}"
                        : $"-i {inputFileName} -vn -acodec pcm_s16le {outputFileName}"
                };

                proc.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Information("Converting mp3 to wav: {Output}", e.Data);
                    }
                };

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(outputFileName))
                return await File.ReadAllBytesAsync(outputFileName, cancellationToken).ConfigureAwait(false);
            
            Log.Information("Converting mp3 to wav, failed to generate wav");
            
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Converting mp3 to wav error occurred");
            return Array.Empty<byte>();
        }
        finally
        {
            Log.Information("Converting mp3 to wav finally deleting files");
            
            if (File.Exists(inputFileName)) File.Delete(inputFileName);
            if (File.Exists(outputFileName)) File.Delete(outputFileName);
        }
    }
    
    public async Task<List<byte[]>> SplitAudioAsync(byte[] audioBytes, long secondsPerAudio,
        CancellationToken cancellationToken)
    {
        var audioDataList = new List<byte[]>();
        var baseFileName = Guid.NewGuid().ToString();
        var inputFileName = $"{baseFileName}.wav";

        try
        {
            Log.Information("Splitting audio, the audio length is {Length}", audioBytes.Length);

            await File.WriteAllBytesAsync(inputFileName, audioBytes, cancellationToken).ConfigureAwait(false);

            if (!File.Exists(inputFileName))
            {
                Log.Error("Splitting audio, persisted failed");
                return audioDataList;
            }

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments =
                        $"-i {inputFileName} -segment_time {secondsPerAudio} -f segment -c copy {baseFileName}-split-%03d.wav"
                };

                proc.OutputDataReceived += (_, e) => Log.Information("Splitting audio, {@Output}", e);

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }

            var index = 0;
            string splitFileName;

            while (File.Exists(splitFileName = $"{baseFileName}-split-{index:D3}.wav"))
            {
                audioDataList.Add(await File.ReadAllBytesAsync(splitFileName, cancellationToken).ConfigureAwait(false));

                File.Delete(splitFileName);

                index++;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Splitting audio error.");
        }
        finally
        {
            Log.Information("Splitting audio finally deleting files");

            if (File.Exists(inputFileName))
                File.Delete(inputFileName);
        }

        return audioDataList;
    }
    
    public async Task<List<byte[]>> SpiltAudioAsync(byte[] audioBytes, long startTime, long endTime, CancellationToken cancellationToken)
    {
        var audioDataList = new List<byte[]>();
        var baseFileName = Guid.NewGuid().ToString();
        var inputFileName = $"{baseFileName}.wav";
        var outputFileName = $"{baseFileName}-spilt.wav";

        var startTimeSpan = TimeSpan.FromMilliseconds(startTime);
        var endTimeSpan = TimeSpan.FromMilliseconds(endTime);

        var startTimeFormatted = startTimeSpan.ToString(@"hh\:mm\:ss\.fff");
        var endTimeFormatted = endTimeSpan.ToString(@"hh\:mm\:ss\.fff");

        try
        {
            Log.Information("According stareTime Splitting audio, the audio length is {Length}", audioBytes.Length);

            await File.WriteAllBytesAsync(inputFileName, audioBytes, cancellationToken).ConfigureAwait(false);

            if (!File.Exists(inputFileName))
            {
                Log.Error("Splitting audio, persisted failed");
                return audioDataList;
            }

            var spiltArguments =
                $"-i {inputFileName} -ss {startTimeFormatted} -to {endTimeFormatted} -c copy {outputFileName}";

            Log.Information("spilt command arguments: {spiltArguments}", spiltArguments);

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = spiltArguments
                };
                proc.OutputDataReceived += (_, e) => Log.Information("Splitting audio, {@Output}", e);

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(outputFileName))
            {
                audioDataList.Add(await File.ReadAllBytesAsync(outputFileName, cancellationToken)
                    .ConfigureAwait(false));

                File.Delete(outputFileName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Splitting audio error.");
        }
        finally
        {
            Log.Information("Splitting audio finally deleting files");

            if (File.Exists(inputFileName))
                File.Delete(inputFileName);
        }

        return audioDataList;
    }
    
    public async Task<byte[]> ConvertFileFormatAsync(byte[] file, TranscriptionFileType fileType,
        CancellationToken cancellationToken)
    {
        return fileType switch
        {
            TranscriptionFileType.Wav => file,
            TranscriptionFileType.Mp4 => await ConvertMp4ToWavAsync(file, cancellationToken).ConfigureAwait(false),
            TranscriptionFileType.Mp3 => await ConvertMp3ToWavAsync(file,cancellationToken: cancellationToken).ConfigureAwait(false),
            _ => Array.Empty<byte>()
        };
    }
    
    public async Task<byte[]> VideoToAudioConverterAsync(string url, CancellationToken cancellationToken)
    {
        var outputFileName = $"{Guid.NewGuid()}.mp3";
        
        try
        {
            var arguments = $"-i {url} -vn -acodec libmp3lame -ac 1 {outputFileName}";

            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = arguments
                };

                proc.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Information("FFmpeg Output: {Output}", e.Data);
                    }
                };

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }
            
            if (File.Exists(outputFileName))
                return await File.ReadAllBytesAsync(outputFileName, cancellationToken).ConfigureAwait(false);
            
            Log.Error("Failed to generate the combined video file.");
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Video to audio error.");

            return Array.Empty<byte>();
        }
        finally
        {
            Log.Information("Video to audio file finally deleting files");

            if (File.Exists(outputFileName))
                File.Delete(outputFileName);
        }
    }
}
