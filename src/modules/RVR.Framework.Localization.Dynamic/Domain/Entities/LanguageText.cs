namespace RVR.Framework.Localization.Dynamic.Domain.Entities;
public class LanguageText
{
    public Guid Id { get; set; }
    public string Culture { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
