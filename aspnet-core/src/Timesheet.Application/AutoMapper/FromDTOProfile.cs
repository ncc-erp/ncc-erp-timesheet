using AutoMapper;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Mappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RMALMS.AutoMapper
{
    public class FromDTOProfile : Profile
    {
        public FromDTOProfile()
        {
            AddMemberConfiguration().AddName<PrePostfixName>(
                _ => _.AddStrings(p => p.DestinationPostfixes, "Dto"));
            AddConditionalObjectMapper().Where((s, d) => d.Name == s.Name + "Dto");
        }
    }
}
