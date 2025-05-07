using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IChatBotRepository
    {
        public Task<List<ChatBot>> GetAll();
        Task<ChatBot> GetById(int id);

        public Task<ChatBot> CreateUser(ChatBot data);
        public Task<ChatBot> UpdateUser(ChatBot data);
        public Task<bool> DeleteUser(ChatBot data);
    }
}
