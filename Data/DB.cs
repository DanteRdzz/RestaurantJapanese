using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using RestaurantJapanese.Models;

namespace RestaurantJapanese.Data
{
    public static class DB
    {
        // CAMBIAR DATOS POR SU DB LOCAL!!! (Su servidor)
        public static string connectionString = 
            "Server=DESKTOPE8EQ0FM;Database=REST_JP;Trusted_Connection=True;TrustServerCertificate=True;";

        public static (int id, string display)? ValidateUser(string user, string password)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("dbo.sp_Login_Validate", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserName", user);
            cmd.Parameters.AddWithValue("@Password", password);

          using var read = cmd.ExecuteReader();
            if (read.Read())
            {
                return (Convert.ToInt32(read["IdUser"]), Convert.ToString(read["DisplayName"])!);
            }
            return null;
        }

        public static List<MenuItem> GetMenu()
        {
            var list = new List<MenuItem>();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("dbo.sp_Menu_GetAll", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            using var read = cmd.ExecuteReader();   

            while (read.Read())
            {
                list.Add(new MenuItem
                {
                    IdMenuItem = Convert.ToInt32(read["IdMenuItem"]),
                    Name = Convert.ToString(read["Name"])!,
                    Description = read["Description"] is DBNull ? null : Convert.ToString(read["Description"]),
                    Price = Convert.ToDecimal(read["Price"])
                });
            }
            return list;
        }

        public static int CreateTicket(int userId, decimal tip, List<CartItem> items,
                                       out decimal subtotal, out decimal tax, out decimal total)
        {
            subtotal = 0m;
            foreach (var i in items) subtotal += i.UnitPrice * i.Qty;
            tax = Math.Round(subtotal * 0.16m, 2);
            total = subtotal + tax + tip;

            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                int idTicket;
                // Encabezado
                using (var cmd = new SqlCommand("dbo.sp_Ticket_CreateHeader", conn, tx)
                { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@CreatedBy", userId);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Tax", tax);
                    cmd.Parameters.AddWithValue("@Tip", tip);
                    cmd.Parameters.AddWithValue("@Total", total);
                    idTicket = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Detalle
                foreach (var it in items)
                {
                    using var cmd = new SqlCommand("dbo.sp_Ticket_AddItem", conn, tx)
                    { CommandType = System.Data.CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@IdTicket", idTicket);
                    cmd.Parameters.AddWithValue("@IdMenuItem", it.IdMenuItem);
                    cmd.Parameters.AddWithValue("@Qty", it.Qty);
                    cmd.Parameters.AddWithValue("@UnitPrice", it.UnitPrice);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return idTicket;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

    }
}
