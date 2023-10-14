namespace Application.Contracts
{
    public class GenericResponse
    {
        public bool Result { get; }
        public IList<string> Errors { get; }
        public object? Content { get; }

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
