﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3082
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Data.Objects.DataClasses.EdmSchemaAttribute()]

// Original file name:
// Generation date: 19/09/2009 17:04:54
namespace OpenTibiaXna.OTServer.Entities
{
    
    /// <summary>
    /// There are no comments for MySQLEntities in the schema.
    /// </summary>
    public partial class MySQLEntities : global::System.Data.Objects.ObjectContext
    {
        /// <summary>
        /// Initializes a new MySQLEntities object using the connection string found in the 'MySQLEntities' section of the application configuration file.
        /// </summary>
        public MySQLEntities() : 
                base("name=MySQLEntities", "MySQLEntities")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new MySQLEntities object.
        /// </summary>
        public MySQLEntities(string connectionString) : 
                base(connectionString, "MySQLEntities")
        {
            this.OnContextCreated();
        }
        /// <summary>
        /// Initialize a new MySQLEntities object.
        /// </summary>
        public MySQLEntities(global::System.Data.EntityClient.EntityConnection connection) : 
                base(connection, "MySQLEntities")
        {
            this.OnContextCreated();
        }
        partial void OnContextCreated();
    }
}
