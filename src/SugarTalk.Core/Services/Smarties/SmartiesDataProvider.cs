using System.Threading;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Data;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.SpeechMatics;

namespace SugarTalk.Core.Services.Smarties;

public interface ISmartiesDataProvider : IScopedDependency
{
    Task<SpeechMaticsRecord> GetSpeechMaticsRecordAsync(string transcriptionJobId, CancellationToken cancellationToken);
    
    Task CreateSpeechMaticsRecordAsync(SpeechMaticsRecord record, bool forceSave = true, CancellationToken cancellationToken = default);
}

public class SmartiesDataProvider : ISmartiesDataProvider
{
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SmartiesDataProvider(IRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpeechMaticsRecord> GetSpeechMaticsRecordAsync(string transcriptionJobId, CancellationToken cancellationToken)
    {
        return await _repository.FirstOrDefaultAsync<SpeechMaticsRecord>(x => x.TranscriptionJobId == transcriptionJobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateSpeechMaticsRecordAsync(SpeechMaticsRecord record, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(record, cancellationToken).ConfigureAwait(false);

        if (forceSave)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}