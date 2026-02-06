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

    public async Task<Response<SubjectDto>> UpdateSubjectAsync(int id, UpdateSubjectRequest request)
    {
        var subject = await context.Subjects.FindAsync(id);

        if (subject == null)
            return new Response<SubjectDto>(HttpStatusCode.NotFound, "Фан ёфт нашуд");

        if (string.IsNullOrWhiteSpace(request.Name))
            return new Response<SubjectDto>(HttpStatusCode.BadRequest, "Номи фан холӣ аст");

        var existingSubject = await context.Subjects
            .FirstOrDefaultAsync(s => s.Name == request.Name && s.Id != id);

        if (existingSubject != null)
            return new Response<SubjectDto>(HttpStatusCode.Conflict, "Фан бо ин ном аллакай вуҷуд дорад");

        subject.Name = request.Name;

        if (request.Icon != null)
        {
            if (!string.IsNullOrEmpty(subject.IconUrl))
            {
                await fileStorageService.DeleteFileAsync(subject.IconUrl, "uploads/subject-icons");
            }

            var iconPath = await fileStorageService.SaveFileAsync(request.Icon, "uploads/subject-icons");
            subject.IconUrl = iconPath;
        }

        context.Subjects.Update(subject);
        await context.SaveChangesAsync();

        var dto = new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            IconUrl = subject.IconUrl
        };

        return new Response<SubjectDto>(dto) { Message = "Фан тағйир ёфт" };
    }

    public async Task<Response<object>> DeleteSubjectAsync(int id)
    {
        var subject = await context.Subjects.FindAsync(id);

        if (subject == null)
            return new Response<object>(HttpStatusCode.NotFound, "Фан ёфт нашуд");

        var hasQuestions = await context.Questions.AnyAsync(q => q.SubjectId == id);
        if (hasQuestions)
            return new Response<object>(HttpStatusCode.BadRequest, "Фан дорои саволҳо аст ва нест карда намешавад");

        if (!string.IsNullOrEmpty(subject.IconUrl))
        {
            await fileStorageService.DeleteFileAsync(subject.IconUrl, "uploads/subject-icons");
        }

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync();

        return new Response<object>(null!) { Message = "Фан нест карда шуд" };
    }
}
