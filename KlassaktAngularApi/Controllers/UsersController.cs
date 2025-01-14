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
            _configuration = configuration;
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
    }
}
