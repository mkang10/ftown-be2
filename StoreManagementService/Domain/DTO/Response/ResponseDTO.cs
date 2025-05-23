﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{

    namespace Domain.DTO.Response
    {
        public class PaginatedResponseDTO<T>
        {
            public List<T> Data { get; set; }
            public int TotalRecords { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }

            public PaginatedResponseDTO(List<T> data, int totalRecords, int page, int pageSize)
            {
                Data = data;
                TotalRecords = totalRecords;
                Page = page;
                PageSize = pageSize;
            }
        }

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



}
