using FluentValidation;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Infrastructure;

namespace QuakeNote.Earthquakes.Core.Validators;

/// <summary>Правила для фильтра по области: либо все четыре значения, либо ни одного.</summary>
public abstract class BboxFilterValidator<T> : AbstractValidator<T> where T : IBboxFilter
{
    protected BboxFilterValidator()
    {
        When(HasAny, () =>
        {
            RuleFor(x => x).Must(HasAll)
                .WithMessage("Для фильтра по области нужны все четыре значения: minLat, maxLat, minLon, maxLon.")
                .OverridePropertyName("bbox");

            When(HasAll, () =>
            {
                RuleFor(x => x.MinLat).InclusiveBetween(-90m, 90m).WithMessage("minLat должен быть от -90 до 90.");
                RuleFor(x => x.MaxLat).InclusiveBetween(-90m, 90m).WithMessage("maxLat должен быть от -90 до 90.");
                RuleFor(x => x.MinLon).InclusiveBetween(-180m, 180m).WithMessage("minLon должен быть от -180 до 180.");
                RuleFor(x => x.MaxLon).InclusiveBetween(-180m, 180m).WithMessage("maxLon должен быть от -180 до 180.");

                RuleFor(x => x).Must(x => x.MinLat <= x.MaxLat)
                    .WithMessage("minLat не может быть больше maxLat.").OverridePropertyName("bbox");
                RuleFor(x => x).Must(x => x.MinLon <= x.MaxLon)
                    .WithMessage("minLon не может быть больше maxLon.").OverridePropertyName("bbox");
            });
        });
    }

    private static bool HasAny(T x) => x.MinLat.HasValue || x.MaxLat.HasValue || x.MinLon.HasValue || x.MaxLon.HasValue;
    private static bool HasAll(T x) => x.MinLat.HasValue && x.MaxLat.HasValue && x.MinLon.HasValue && x.MaxLon.HasValue;
}

public class EarthquakesFilterDtoValidator : BboxFilterValidator<EarthquakesFilterDto>
{
    public EarthquakesFilterDtoValidator()
    {
        RuleFor(x => x).Must(x => x.From!.Value <= x.To!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("Начало периода не может быть позже конца.").OverridePropertyName("period");

        RuleFor(x => x.MinMagnitude).InclusiveBetween(0m, 10m).When(x => x.MinMagnitude.HasValue)
            .WithMessage("minMagnitude должна быть от 0 до 10.");
        RuleFor(x => x.MaxMagnitude).InclusiveBetween(0m, 10m).When(x => x.MaxMagnitude.HasValue)
            .WithMessage("maxMagnitude должна быть от 0 до 10.");
        RuleFor(x => x).Must(x => x.MinMagnitude!.Value <= x.MaxMagnitude!.Value)
            .When(x => x.MinMagnitude.HasValue && x.MaxMagnitude.HasValue)
            .WithMessage("minMagnitude не может быть больше maxMagnitude.").OverridePropertyName("magnitude");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Номер страницы начинается с 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Размер страницы должен быть от 1 до 100.");
    }
}

public class StationsFilterDtoValidator : BboxFilterValidator<StationsFilterDto>;

public abstract class EarthquakeInputValidator<T> : AbstractValidator<T> where T : IEarthquakeInput
{
    protected EarthquakeInputValidator()
    {
        RuleFor(x => x.EpicenterLat).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите широту эпицентра.")
            .InclusiveBetween(-90m, 90m).WithMessage("Широта должна быть от -90 до 90.");

        RuleFor(x => x.EpicenterLon).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите долготу эпицентра.")
            .InclusiveBetween(-180m, 180m).WithMessage("Долгота должна быть от -180 до 180.");

        RuleFor(x => x.Depth).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите глубину очага.")
            .InclusiveBetween(0m, 1000m).WithMessage("Глубина должна быть от 0 до 1000 км.");

        RuleFor(x => x.Magnitude).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите магнитуду.")
            .InclusiveBetween(0m, 10m).WithMessage("Магнитуда должна быть от 0 до 10.");

        RuleFor(x => x.OccurredAt).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите время события.")
            .Must(d => d!.Value.ToUtc() <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Время события не может быть в будущем.");

        RuleFor(x => x.RegionName).NotEmpty().WithMessage("Укажите регион.")
            .MaximumLength(50).WithMessage("Название региона не длиннее 50 символов.");

        RuleFor(x => x.Source).MaximumLength(100).WithMessage("Источник не длиннее 100 символов.");
        RuleFor(x => x.ExternalId).MaximumLength(50).WithMessage("Внешний идентификатор не длиннее 50 символов.");
    }
}

public class CreateEarthquakeDtoValidator : EarthquakeInputValidator<CreateEarthquakeDto>;

public class UpdateEarthquakeDtoValidator : EarthquakeInputValidator<UpdateEarthquakeDto>;

public abstract class StationInputValidator<T> : AbstractValidator<T> where T : IStationInput
{
    protected StationInputValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Укажите код станции.")
            .MaximumLength(20).WithMessage("Код не длиннее 20 символов.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Код может содержать только латинские буквы, цифры, «_» и «-».");

        RuleFor(x => x.Name).NotEmpty().WithMessage("Укажите название станции.")
            .MaximumLength(100).WithMessage("Название не длиннее 100 символов.");

        RuleFor(x => x.Latitude).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите широту.")
            .InclusiveBetween(-90m, 90m).WithMessage("Широта должна быть от -90 до 90.");

        RuleFor(x => x.Longitude).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите долготу.")
            .InclusiveBetween(-180m, 180m).WithMessage("Долгота должна быть от -180 до 180.");

        RuleFor(x => x.Region).NotEmpty().WithMessage("Укажите регион.")
            .MaximumLength(50).WithMessage("Регион не длиннее 50 символов.");

        RuleFor(x => x.Organization).NotEmpty().WithMessage("Укажите организацию.")
            .MaximumLength(100).WithMessage("Организация не длиннее 100 символов.");
    }
}

public class CreateStationDtoValidator : StationInputValidator<CreateStationDto>;

public class UpdateStationDtoValidator : StationInputValidator<UpdateStationDto>;

public class ReadingInputDtoValidator : AbstractValidator<ReadingInputDto>
{
    public ReadingInputDtoValidator()
    {
        RuleFor(x => x.StationUid).NotNull().WithMessage("Укажите станцию (stationUid).")
            .NotEqual(Guid.Empty).WithMessage("stationUid не может быть пустым.");

        RuleFor(x => x.Pga).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите PGA.")
            .InclusiveBetween(0m, 99999m).WithMessage("PGA должен быть от 0 до 99999 см/с².");

        RuleFor(x => x.Pgv).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Укажите PGV.")
            .InclusiveBetween(0m, 99999m).WithMessage("PGV должен быть от 0 до 99999 см/с.");

        RuleFor(x => x.Pgd).InclusiveBetween(0m, 99999m).When(x => x.Pgd.HasValue)
            .WithMessage("PGD должен быть от 0 до 99999 см.");
        RuleFor(x => x.AriasIntensity).InclusiveBetween(0m, 9999m).When(x => x.AriasIntensity.HasValue)
            .WithMessage("Интенсивность Ариаса должна быть от 0 до 9999.");
        RuleFor(x => x.DistanceToEpicenterKm).InclusiveBetween(0m, 999999m).When(x => x.DistanceToEpicenterKm.HasValue)
            .WithMessage("Расстояние должно быть от 0 до 999999 км.");

        RuleFor(x => x.PWaveArrivalAt).NotNull().WithMessage("Укажите время прихода P-волны.");
    }
}

public class UpsertReadingsDtoValidator : AbstractValidator<UpsertReadingsDto>
{
    private const int MaxReadings = 500;

    public UpsertReadingsDtoValidator()
    {
        RuleFor(x => x.Readings).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Передайте список показаний.")
            .NotEmpty().WithMessage("Список показаний не может быть пустым.")
            .Must(l => l!.Count <= MaxReadings).WithMessage($"За один раз можно передать не более {MaxReadings} показаний.")
            .Must(l => l!.Select(r => r.StationUid).Distinct().Count() == l!.Count)
            .WithMessage("Одна станция не может встречаться в списке дважды.");

        RuleForEach(x => x.Readings!).SetValidator(new ReadingInputDtoValidator());
    }
}

public class RecomputeAssessmentRequestValidator : AbstractValidator<RecomputeAssessmentRequest>
{
    public RecomputeAssessmentRequestValidator()
    {
        RuleFor(x => x.InstrumentalIntensity).InclusiveBetween(0m, 12m)
            .When(x => x.InstrumentalIntensity.HasValue)
            .WithMessage("Интенсивность должна быть от 0 до 12.");
    }
}