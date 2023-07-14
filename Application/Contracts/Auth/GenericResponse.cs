namespace Application.Contracts.Auth
{
    public class GenericResponse
    {
        public readonly bool Result;
        public readonly IList<string> Errors;
        public readonly object? Content;

        public GenericResponse(bool result, IList<string> errors, object? content = null)
        {
            Result = result;
            Content = content;
            Errors = errors;
        }
    }
}
