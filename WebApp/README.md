# IQRA Platform - API Documentation

**Версия:** 1.0  
**Статус:** Фаза 1 Тамом ✅  
**Охирин навсозӣ:** 2026-01-10

---

## 📊 Прогресс Лоиҳа

### ✅ ФАЗА 0: FOUNDATION (100% ТАМОМ)

**Муҳлат:** 2026-01-08 - 2026-01-09

**Анҷомёфта:**
- ✅ Domain Entities Extended
- ✅ Reference Entities (School, University, Faculty, Major, ClusterDefinition)
- ✅ Database Migrations
- ✅ Seed Data (5 Clusters, 4 Unis, 22 Majors, 20 Schools)
- ✅ Admin User Seeding (admin/Admin@123)
- ✅ UserService + UserController (3 endpoints)

---

### ✅ ФАЗА 1: TESTING ENGINE (100% ТАМОМ)

**Муҳлат:** 2026-01-09 - 2026-01-10

**Анҷомёфта:**

#### 1. TestTemplate Entity
- 5 кластери ДМТ бо тақсимоти фанҳо
- Автоматӣ seed дар startup

#### 2. Testing DTOs
- `QuestionWithAnswersDto` - барои намоиши саволҳо
- `SubmitAnswerRequest` - барои фиристодани ҷавоб
- `TestResultDto` - барои натиҷаҳои муфассал
- `StartTestRequest`, `TestSessionDto`

#### 3. Services
- **ITestService / TestService:**
  - `StartTestAsync` - Оғози тест
  - `GetTestQuestionsAsync` - Гирифтани саволҳо
  - `SubmitAnswerAsync` - Фиристодани ҷавоб
  - `FinishTestAsync` - Хотимаи тест
  - `GetUserTestHistoryAsync` - Таърихи тестҳо

- **IQuestionService / QuestionService:**
  - `GetRandomQuestionsAsync` - Интихоби random саволҳо
  - `GetQuestionByIdAsync` - Гирифтани савол

#### 4. Question Management System
- **IQuestionManagementService / QuestionManagementService:**
  - Bulk import саволҳо (JSON)
  - CRUD operations
  - Validation барои 3 навъи саволҳо
  - Statistics

#### 5. Controllers
- **TestController** (5 endpoints)
- **QuestionManagementController** (8 endpoints, AdminOnly)
- **UserController** (3 endpoints)
- **AuthController** (3 endpoints)

#### 6. Infrastructure
- Swagger JWT Authorization ✅
- Service Registration ✅
- Database Auto-Migration ✅

---

## 🎯 ФАЗА 2: ANALYTICS & MISTAKE TRACKING (Қадами Навбатӣ)

**Тахмин:** 2-3 рӯз

**Кор:**

### 1. MistakeBank Entity
```sql
- Id, UserId, QuestionId, SubjectId
- MistakeCount, LastMistakeDate
- IsResolved, ResolvedDate
```

### 2. Analytics Services
- Subject-wise performance
- Weak topic detection
- Improvement recommendations
- Progress tracking

### 3. Dashboard Endpoints
- GET /api/analytics/my-stats
- GET /api/analytics/weak-subjects
- GET /api/analytics/progress
- GET /api/analytics/recommendations

---

## 📚 API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/send-password` | Фиристодани password | ❌ |
| POST | `/register` | Бақайдгирӣ | ❌ |
| POST | `/login` | Воруд | ❌ |

### User Management (`/api/user`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/profile` | Профили ман | ✅ |
| PUT | `/profile` | Навсозии профил | ✅ |
| POST | `/verify-otp` | Тасдиқи OTP | ✅ |

### Testing (`/api/test`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/start` | Оғози тест | ✅ |
| GET | `/{id}/questions` | Саволҳои тест | ✅ |
| POST | `/answer` | Фиристодани ҷавоб | ✅ |
| POST | `/{id}/finish` | Хотимаи тест | ✅ |
| GET | `/history` | Таърихи тестҳо | ✅ |

### Question Management (`/api/questions/manage`) - **ADMIN ONLY**
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/questions/import` | Bulk import саволҳо | 🔒 Admin |
| POST | `/questions/validate` | Санҷиши import | 🔒 Admin |
| POST | `/questions` | Эҷоди савол | 🔒 Admin |
| PUT | `/questions/{id}` | Навсозии савол | 🔒 Admin |
| DELETE | `/questions/{id}` | Нест кардани савол | 🔒 Admin |
| GET | `/questions/subject/{id}` | Саволҳо аз рӯи фан | 🔒 Admin |
| GET | `/questions/{id}` | Як савол | 🔒 Admin |
| GET | `/questions/stats` | Омор | 🔒 Admin |

---

## 🔐 Authentication

### JWT Bearer Token
1. `/api/auth/login` бо credentials
2. Токенро аз response гиред
3. Дар Swagger "Authorize" зада `Bearer {token}` ворид кунед
4. Ҳамаи protected endpoints истифода мешаванд

**Default Admin:**
- Username: `admin`
- Password: `Admin@123`
- ⚠️ **ДИQQАТ:** Дар production тағйир диҳед!

---

## 📝 Question Types

### 1. SingleChoice (type: 1)
```json
{
  "subjectId": 1,
  "content": "2 + 2 = ?",
  "type": 1,
  "difficulty": 1,
  "answers": [
    {"text": "3", "isCorrect": false},
    {"text": "4", "isCorrect": true},
    {"text": "5", "isCorrect": false}
  ]
}
```

### 2. Matching (type: 2)
```json
{
  "subjectId": 5,
  "content": "Мувофиқ кунед",
  "type": 2,
  "difficulty": 2,
  "answers": [
    {"text": "Русия", "isCorrect": true, "matchPair": "862"},
    {"text": "Франсия", "isCorrect": true, "matchPair": "843"}
  ]
}
```

### 3. ClosedAnswer (type: 3)
```json
{
  "subjectId": 6,
  "content": "Муаллифи Шоҳнома?",
  "type": 3,
  "difficulty": 1,
  "correctAnswer": "Абулқосим Фирдавсӣ"
}
```

---

## 🗄️ Database

### Auto-Migration
Барномаи автоматӣ ҳангоми startup:
1. Database seed мешавад
2. Admin user эҷод мешавад
3. Reference data (Unis, Schools) import мешавад
4. TestTemplates эҷод мешаванд

### Connection String
`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=IQRA;..."
  }
}
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server
- Postman/Swagger UI

### Running the App

```bash
# 1. Restore packages
dotnet restore

# 2. Update database (автоматӣ)
dotnet run --project WebApp

# 3. Access Swagger
https://localhost:5001/swagger
```

### First Steps
1. Swagger-ро кушоед
2. POST `/api/auth/login` бо admin/Admin@123
3. Токенро copy кунед
4. "Authorize" button → `Bearer {token}`
5. Endpoints-ро test кунед

---

## 📂 Project Structure

```
IQRA/
├── Domain/              # Entities, Enums
│   ├── Entities/
│   │   ├── Users/
│   │   ├── Education/
│   │   ├── Testing/
│   │   └── Reference/
│   └── Enums/
├── Application/         # DTOs, Interfaces
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Users/
│   │   ├── Testing/
│   │   └── Education/
│   └── Interfaces/
├── Infrastructure/      # Services, Data
│   ├── Services/
│   ├── Data/
│   └── Helpers/
└── WebApp/             # Controllers, Startup
    ├── Controllers/
    ├── Extensions/
    └── Program.cs
```

---

## 🔧 Configuration

### JWT Settings
`appsettings.json`:
```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "IQRA",
    "Audience": "IQRA",
    "DurationInMinutes": 1440
  }
}
```

### OsonSMS Settings
```json
{
  "OsonSms": {
    "Login": "your_login",
    "Password": "your_password",
    "Url": "https://api.osonsms.com"
  }
}
```

---

## 📊 Statistics

**Ҷамъи Endpoints:** 19  
**Controllers:** 4  
**Services:** 6  
**Entities:** 15+  
**Test Coverage:** Manual testing via Swagger  

---

## 🐛 Known Issues

❌ **Lock icons дар Swagger endpoints намуд намешаванд**
- ✅ **Fixed:** "Authorize" button кор мекунад
- Баъд аз authorize кардан ҳамаи protected endpoints кор мекунанд

---

## 📅 Roadmap

### ✅ Phase 0: Foundation (2026-01-08 - 2026-01-09)
### ✅ Phase 1: Testing Engine (2026-01-09 - 2026-01-10)
### 🎯 Phase 2: Analytics (Next - 2-3 days)
### ⏳ Phase 3: Gamification (Future)
### ⏳ Phase 4: Duel System (Future)
### ⏳ Phase 5: League System (Future)

---

## 👥 Team

**Developer:** [Your Name]  
**Architecture:** Clean Architecture (DDD)  
**Framework:** .NET 10  
**Database:** SQL Server  
**API Style:** RESTful  

---

## 📞 Support

Барои саволҳо ва мушкилот:
- 📧 Email: support@iqra.tj
- 📱 Telegram: @iqra_support

---

## 📜 License

Copyright © 2026 IQRA Platform. All rights reserved.
