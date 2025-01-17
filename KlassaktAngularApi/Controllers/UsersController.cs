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
        public IActionResult CreateUser([FromBody] UserModel usermodel)
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

            // SQL query to insert user data
            string query = "INSERT INTO Users (Name, Gender,Address,Course,is_active,Role,Email,LoginName) VALUES (@Name, @Gender,@Address,@Course,@is_active,@Role,@Email,@LoginName)";

            // Using SqlConnection and SqlCommand to insert data
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@Name", usermodel.Name);
                        command.Parameters.AddWithValue("@Email", usermodel.Email);
                        command.Parameters.AddWithValue("@Gender", usermodel.Gender);
                        command.Parameters.AddWithValue("@Course", usermodel.Course);
                        command.Parameters.AddWithValue("@LoginName", usermodel.LoginName);
                        command.Parameters.AddWithValue("@Address", usermodel.Address);
                        command.Parameters.AddWithValue("@Role", usermodel.Role);
                        command.Parameters.AddWithValue("@is_active", usermodel.IsActive);

                        int result = command.ExecuteNonQuery(); // Executes the insert query

                        if (result > 0)
                        {
                            // Return success response
                            return Ok("User created successfully.");
                        }
                        else
                        {
                            return StatusCode(500, "Internal server error.");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log exception and return an error response
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Return a general error message
                    return StatusCode(500, $"General error: {ex.Message}");
                }
            }
        }
    }
}
