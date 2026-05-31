using AutoMapper;
using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Domain.Entities;

namespace AIResumeAnalyzer.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role.Name));

        CreateMap<Resume, ResumeDto>();

        CreateMap<ResumeAnalysis, ResumeAnalysisDto>();

        CreateMap<Skill, SkillDto>();

        CreateMap<JobDescription, JobDescriptionDto>();

        CreateMap<InterviewQuestion, InterviewQuestionDto>();
    }
}
