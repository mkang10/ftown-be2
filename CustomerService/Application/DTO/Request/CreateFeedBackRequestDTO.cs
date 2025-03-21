﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateFeedBackRequestDTO

    {
        public int? orderDetailId { get; set; }
        public int AccountId { get; set; }

        public int ProductId { get; set; }

        public string? Title { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ImagePath { get; set; }
    }
    public class UpdateFeedbackRequestDTO
    {
        public int ProductId { get; set; }

        public string? Title { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ImagePath { get; set; }
    }
    public class CreateReplyRequestDTO
    {
        public int FeedbackId { get; set; }

        public int AccountId { get; set; }

        public string ReplyText { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
    }
    public class UpdateReplyRequestDTO
    {
        public string ReplyText { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
    }
}
