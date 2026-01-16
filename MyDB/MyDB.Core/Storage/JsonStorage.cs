using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MyDB.Core.Storage
{
    public static class JsonStorage
    {
        private static readonly string DataFolder = Path.Combine(AppContext.BaseDirectory, "data");

        static JsonStorage()
        {
            // Ensure the data folder exists
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
        }

        // Save a table's rows to JSON
        public static void SaveTable(string tableName, List<Dictionary<string, object>> rows)
        {
            string path = Path.Combine(DataFolder, $"{tableName}.json");
            string json = JsonSerializer.Serialize(rows, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
        }

        // Load a table's rows from JSON
        public static List<Dictionary<string, object>> LoadTable(string tableName)
        {
            string path = Path.Combine(DataFolder, $"{tableName}.json");
            if (!File.Exists(path))
                return new List<Dictionary<string, object>>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json)
                   ?? new List<Dictionary<string, object>>();
        }

        // Save database schema (list of tables)
        public static void SaveSchema(List<Table> tables)
        {
            string path = Path.Combine(DataFolder, "schema.json");
            string json = JsonSerializer.Serialize(tables, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
        }

        // Load database schema
        public static List<Table> LoadSchema()
        {
            string path = Path.Combine(DataFolder, "schema.json");
            if (!File.Exists(path))
                return new List<Table>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Table>>(json)
                   ?? new List<Table>();
        }
    }
}
