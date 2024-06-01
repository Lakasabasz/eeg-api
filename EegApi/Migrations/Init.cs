using System.Data;
using Dapper;

namespace EegApi.Migrations;

class Init: IMigration
{
    private const string LabelsTableSql = """
                        CREATE TABLE `Labels` (
                          `labelId` int(11) NOT NULL AUTO_INCREMENT,
                          `labelName` varchar(64) NOT NULL,
                          PRIMARY KEY (`labelId`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
    """;

    private const string DescriptionsTableSql = """
        CREATE TABLE `Descriptions` (
                                        `descriptionId` uuid NOT NULL,
                                        `recordDate` datetime NOT NULL,
                                        `sourceIp` varchar(128) NOT NULL,
                                        `headsetName` varchar(128) NOT NULL,
                                        `labelId` int(11) NOT NULL,
                                        PRIMARY KEY (`descriptionId`),
                                        KEY `Descriptions_Labels_labelId_fk` (`labelId`),
                                        CONSTRAINT `Descriptions_Labels_labelId_fk` FOREIGN KEY (`labelId`) REFERENCES `Labels` (`labelId`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
    """;

    private const string DataTableSql = """
        CREATE TABLE `Data` (
                            `rowId` uuid NOT NULL,
                            `descriptionId` uuid NOT NULL,
                            `AF3` double DEFAULT NULL,
                            `F7` double DEFAULT NULL,
                            `F3` double DEFAULT NULL,
                            `FC5` double DEFAULT NULL,
                            `T7` double DEFAULT NULL,
                            `P7` double DEFAULT NULL,
                            `O1` double DEFAULT NULL,
                            `O2` double DEFAULT NULL,
                            `P8` double DEFAULT NULL,
                            `T8` double DEFAULT NULL,
                            `FC6` double DEFAULT NULL,
                            `F4` double DEFAULT NULL,
                            `F8` double DEFAULT NULL,
                            `AF4` double DEFAULT NULL,
                            PRIMARY KEY (`rowId`),
                            KEY `Data_Descriptions_descriptionId_fk` (`descriptionId`),
                            CONSTRAINT `Data_Descriptions_descriptionId_fk` FOREIGN KEY (`descriptionId`) REFERENCES `Descriptions` (`descriptionId`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
    """;
    
    public DateTime Creation { get; } = new(2024, 6, 1, 23, 10, 0);
    public Guid Id { get; } = new("bcc6cc8d-9824-4a08-ab3c-7d70baf904b8");
    public void Apply(IDbConnection connection, IDbTransaction transaction)
    {
        connection.Execute(LabelsTableSql, transaction: transaction);
        connection.Execute(DescriptionsTableSql, transaction: transaction);
        connection.Execute(DataTableSql, transaction: transaction);
    }
}