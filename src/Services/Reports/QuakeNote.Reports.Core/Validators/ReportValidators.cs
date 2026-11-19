using FluentValidation;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Shared.Contracts;

namespace QuakeNote.Reports.Core.Validators;

/// <summary>Правила, общие для создания и редактирования базового отчёта.</summary>
public abstract class ReportInputValidator<T> : AbstractValidator<T> where T : IReportInput
{
    protected ReportInputValidator()
    {
        RuleFor(x => x.Latitude).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите широту.")
            .InclusiveBetween(-90m, 90m).WithMessage("Широта должна быть от -90 до 90.");

        RuleFor(x => x.Longitude).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите долготу.")
            .InclusiveBetween(-180m, 180m).WithMessage("Долгота должна быть от -180 до 180.");

        RuleFor(x => x.FeltIntensity).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите интенсивность.")
            .InclusiveBetween(1, 10).WithMessage("Интенсивность должна быть от 1 до 10.");

        RuleFor(x => x.RegionName)
            .MaximumLength(50).WithMessage("Название региона не длиннее 50 символов.");
    }
}

public class CreateReportDtoValidator : ReportInputValidator<CreateReportDto>;

public class UpdateReportDtoValidator : ReportInputValidator<UpdateReportDto>;

public class DetailedReportDtoValidator : AbstractValidator<DetailedReportDto>
{
    private const int MaxItems = 30;
    private const int MaxItemLength = 100;
    private const string ListMessage = "Допускается не более 30 значений, каждое не длиннее 100 символов.";

    public DetailedReportDtoValidator()
    {
        RuleFor(x => x.EventDate)
            .Must(d => ReportMappings.ToUtc(d!.Value) <= DateTime.UtcNow.AddMinutes(5))
            .When(x => x.EventDate.HasValue)
            .WithMessage("Дата события не может быть в будущем.");

        RuleFor(x => x.Duration).MaximumLength(20);
        RuleFor(x => x.LocationType).MaximumLength(50);
        RuleFor(x => x.HumanAction).MaximumLength(100);
        RuleFor(x => x.OscillationType).MaximumLength(50);
        RuleFor(x => x.WitnessesCount).MaximumLength(50);
        RuleFor(x => x.CrackWidth).MaximumLength(20);
        RuleFor(x => x.CrackLength).MaximumLength(20);
        RuleFor(x => x.ContactName).MaximumLength(200);
        RuleFor(x => x.AdditionalInfo).MaximumLength(2000);

        RuleFor(x => x.ContactEmail).MaximumLength(255)
            .EmailAddress().WithMessage("Некорректный email.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhone).MaximumLength(20)
            .Matches(@"^[0-9+()\-\s]{5,20}$").WithMessage("Некорректный номер телефона.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.Reactions).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.AffectedObjects).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.BuildingElements).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.ObservationsInside).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.OutdoorObjects).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.Infrastructure).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.SurfacePhenomena).Must(ItemsAreValid).WithMessage(ListMessage);
        RuleFor(x => x.OtherObservations).Must(ItemsAreValid).WithMessage(ListMessage);
    }

    private static bool ItemsAreValid(List<string>? list)
        => list is null || (list.Count <= MaxItems && list.All(i => i is null || i.Length <= MaxItemLength));
}

public class MatchReportDtoValidator : AbstractValidator<MatchReportDto>
{
    private static readonly string[] Allowed =
        [ValidationStatuses.Verified, ValidationStatuses.Outlier, ValidationStatuses.Unverified];

    public MatchReportDtoValidator()
    {
        RuleFor(x => x.ValidationStatus)
            .NotEmpty().WithMessage("Укажите статус.")
            .Must(s => Allowed.Contains(s)).WithMessage("Допустимые статусы: verified, outlier, unverified.");

        RuleFor(x => x.ExpectedIntensity).InclusiveBetween(0m, 12m).When(x => x.ExpectedIntensity.HasValue);
        RuleFor(x => x.Deviation).InclusiveBetween(-12m, 12m).When(x => x.Deviation.HasValue);

        // verified и outlier возможны только при расчётной интенсивности и привязке к событию
        When(x => x.ValidationStatus is ValidationStatuses.Verified or ValidationStatuses.Outlier, () =>
        {
            RuleFor(x => x.ExpectedIntensity).NotNull().WithMessage("Для этого статуса нужна расчётная интенсивность.");
            RuleFor(x => x.Deviation).NotNull().WithMessage("Для этого статуса нужно отклонение.");
            RuleFor(x => x.MatchedEarthquakeUid).NotNull().WithMessage("Для этого статуса нужно событие.");
        });
    }
}