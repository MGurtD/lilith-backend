namespace Application.Contracts.Shared
{
    public class ReportRequest : Contract
    {
        public Domain.Entities.File File { get; set; } = null!;
        public IEnumerable<KeyValueParameter> Parameters { get; set; } = Enumerable.Empty<KeyValueParameter>();
    }

    public class KeyValueParameter
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
