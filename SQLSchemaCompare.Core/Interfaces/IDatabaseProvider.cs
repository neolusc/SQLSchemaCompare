﻿using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanisms to retrieve various information of a database
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Gets the database structure
        /// </summary>
        /// <param name="taskInfo">The task info for async operations</param>
        /// <returns>The database structure</returns>
        ABaseDb GetDatabase(TaskInfo taskInfo);

        /// <summary>
        /// Gets the list of available database
        /// </summary>
        /// <returns>The list of database names</returns>
        List<string> GetDatabaseList();
    }
}