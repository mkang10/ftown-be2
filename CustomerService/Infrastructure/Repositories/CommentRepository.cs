﻿using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {

        private readonly FtownContext _context;

        public CommentRepository(FtownContext context)
        {
            _context = context;
        }
        public async Task<Feedback> CreateFeedback(Feedback data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<ReplyFeedback> CreateReply(ReplyFeedback data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<bool> DeleteFeedback(Feedback data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReply(ReplyFeedback data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Feedback> GetFeedBackById(int id)
        {
            var data = await _context.Feedbacks.SingleOrDefaultAsync(x => x.FeedbackId.Equals(id));
            return data;
        }

        public async Task<ReplyFeedback> GetReplyFeedBackById(int id)
        {
            var data = await _context.ReplyFeedbacks.SingleOrDefaultAsync(x => x.ReplyId.Equals(id));
            return data;
        }

        public async Task<Pagination<Feedback>> GettAllCommentByAccountId(int id, PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Feedbacks
                                                       .Where(f => f.AccountId == id)
                                                       .CountAsync();

            var items = await _context.Feedbacks
                                       .Where(f => f.AccountId == id)
                                       .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                       .Take(paginationParameter.PageSize)
                                       .AsNoTracking()
                                       .ToListAsync();

            var result = new Pagination<Feedback>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Pagination<Feedback>> GettAllFeedbackByProductId(int id, PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Feedbacks
                                           .Where(f => f.ProductId == id) 
                                           .CountAsync();

            var items = await _context.Feedbacks
                                       .Where(f => f.ProductId == id) 
                                       .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                       .Take(paginationParameter.PageSize)
                                       .AsNoTracking()
                                       .ToListAsync();

            var result = new Pagination<Feedback>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Pagination<ReplyFeedback>> GettAllReplyByFeedbackId(int id, PaginationParameter paginationParameter)
        {
            var itemCount = await _context.ReplyFeedbacks
                                                                   .Where(f => f.FeedbackId == id)
                                                                   .CountAsync();

            var items = await _context.ReplyFeedbacks
                                       .Where(f => f.FeedbackId == id)
                                       .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                       .Take(paginationParameter.PageSize)
                                       .AsNoTracking()
                                       .ToListAsync();

            var result = new Pagination<ReplyFeedback>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Feedback> UpdateFeedback(Feedback data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<ReplyFeedback> UpdateReply(ReplyFeedback data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }
    }
}
