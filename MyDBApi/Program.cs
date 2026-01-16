using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyDB.Core.Storage;
using MyDB.Core.AST.Statements;
using MyDB.Core.AST.Expressions;
using MyDB.Common;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Add services
builder.Services.AddSingleton<Database>(sp =>
{
    // Path to the data folder inside MyDB.Core
    return new Database(@"..\MyDB\MyDB.Core\data");
});

var app = builder.Build();

/// ----------------------
/// Dynamic GET endpoint: fetch all rows from any table
/// ----------------------
app.MapGet("/{tableName}", (string tableName, Database db) =>
{
    try
    {
        var selectStmt = new SelectStatement(
            columns: new List<Expression> { new ColumnExpression("*") },
            tableName: tableName
        );

        var rows = db.Select(selectStmt);
        return Results.Ok(rows);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

/// ----------------------
/// Dynamic GET endpoint: fetch single row by ID
/// ----------------------
app.MapGet("/{tableName}/{id:int}", (string tableName, int id, Database db) =>
{
    try
    {
        var whereExpr = new BinaryExpression(
            new ColumnExpression("id"),
            TokenType.EQUALS,
            new LiteralExpression(id)
        );

        var selectStmt = new SelectStatement(
            columns: new List<Expression> { new ColumnExpression("*") },
            tableName: tableName,
            whereClause: whereExpr
        );

        var rows = db.Select(selectStmt);
        return rows.Count == 0 ? Results.NotFound() : Results.Ok(rows[0]);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

/// ----------------------
/// Dynamic POST endpoint: insert row into any table
/// ----------------------
app.MapPost("/{tableName}", async (string tableName, Database db, HttpRequest request) =>
{
    try
    {
        var data = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<string, object>>(request.Body);
        if (data == null) return Results.BadRequest("Invalid payload");

        var insertStmt = new InsertStatement(
            tableName,
            new List<string>(data.Keys),
            new List<Expression>()
        );

        foreach (var val in data.Values)
        {
            insertStmt.Values.Add(new LiteralExpression(val));
        }

        db.Insert(insertStmt);

        return Results.Ok(new { message = "Row inserted successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

/// ----------------------
/// Dynamic PUT endpoint: update row(s) by WHERE column
/// ----------------------
app.MapPut("/{tableName}/{id:int}", async (string tableName, int id, Database db, HttpRequest request) =>
{
    try
    {
        var data = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<string, object>>(request.Body);
        if (data == null) return Results.BadRequest("Invalid payload");

        // Build WHERE expression
        var whereExpr = new BinaryExpression(
            new ColumnExpression("id"),
            TokenType.EQUALS,
            new LiteralExpression(id)
        );

        var updateStmt = new UpdateStatement(
            tableName,
            new Dictionary<string, Expression>(),
            whereExpr
        );

        foreach (var kvp in data)
        {
            updateStmt.SetClauses[kvp.Key] = new LiteralExpression(kvp.Value);
        }

        int updated = db.Update(updateStmt);
        return Results.Ok(new { updated });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

/// ----------------------
/// Dynamic DELETE endpoint: delete row(s) by id
/// ----------------------
app.MapDelete("/{tableName}/{id:int}", (string tableName, int id, Database db) =>
{
    try
    {
        var whereExpr = new BinaryExpression(
            new ColumnExpression("id"),
            TokenType.EQUALS,
            new LiteralExpression(id)
        );

        var deleteStmt = new DeleteStatement(
            tableName,
            whereExpr
        );

        int deleted = db.Delete(deleteStmt);
        return Results.Ok(new { deleted });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

// 6️⃣ Run the API
app.Run("http://localhost:5000");
