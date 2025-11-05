using System;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace RestaurantJapanese.DataAcces
{
    public class ConnectionFactory : IConnFactory
    {
        private readonly string _conn;
        public ConnectionFactory()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "db.local.json");
            if (!File.Exists(path)) throw new InvalidOperationException($"Falta {path}");
            var json = File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize<DbCfg>(json) ?? throw new InvalidOperationException("db.local.json inválido");
            _conn = obj.ConnectionString ?? throw new InvalidOperationException("ConnectionString vacío");
        }
        public IDbConnection Create() => new SqlConnection(_conn);

        private class DbCfg { public string? ConnectionString { get; set; } }
    }
}
