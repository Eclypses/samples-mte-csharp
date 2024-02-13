
namespace MteConsoleMultipleClients.Models
{
    public class ResponseModel<T> : ResponseModel
    {
        
        public T Data { get; set; }
        public ResponseModel(T data) : base()
        {
            Data = data;
        }

        public ResponseModel()
        {

        }
    }
    public class ResponseModel
    {
        
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public string ResultCode { get; set; }

        public ResponseModel()
        {
            Message = string.Empty;
            Success = true;
            ResultCode = Constants.RC_SUCCESS;
        }
    }
}
