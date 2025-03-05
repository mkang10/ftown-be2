using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
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


}
