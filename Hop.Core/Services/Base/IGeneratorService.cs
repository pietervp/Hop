namespace Hop.Core
{
    public interface IGeneratorService
    {
        Materializer<T> CreateMaterializer<T>();
    }
}