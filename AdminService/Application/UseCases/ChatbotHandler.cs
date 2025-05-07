using Application.DTO.Request;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using StackExchange.Redis;
using Domain.Commons;

namespace Application.UseCases
{
    public class ChatbotHandler
    {
        private readonly IChatBotRepository _chatbot;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ChatbotHandler(IChatBotRepository chatbot, IMapper mapper, IConfiguration configuration)
        {
            _chatbot = chatbot;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ChatBotDTO> create(ChatBotDTO user)
        {
            try
            {             
                var map = _mapper.Map<ChatBot>(user);
                var userCreate = await _chatbot.CreateUser(map);
                var result = _mapper.Map<ChatBotDTO>(userCreate);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> delete(int id)
        {
            try
            {
                var user = await _chatbot.GetById(id);
                if (user == null)
                {
                    throw new Exception($"Bot {id} does not exist");
                }

                await _chatbot.DeleteUser(user);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ChatBotListDTO>> GetAllAscyn( )
        {
            try
            {
                var trips = await _chatbot.GetAll();

                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }
                foreach (var trip in trips)
                {
                    int id = trip.ChatBotId;
                    Console.WriteLine($"ChatBotId: {id}");
                }
                var tripModels = _mapper.Map<List<ChatBotListDTO>>(trips);
                return tripModels;  
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }


        public async Task<bool> update(int id, ChatBotDTO user)
        {
            try
            {
                var userData = await _chatbot.GetById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }
                _mapper.Map(user, userData);
                await _chatbot.UpdateUser(userData);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
        public async Task<bool> activeDeactiveBot(int id, ChatBotDStatusTO user)
        {
            try
            {
                var allBots = await _chatbot.GetAll();

                if (!allBots.Any())
                {
                    throw new Exception("No data!");
                }

                if (user.IsDefault)
                {
                    // Tắt các bot khác đang mặc định
                    var activeBots = allBots
                        .Where(b => b.IsDefault == true && b.ChatBotId != id)
                        .ToList();

                    foreach (var bot in activeBots)
                    {
                        bot.IsDefault = false;
                        await _chatbot.UpdateUser(bot);
                    }
                }
                // cập nhật con bot mới cho nó chạy
                var userData = await _chatbot.GetById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }
                _mapper.Map(user, userData);
                await _chatbot.UpdateUser(userData);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

    }
}
