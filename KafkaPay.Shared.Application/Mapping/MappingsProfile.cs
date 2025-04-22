using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using KafkaPay.Shared.Application.DTOS;
using KafkaPay.Shared.Domain.Entities;

namespace KafkaPay.Shared.Application.Mapping
{
    public class MappingsProfile : Profile
    {
        public MappingsProfile()
        {
            CreateMap<UserDto, User>();
            CreateMap<User, UserDto>();
            CreateMap<TransactionDTO, TnxTransaction>();
            CreateMap<TnxTransaction, TransactionDTO>();
            CreateMap<TransactionStatusDto, Domain.Entities.TransactionStatus>().ReverseMap();

            CreateMap<Account, AccountDto>();


        }
    }
}
