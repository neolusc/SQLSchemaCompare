﻿using Microsoft.Extensions.Logging;
using SQLCompare.Infrastructure.EntityFramework;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.EntityFramework
{
    /// <summary>
    /// Test class for the DatabaseContext
    /// </summary>
    public class DatabaseContextTests : BaseTests<DatabaseContextTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContextTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public DatabaseContextTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the connection with all the databases
        /// </summary>
        [Fact]
        public void DatabaseConnection()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerProGlobal", "brokerpro", "brokerpro05"))
            {
                this.Logger.LogInformation("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    this.Logger.LogInformation($"=> {table.TableName}");
                }
            }

            this.Logger.LogInformation(string.Empty);

            using (var c = new PostgreSqlDatabaseContext("localhost", "world", "postgres", "test1234"))
            {
                this.Logger.LogInformation("PostgreSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    this.Logger.LogInformation($"=> {table.TableName}");
                }
            }

            this.Logger.LogInformation(string.Empty);

            using (var c = new MySqlDatabaseContext("localhost", "employees", "admin", "test1234"))
            {
                this.Logger.LogInformation("MySqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    this.Logger.LogInformation($"=> {table.TableName}");
                }
            }
        }
    }
}