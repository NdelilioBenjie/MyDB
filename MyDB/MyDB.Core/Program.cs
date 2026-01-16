using System;
using System.Collections.Generic;
using MyDB.Common;
using MyDB.Core.Tokenizer;
using MyDB.Core.Parser;
using MyDB.Core.Storage;
using MyDB.Core.AST;
using MyDB.Core.AST.Statements;
using MyDB.Core.AST.Expressions;

class Program
{
    static void Main()
    {
        var db = new Database(); // our in-memory DB
        Console.WriteLine("Welcome to MyDB REPL. Type 'exit' to quit.");

        while (true)
        {
            Console.Write("\nMyDB> ");
            string? line = Console.ReadLine(); // nullable to avoid warning
            if (line == null || line.Trim().ToLower() == "exit")
                break;

            line = line.Trim();
            if (line == "") continue;

            try
            {
                // Tokenize
                var tokenizer = new Tokenizer(line);
                var tokens = tokenizer.Tokenize();

                // Show tokens (optional, can comment out later)
                Console.WriteLine("\n--- Tokens ---");
                foreach (var t in tokens)
                    Console.WriteLine($"{t.Type} : {t.Lexeme}");

                // Parse
                var parser = new Parser(tokens);
                var stmt = parser.ParseStatement();

                // Execute
                switch (stmt)
                {
                    case CreateTableStatement createStmt:
                        db.CreateTable(createStmt);
                        Console.WriteLine($"\nTable '{createStmt.TableName}' created successfully.");
                        break;

                    case InsertStatement insertStmt:
                        db.Insert(insertStmt);
                        Console.WriteLine($"\nInserted row into table '{insertStmt.TableName}'.");
                        break;

                    case UpdateStatement updateStmt:
                        int updated = db.Update(updateStmt);
                        Console.WriteLine($"\nUpdated {updated} row(s) in table '{updateStmt.TableName}'.");
                        break;

                    case DeleteStatement deleteStmt:
                        int deleted = db.Delete(deleteStmt);
                        Console.WriteLine($"\nDeleted {deleted} row(s) from table '{deleteStmt.TableName}'.");
                        break;

                    case SelectStatement selectStmt:
                        var results = db.Select(selectStmt);
                        Console.WriteLine($"\nSELECT results for table '{selectStmt.TableName}':");
                        foreach (var row in results)
                        {
                            foreach (var kvp in row)
                                Console.Write($"{kvp.Key}={kvp.Value} ");
                            Console.WriteLine();
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown statement type.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Exiting MyDB REPL. Goodbye!");
    }
}
