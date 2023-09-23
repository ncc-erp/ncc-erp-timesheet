using AutoMapper;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Mappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RMALMS.AutoMapper
{
    public class ToDTOProfile : Profile
    {
        public ToDTOProfile()
        {
            AddMemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(
                    _ => _.AddStrings(p => p.Postfixes, "Dto"));
            AddConditionalObjectMapper().Where((s, d) => s.Name == d.Name + "Dto");
        }
    }
}
