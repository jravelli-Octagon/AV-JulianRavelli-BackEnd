
namespace AmericanVirtual.Weather.Challenge.Repository.Interfaces
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>() where T : class;
    }
}