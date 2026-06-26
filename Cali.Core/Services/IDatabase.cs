using SQLite;
using Cali.Core.Models;

namespace Cali.Core.Services;

public interface IDatabase
{
    SQLiteAsyncConnection Conn { get; }
    Task InitAsync();
}

public sealed class Database : IDatabase
{
    private Task? _init;
    public SQLiteAsyncConnection Conn { get; }
    public Database(string path) => Conn = new SQLiteAsyncConnection(path);

    // Idempotent: concurrent/repeat callers share a single table-creation task.
    public Task InitAsync() => _init ??= InitCoreAsync();

    private async Task InitCoreAsync()
    {
        await Conn.CreateTableAsync<Plan>();
        await Conn.CreateTableAsync<PlanExercise>();
        await Conn.CreateTableAsync<WorkoutRecord>();
        await Conn.CreateTableAsync<PersonalRecord>();
        await Conn.CreateTableAsync<AchievementUnlock>();
    }
}
