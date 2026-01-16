using System.Net;
using Application.DTOs.Reference;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Education;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SubjectService(ApplicationDbContext context, IFileStorageService fileStorageService) : ISubjectService
{
    public async Task<Response<List<SubjectDto>>> GetAllForSelectAsync()
    {
        var subjects = await context.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                IconUrl = s.IconUrl
            })
            .ToListAsync();

        return new Response<List<SubjectDto>>(subjects);
    }

    public async Task<Response<SubjectDto>> CreateSubjectAsync(CreateSubjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new Response<SubjectDto>(HttpStatusCode.BadRequest, "Номи фан холӣ аст");

        var existingSubject = await context.Subjects
            .FirstOrDefaultAsync(s => s.Name == request.Name);

        if (existingSubject != null)
            return new Response<SubjectDto>(HttpStatusCode.Conflict, "Фан бо ин ном аллакай вуҷуд дорад");

        var subject = new Subject
        {
            Name = request.Name,
            IconUrl = string.Empty
        };

        if (request.Icon != null)
        {
            var iconPath = await fileStorageService.SaveFileAsync(request.Icon, "uploads/subject-icons");
            subject.IconUrl = iconPath;
        }

        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var dto = new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            IconUrl = subject.IconUrl
        };

        return new Response<SubjectDto>(dto) { Message = "Фан эҷод шуд" };
    }

    public async Task<Response<SubjectDto>> GetByIdAsync(int id)
    {
        var subject = await context.Subjects.FindAsync(id);

        if (subject == null)
            return new Response<SubjectDto>(HttpStatusCode.NotFound, "Фан ёфт нашуд");

        var dto = new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            IconUrl = subject.IconUrl
        };

        return new Response<SubjectDto>(dto);
    }
}
