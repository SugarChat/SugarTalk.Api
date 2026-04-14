using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.OME;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.OME;

public interface IOMEDataProvider : IScopedDependency
{
    Task AddUserAsync(OMEUserAccount userAccount, bool forceSave = true, CancellationToken cancellationToken = default);
}

public class OMEDataProvider : IOMEDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public OMEDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddUserAsync(OMEUserAccount userAccount, bool forceSave = true, CancellationToken cancellationToken = default)
    {
        var user = await _repository.Query<OMEUserAccount>()
            .Where(x => x.Id == userAccount.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (user != null)
        {
            await _repository.DeleteAsync(user, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        
        await _repository.InsertAsync(userAccount, cancellationToken).ConfigureAwait(false);
        
        if (forceSave) await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}