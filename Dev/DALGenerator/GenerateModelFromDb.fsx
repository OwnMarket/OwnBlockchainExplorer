#I __SOURCE_DIRECTORY__
#r "Npgsql.dll"

open System
open System.Data
open System.IO
open System.Text
open Npgsql
open NpgsqlTypes

////////////////////////////////////////////////////////////////////////////////////////////////////
// Variables
////////////////////////////////////////////////////////////////////////////////////////////////////
let server = "localhost"
let username = "postgres"
let password = "postgres"
let database = "own_blockchain_explorer"
let schema = "own"
let entitiesNamespace = "Own.BlockchainExplorer.Core.Models"
let entitiesOutputDir = Path.Combine (__SOURCE_DIRECTORY__, "../../Source/Own.BlockchainExplorer.Core/Models")
let dbContextNamespace = "Own.BlockchainExplorer.Infrastructure.Data.EF"
let dbContextOutputDir = Path.Combine (__SOURCE_DIRECTORY__, "../../Source/Own.BlockchainExplorer.Infrastructure/Data/EF")
let dbContextName = "OwnDb"
let ignoredTables =
    [
        "database_version"
    ]

////////////////////////////////////////////////////////////////////////////////////////////////////
// Types
////////////////////////////////////////////////////////////////////////////////////////////////////
type TableInfo =
    {
        Id : int
        Name : string
        Schema : string
    }

type ColumnInfo =
    {
        TableName : string
        ColumnName : string
        OrdinalPosition : int
        IsNullable : bool
        DataType : string
        IsAutoIncrement : bool
    }

type PrimaryKeyInfo =
    {
        TableId : int
        ColumnName : string
    }

type ForeignKeyInfo =
    {
        TableName : string
        ColumnName : string
        IsNullable : bool
        ForeignTableName : string
        ForeignColumnName : string
    }

type DbSchema =
    {
        Tables : TableInfo list
        Columns : ColumnInfo list
        PrimaryKeys : PrimaryKeyInfo list
        ForeignKeys : ForeignKeyInfo list
    }

////////////////////////////////////////////////////////////////////////////////////////////////////
// DB Schema
////////////////////////////////////////////////////////////////////////////////////////////////////
let dbSchema =
    let getTables (conn : NpgsqlConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <- sprintf """
            SELECT
                table_name,
                CAST(table_name::regclass AS int) AS table_id,
                table_schema
            FROM information_schema.tables
            WHERE table_schema = '%s'
            """ schema

        use rdr = cmd.ExecuteReader()
        seq {
            while rdr.Read() do
                yield {
                    Id = Convert.ToInt32(rdr.["table_id"])
                    Name = string rdr.["table_name"]
                    Schema = string rdr.["table_schema"]
                }
        }
        |> Seq.filter (fun t -> ignoredTables |> Seq.contains t.Name |> not)
        |> Seq.toList

    let getColumns (conn : NpgsqlConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <- sprintf """
            SELECT
                table_name,
                column_name,
                ordinal_position,
                is_nullable,
                --data_type,
                udt_name,
                column_default
            FROM information_schema.columns
            WHERE table_schema = '%s'
            """ schema
        use rdr = cmd.ExecuteReader()
        seq {
            while rdr.Read() do
                yield {
                    TableName = string rdr.["table_name"]
                    ColumnName = string rdr.["column_name"]
                    OrdinalPosition = Convert.ToInt32(rdr.["ordinal_position"])
                    IsNullable =
                        match string rdr.["is_nullable"] with
                        | "YES" -> true
                        | "NO" -> false
                        | _ -> failwith "Unknown value for is_nullable"
                    DataType = string rdr.["udt_name"]
                    IsAutoIncrement = (string rdr.["column_default"]).Contains("nextval")
                }
        }
        |> Seq.toList

    let getPrimaryKeys (conn : NpgsqlConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <- sprintf """
	    	SELECT
                i.indrelid AS table_id,
                CAST(a.attname AS text) AS column_name
            FROM pg_index AS i
            JOIN pg_attribute AS a
                ON a.attrelid = i.indrelid
                AND a.attnum = ANY(i.indkey)
            WHERE i.indisprimary
            AND i.indrelid IN (
                SELECT CAST(table_name::regclass AS int) AS table_id
                FROM information_schema.tables
                WHERE table_type <> 'VIEW'
				AND table_schema = '%s'
            )
			UNION
			SELECT
				CAST(table_name::regclass AS int) AS table_id,
				table_name || '_id' AS column_name
			FROM information_schema.tables
			WHERE table_schema = '%s'
			AND table_type = 'VIEW'
            """ schema schema
        use rdr = cmd.ExecuteReader()
        seq {
            while rdr.Read() do
                yield {
                    TableId = Convert.ToInt32(rdr.["table_id"])
                    ColumnName = string rdr.["column_name"]
                }
        }
        |> Seq.toList

    let getForeignKeys (conn : NpgsqlConnection) isNullable =
        use cmd = conn.CreateCommand()
        cmd.CommandText <- """
            SELECT
                tc.table_name,
                kcu.column_name,
                ccu.table_name AS foreign_table_name,
                ccu.column_name AS foreign_column_name
            FROM
                information_schema.table_constraints AS tc
                JOIN information_schema.key_column_usage AS kcu
                  ON tc.constraint_name = kcu.constraint_name
                JOIN information_schema.constraint_column_usage AS ccu
                  ON ccu.constraint_name = tc.constraint_name
            WHERE constraint_type = 'FOREIGN KEY'
            """
        use rdr = cmd.ExecuteReader()
        seq {
            while rdr.Read() do
                let tableName = string rdr.["table_name"]
                let columnName = string rdr.["column_name"]

                yield {
                    TableName = tableName
                    ColumnName = columnName
                    IsNullable = isNullable tableName columnName
                    ForeignTableName = string rdr.["foreign_table_name"]
                    ForeignColumnName = string rdr.["foreign_column_name"]
                }
        }
        |> Seq.toList

    let isNullable (columns : ColumnInfo seq) tableName columnName =
        columns
        |> Seq.exists (fun c ->
            c.TableName = tableName
            && c.ColumnName = columnName
            && c.IsNullable)

    let cb =
        new NpgsqlConnectionStringBuilder(
            Host = server,
            Username = username,
            Password = password,
            Database = database,
            SearchPath = schema + ", public")

    use conn = new NpgsqlConnection(cb.ConnectionString)
    conn.Open()

    let tables = getTables conn
    let columns = getColumns conn
    let primaryKeys = getPrimaryKeys conn
    let foreignKeys = getForeignKeys conn (isNullable columns)
    {
        Tables = tables
        Columns = columns
        PrimaryKeys = primaryKeys
        ForeignKeys = foreignKeys
    }

////////////////////////////////////////////////////////////////////////////////////////////////////
// Helpers
////////////////////////////////////////////////////////////////////////////////////////////////////
let dbToPascalCase (str : string) =
    let firstUpper (s : string) = s.[0].ToString().ToUpper() + s.Substring(1)
    (str.Split('_'))
    |> Seq.map firstUpper
    |> (fun parts -> String.Join("", parts))

let toCamelCase (str : string) = str.[0].ToString().ToLower() + str.Substring(1)
let dbToCamelCase (str : string) = str |> dbToPascalCase |> toCamelCase

let dbToCodeDataType (dbType : string) isNullable =
    let codeType =
        match dbType with
        | "text" -> "string"
        | "varchar" -> "string"
        | "json" -> "string"
        | "jsonb" -> "string"
        | "bool" -> "bool"
        | "int2" -> "short"
        | "int4" -> "int"
        | "int8" -> "long"
        | "numeric" -> "decimal"
        | "decimal" -> "decimal"
        | "date" -> "DateTime"
        | "time" -> "TimeSpan"
        | "timestamp" -> "DateTime"
        | "uuid" -> "Guid"
        | "bytea" -> "byte[]"
        | _ -> failwithf "Unknown type: %s" dbType

    if isNullable && (codeType <> "string") && (codeType <> "byte[]")
    then codeType + "?"
    else codeType

let trim (s : string) = s.Trim()
let trimEnd (s : string) = s.TrimEnd()
let indent (levels : int) (lines : string seq) = lines |> Seq.map (fun l -> "".PadLeft(levels * 4, ' ') + l)
let joinLines (props : string seq) = String.Join(Environment.NewLine, props)

let pluralize (s : string) =
    let vowels = ["a"; "e"; "i"; "o"; "u"]
    let nextToLast = s.Substring(s.Length - 2, 1)
    if s.EndsWith("y") && (vowels |> List.contains nextToLast |> not) then
        s.TrimEnd('y') + "ies"
    elif s.EndsWith("ss") then
        s + "es"
    else
        s + "s"

let getOrdinalPosition columnName tableName =
    let column =
        dbSchema.Columns
        |> Seq.find (fun c -> c.TableName = tableName && c.ColumnName = columnName)
    column.OrdinalPosition

let entityName tableName = dbToPascalCase tableName
let entitySetName tableName = entityName tableName |> pluralize

let fkEntity (fkColumnName : string) =
    if fkColumnName.EndsWith("Id")
    then fkColumnName.Substring(0, fkColumnName.Length - 2)
    else fkColumnName + "Entity"

////////////////////////////////////////////////////////////////////////////////////////////////////
// Models
////////////////////////////////////////////////////////////////////////////////////////////////////
let generateEntity tableInfo =
    let generateProperties () =
        let generateProperty columnInfo =
            let codeDataType = dbToCodeDataType columnInfo.DataType columnInfo.IsNullable
            let propertyName = dbToPascalCase columnInfo.ColumnName
            sprintf "public %s %s { get; set; }" codeDataType propertyName

        dbSchema.Columns
        |> Seq.filter (fun c -> c.TableName = tableInfo.Name)
        |> Seq.sortBy (fun c -> c.OrdinalPosition)
        |> Seq.map generateProperty
        |> Seq.toList

    let generateNavigationProperties () =
        let generateNavigationProperty fkInfo =
            let codeDataType = dbToPascalCase fkInfo.ForeignTableName
            let propertyName =
                dbToPascalCase fkInfo.ColumnName
                |> fkEntity
            sprintf "public virtual %s %s { get; set; }" codeDataType propertyName

        dbSchema.ForeignKeys
        |> Seq.filter (fun fk -> fk.TableName = tableInfo.Name)
        |> Seq.sortBy (fun fk -> getOrdinalPosition fk.ColumnName fk.TableName)
        |> Seq.map generateNavigationProperty
        |> Seq.toList

    let generateCollectionNavigationProperties () =
        let generateNavigationProperty fkInfo =
            let codeDataType = dbToPascalCase fkInfo.TableName
            let propertyName =
                sprintf "%sBy%s" <| entitySetName fkInfo.TableName <| dbToPascalCase fkInfo.ColumnName
            let property = sprintf "public virtual ICollection<%s> %s { get; set; }" codeDataType propertyName
            (propertyName, codeDataType, property)

        dbSchema.ForeignKeys
        |> Seq.filter (fun fk -> fk.ForeignTableName = tableInfo.Name)
        |> Seq.sortBy (fun fk -> fk.TableName, getOrdinalPosition fk.ColumnName fk.TableName)
        |> Seq.map generateNavigationProperty
        |> Seq.toList

    let collectionNavigationProps = generateCollectionNavigationProperties ()
    let propsCode =
        generateProperties ()
        @ [""]  // One line between simple and navigation properties
        @ generateNavigationProperties ()
        @ (collectionNavigationProps |> List.map (fun (_, _, p) -> p))
        |> indent 2
        |> joinLines
        |> trim

    let constructorCode =
        collectionNavigationProps
        |> List.map (fun (n, t, _) -> sprintf "this.%s = new HashSet<%s>();" n t)
        |> indent 3
        |> joinLines
        |> trim

    let entityTemplate = """
////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace {0}
{{
    public partial class {1}
    {{
        {2}

        public {1}()
        {{
            {3}
        }}
    }}
}}
"""

    let entityName = dbToPascalCase tableInfo.Name
    let entityCode =
        String.Format(entityTemplate,
            entitiesNamespace,
            entityName,
            propsCode,
            constructorCode)
        |> trim

    (entityName, entityCode)

////////////////////////////////////////////////////////////////////////////////////////////////////
// DbContext
////////////////////////////////////////////////////////////////////////////////////////////////////
let dbContextCode =
    let dbSets =
        dbSchema.Tables
        |> List.sortBy (fun t -> t.Name)
        |> List.map (fun t ->
            String.Format("public virtual DbSet<{0}> {1} {{ get; set; }}", entityName t.Name, entitySetName t.Name))
        |> indent 2
        |> joinLines
        |> trim

    let entityConfigs =
        let entityConfig table =
            let primaryKeyColumnName =
                dbSchema.PrimaryKeys
                |> Seq.find (fun t -> t.TableId = table.Id)
                |> fun k -> dbToPascalCase k.ColumnName

            let columnConfigs =
                let columnConfig (column : ColumnInfo) =
                    [
                        String.Format("{0}.Property(e => e.{1})",
                            dbToCamelCase column.TableName,
                            dbToPascalCase column.ColumnName)
                        String.Format("""    .HasColumnName("{0}")""", column.ColumnName)
                        (if column.DataType = "json" || column.DataType = "jsonb"
                            then (sprintf "    .HasColumnType(\"%s\")" column.DataType)
                            else "")
                        (if column.IsNullable
                            then ""
                            else "    .IsRequired()")
                        (if column.IsAutoIncrement
                            then "    .ValueGeneratedOnAdd()"
                            else "")
                    ]
                    |> List.fold (fun (acc : string) l ->
                        match l with
                        | "" -> acc
                        | _ -> acc.TrimEnd() + Environment.NewLine + l
                        ) ""
                    |> trim
                    |> fun c -> c + ";"

                dbSchema.Columns
                |> Seq.filter (fun c -> c.TableName = table.Name)
                |> Seq.map columnConfig
                |> joinLines

            let relationConfigs =
                let relationConfig (fkInfo : ForeignKeyInfo) =
                    let setName =
                        sprintf "%sBy%s" <| entitySetName fkInfo.TableName <| dbToPascalCase fkInfo.ColumnName

                    [
                        String.Format("{0}.HasOne(e => e.{1})",
                            dbToCamelCase fkInfo.TableName,
                            fkEntity <| dbToPascalCase fkInfo.ColumnName)
                        String.Format("    .WithMany(e => e.{0})", setName)
                        (if fkInfo.IsNullable then "" else String.Format("    .IsRequired()"))
                        String.Format("    .HasForeignKey(e => e.{0});", dbToPascalCase fkInfo.ColumnName)
                    ]
                    |> List.except [""]
                    |> joinLines

                dbSchema.ForeignKeys
                |> Seq.filter (fun fk -> fk.TableName = table.Name)
                |> Seq.sortBy (fun fk -> fk.TableName, getOrdinalPosition fk.ColumnName fk.TableName)
                |> Seq.map (relationConfig >> trim)
                |> joinLines

            let entityConfigTemplate = """
// {0}
var {1} = modelBuilder.Entity<{0}>()
    .ToTable("{3}", "{2}");
{1}.HasKey(c => c.{4});
{5}
{6}
"""
            String.Format(entityConfigTemplate,
                dbToPascalCase table.Name,
                dbToCamelCase table.Name,
                table.Schema,
                table.Name,
                primaryKeyColumnName,
                columnConfigs,
                relationConfigs)
            |> trim
            |> fun t -> t + Environment.NewLine

        dbSchema.Tables
        |> List.sortBy (fun t -> t.Name)
        |> List.map entityConfig
        |> List.collect (fun code -> code.Split('\n') |> Array.toList)
        |> List.map trimEnd
        |> indent 3
        |> joinLines
        |> trim

    let dbContextTemplate = """
////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using {0};

namespace {1}
{{
    public partial class {2} : DbContext
    {{
        {3}

        private void ConfigureEntities(ModelBuilder modelBuilder)
        {{
            {4}
        }}
    }}
}}
"""

    String.Format(dbContextTemplate,
        entitiesNamespace, dbContextNamespace, dbContextName, dbSets, entityConfigs).Trim()

////////////////////////////////////////////////////////////////////////////////////////////////////
// Write to files
////////////////////////////////////////////////////////////////////////////////////////////////////
let writeEntityFile dir (name, code) =
    let filePath = Path.Combine(dir, name + ".cs")
    File.WriteAllText(filePath, code)

dbSchema.Tables
|> Seq.map generateEntity
|> Seq.iter (writeEntityFile entitiesOutputDir)

(dbContextName, dbContextCode)
|> writeEntityFile dbContextOutputDir
