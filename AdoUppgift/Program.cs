using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Data;
namespace AdoUppgift
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine("  FILMDATABAS: Sök skådespelares filmer");
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Den här appen låter dig söka efter vilka filmer");
            Console.WriteLine("en skådespelare medverkat i i Sakila-databasen.");
            Console.WriteLine();
            Console.WriteLine("Ange först förnamn, sedan efternamn (t.ex. PENELOPE GUINESS)");
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine();

            var config = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json")
            .Build();

            Console.WriteLine("Ange ett skådespelares förnamn:");
            string firstName = Console.ReadLine()?.ToUpper().Trim() ?? string.Empty;

            Console.WriteLine("Ange ett skådespelares efternamn:");
            string lastName = Console.ReadLine()?.ToUpper().Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("Ogiltigt namn. Försök igen.");
                return;
            }

            string commandTxt = @"
            SELECT a.first_name, a.last_name, f.title 
            FROM actor a 
            INNER JOIN film_actor fa ON a.actor_id = fa.actor_id 
            INNER JOIN film f ON fa.film_id = f.film_id 
            WHERE a.first_name = @firstName AND a.last_name = @lastName";

            var connection = new SqlConnection(config.GetSection("connectionString")["SakilaDb"]);
            var command = new SqlCommand(commandTxt, connection);

            command.Parameters.Add("@firstName", SqlDbType.NVarChar, 50).Value = firstName;
            command.Parameters.Add("@lastName", SqlDbType.NVarChar, 50).Value = lastName;

            try
            {
                connection.Open();
                using var dbRec = command.ExecuteReader();

                if (!dbRec.HasRows)
                {
                    Console.WriteLine($"Ingen skådespelare hittades med namnet {firstName} {lastName}");
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"\nSkådespelaren {firstName} {lastName} var med i: \n");
                    while (dbRec.Read())
                    {
                        Console.WriteLine($"- {dbRec["title"]}");
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod: {ex.Message}");
            }
        }
    }
}
