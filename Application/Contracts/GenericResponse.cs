namespace Application.Contracts
{
    public class GenericResponse
    {
        public readonly bool Result;
        public readonly IList<string> Errors;

        public GenericResponse(bool result, IList<string> errors)
        {
            Result = result;
            Errors = errors;
        }
    }
}
