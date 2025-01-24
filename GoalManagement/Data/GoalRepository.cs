using Dapper;
using Microsoft.Data.SqlClient;
using GoalManagement.Models;

namespace GoalManagement.Data
{
    public class GoalRepository
    {
        private readonly string _connectionString;

        public GoalRepository(string cns)
        {
            _connectionString = cns;
        }

        // Get all TaskItems for the logged-in user
        public async Task<IEnumerable<GoalItem>> GetTasksByUserAsync(string userEmail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT T.*
                FROM Goals T
                INNER JOIN UserGoals UT ON T.Id = UT.GoalId
                WHERE UT.EmailAddress = @EmailAddress";
            return await connection.QueryAsync<GoalItem>(query, new { EmailAddress = userEmail });
        }

        // Get a single TaskItem by ID for the logged-in user
        public async Task<GoalItem?> GetTaskByIdAsync(int goalId, string userEmail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT T.*
                FROM Goals T
                INNER JOIN UserGoals UT ON T.Id = UT.GoalId
                WHERE T.Id = @Id AND UT.EmailAddress = @EmailAddress";
            return await connection.QueryFirstOrDefaultAsync<GoalItem>(query, new { Id = goalId, EmailAddress = userEmail });
        }

        // Create a TaskItem and associate it with the logged-in user
        public async Task AddTaskAsync(GoalItem task, string userEmail)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(); // Test the connection to the database

                var query = @"INSERT INTO Goals (Name, Description, DueDate, IsCompleted, Complexity)
                      OUTPUT INSERTED.Id
                      VALUES (@Name, @Description, @DueDate, @IsCompleted,@Complexity)";
                var goalId = await connection.ExecuteScalarAsync<int>(query, task);

                var userGoalQuery = @"INSERT INTO UserGoals (EmailAddress, GoalId) VALUES (@EmailAddress, @GoalId)";
                await connection.ExecuteAsync(userGoalQuery, new { EmailAddress = userEmail, GoalId = goalId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while adding task: {ex.Message}");
                // Optionally log the error or rethrow if needed
                throw;
            }
        }


        // Update a TaskItem (restricted to the user's tasks)
        public async Task UpdateTaskAsync(GoalItem task, string userEmail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                UPDATE Goals
                SET Name = @Name, Description = @Description, DueDate = @DueDate,
                    IsCompleted = @IsCompleted,Complexity = @Complexity
                WHERE Id = @Id AND EXISTS (
                    SELECT 1 FROM UserGoals WHERE GoalId = @Id AND EmailAddress = @EmailAddress)";
            await connection.ExecuteAsync(query, new { task.Name, task.Description, task.DueDate, task.IsCompleted,task.Complexity, task.Id, EmailAddress = userEmail });
        }

        // Delete a TaskItem (restricted to the user's tasks)
        public async Task DeleteTaskAsync(int goalId, string userEmail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = @"
                DELETE FROM Goals
                WHERE Id = @Id AND EXISTS (
                    SELECT 1 FROM UserGoals WHERE GoalId = @Id AND EmailAddress = @EmailAddress)";
            await connection.ExecuteAsync(query, new { Id = goalId, EmailAddress = userEmail });
        }
    }
}
