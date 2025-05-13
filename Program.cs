// /opt/madden_api/src/Program.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// (Опционально) Swagger для отладки
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// (Опционально) включаем Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Madden API v1"));

// Проверочный GET /
app.MapGet("/", () => Results.Text("Madden Companion Export API is up", "text/plain"));

// Корневая папка для данных (../Data относительно publish/)
const string DataRoot = "../Data";

// Функция для построения пути к файлу
static string GetPath(string username, string platform, int league, string folder, string filename)
{
    var dir = Path.Combine(DataRoot, username, platform, league.ToString(), folder);
    if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
    return Path.Combine(dir, filename);
}

// Запись JSON-файла и ответ { success: true }
static IResult WriteData(
    ILogger logger,
    string endpoint,
    string username,
    string platform,
    int league,
    string directory,
    string filename,
    JsonElement data)
{
    logger.LogInformation("{stamp:yyyy-MM-dd hh:mm:ss tt} Called {endpoint}",
        DateTime.Now, endpoint);

    var path = GetPath(username, platform, league, directory, filename);
    File.WriteAllText(path, data.GetRawText());

    return Results.Json(new { success = true });
}

// Преобразование JSON-массива в CSV
static IResult JsonArrayToCsv(string path, string arrayPropName)
{
    if (!File.Exists(path))
        return Results.NotFound();

    var json = File.ReadAllText(path);
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    JsonElement array;
    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(arrayPropName, out var arr))
        array = arr;
    else if (root.ValueKind == JsonValueKind.Array)
        array = root;
    else
        return Results.BadRequest($"JSON does not contain '{arrayPropName}' array");

    if (array.GetArrayLength() == 0)
        return Results.Text(string.Empty, "text/csv");

    // Заголовки из первого объекта
    var first = array.EnumerateArray().First();
    var headers = first.EnumerateObject().Select(p => p.Name).ToList();

    var sb = new StringBuilder();
    // Шапка CSV
    sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

    // Строки CSV
    foreach (var elem in array.EnumerateArray())
    {
        var row = headers.Select(h =>
        {
            if (elem.TryGetProperty(h, out var v))
            {
                return v.ValueKind switch
                {
                    JsonValueKind.String  => $"\"{v.GetString().Replace("\"", "\"\"")}\"",
                    JsonValueKind.Number  => v.GetRawText(),
                    JsonValueKind.True    => "true",
                    JsonValueKind.False   => "false",
                    _                     => $"\"{v.GetRawText().Replace("\"", "\"\"")}\""
                };
            }
            return "\"\"";
        });
        sb.AppendLine(string.Join(",", row));
    }

    return Results.Text(sb.ToString(), "text/csv");
}

// --- POST-эндпойнты Companion App ---

app.MapPost("/{username}/{platform}/{league:int}/leagueteams",
    (string username, string platform, int league, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, "leagueteams", username, platform, league, "", "teams.json", data));

app.MapPost("/{username}/{platform}/{league:int}/standings",
    (string username, string platform, int league, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, "standings", username, platform, league, "", "standings.json", data));

app.MapPost("/{username}/{platform}/{league:int}/freeagents/roster",
    (string username, string platform, int league, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, "freeagents", username, platform, league, "", "freeagents.json", data));

app.MapPost("/{username}/{platform}/{league:int}/team/{team:int}/roster",
    (string username, string platform, int league, int team, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, "team-roster", username, platform, league, "rosters", $"roster-{team}.json", data));

app.MapPost("/{username}/{platform}/{league:int}/week/{stage}/{week:int}/schedules",
    (string username, string platform, int league, string stage, int week, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, "schedules", username, platform, league, "schedules", $"{stage}-week-{week}-schedule.json", data));

app.MapPost("/{username}/{platform}/{league:int}/week/{stage}/{week:int}/{stat}",
    (string username, string platform, int league, string stage, int week, string stat, JsonElement data, ILogger<Program> logger) =>
        WriteData(logger, stat, username, platform, league, Path.Combine("stats", stat), $"{stage}-week-{week}-{stat}.json", data));

// --- НОВЫЕ GET-эндпойнты для CSV ---

// CSV League Teams
app.MapGet("/{username}/{platform}/{league:int}/csv/teams", (string username, string platform, int league) =>
{
    var path = Path.Combine(DataRoot, username, platform, league.ToString(), "teams.json");
    return JsonArrayToCsv(path, "leagueTeamInfoList");
});

// CSV Freeagents Roster
app.MapGet("/{username}/{platform}/{league:int}/csv/freeagents", (string username, string platform, int league) =>
{
    var path = Path.Combine(DataRoot, username, platform, league.ToString(), "freeagents.json");
    return JsonArrayToCsv(path, "rosterInfoList");
});

app.Run();
