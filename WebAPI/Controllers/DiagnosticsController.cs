using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DiagnosticsController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("db-test")]
    public IActionResult TestDatabaseConnection()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("UnderGroundhoopersDB");
            var result = new
            {
                ConnectionString = connectionString,
                CanConnect = false,
                ServerVersion = "",
                DatabaseCount = 0,
                EnvironmentDetails = new
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingDirectory = Environment.CurrentDirectory
                },
                Error = ""
            };

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if we can connect
                result = new
                {
                    ConnectionString = connectionString,
                    CanConnect = true,
                    ServerVersion = connection.ServerVersion,
                    DatabaseCount = GetDatabaseCount(connection),
                    EnvironmentDetails = result.EnvironmentDetails,
                    Error = ""
                };
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = ex.Message,
                InnerError = ex.InnerException?.Message,
                StackTrace = ex.StackTrace,
                ConnectionString = _configuration.GetConnectionString("UnderGroundhoopersDB"),
                EnvironmentDetails = new
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            });
        }
    }

    private int GetDatabaseCount(SqlConnection connection)
    {
        using (var command = new SqlCommand("SELECT COUNT(*) FROM sys.databases", connection))
        {
            return (int)command.ExecuteScalar();
        }
    }
}