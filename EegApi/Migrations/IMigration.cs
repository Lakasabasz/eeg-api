using System.Data;

namespace EegApi.Migrations;

interface IMigration
{
    DateTime Creation { get; }
    Guid Id { get; }

    void Apply(IDbConnection connection, IDbTransaction transaction);
}