using System.Data;

namespace RestaurantJapanese.DataAcces
{
    public interface IConnFactory
    {
        IDbConnection Create();
    }
}
