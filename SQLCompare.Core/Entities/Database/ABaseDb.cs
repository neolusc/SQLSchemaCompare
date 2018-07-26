﻿using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information of database classes
    /// </summary>
    public abstract class ABaseDb : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database last modification date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the database's tables
        /// </summary>
        public List<ABaseDbTable> Tables { get; } = new List<ABaseDbTable>();

        /// <summary>
        /// Gets the database's views
        /// </summary>
        public List<ABaseDbView> Views { get; } = new List<ABaseDbView>();

        /// <summary>
        /// Gets the database's functions
        /// </summary>
        public List<ABaseDbRoutine> Functions { get; } = new List<ABaseDbRoutine>();

        /// <summary>
        /// Gets the database's stored procedures
        /// </summary>
        public List<ABaseDbRoutine> StoredProcedures { get; } = new List<ABaseDbRoutine>();

        /// <summary>
        /// Gets the database's data types
        /// </summary>
        public List<ABaseDbObject> DataTypes { get; } = new List<ABaseDbObject>();
    }
}