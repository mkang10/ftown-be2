﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateMultipleFeedBackRequest
    {
        public List<CreateFeedBackRequestDTO> Feedbacks { get; set; }
    }

}
