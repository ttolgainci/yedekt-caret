namespace MarbleWebProject.Services
{
    public class BaseResponse
    {
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }

        public void Success()
        {
            Status = true;
        }

        public void Error(Exception ex)
        {
            Status = false;
            ErrorMessage = ex.Message;
        }

        public void Error(string message)
        {
            Status = false;
            ErrorMessage = message;
        }
    }

    public class BaseResponse<T> : BaseResponse
    {
        public T Data { get; set; }
    }
}
