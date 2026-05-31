using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCRM.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace SmartCRM.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly SmartCrmDbContext _context;

    public DatabaseController(SmartCrmDbContext context)
    {
        _context = context;
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        var tables = new List<string>();
        var connection = _context.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
        }
        return Ok(tables);
    }

    [HttpGet("data/{tableName}")]
    public async Task<IActionResult> GetTableData(
        string tableName, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchColumn = null,
        [FromQuery] string? searchQuery = null)
    {
        // 1. Validate table name to prevent SQL injection
        var allowedTables = await GetTablesListInternal();
        if (!allowedTables.Contains(tableName)) return BadRequest("Invalid table name.");

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        var offset = (page - 1) * pageSize;
        var data = new List<IDictionary<string, object?>>();
        var columns = new List<string>();
        long totalCount = 0;

        using (var command = connection.CreateCommand())
        {
            // Count total
            var countSql = $"SELECT COUNT(*) FROM public.\"{tableName}\"";
            if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(searchQuery))
            {
                countSql += $" WHERE CAST(\"{searchColumn}\" AS TEXT) ILIKE @search";
            }
            command.CommandText = countSql;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@search";
                param.Value = $"%{searchQuery}%";
                command.Parameters.Add(param);
            }
            totalCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

            // Fetch data
            command.Parameters.Clear();
            var dataSql = $"SELECT * FROM public.\"{tableName}\"";
            if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(searchQuery))
            {
                dataSql += $" WHERE CAST(\"{searchColumn}\" AS TEXT) ILIKE @search";
                var param = command.CreateParameter();
                param.ParameterName = "@search";
                param.Value = $"%{searchQuery}%";
                command.Parameters.Add(param);
            }
            dataSql += $" ORDER BY 1 LIMIT {pageSize} OFFSET {offset}";
            command.CommandText = dataSql;

            using (var reader = await command.ExecuteReaderAsync())
            {
                // Get column names
                for (int i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));

                while (await reader.ReadAsync())
                {
                    var row = new ExpandoObject() as IDictionary<string, object?>;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var val = reader.GetValue(i);
                        row.Add(reader.GetName(i), val == DBNull.Value ? null : val);
                    }
                    data.Add(row);
                }
            }
        }

        return Ok(new
        {
            Columns = columns,
            Rows = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    private async Task<List<string>> GetTablesListInternal()
    {
        var tables = new List<string>();
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
            }
        }
        return tables;
    }
}
