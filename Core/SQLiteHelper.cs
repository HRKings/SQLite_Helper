using SQLite_Helper.Helper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace SQLite_Helper.Core
{
    public class SQLiteHelper
    {
        /// <summary>
        /// The Database used in the helper
        /// </summary>
        internal SQLiteConnection Database { get; private set; }

        /// <summary>
        /// Initializes an empty helper
        /// </summary>
        public SQLiteHelper()
        {
            this.Database = null;
        }

        /// <summary>
        /// Initializes the helper with the given database or create a new one if required
        /// </summary>
        /// <param name="path">The path of the database</param>
        public SQLiteHelper(string path)
        {
            if (File.Exists(path))
            {
                this.Database = new SQLiteConnection($"Data Source={path};Version=3;");
            }
            else
            {
                ////Logger.Warn($"Database not found in ({path}). Creating a new one");
                this.Database = new SQLiteConnection($"Data Source={path};Version=3;");
            }
        }

        /*internal SQLite_Helper(string path, string password)
        {
            this.Database = new SQLiteConnection($"Data Source={path};Version=3;Password={password};");
        }*/

        /// <summary>
        /// Set the helper database with the given path
        /// </summary>
        /// <param name="path">The path of the database</param>
        public void SetDatabase(string path)
        {
            if (File.Exists(path))
            {
                this.Database = new SQLiteConnection($"Data Source={path};Version=3;");
            }
            else
            {
                ////Logger.Error("Database not found in ({path})");
            }
        }

        /// <summary>
        /// Creates a database file in the given path if it do not exists already
        /// </summary>
        /// <param name="path">The path where the database will be created</param>
        public void CreateDatabase(string path)
        {
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
            else
            {
                ////Logger.Error($"Database file in ({path}) already exists");
            }
        }

        /// <summary>
        /// Executes a single SQL command, using a transaction for rolling back in case of errors
        /// </summary>
        /// <param name="sql">The SQL command to execute</param>
        /// <returns>The number of rows affected</returns>
        public int ExecuteSQL(string sql)
        {
            int rowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(sql, Database, transaction))
                        {
                            rowsAffected = command.ExecuteNonQuery();
                            ////Logger.Database($"{rowsAffected} rows affected from '{sql}'");
                        }

                        transaction.Commit();
                        ////Logger.Database("Commited changes for transaction");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ////Logger.Error($"\'{sql}\' failed, rolling back changes \n Exeption: {e}");
                    }
                }

                Database.Close();
            }
            else
            {
                ////Logger.Error("Database is not set");
            }

            return rowsAffected;
        }

        /// <summary>
        /// Execute a series of SQL commands, using a transaction for rolling back in case of any errors
        /// </summary>
        /// <param name="sqls">The SQL commands to use</param>
        /// <returns>The total of rows affected by the SQL commands</returns>
        public int ExecuteSQLs(params string[] sqls)
        {
            int rowsAffected = 0;
            int totalRowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    foreach (string sql in sqls)
                    {
                        try
                        {
                            using (SQLiteCommand command = new SQLiteCommand(sql, Database, transaction))
                            {
                                rowsAffected = command.ExecuteNonQuery();
                                totalRowsAffected += rowsAffected;
                                ////Logger.Database($"{rowsAffected} rows affected from '{sql}'");
                            }

                            transaction.Commit();
                            ////Logger.Database($"Commited changes for transaction. {sqls.Length} SQL commands executed");
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            ////Logger.Error($"'{sql}\' failed, rolling back changes \n Exeption: {e}");
                        }
                    }
                }

                Database.Close();
            }
            else
            {
                ////Logger.Error("Database is not set");
            }

            return totalRowsAffected;
        }

        /// <summary>
        /// Retrive the columns from the table
        /// </summary>
        /// <param name="table">The table to retrieve</param>
        /// <param name="columns">The columns to retrive from the table</param>
        /// <returns>A dictionary with <string, dynamic> the structure of the string is columnName_rowNumber</returns>
        public Dictionary<string, object> RetrieveTableContent(string table, params string[] columns)
        {
            Dictionary<string, object> results = new Dictionary<string, dynamic>();

            if (Database != null)
            {
                Database.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM {table}", Database))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach (string column in columns)
                            {
                                try
                                {
                                    results.Add($"{column}_{reader.StepCount}", reader[column]);
                                    ////Logger.Database($"Returned: {reader[column]}. Column: {column}. Row: {reader.StepCount}");
                                }
                                catch
                                {
                                    ////Logger.Error($"Column: {column}. Row: {reader.StepCount}. Value: {reader[column]}");
                                }
                            }
                        }
                    }
                }

                Database.Close();
            }

            return results;
        }

        /// <summary>
        /// Retrive the columns from the table
        /// </summary>
        /// <param name="table">The table to retrieve</param>
        /// <param name="condition">The condition to retrive from the table</param>
        /// <param name="columns">The columns to retrive from the table</param>
        /// <returns>A dictionary with <string, dynamic> the structure of the string is columnName_rowNumber</returns>
        public Dictionary<string, object> RetrieveValues(string table, string condition, params string[] columns)
        {
            Dictionary<string, object> results = new Dictionary<string, dynamic>();

            if (Database != null)
            {
                Database.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM {table} WHERE {condition}", Database))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach (string column in columns)
                            {
                                try
                                {
                                    results.Add($"{column}_{reader.StepCount}", reader[column]);
                                    ////Logger.Database($"Returned: {reader[column]}. Column: {column}. Row: {reader.StepCount} ({column}_{reader.StepCount})");
                                }
                                catch
                                {
                                    ////Logger.Error($"Column: {column}. Row: {reader.StepCount}. Value: {reader[column]}");
                                }
                            }
                        }
                    }
                }

                Database.Close();
            }

            return results;
        }

        /// <summary>
        /// Insert into the table and column the value
        /// </summary>
        /// <param name="table">The table to insert</param>
        /// <param name="column">The column to insert</param>
        /// <param name="value">The value to insert</param>
        /// <returns>The number of rows affected</returns>
        public int InsertValue(string table, string column, object value)
        {
            int rowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    string command_text = $"INSERT INTO {table} ({column}) VALUES ({value})";
                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(command_text, Database, transaction))
                        {
                            rowsAffected = command.ExecuteNonQuery();
                            ////Logger.Database($"Inserted: {value}. Table: {table}. Column: {column}");
                        }

                        transaction.Commit();
                        ////Logger.Database($"Commited changes for transaction");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ////Logger.Error($"SQL: {command_text}. failed, rolling back changes \n Exeption: {e}");
                    }
                }

                Database.Close();
            }
            else
            {
                ////Logger.Error("Database is not set");
            }

            return rowsAffected;
        }

        /// <summary>
        /// Insert multiple values into table
        /// </summary>
        /// <param name="table">The table to insert into</param>
        /// <param name="column_value">The column name and value in the format (column, value)</param>
        /// <returns>The number of rows affected</returns>
        public int InsertValues(string table, params (string, object)[] column_value)
        {
            int rowsAffected = 0;
            int totalRowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    (string, string) values = column_value.PairsToString();
                    string command_text = $"INSERT INTO {table} ({values.Item1}) VALUES ({values.Item2})";

                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(command_text, Database, transaction))
                        {
                            rowsAffected = command.ExecuteNonQuery();
                            totalRowsAffected += rowsAffected;
                            ////Logger.Database($"Inserted: {values.Item2}. Table: {table}. Column: {values.Item1}");
                        }

                        transaction.Commit();
                        ////Logger.Database($"Commited changes for transaction. {column_value.Length} values inserted");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ////Logger.Error($"SQL: {command_text}. failed, rolling back changes \n Exeption: {e}");
                    }
                }

                Database.Close();
            }
            else
            {
                ////Logger.Error("Database is not set");
            }

            return rowsAffected;
        }

        /// <summary>
        /// Update a value from a talbe where the condition is met
        /// </summary>
        /// <param name="table">The table to update</param>
        /// <param name="column">The column to update</param>
        /// <param name="condition">The condition to update</param>
        /// <param name="value">The new value of the column</param>
        /// <returns>The number of rows affected</returns>
        public int UpdateValue(string table, string column, string condition, object value)
        {
            int rowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand($"UPDATE {table} SET {column} = {value} WHERE {condition}", Database, transaction))
                        {
                            rowsAffected = command.ExecuteNonQuery();
                            ////Logger.Database($"Updated: {table}. Condition: {condition}. Column: {column} = {value}");
                        }

                        transaction.Commit();
                        ////Logger.Database($"Commited changes for transaction");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ////Logger.Error($"Updated: {table}. Condition: {condition}. Column: {column} = {value}. failed, rolling back changes \n Exeption: {e}");
                    }
                }

                Database.Close();
            }
            else
            {
                ////Logger.Error("Database is not set");
            }

            return rowsAffected;
        }

        /// <summary>
        /// Delete a value from the table
        /// </summary>
        /// <param name="table">The table to delete</param>
        /// <param name="condition">The condition to delete</param>
        /// <returns>The number of rows affected</returns>
        public bool DeleteValue(string table, string condition)
        {
            int rowsAffected = 0;

            if (Database != null)
            {
                Database.Open();

                using (SQLiteTransaction transaction = Database.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand($"DELETE FROM {table} WHERE {condition}", Database, transaction))
                        {
                            rowsAffected = command.ExecuteNonQuery();
                            ////Logger.Database($"Deleted: {table}. Condition: {condition}");
                        }

                        transaction.Commit();
                        //Logger.Database($"Commited changes for transaction");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        //Logger.Error($"Deleted: {table}. Condition: {condition}. failed, rolling back changes \n Exeption: {e}");
                    }
                }

                Database.Close();
            }
            else
            {
                //Logger.Error("Database is not set");
            }

            return rowsAffected > 0;
        }
    }
}
