using KlassaktAngularApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace KlassaktAngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("GetUsers")]
        public ActionResult<IEnumerable<IDictionary<string, object>>> GetUsers(IConfiguration _configuration)
        {
            // Get the connection string from appsettings.json

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;


            // List to store rows fetched from the database as dictionaries
            var result = new List<Dictionary<string, object>>();

            // Query to get all users
            string query = "SELECT * FROM Users";

            // Create and open SQL connection
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            // Read each row and map it to a dictionary
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();

                                // Loop through columns and add them to the dictionary
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }

                                result.Add(row);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Handle exception (logging, etc.)
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            // Return the result as JSON
            return Ok(result);
        }
        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] UserModel usermodel )
        {
            if (usermodel == null)
            {
                return BadRequest("User data is null.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, "Database connection string is not configured.");
            }

            string query1 = "INSERT INTO Users (Name, Gender, Address, PhoneNumber, is_active, Role, Email, LoginName) " +
                            "VALUES (@Name, @Gender, @Address, @PhoneNumber, @is_active, @Role, @Email, @LoginName);";
            string query2 = "INSERT INTO Credentials (LoginName, password) VALUES (@LoginName, @Password);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction(); // Start the transaction

                    // Begin using SqlCommand objects for both queries inside the same `using` block
                    using (SqlCommand command1 = new SqlCommand(query1, connection, transaction),
                           command2 = new SqlCommand(query2, connection, transaction))
                    {
                        // Add parameters for the first query (Users table)
                        command1.Parameters.AddWithValue("@Name", usermodel.Name);
                        command1.Parameters.AddWithValue("@Gender", usermodel.Gender);
                        command1.Parameters.AddWithValue("@Address", usermodel.Address);
                        command1.Parameters.AddWithValue("@PhoneNumber", usermodel.phonenumber);
                        command1.Parameters.AddWithValue("@is_active", usermodel.IsActive);
                        command1.Parameters.AddWithValue("@Role", usermodel.Role);
                        command1.Parameters.AddWithValue("@Email", usermodel.Email);
                        command1.Parameters.AddWithValue("@LoginName", usermodel.LoginName);

                        // Add parameters for the second query (Credentials table)
                        command2.Parameters.AddWithValue("@LoginName", usermodel.LoginName);
                        command2.Parameters.AddWithValue("@Password", usermodel.Password);  // Ensure Password is provided

                        int result1 = command1.ExecuteNonQuery(); // Execute the first insert query

                        if (result1 <= 0)
                        {
                            transaction.Rollback(); // If the first query fails, rollback the transaction
                            return StatusCode(500, "Failed to insert user into Users table.");
                        }

                        int result2 = command2.ExecuteNonQuery(); // Execute the second insert query

                        if (result2 <= 0)
                        {
                            transaction.Rollback(); // If the second query fails, rollback the transaction
                            return StatusCode(500, "Failed to insert data into Credentials table.");
                        }

                        // If both queries succeed, commit the transaction
                        transaction.Commit();
                    }
                    return Ok("User created successfully.");
                }
                catch (SqlException ex)
                {
                    // Log exception and rollback the transaction if it failed
                    if (transaction != null)
                    {
                        transaction.Rollback(); // Rollback on error
                    }
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Rollback the transaction and return a general error message
                    if (transaction != null)
                    {
                        transaction.Rollback(); // Rollback on error
                    }
                    return StatusCode(500, $"General error: {ex.Message}");
                }
            }
        }

    }
}
