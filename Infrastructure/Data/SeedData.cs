using Domain.Entities.Reference;
using Domain.Entities.Testing;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        if (!await context.ClusterDefinitions.AnyAsync())
            await SeedClusters(context);

        if (!await context.Universities.AnyAsync())
            await SeedUniversities(context);

        if (!await context.Schools.AnyAsync())
            await SeedSchools(context);

        if (!await context.TestTemplates.AnyAsync())
            await SeedTestTemplates(context);

        await SeedAdminAsync(userManager);
    }

    private static async Task SeedAdminAsync(UserManager<AppUser> userManager)
    {
        var adminUsername = "admin";
        var adminPassword = "Admin@123";

        var existingAdmin = await userManager.FindByNameAsync(adminUsername);
        if (existingAdmin != null)
        {
            return;
        }

        var admin = new AppUser
        {
            UserName = adminUsername,
            PhoneNumber = "+992000000000",
            PhoneNumberConfirmed = true,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

       
    }

    private static async Task SeedClusters(ApplicationDbContext context)
    {
        var clusters = new[]
        {
            new ClusterDefinition
            {
                Id = 1,
                ClusterNumber = 1,
                Description = "Математика, Физика, Забони англисӣ",
                SubjectIdsJson = "[1,2,3]"
            },
            new ClusterDefinition
            {
                Id = 2,
                ClusterNumber = 2,
                Description = "Математика, Химия, Забони англисӣ",
                SubjectIdsJson = "[1,4,3]"
            },
            new ClusterDefinition
            {
                Id = 3,
                ClusterNumber = 3,
                Description = "Таърих, Адабиёти тоҷик, Забони русӣ",
                SubjectIdsJson = "[5,6,7]"
            },
            new ClusterDefinition
            {
                Id = 4,
                ClusterNumber = 4,
                Description = "Иқтисод, Математика, Забони англисӣ",
                SubjectIdsJson = "[8,1,3]"
            },
            new ClusterDefinition
            {
                Id = 5,
                ClusterNumber = 5,
                Description = "Забони тоҷикӣ, Таърих, Ҷуғрофия",
                SubjectIdsJson = "[9,5,10]"
            }
        };

        context.ClusterDefinitions.AddRange(clusters);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUniversities(ApplicationDbContext context)
    {
        var dmt = new University
        {
            Id = 1,
            Name = "Донишгоҳи миллии Тоҷикистон (ДМТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Id = 1, Name = "Факултаи ҳуқуқшиносӣ", Majors = new List<Major>
                {
                    new() { Id = 1, Name = "Ҳуқуқшиносӣ", MinScore2024 = 265, MinScore2025 = 270 },
                    new() { Id = 2, Name = "Ҳуқуқи байналмилалӣ", MinScore2024 = 270, MinScore2025 = 275 }
                }},
                new() { Id = 2, Name = "Факултаи иқтисодиёт", Majors = new List<Major>
                {
                    new() { Id = 3, Name = "Иқтисодиёт", MinScore2024 = 250, MinScore2025 = 255 },
                    new() { Id = 4, Name = "Бонкдорӣ ва молия", MinScore2024 = 260, MinScore2025 = 265 }
                }},
                new() { Id = 3, Name = "Факултаи филология", Majors = new List<Major>
                {
                    new() { Id = 5, Name = "Филологияи тоҷик", MinScore2024 = 240, MinScore2025 = 245 },
                    new() { Id = 6, Name = "Забон ва адабиёти англисӣ", MinScore2024 = 245, MinScore2025 = 250 }
                }},
                new() { Id = 4, Name = "Факултаи математика", Majors = new List<Major>
                {
                    new() { Id = 7, Name = "Математика", MinScore2024 = 255, MinScore2025 = 260 },
                    new() { Id = 8, Name = "Информатика", MinScore2024 = 265, MinScore2025 = 270 }
                }},
                new() { Id = 5, Name = "Факултаи таърих", Majors = new List<Major>
                {
                    new() { Id = 9, Name = "Таърих", MinScore2024 = 235, MinScore2025 = 240 },
                    new() { Id = 10, Name = "Археология", MinScore2024 = 230, MinScore2025 = 235 }
                }}
            }
        };

        var dtt = new University
        {
            Id = 2,
            Name = "Донишгоҳи техникии Тоҷикистон (ДТТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Id = 6, Name = "Факултаи сохтмонӣ", Majors = new List<Major>
                {
                    new() { Id = 11, Name = "Сохтмони гражданӣ", MinScore2024 = 245, MinScore2025 = 250 },
                    new() { Id = 12, Name = "Меъмории бино", MinScore2024 = 255, MinScore2025 = 260 }
                }},
                new() { Id = 7, Name = "Факултаи энергетика", Majors = new List<Major>
                {
                    new() { Id = 13, Name = "Электроэнергетика", MinScore2024 = 250, MinScore2025 = 255 },
                    new() { Id = 14, Name = "Энергетикаи гармӣ", MinScore2024 = 240, MinScore2025 = 245 }
                }},
                new() { Id = 8, Name = "Факултаи технологияи компютерӣ", Majors = new List<Major>
                {
                    new() { Id = 15, Name = "Инҷинирии нармафзор", MinScore2024 = 275, MinScore2025 = 280 },
                    new() { Id = 16, Name = "Шабакаҳои компютерӣ", MinScore2024 = 270, MinScore2025 = 275 }
                }}
            }
        };

        var dadi = new University
        {
            Id = 3,
            Name = "Донишгоҳи давлатии омӯзгории Тоҷикистон (ДАДИ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Id = 9, Name = "Факултаи педагогика", Majors = new List<Major>
                {
                    new() { Id = 17, Name = "Педагогика ва психология", MinScore2024 = 230, MinScore2025 = 235 },
                    new() { Id = 18, Name = "Омӯзгории ибтидоӣ", MinScore2024 = 225, MinScore2025 = 230 }
                }},
                new() { Id = 10, Name = "Факултаи филологияи рус", Majors = new List<Major>
                {
                    new() { Id = 19, Name = "Забон ва адабиёти рус", MinScore2024 = 235, MinScore2025 = 240 }
                }}
            }
        };

        var ddmt = new University
        {
            Id = 4,
            Name = "Донишгоҳи давлатии тиббии Тоҷикистон (ДДМТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Id = 11, Name = "Факултаи тиббӣ", Majors = new List<Major>
                {
                    new() { Id = 20, Name = "Тиббиёти умумӣ", MinScore2024 = 280, MinScore2025 = 285 },
                    new() { Id = 21, Name = "Стоматология", MinScore2024 = 275, MinScore2025 = 280 }
                }},
                new() { Id = 12, Name = "Факултаи фармацевтика", Majors = new List<Major>
                {
                    new() { Id = 22, Name = "Фармацевтика", MinScore2024 = 265, MinScore2025 = 270 }
                }}
            }
        };

        context.Universities.AddRange(dmt, dtt, dadi, ddmt);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSchools(ApplicationDbContext context)
    {
        var schools = new[]
        {
            new School { Id = 1, Name = "Лицеи №1 (Душанбе)", Province = "Душанбе", District = "Маркази шаҳр" },
            new School { Id = 2, Name = "Лицеи №2 им. А.С. Пушкин", Province = "Душанбе", District = "Маркази шаҳр" },
            new School { Id = 3, Name = "Мактаби миёнаи №5", Province = "Душанбе", District = "Исмоили Сомонӣ" },
            new School { Id = 4, Name = "Мактаби миёнаи №10", Province = "Душанбе", District = "Фирдавсӣ" },
            new School { Id = 5, Name = "Мактаби миёнаи №25", Province = "Душанбе", District = "Шоҳмансур" },
            
            new School { Id = 6, Name = "Лицеи Хуҷанд №1", Province = "Суғд", District = "Хуҷанд" },
            new School { Id = 7, Name = "Мактаби миёнаи №3 (Хуҷанд)", Province = "Суғд", District = "Хуҷанд" },
            new School { Id = 8, Name = "Мактаби миёнаи Истаравшан", Province = "Суғд", District = "Истаравшан" },
            new School { Id = 9, Name = "Мактаби миёнаи Панҷакент", Province = "Суғд", District = "Панҷакент" },
            new School { Id = 10, Name = "Мактаби миёнаи Исфара", Province = "Суғд", District = "Исфара" },
            
            new School { Id = 11, Name = "Мактаби миёнаи №1 (Кӯлоб)", Province = "Хатлон", District = "Кӯлоб" },
            new School { Id = 12, Name = "Лицеи Қурғонтеппа", Province = "Хатлон", District = "Қурғонтеппа" },
            new School { Id = 13, Name = "Мактаби миёнаи Кулоб №7", Province = "Хатлон", District = "Кӯлоб" },
            new School { Id = 14, Name = "Мактаби миёнаи Бохтар", Province = "Хатлон", District = "Бохтар" },
            new School { Id = 15, Name = "Мактаби миёнаи Восеъ", Province = "Хатлон", District = "Восеъ" },
            
            new School { Id = 16, Name = "Мактаби миёнаи Хоруғ №1", Province = "ВМКБ", District = "Хоруғ" },
            new School { Id = 17, Name = "Лицеи Хоруғ", Province = "ВМКБ", District = "Хоруғ" },
            new School { Id = 18, Name = "Мактаби миёнаи Рӯшон", Province = "ВМКБ", District = "Рӯшон" },
            
            new School { Id = 19, Name = "Мактаби миёнаи Турсунзода", Province = "РТҶ", District = "Турсунзода" },
            new School { Id = 20, Name = "Мактаби миёнаи Ҳисор", Province = "РТҶ", District = "Ҳисор" }
        };

        context.Schools.AddRange(schools);
        await context.SaveChangesAsync();
        Console.WriteLine("[Seed] ✓ 20 Мактаб");
    }

    private static async Task SeedTestTemplates(ApplicationDbContext context)
    {
        var templates = new[]
        {
            new TestTemplate
            {
                Id = 1,
                ClusterNumber = 1,
                Name = "ДМТ Кластер 1 (Математика-Физика-Англисӣ)",
                SubjectDistributionJson = "{\"1\": 15, \"2\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                Id = 2,
                ClusterNumber = 2,
                Name = "ДМТ Кластер 2 (Математика-Химия-Англисӣ)",
                SubjectDistributionJson = "{\"1\": 15, \"4\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                Id = 3,
                ClusterNumber = 3,
                Name = "ДМТ Кластер 3 (Таърих-Адабиёт-Русӣ)",
                SubjectDistributionJson = "{\"5\": 15, \"6\": 10, \"7\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                Id = 4,
                ClusterNumber = 4,
                Name = "ДМТ Кластер 4 (Иқтисод-Математика-Англисӣ)",
                SubjectDistributionJson = "{\"8\": 15, \"1\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                Id = 5,
                ClusterNumber = 5,
                Name = "ДМТ Кластер 5 (Тоҷикӣ-Таърих-Ҷуғрофия)",
                SubjectDistributionJson = "{\"9\": 15, \"5\": 10, \"10\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            }
        };

        context.TestTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}
