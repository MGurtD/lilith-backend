namespace Application.Contracts.Shared;

public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    string Icon,
    bool IsDefault,
    int SortOrder
);
