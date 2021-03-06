﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arebis.Data;
using Arebis.Data.Properties;

namespace Arebis.Data.Model
{
    public class SqlModelBuilder : Arebis.Data.Model.IModelBuilder
    {
        public DatabaseModel Build(DbConnection connection)
        {
            var model = CreateDatabaseModel();

            using (var cmd = connection.CreateCommand(MSSqlModelQueries.GetDatabase))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    model.Name = reader.GetStringOrNull(0);
                    model.Version = reader.GetByte(1);
                    model.ExtraData["Collation_Name"] = reader.GetStringOrNull(2);
                }
            }

            using (var cmd = connection.CreateCommand((MSSqlModelQueries.GetModelTables)))
            using (var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    if (reader.GetString(2) == "VIEW")
                    {
                        var view = CreateModelView(model);
                        view.Schema = reader.GetString(0);
                        view.Name = reader.GetString(1);
                        model.Tables.Add(view);
                    }
                    else
                    {
                        var table = CreateModelTable(model);
                        table.Schema = reader.GetString(0);
                        table.Name = reader.GetString(1);
                        model.Tables.Add(table);
                    }
                }
            }

            using (var cmd = connection.CreateCommand((MSSqlModelQueries.GetModelColumns)))
            using (var reader = cmd.ExecuteReader())
            {
                var lastTable = (ModelTable)null;

                while (reader.Read())
                {
                    var tableSchema = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    if (lastTable == null || lastTable.Schema.Equals(tableSchema, StringComparison.OrdinalIgnoreCase) == false || lastTable.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        lastTable = model.GetTableOrView(tableSchema, tableName);
                        if (lastTable == null) continue;
                    }

                    var column = this.CreateModelColumn(lastTable);
                    column.Name = reader.GetString(2);
                    column.TypeName = reader.GetStringOrNull(4);
                    column.CharacterLength = reader.GetInt32OrNull(6);
                    column.NumericPrecision = reader.GetByte(7);
                    column.NumericScale = reader.GetByte(8);
                    column.IsNullable = reader.GetBoolean(12);
                    column.IsAutoGenerated = reader.GetBoolean(13);
                    column.IsComputed = reader.GetBoolean(14);
                    #region column.DbType =, column.NetType =
                    switch (column.TypeName)
                    { 
                        case "bigint":
                            column.DbType = System.Data.SqlDbType.BigInt;
                            column.NetType = typeof(Int64);
                            break;
                        case "binary":
                            column.DbType = System.Data.SqlDbType.Binary;
                            column.NetType = typeof(byte[]);
                            break;
                        case "bit":
                            column.DbType = System.Data.SqlDbType.Bit;
                            column.NetType = typeof(bool);
                            break;
                        case "char":
                            column.DbType = System.Data.SqlDbType.Char;
                            column.NetType = typeof(string);
                            break;
                        case "date":
                            column.DbType = System.Data.SqlDbType.Date;
                            column.NetType = typeof(DateTime);
                            break;
                        case "datetime":
                            column.DbType = System.Data.SqlDbType.DateTime;
                            column.NetType = typeof(DateTime);
                            break;
                        case "datetime2":
                            column.DbType = System.Data.SqlDbType.DateTime2;
                            column.NetType = typeof(DateTime);
                            break;
                        case "datetimeoffset":
                            column.DbType = System.Data.SqlDbType.DateTimeOffset;
                            column.NetType = typeof(DateTimeOffset);
                            break;
                        case "decimal":
                            column.DbType = System.Data.SqlDbType.Decimal;
                            column.NetType = typeof(decimal);
                            break;
                        case "float":
                            column.DbType = System.Data.SqlDbType.Float;
                            column.NetType = typeof(Single);
                            break;
                        //case "geography":
                        //    column.DbType = System.Data.SqlDbType.Udt;
                        //    column.NetType = typeof(object);
                        //    break;
                        //case "geometry":
                        //    column.DbType = System.Data.SqlDbType.Udt;
                        //    column.NetType = typeof(object);
                        //    break;
                        //case "hierarchyid":
                        //    column.DbType = System.Data.SqlDbType.Udt;
                        //    column.NetType = typeof(object);
                        //    break;
                        case "image":
                            column.DbType = System.Data.SqlDbType.Image;
                            column.NetType = typeof(byte[]);
                            break;
                        case "int":
                            column.DbType = System.Data.SqlDbType.Int;
                            column.NetType = typeof(Int32);
                            break;
                        case "money":
                            column.DbType = System.Data.SqlDbType.Money;
                            column.NetType = typeof(decimal);
                            break;
                        case "nchar":
                            column.DbType = System.Data.SqlDbType.NChar;
                            column.NetType = typeof(string);
                            break;
                        case "ntext":
                            column.DbType = System.Data.SqlDbType.NText;
                            column.NetType = typeof(string);
                            break;
                        case "numeric":
                            column.DbType = System.Data.SqlDbType.Real;
                            column.NetType = typeof(double);
                            break;
                        case "nvarchar":
                            column.DbType = System.Data.SqlDbType.NVarChar;
                            column.NetType = typeof(string);
                            break;
                        case "real":
                            column.DbType = System.Data.SqlDbType.Real;
                            column.NetType = typeof(double);
                            break;
                        case "smalldatetime":
                            column.DbType = System.Data.SqlDbType.SmallDateTime;
                            column.NetType = typeof(DateTime);
                            break;
                        case "smallint":
                            column.DbType = System.Data.SqlDbType.SmallInt;
                            column.NetType = typeof(Int16);
                            break;
                        case "smallmoney":
                            column.DbType = System.Data.SqlDbType.SmallMoney;
                            column.NetType = typeof(decimal);
                            break;
                        //case "sql_variant":
                        //    column.DbType = System.Data.SqlDbType.Udt;
                        //    column.NetType = typeof(object);
                        //    break;
                        case "sysname":
                            column.DbType = System.Data.SqlDbType.NVarChar;
                            column.NetType = typeof(string);
                            break;
                        case "text":
                            column.DbType = System.Data.SqlDbType.Text;
                            column.NetType = typeof(string);
                            break;
                        case "time":
                            column.DbType = System.Data.SqlDbType.Time;
                            column.NetType = typeof(TimeSpan);
                            break;
                        case "timestamp":
                            column.DbType = System.Data.SqlDbType.Timestamp;
                            column.NetType = typeof(DateTime);
                            break;
                        case "tinyint":
                            column.DbType = System.Data.SqlDbType.TinyInt;
                            column.NetType = typeof(byte);
                            break;
                        case "uniqueidentifier":
                            column.DbType = System.Data.SqlDbType.UniqueIdentifier;
                            column.NetType = typeof(Guid);
                            break;
                        case "varbinary":
                            column.DbType = System.Data.SqlDbType.VarBinary;
                            column.NetType = typeof(byte[]);
                            break;
                        case "varchar":
                            column.DbType = System.Data.SqlDbType.VarChar;
                            column.NetType = typeof(string);
                            break;
                        case "xml":
                            column.DbType = System.Data.SqlDbType.Xml;
                            column.NetType = typeof(string);
                            break;
                        default:
                            column.DbType = System.Data.SqlDbType.Udt;
                            column.NetType = typeof(Object);
                            break;
                    }
                    #endregion
                    lastTable.Columns.Add(column);
                }
            }

            using (var cmd = connection.CreateCommand((MSSqlModelQueries.GetModelIndexes)))
            using (var reader = cmd.ExecuteReader())
            {
                var lastTable = (ModelTable)null;
                var lastIndex = (ModelIndex)null;

                while (reader.Read())
                {
                    var tableSchema = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    var indexName = reader.GetString(2);
                    if (lastTable == null || lastTable.Schema.Equals(tableSchema, StringComparison.OrdinalIgnoreCase) == false || lastTable.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        lastTable = model.GetTableOrView(tableSchema, tableName);
                        lastIndex = null;
                        if (lastTable == null) continue;
                    }
                    if (lastIndex == null || lastIndex.Name.Equals(indexName, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        lastIndex = this.CreateModelIndex(lastTable);
                        lastIndex.Name = indexName;
                        lastIndex.IsUnique = reader.GetBoolean(3);
                        lastIndex.IsPrimary = reader.GetBoolean(4);
                        lastTable.Indexes.Add(lastIndex);
                    }

                    var column = lastTable.GetColumn(reader.GetString(5));
                    if (column != null)
                    {
                        lastIndex.Columns.Add(column);
                        if (lastIndex.IsPrimary) column.IsPrimaryKey = true;
                    }
                }
            }

            using (var cmd = connection.CreateCommand((MSSqlModelQueries.GetModelRelations)))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var lastRelation = (ModelRelation)null;
                    while (reader.Read())
                    {
                        var relationSchema = reader.GetString(0);
                        var relationName = reader.GetString(1);
                        if (lastRelation == null || lastRelation.Schema.Equals(relationSchema, StringComparison.OrdinalIgnoreCase) == false || lastRelation.Name.Equals(relationName, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            lastRelation = this.CreateModelRelation(model);
                            lastRelation.Schema = relationSchema;
                            lastRelation.Name = relationName;
                            lastRelation.PrimaryTable = model.GetTableOrView(reader.GetString(3), reader.GetString(4));
                            lastRelation.ForeignTable = model.GetTableOrView(reader.GetString(7), reader.GetString(8));
                            model.Relations.Add(lastRelation);
                        }

                        if (lastRelation.PrimaryTable != null)
                        {
                            var col = lastRelation.PrimaryTable.GetColumn(reader.GetString(5));
                            if (col != null) lastRelation.PrimaryColumns.Add(col);
                        }

                        if (lastRelation.ForeignTable != null)
                        {
                            var col = lastRelation.ForeignTable.GetColumn(reader.GetString(9));
                            if (col != null) lastRelation.ForeignColumns.Add(col);
                        }
                    }
                }
            }

            return model;
        }

        #region Create Model Elements

        protected virtual DatabaseModel CreateDatabaseModel()
        {
            return new DatabaseModel();
        }

        protected virtual ModelTable CreateModelTable(DatabaseModel owner)
        {
            return new ModelTable() { Model = owner };
        }

        protected virtual ModelView CreateModelView(DatabaseModel owner)
        {
            return new ModelView() { Model = owner };
        }

        protected virtual ModelRelation CreateModelRelation(DatabaseModel owner)
        {
            return new ModelRelation() { Model = owner };
        }

        protected virtual ModelColumn CreateModelColumn(ModelTable owner)
        {
            return new ModelColumn() { Table = owner };
        }

        protected virtual ModelIndex CreateModelIndex(ModelTable owner)
        {
            return new ModelIndex() { Table = owner };
        }

        #endregion
    }
}
