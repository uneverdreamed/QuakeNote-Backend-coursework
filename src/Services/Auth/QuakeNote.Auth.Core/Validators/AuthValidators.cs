using FluentValidation;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Core.Options;

namespace QuakeNote.Auth.Core.Validators;

public class RequestCodeDtoValidator : AbstractValidator<RequestCodeDto>
{
    public RequestCodeDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Укажите email.")
            .MaximumLength(255).WithMessage("Email не должен быть длиннее 255 символов.")
            .EmailAddress().WithMessage("Некорректный формат email.");
    }
}

public class VerifyCodeDtoValidator : AbstractValidator<VerifyCodeDto>
{
    public VerifyCodeDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Укажите email.")
            .MaximumLength(255).WithMessage("Email не должен быть длиннее 255 символов.")
            .EmailAddress().WithMessage("Некорректный формат email.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Укажите код подтверждения.")
            .Matches($@"^\d{{{AuthOptions.CodeLength}}}$")
            .WithMessage($"Код должен состоять из {AuthOptions.CodeLength} цифр.");

        RuleFor(x => x.BirthDate)
            .Must(d => d is null || (d >= new DateOnly(1900, 1, 1) && d <= DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithMessage("Некорректная дата рождения.");
    }
}

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(20).WithMessage("Имя не должно быть длиннее 20 символов.");
        RuleFor(x => x.LastName).MaximumLength(30).WithMessage("Фамилия не должна быть длиннее 30 символов.");
        RuleFor(x => x.Location).MaximumLength(50).WithMessage("Местоположение не должно быть длиннее 50 символов.");
    }
}

public class UpdateSettingsDtoValidator : AbstractValidator<UpdateSettingsDto>
{
    public UpdateSettingsDtoValidator()
    {
        RuleFor(x => x.MinMagnitude)
            .InclusiveBetween((short)1, (short)10)
            .WithMessage("Минимальная магнитуда должна быть от 1 до 10.");

        RuleFor(x => x.DefaultRegion).MaximumLength(50).WithMessage("Регион не должен быть длиннее 50 символов.");
    }
}