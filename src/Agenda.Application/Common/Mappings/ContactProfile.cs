using Agenda.Application.Contacts;
using Agenda.Domain.Entities;
using AutoMapper;

namespace Agenda.Application.Common.Mappings;

public sealed class ContactProfile : Profile
{
    public ContactProfile()
    {
        CreateMap<Contact, ContactDto>();
    }
}
