using Domain.Entities.Reference;
using Domain.Entities.Testing;
using Domain.Entities.Users;
using Domain.Entities.Gamification;
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

        if (!await context.Leagues.AnyAsync())
            await SeedLeagues(context);

        await SeedAdminAsync(context, userManager);

        await SeedGamificationAsync(context, userManager);

        await AssignDefaultLeaguesAsync(context);

        await ResetSequencesAsync(context);
    }

    private static async Task ResetSequencesAsync(ApplicationDbContext context)
    {
        var tables = new[] { "Schools", "Universities", "Faculties", "Majors", "ClusterDefinitions", "TestTemplates", "Leagues" };
        foreach (var table in tables)
        {
            
            await context.Database.ExecuteSqlRawAsync($"SELECT setval(pg_get_serial_sequence('\"{table}\"', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM \"{table}\"), 1));");
        }
    }

    private static async Task SeedAdminAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        var adminUsername = "admin";
        var adminPassword = "Admin@123";

        var admin = await userManager.FindByNameAsync(adminUsername);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminUsername,
                PhoneNumber = "+992900000000",
                PhoneNumberConfirmed = true,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded) return;
        }

        if (!await context.UserProfiles.AnyAsync(p => p.UserId == admin.Id))
        {
            var profile = new UserProfile
            {
                UserId = admin.Id,
                FirstName = "Admin",
                LastName = " ",
                Gender = Gender.Male,
                XP = 0,
                EloRating = 1000,
                Province = "Душанбе",
                District = "Сино"
            };

            context.UserProfiles.Add(profile);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedClusters(ApplicationDbContext context)
    {
        var clusters = new[]
        {
            new ClusterDefinition
            {
                ClusterNumber = 1,
                Description = "Математика, Физика, Забони англисӣ",
                SubjectIdsJson = "[1,2,3]"
            },
            new ClusterDefinition
            {
                ClusterNumber = 2,
                Description = "Математика, Химия, Забони англисӣ",
                SubjectIdsJson = "[1,4,3]"
            },
            new ClusterDefinition
            {
                ClusterNumber = 3,
                Description = "Таърих, Адабиёти тоҷик, Забони русӣ",
                SubjectIdsJson = "[5,6,7]"
            },
            new ClusterDefinition
            {
                ClusterNumber = 4,
                Description = "Иқтисод, Математика, Забони англисӣ",
                SubjectIdsJson = "[8,1,3]"
            },
            new ClusterDefinition
            {
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
            Name = "Донишгоҳи миллии Тоҷикистон (ДМТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Name = "Факултаи ҳуқуқшиносӣ", Majors = new List<Major>
                {
                    new() { Name = "Ҳуқуқшиносӣ", MinScore2024 = 265, MinScore2025 = 270 },
                    new() { Name = "Ҳуқуқи байналмилалӣ", MinScore2024 = 270, MinScore2025 = 275 }
                }},
                new() { Name = "Факултаи иқтисодиёт", Majors = new List<Major>
                {
                    new() { Name = "Иқтисодиёт", MinScore2024 = 250, MinScore2025 = 255 },
                    new() { Name = "Бонкдорӣ ва молия", MinScore2024 = 260, MinScore2025 = 265 }
                }},
                new() { Name = "Факултаи филология", Majors = new List<Major>
                {
                    new() { Name = "Филологияи тоҷик", MinScore2024 = 240, MinScore2025 = 245 },
                    new() { Name = "Забон ва адабиёти англисӣ", MinScore2024 = 245, MinScore2025 = 250 }
                }},
                new() { Name = "Факултаи математика", Majors = new List<Major>
                {
                    new() { Name = "Математика", MinScore2024 = 255, MinScore2025 = 260 },
                    new() { Name = "Информатика", MinScore2024 = 265, MinScore2025 = 270 }
                }},
                new() { Name = "Факултаи таърих", Majors = new List<Major>
                {
                    new() { Name = "Таърих", MinScore2024 = 235, MinScore2025 = 240 },
                    new() { Name = "Археология", MinScore2024 = 230, MinScore2025 = 235 }
                }}
            }
        };

        var dtt = new University
        {
            Name = "Донишгоҳи техникии Тоҷикистон (ДТТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Name = "Факултаи сохтмонӣ", Majors = new List<Major>
                {
                    new() { Name = "Сохтмони гражданӣ", MinScore2024 = 245, MinScore2025 = 250 },
                    new() { Name = "Меъмории бино", MinScore2024 = 255, MinScore2025 = 260 }
                }},
                new() { Name = "Факултаи энергетика", Majors = new List<Major>
                {
                    new() { Name = "Электроэнергетика", MinScore2024 = 250, MinScore2025 = 255 },
                    new() { Name = "Энергетикаи гармӣ", MinScore2024 = 240, MinScore2025 = 245 }
                }},
                new() { Name = "Факултаи технологияи компютерӣ", Majors = new List<Major>
                {
                    new() { Name = "Инҷинирии нармафзор", MinScore2024 = 275, MinScore2025 = 280 },
                    new() { Name = "Шабакаҳои компютерӣ", MinScore2024 = 270, MinScore2025 = 275 }
                }}
            }
        };

        var dadi = new University
        {
            Name = "Донишгоҳи давлатии омӯзгории Тоҷикистон (ДАДИ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Name = "Факултаи педагогика", Majors = new List<Major>
                {
                    new() { Name = "Педагогика ва психология", MinScore2024 = 230, MinScore2025 = 235 },
                    new() { Name = "Омӯзгории ибтидоӣ", MinScore2024 = 225, MinScore2025 = 230 }
                }},
                new() { Name = "Факултаи филологияи рус", Majors = new List<Major>
                {
                    new() { Name = "Забон ва адабиёти рус", MinScore2024 = 235, MinScore2025 = 240 }
                }}
            }
        };

        var ddmt = new University
        {
            Name = "Донишгоҳи давлатии тиббии Тоҷикистон (ДДМТ)",
            City = "Душанбе",
            Faculties = new List<Faculty>
            {
                new() { Name = "Факултаи тиббӣ", Majors = new List<Major>
                {
                    new() { Name = "Тиббиёти умумӣ", MinScore2024 = 280, MinScore2025 = 285 },
                    new() { Name = "Стоматология", MinScore2024 = 275, MinScore2025 = 280 }
                }},
                new() { Name = "Факултаи фармацевтика", Majors = new List<Major>
                {
                    new() { Name = "Фармацевтика", MinScore2024 = 265, MinScore2025 = 270 }
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
            new School { Name = "Лицеи №1 (Душанбе)", Province = "Душанбе", District = "Маркази шаҳр" },
            new School { Name = "Лицеи №2 им. А.С. Пушкин", Province = "Душанбе", District = "Маркази шаҳр" },
            new School { Name = "Мактаби миёнаи №5", Province = "Душанбе", District = "Исмоили Сомонӣ" },
            new School { Name = "Мактаби миёнаи №10", Province = "Душанбе", District = "Фирдавсӣ" },
            new School { Name = "Мактаби миёнаи №25", Province = "Душанбе", District = "Шоҳмансур" },
            
            new School { Name = "Лицеи Хуҷанд №1", Province = "Суғд", District = "Хуҷанд" },
            new School { Name = "Мактаби миёнаи №3 (Хуҷанд)", Province = "Суғд", District = "Хуҷанд" },
            new School { Name = "Мактаби миёнаи Истаравшан", Province = "Суғд", District = "Истаравшан" },
            new School { Name = "Мактаби миёнаи Панҷакент", Province = "Суғд", District = "Панҷакент" },
            new School { Name = "Мактаби миёнаи Исфара", Province = "Суғд", District = "Исфара" },
            
            new School { Name = "Мактаби миёнаи №1 (Кӯлоб)", Province = "Хатлон", District = "Кӯлоб" },
            new School { Name = "Лицеи Қурғонтеппа", Province = "Хатлон", District = "Қурғонтеппа" },
            new School { Name = "Мактаби миёнаи Кулоб №7", Province = "Хатлон", District = "Кӯлоб" },
            new School { Name = "Мактаби миёнаи Бохтар", Province = "Хатлон", District = "Бохтар" },
            new School { Name = "Мактаби миёнаи Восеъ", Province = "Хатлон", District = "Восеъ" },
            
            new School { Name = "Мактаби миёнаи Хоруғ №1", Province = "ВМКБ", District = "Хоруғ" },
            new School { Name = "Лицеи Хоруғ", Province = "ВМКБ", District = "Хоруғ" },
            new School { Name = "Мактаби миёнаи Рӯшон", Province = "ВМКБ", District = "Рӯшон" },
            
            new School { Name = "Мактаби миёнаи Турсунзода", Province = "РТҶ", District = "Турсунзода" },
            new School { Name = "Мактаби миёнаи Ҳисор", Province = "РТҶ", District = "Ҳисор" }
        };

        context.Schools.AddRange(schools);
        await context.SaveChangesAsync();
        
        // Reset sequence after manual seeding
        await context.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"Schools\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Schools\"));");
        
        Console.WriteLine("[Seed] ✓ 20 Мактаб");
    }

    private static async Task SeedTestTemplates(ApplicationDbContext context)
    {
        var templates = new[]
        {
            new TestTemplate
            {
                ClusterNumber = 1,
                Name = "ДМТ Кластер 1 (Математика-Физика-Англисӣ)",
                SubjectDistributionJson = "{\"1\": 15, \"2\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                ClusterNumber = 2,
                Name = "ДМТ Кластер 2 (Математика-Химия-Англисӣ)",
                SubjectDistributionJson = "{\"1\": 15, \"4\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                ClusterNumber = 3,
                Name = "ДМТ Кластер 3 (Таърих-Адабиёт-Русӣ)",
                SubjectDistributionJson = "{\"5\": 15, \"6\": 10, \"7\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
                ClusterNumber = 4,
                Name = "ДМТ Кластер 4 (Иқтисод-Математика-Англисӣ)",
                SubjectDistributionJson = "{\"8\": 15, \"1\": 10, \"3\": 10}",
                TotalQuestions = 35,
                DurationMinutes = 180
            },
            new TestTemplate
            {
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

    private static async Task SeedLeagues(ApplicationDbContext context)
    {
        var leagues = new[]
        {
            new League { Id = 1, Name = "Bronze League", MinXP = 0, Color = "#CD7F32", PromotionThreshold = 0.15, RelegationThreshold = 0.0, BadgeUrl = "/assets/leagues/bronze.png" },
            new League { Id = 2, Name = "Silver League", MinXP = 500, Color = "#C0C0C0", PromotionThreshold = 0.15, RelegationThreshold = 0.15, BadgeUrl = "/assets/leagues/silver.png" },
            new League { Id = 3, Name = "Gold League", MinXP = 1500, Color = "#FFD700", PromotionThreshold = 0.15, RelegationThreshold = 0.15, BadgeUrl = "/assets/leagues/gold.png" },
            new League { Id = 4, Name = "Platinum League", MinXP = 3000, Color = "#E5E4E2", PromotionThreshold = 0.15, RelegationThreshold = 0.15, BadgeUrl = "/assets/leagues/platinum.png" },
            new League { Id = 5, Name = "Diamond League", MinXP = 6000, Color = "#B9F2FF", PromotionThreshold = 0.0, RelegationThreshold = 0.15, BadgeUrl = "/assets/leagues/diamond.png" }
        };

        context.Leagues.AddRange(leagues);
        await context.SaveChangesAsync();
        
        Console.WriteLine("[Seed] ✓ 5 Leagues");
    }

    private static async Task AssignDefaultLeaguesAsync(ApplicationDbContext context)
    {
        var usersWithoutLeague = await context.UserProfiles
            .Where(p => p.CurrentLeagueId == null)
            .ToListAsync();

        if (usersWithoutLeague.Any())
        {
            foreach (var user in usersWithoutLeague)
            {
                user.CurrentLeagueId = 1; 
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[Seed] ✓ Assigned Bronze League to {usersWithoutLeague.Count} users");
        }
    }

    private static async Task SeedGamificationAsync(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        if (await context.UserProfiles.CountAsync() > 5) return; // Only seed if empty (apart from admin)

        var random = new Random();
        var users = new List<AppUser>();
        var profiles = new List<UserProfile>();
        var testUser = new AppUser { UserName = "user", PhoneNumber = "+992900000001", PhoneNumberConfirmed = true, Role = UserRole.Student, CreatedAt = DateTime.UtcNow, IsActive = true };
        await userManager.CreateAsync(testUser, "User@123");
        
        var testProfile = new UserProfile
        {
            UserId = testUser.Id,
            FirstName = "Test",
            LastName = "User",
            Gender = Gender.Male,
            XP = 850,
            WeeklyXP = 350,
            CurrentLeagueId = 2, 
            Province = "Душанбе",
            District = "Шоҳмансур"
        };
        profiles.Add(testProfile);
        
        var diamondUser = new AppUser { UserName = "champion", PhoneNumber = "+992900000002", PhoneNumberConfirmed = true, Role = UserRole.Student, CreatedAt = DateTime.UtcNow, IsActive = true };
        await userManager.CreateAsync(diamondUser, "User@123");

        var diamondProfile = new UserProfile
        {
            UserId = diamondUser.Id,
            FirstName = "Diamond",
            LastName = "Champion",
            Gender = Gender.Female,
            XP = 15000,
            WeeklyXP = 2500,
            CurrentLeagueId = 5, // Diamond
            DiamondWinStreak = 2, // Ready to win!
            Province = "Хуҷанд",
            District = "Марказ"
        };
        profiles.Add(diamondProfile);

        // 3. Create 48 Random Users
        var references = new[]
        {
            ("Ахмед", "Саидов"), ("Мадина", "Каримова"), ("Фарҳод", "Ҷураев"), ("Нигина", "Раҳимова"),
            ("Далер", "Назаров"), ("Заррина", "Қосимова"), ("Рустам", "Ҳакимов"), ("Шаҳноза", "Юсупова"),
            ("Искандар", "Мирзоев"), ("Гулноза", "Алиева")
        };

        for (int i = 0; i < 48; i++)
        {
            var (fn, ln) = references[random.Next(references.Length)];
            var username = $"user{i + 3}";
            var user = new AppUser { UserName = username, PhoneNumber = $"+9929000010{i:D2}", PhoneNumberConfirmed = true, Role = UserRole.Student, CreatedAt = DateTime.UtcNow, IsActive = true };
            await userManager.CreateAsync(user, "User@123");

            int leagueId = random.Next(1, 6); // 1-5
            int xp = leagueId * 1000 + random.Next(0, 500);
            
            profiles.Add(new UserProfile
            {
                UserId = user.Id,
                FirstName = fn,
                LastName = $"{ln} {i}",
                Gender = i % 2 == 0 ? Gender.Male : Gender.Female,
                XP = xp,
                WeeklyXP = random.Next(0, 1000),
                CurrentLeagueId = leagueId,
                Province = "Душанбе",
                District = "Сино"
            });
        }

        context.UserProfiles.AddRange(profiles);
        await context.SaveChangesAsync();
        
        var leagues = await context.Leagues.ToListAsync();
        foreach (var league in leagues)
        {
            var leagueProfiles = profiles.Where(p => p.CurrentLeagueId == league.Id)
                .OrderByDescending(p => p.WeeklyXP)
                .ToList();
            
            for (int i = 0; i < leagueProfiles.Count; i++)
            {
                leagueProfiles[i].LastDayRank = i + 2; 
            }
        }
        
        await context.SaveChangesAsync();
        Console.WriteLine($"[Seed] ✓ Gamification Data: 50 Users Created");
    }
}
