using DbUp;
using MySqlConnector;

namespace SugarTalk.Core.DbUp;

public class DbUpRunner
{
    private readonly string _connectionString;

    public DbUpRunner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Run()
    {
        CreateDatabaseIfNotExist(_connectionString);
        
        EnsureDatabase.For.MySqlDatabase(_connectionString);

        var upgradeEngine = DeployChanges.To.MySqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DbUpRunner).Assembly)
            .WithTransaction()
            .LogToAutodetectedLog()
            .LogToConsole()
            .Build();

        var result = upgradeEngine.PerformUpgrade();

        if (!result.Successful)
            throw result.Error;
    }

    private void CreateDatabaseIfNotExist(string connectionStr)
    {
        var (connectionString, databaseName) = GetConnectionStringThatWithoutConnectingToAnyDatabase(connectionStr);

        using var connection = new MySqlConnection(connectionString);

        using var command = new MySqlCommand(
            "CREATE SCHEMA If Not Exists `" + databaseName + "` Character Set UTF8;", connection);

        try
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }
        
    private (string ConnectionString, string DatabaseName) GetConnectionStringThatWithoutConnectingToAnyDatabase(string connectionStr)
    {
        var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionStr);
        var database = connectionStringBuilder.Database;
        connectionStringBuilder.Database = null;
        return (connectionStringBuilder.ToString(), database);
    }
}