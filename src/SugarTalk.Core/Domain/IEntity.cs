namespace SugarTalk.Core.Domain;

public interface IEntity
{
}

public interface IEntity<TId> : IEntity
{
    public TId Id { get; set; }
}
