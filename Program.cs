const string DataPath = @"..\Data";

static string GetPath(string folder, string filename)
{
    var path = Path.Join(DataPath, folder);

    if (Directory.Exists(path) is false)
        Directory.CreateDirectory(path);

    return Path.Join(path, filename);
}
static void WriteData(ILogger logger, string endpoint, string directory, string filename, object data)
{
    logger.LogInformation("{stamp:yyyy-MM-dd hh:mm:ss tt} Called {endpoint}", DateTime.Now, endpoint);

    var path = GetPath(directory, filename);

    using var writer = new StreamWriter(path);

    writer.Write(data);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/pc/{league:int}/freeagents/roster", (int league, object data)                                                   => WriteData(app.Logger, "freeagents", "", "freeagents.json", data));
app.MapPost("/pc/{league:int}/leagueteams", (int league, object data)                                                         => WriteData(app.Logger, "teams", "", "teams.json", data));
app.MapPost("/pc/{league:int}/standings", (int league, object data)                                                           => WriteData(app.Logger, "standings", "", "standings.json", data));
app.MapPost("/pc/{league:int}/team/{team:int}/roster", (int league, int team, object data)                                    => WriteData(app.Logger, "rosters", "rosters", $"roster-{team}.json", data));
app.MapPost("/pc/{league:int}/week/{stage}/{week:int}/{stat}", (int league, string stage, int week, string stat, object data) => WriteData(app.Logger, stat, $"stats/{stat}", $"{stage}-week-{week}-{stat}.json", data));
app.MapPost("/pc/{league:int}/week/{stage}/{week:int}/schedules", (int league, string stage, int week, object data)           => WriteData(app.Logger, "schedules", "schedules", $"{stage}-week-{week}-schedule.json", data));
app.Run();