using Microsoft.Data.SqlClient;

namespace ZATCA_V2.Helpers
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlHealthCheck : IHealthCheck
    {
        private readonly string? _connectionString;

        public SqlHealthCheck(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var sqlConnection = new SqlConnection(_connectionString);

                await sqlConnection.OpenAsync(cancellationToken);

                using var command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT 1";

                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch(Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    context.Registration.FailureStatus.ToString(),
                    exception: ex);
            }
        }
    }

}
