using Application.DTO.Request;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GHNLogHandler
    {
        private readonly IGHNLogRepository _ghNLogRepository;
        private readonly IMapper _mapper;

        public GHNLogHandler(IGHNLogRepository ghNLogRepository, IMapper mapper)
        {
            _ghNLogRepository = ghNLogRepository;
            _mapper = mapper;
        }

        public async Task<CreateLogGHNDTO> CreateTrackingLogGHN(CreateLogGHNDTO data)
        {
            try
            {
                // check data
                if (data == null)
                {
                    throw new Exception("Dont have any data!");
                }

                var map = _mapper.Map<AuditLog>(data);
                var userCreate = await _ghNLogRepository.CreateAuditLog(map);
                var result = _mapper.Map<CreateLogGHNDTO>(userCreate);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
