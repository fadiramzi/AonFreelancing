namespace AonFreelancing.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Results { get; set; }
        public Error Error { get; set; }
    }

    public class Error
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

}
