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
using formneo.repository.Repositories;

namespace formneo.service.Services
{
    public class WorkFlowDefinationService : Service<WorkFlowDefination>, IWorkFlowDefinationService
    {

        private readonly IWorkFlowDefinationRepository _workFlowDefinationRepository;
        private readonly IGenericRepository<WorkFlowDefination> _repository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkFlowDefinationService(IGenericRepository<WorkFlowDefination> repository, IUnitOfWork unitOfWork, IMapper mapper, IWorkFlowDefinationRepository workFlowRepository) : base(repository, unitOfWork)
        {
            _mapper = mapper;
            _repository = repository;

            //var s=  workFlowItemRepository.GetAll();

            _workFlowDefinationRepository = workFlowRepository;


            _unitOfWork = unitOfWork;
        }

        public override async Task<IEnumerable<WorkFlowDefination>> GetAllAsync()
        {
            return await _repository.GetAll().Include(w => w.Form).ToListAsync();
        }

        public override async Task<WorkFlowDefination> GetByIdStringGuidAsync(Guid id)
        {
            return await _repository.GetAll().Include(w => w.Form).FirstOrDefaultAsync(w => w.Id == id);
        }


    }
}
 