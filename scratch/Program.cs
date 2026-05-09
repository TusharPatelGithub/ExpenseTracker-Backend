using System;
using Npgsql;

namespace Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Host=localhost;Port=5432;Database=SpendSmart_Auth;Username=postgres;Password=postgres";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Email\" ILIKE '%Tushar@1234.com%'", connection);
            int rowsAffected = cmd.ExecuteNonQuery();

            Console.WriteLine($"Rows affected: {rowsAffected}");
        }
    }
}
