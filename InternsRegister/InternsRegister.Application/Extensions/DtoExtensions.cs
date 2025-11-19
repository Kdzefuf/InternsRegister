using InternsRegister.Application.DTOs;
using InternsRegister.Domain.Entities;
using InternsRegister.Persistence;

namespace InternsRegister.Application.Extensions
{
    public static class DtoExtensions
    {
        public static Direction? ToEntity(this DirectionDto? dto, InternsRegisterDbContext context)
            => dto == null ? null : context.Directions.Find(dto.Id);

        public static Project? ToEntity(this ProjectDto? dto, InternsRegisterDbContext context)
            => dto == null ? null : context.Projects.Find(dto.Id);
    }
}
