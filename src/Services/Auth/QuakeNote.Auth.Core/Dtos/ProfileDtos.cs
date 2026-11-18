namespace QuakeNote.Auth.Core.Dtos;

public record ProfileDto(string? FirstName, string? LastName, string? Location);

public record UpdateProfileDto(string? FirstName, string? LastName, string? Location);