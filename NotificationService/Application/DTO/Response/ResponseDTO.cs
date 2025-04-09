using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class ResponseDTO<T>
    {
        public T Data { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

        public ResponseDTO(T data, bool status, string message)
        {
            Data = data;
            Status = status;
            Message = message;
        }
    }
    public class ResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public ResponseDTO(bool status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
