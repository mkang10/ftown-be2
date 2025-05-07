using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class ChatBotRepository : IChatBotRepository
    {
        private readonly FtownContext _context;

        public ChatBotRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<ChatBot> CreateUser(ChatBot data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<bool> DeleteUser(ChatBot data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChatBot>> GetAll()
        {
            var data = await _context.ChatBots.ToListAsync();
            return data;
        }

        public async Task<ChatBot> GetById(int id)
        {
            var data = await _context.ChatBots.SingleOrDefaultAsync(o => o.ChatBotId.Equals(id));
            return data;    
        }

        public async Task<ChatBot> UpdateUser(ChatBot data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }
    }
    
}
