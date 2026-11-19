using QuakeNote.Reports.Core.Abstractions;

namespace QuakeNote.Reports.Core.Infrastructure;

/// <summary>Редактировать можно в течение 15 минут после создания отчёта.</summary>
public sealed class FifteenMinuteEditWindow(TimeProvider time) : IEditWindowPolicy
{
    public TimeSpan Window { get; } = TimeSpan.FromMinutes(15);

    public bool IsOpen(DateTime createdAtUtc)
        => time.GetUtcNow().UtcDateTime - createdAtUtc <= Window;
}