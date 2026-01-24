using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;

namespace formneo.service.Services
{
    public class FormItemsService : Service<FormItems>, IFormItemsService
    {
        private readonly IFormItemsRepository _formItemsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FormItemsService(IGenericRepository<FormItems> repository, IUnitOfWork unitOfWork, IMapper mapper, IFormItemsRepository formItemsRepository) : base(repository, unitOfWork)
        {
            _mapper = mapper;
            _formItemsRepository = formItemsRepository;

            _unitOfWork = unitOfWork;

        }

        public async Task<List<FormItemsDto>> GetAllRelationTable()
        {
            var list = _formItemsRepository.GetAll();

            var ss = _formItemsRepository.GetAll()
                .Include(e => e.WorkflowItem).ThenInclude(e => e.WorkflowHead)
                .Include(e => e.Form)
                .Include(e => e.FormUser)  // ✅ Navigation property'yi include et
                .ToList();

            var dto = _mapper.Map<List<FormItemsDto>>(ss.OrderByDescending(e => e.CreatedDate));

            return dto;
        }

    }
}

