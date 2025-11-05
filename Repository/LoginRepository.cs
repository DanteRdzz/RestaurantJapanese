using RestaurantJapanese.Repository.Interfaces;
using RestaurantJapanese.DataAcces;
using RestaurantJapanese.Models;
using Dapper;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace RestaurantJapanese.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IConnFactory _connFactory;
        public LoginRepository(IConnFactory connFactory) => _connFactory = connFactory;

        public async Task<UserModel?> ValidateAsync(string user, string password)
        {

            using var conn = (SqlConnection)_connFactory.Create();
            return await conn.QueryFirstOrDefaultAsync<UserModel>
                (
                    "dbo.RestJP_sp_ValidateUser",
                    new { UserName = user, Password = password },
                    commandType: System.Data.CommandType.StoredProcedure
                );
        }
    }
}
