﻿namespace SQLCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql index definition
    /// </summary>
    public class MySqlIndex : ABaseDbIndex
    {
        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public uint OrdinalPosition { get; set; }
    }
}