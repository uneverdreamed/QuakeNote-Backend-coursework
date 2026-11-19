namespace QuakeNote.Reports.Core.Abstractions;

/// <summary>Правило «можно ли ещё редактировать отчёт».</summary>
public interface IEditWindowPolicy
{
    TimeSpan Window { get; }
    bool IsOpen(DateTime createdAtUtc);
}