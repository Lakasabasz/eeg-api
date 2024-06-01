using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using EegApi.Migrations;
using MySqlConnector;

namespace EegApi;

public class DbService
{
    private const string MigrationsTableName = "EegApiMigrations";
    private const string MigrationsTableSql = """
        CREATE TABLE EegApiMigrations (
            migrationId uuid NOT NULL,
            migrationCreationDate datetime NOT NULL,
            PRIMARY KEY (migrationId)
        );
    """;
    
    private readonly IDbConnection _connection;

    public DbService()
    {
        _connection = new MySqlConnection($"Server={EnvironmentData.DbHost};Port={EnvironmentData.DbPort};Database={EnvironmentData.DbName};Uid={EnvironmentData.DbUser};Pwd={EnvironmentData.DbPassword}");
        _connection.Open();
    }

    private void EnsureMigrationTable()
    {
        var tableExists = _connection.ExecuteScalar("""
                select case when 
                    exists((select * from information_schema.tables where table_name = 'EegApiMigrations'))
                    then 1 else 0 end
        """);
        bool exists = (int?)tableExists == 1;
        if (!exists) _connection.Execute(MigrationsTableSql);
    }

    public void Migrate()
    {
        EnsureMigrationTable();
        var migrations = _connection.Query<Migration>($"SELECT * FROM {MigrationsTableName}")
            .ToList();

        var migrationsList = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.GetInterface(nameof(IMigration)) is not null)
            .Select(Activator.CreateInstance)
            .Cast<IMigration>()
            .Where(x => migrations.All(y => y.migrationId != x.Id))
            .OrderBy(x => x.Creation)
            .ToList();
        
        foreach (var migration in migrationsList)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                migration.Apply(_connection, transaction);
                _connection.Execute(
                    $"INSERT INTO {MigrationsTableName}(migrationId, migrationCreationDate)" +
                    "VALUES (@Id, @Creation)", migration,
                    transaction: transaction);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            transaction.Commit();
        }
    }
    
    public Label? GetLabelByName(string labelName)
    {
        return _connection.QueryFirstOrDefault<Label>("SELECT * FROM Labels WHERE LabelName = @label",
            new { label = labelName });
    }

    public void InsertRecording(Description description, IEnumerable<TaggedDataRow> taggedData)
    {
        _connection.Execute(
            "INSERT INTO Descriptions(`descriptionId`, `recordDate`, `sourceIp`, `headsetName`, `labelId`) " +
            "VALUES (@descriptionId, @recordDate, @sourceIp, @headsetName, @labelId)", description);
        foreach (var taggedDataRow in taggedData)
        {
            _connection.Execute(
                "INSERT INTO Data(rowId, descriptionId, AF3, F7, F3, FC5, T7, P7, O1, O2, P8, T8, FC6, F4, F8, AF4)" +
                "VALUES (@rowId, @descriptionId, @AF3, @F7, @F3, @FC5, @T7, @P7, @O1, @O2, @P8, @T8, @FC6, @F4, @F8, @AF4)",
                taggedDataRow);
        }
    }
}