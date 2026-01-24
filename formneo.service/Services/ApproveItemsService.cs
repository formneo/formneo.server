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
    public class ApproveItemsService : Service<ApproveItems>, IApproveItemsService
    {
        private readonly IApproveItemsRepository _approveItemsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ApproveItemsService(IGenericRepository<ApproveItems> repository, IUnitOfWork unitOfWork, IMapper mapper, IApproveItemsRepository approveItemsRepository) : base(repository, unitOfWork)
        {
            _mapper = mapper;
            _approveItemsRepository = approveItemsRepository;

            _unitOfWork = unitOfWork;

        }

        public async Task<List<ApproveItemsDto>> GetAllRelationTable()
        {
            var list = _approveItemsRepository.GetAll(); ;

            var ss = _approveItemsRepository.GetAll()
                .Include(e => e.WorkflowItem).ThenInclude(e => e.WorkflowHead)
                .Include(e => e.ApproveUser)  // ✅ Navigation property'yi include et
                .Include(e => e.ApprovedUser_Runtime)  // ✅ Runtime navigation property'yi include et
                .ToList();

            var dto = _mapper.Map<List<ApproveItemsDto>>(ss.OrderByDescending(e => e.CreatedDate));

            return dto;
        }

    }
}
