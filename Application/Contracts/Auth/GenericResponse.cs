namespace Application.Contracts
{
    public class GenericResponse
    {
        public readonly bool Result;
        public readonly IList<string> Errors;
        public readonly object? Content;

        public GenericResponse(bool result, object? content = null)
        {
            Result = result;
            Errors = new List<string>();
            Content = content;
        }

        public GenericResponse(bool result, IList<string> errors, object? content = null)
        {
            Result = result;
            Content = content;
            Errors = errors;
        }
    }
}
