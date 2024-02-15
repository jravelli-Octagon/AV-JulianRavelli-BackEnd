
namespace AmericanVirtual.Weather.Challenge.Common.DTOs
{
    public class ResponseResultDTO
    {
        private object _data;
        public bool Failed { get; private set; }
        public string Message { get; private set; }
        public string Code { get; private set; }

        private ResponseResultDTO() { }
        public static ResponseResultDTO Ok(object data) => new ResponseResultDTO { Failed = false, _data = data };
        public static ResponseResultDTO Fail(string code, string message) => new ResponseResultDTO { Failed = true, Message = message, Code = code };

        public object GetResult => Failed? new
        {
            message = Code,
            description = Message
        } : _data; 
    }
}