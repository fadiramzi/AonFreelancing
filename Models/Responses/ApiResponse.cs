namespace AonFreelancing.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Results { get; set; }
        public IList<Error> Errors { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

}
