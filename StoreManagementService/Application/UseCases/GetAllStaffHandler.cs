﻿using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{

    public class GetAllStaffHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IStaffDetailRepository _staffRepos;

        public GetAllStaffHandler(IImportRepos importRepos,
                                      IStaffDetailRepository staffRepos)
        {
            _importRepos = importRepos;
            _staffRepos = staffRepos;
        }



        public async Task<ResponseDTO<IEnumerable<StaffNameDto>>> GetAllStaffNamesAsync()
        {
            var staffNames = await _staffRepos.GetAllStaffNamesAsync();
            return new ResponseDTO<IEnumerable<StaffNameDto>>(staffNames, true, "Staff names retrieved successfully");
        }
    }
}

