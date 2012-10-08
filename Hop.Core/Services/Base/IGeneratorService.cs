namespace Hop.Core.Services.Base
{
    public interface IGeneratorService
    {
        Materializer<T> CreateMaterializer<T>();
    }
}