
// MySqlConnection, MySqlCommand, MySqlDataReader from MySql.Data:
//   MySQL Connector/NET Developer Guide:
//   https://dev.mysql.com/doc/connector-net/en/
//
// Using statements for IDisposable resource management:
//   Microsoft Docs — using statement:
//   https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/using
//
// Parameterised queries to prevent SQL injection:
//   Stack Overflow — Parameterised queries in MySQL C#:
//   https://stackoverflow.com/questions/652978/parameterized-query-for-mysql-with-c-sharp
//

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cyber_Security_Awareness_Chatbot
{
    /// <summary>
    /// Handles all MySQL database operations for the CyberGuard task assistant.
    /// Uses parameterised queries throughout to prevent SQL injection.
    /// </summary>
    public class DatabaseManager
    {
        //  Connection String 

        /// <summary>
        /// Replace YOUR_PASSWORD with your actual MySQL root password.
        /// If you use a different user account, update Uid= accordingly.
        /// </summary>
        private readonly string _connectionString =
            "Server=localhost;Database=cyberguard_db;Uid=root;Pwd=Password1234;";

        //  Connection Test 

        /// <summary>
        /// Attempts to open and immediately close a connection.
        /// Returns true if the database is reachable, false otherwise.
        /// Called on app startup so the UI can warn the user early.
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //  CREATE 

        /// <summary>
        /// Inserts a new task into the database and sets the auto-generated Id
        /// on the task object so the caller can reference it immediately.
        /// </summary>
        /// <param name="task">The task to persist. Its Id property will be updated.</param>
        /// <returns>True if the insert succeeded, false on any database error.</returns>
        public bool AddTask(CyberTask task)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                // Use @parameters — never string interpolation — for user-supplied data
                string sql = @"INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted, CreatedAt)
                               VALUES (@title, @desc, @reminder, @done, @created);
                               SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc", task.Description);
                cmd.Parameters.AddWithValue("@reminder", (object?)task.ReminderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@done", task.IsCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@created", task.CreatedAt);

                // ExecuteScalar returns the new auto-incremented Id
                var result = cmd.ExecuteScalar();
                task.Id = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseManager] AddTask error: {ex.Message}");
                return false;
            }
        }

        //  READ 

        /// <summary>
        /// Retrieves all tasks from the database, ordered newest first.
        /// Returns an empty list on any error — never throws to the caller.
        /// </summary>
        public List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt " +
                             "FROM Tasks ORDER BY CreatedAt DESC;";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(new CyberTask
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Description = reader.GetString("Description"),
                        // ReminderDate is nullable — check for DBNull before reading
                        ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate"))
                                           ? (DateTime?)null
                                           : reader.GetDateTime("ReminderDate"),
                        IsCompleted = reader.GetBoolean("IsCompleted"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseManager] GetAllTasks error: {ex.Message}");
            }

            return tasks;
        }

        //  UPDATE — Mark as Completed 

        /// <summary>
        /// Sets IsCompleted = 1 for the task with the given Id.
        /// </summary>
        /// <param name="taskId">Database primary key of the task to complete.</param>
        /// <returns>True if at least one row was affected.</returns>
        public bool MarkTaskCompleted(int taskId)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", taskId);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseManager] MarkTaskCompleted error: {ex.Message}");
                return false;
            }
        }

        //  DELETE 

        /// <summary>
        /// Permanently removes the task with the given Id from the database.
        /// </summary>
        /// <param name="taskId">Database primary key of the task to remove.</param>
        /// <returns>True if at least one row was deleted.</returns>
        public bool DeleteTask(int taskId)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = "DELETE FROM Tasks WHERE Id = @id;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", taskId);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseManager] DeleteTask error: {ex.Message}");
                return false;
            }
        }
    }
}