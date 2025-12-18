# CI/CD Improvements Roadmap - Lilith Backend

> **Last Updated**: December 18, 2025  
> **Status**: Action Plan  
> **Owner**: Backend Team

---

## Overview

This document outlines the prioritized improvements for the Lilith Backend CI/CD pipeline based on comprehensive analysis. Improvements are organized by priority with estimated effort and expected impact.

---

## 🔴 Phase 1: Critical Quick Wins (Weekend Project - 4-6 hours)

### ✅ 1. Add .dockerignore File

**Status**: ✅ COMPLETED  
**Effort**: 5 minutes  
**Impact**: Faster builds, reduced image size, improved security

**Result**:

- Reduces build context by ~300MB+
- Excludes `bin/`, `obj/`, `logs/`, `.git/`, IDE files
- Prevents accidental secret leaks via environment files

---

### 2. Add Health Endpoints

**Effort**: 1 hour  
**Impact**: 🏥 Enable zero-downtime deployments, proper health monitoring

**Implementation**:

Add to `Api/Program.cs`:

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Map endpoints before app.Run()
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/ready", () => Results.Ok(new { status = "ready", timestamp = DateTime.UtcNow }));
```

**Dependencies to add**:

```bash
dotnet add package AspNetCore.HealthChecks.NpgSql
dotnet add package AspNetCore.HealthChecks.UI.Client
```

**Testing**:

```bash
curl http://localhost:5000/health
curl http://localhost:5000/ready
```

---

### 3. Add Container Image Scanning (Trivy)

**Effort**: 15 minutes  
**Impact**: 🔒 Prevent deployment of vulnerable images

**Implementation**:

Add to `.github/workflows/docker-image-ci.yml` after Docker build steps:

```yaml
- name: Scan Docker Image for Vulnerabilities
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: marcgurt/lilith-backend:${{ github.ref_name }}
    format: "sarif"
    output: "trivy-results.sarif"
    severity: "CRITICAL,HIGH"

- name: Upload Trivy Results to GitHub Security
  uses: github/codeql-action/upload-sarif@v2
  if: always()
  with:
    sarif_file: "trivy-results.sarif"

- name: Fail on Critical Vulnerabilities
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: marcgurt/lilith-backend:${{ github.ref_name }}
    format: "table"
    exit-code: "1"
    severity: "CRITICAL"
```

---

### 4. Add Build Caching

**Effort**: 30 minutes  
**Impact**: ⚡ 50-70% faster CI builds

**Implementation**:

Update `.github/workflows/docker-image-ci.yml`:

```yaml
- name: Checkout Repository
  uses: actions/checkout@v4

- name: Cache NuGet Packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-

- name: Build and push (with layer caching)
  uses: docker/build-push-action@v5
  with:
    push: true
    tags: marcgurt/lilith-backend:${{ github.ref_name }}
    cache-from: type=registry,ref=marcgurt/lilith-backend:buildcache
    cache-to: type=registry,ref=marcgurt/lilith-backend:buildcache,mode=max
```

---

### 5. Add Automated Dependency Updates (Dependabot)

**Effort**: 15 minutes  
**Impact**: 🔄 Stay secure, reduce technical debt

**Implementation**:

Create `.github/dependabot.yml`:

```yaml
version: 2
updates:
  # NuGet dependencies
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
    open-pull-requests-limit: 10
    reviewers:
      - "backend-team"
    labels:
      - "dependencies"
      - "backend"
    commit-message:
      prefix: "chore"
      include: "scope"

  # Docker base images
  - package-ecosystem: "docker"
    directory: "/"
    schedule:
      interval: "monthly"
    reviewers:
      - "backend-team"
    labels:
      - "dependencies"
      - "docker"
```

---

## 🟠 Phase 2: Testing Infrastructure (2-4 weeks)

### 6. Add Unit Testing Framework

**Effort**: 1 week (setup) + ongoing  
**Impact**: 🛡️ Catch bugs before deployment, enable TDD

**Implementation Steps**:

1. **Create test project**:

```bash
cd Lilith.Backend
dotnet new xunit -n Api.Tests
dotnet sln add Api.Tests/Api.Tests.csproj
cd Api.Tests
dotnet add reference ../Api/Api.csproj
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

2. **Project structure**:

```
Api.Tests/
├── Unit/
│   ├── Services/
│   │   ├── ExerciseServiceTests.cs
│   │   ├── BudgetServiceTests.cs
│   │   └── ...
│   └── Controllers/
│       └── ExerciseControllerTests.cs
├── Integration/
│   ├── Repositories/
│   │   └── ExerciseRepositoryTests.cs
│   └── Api/
│       └── ExerciseApiTests.cs
└── Fixtures/
    └── TestDataBuilder.cs
```

3. **Example test**:

```csharp
public class ExerciseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILocalizationService> _localizationMock;
    private readonly ExerciseService _sut;

    public ExerciseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _localizationMock = new Mock<ILocalizationService>();
        _sut = new ExerciseService(_unitOfWorkMock.Object, _localizationMock.Object);
    }

    [Fact]
    public async Task GetNextCounter_WithValidExercise_ReturnsIncrementedCounter()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = new Exercise { Id = exerciseId, CurrentCounter = 100 };
        _unitOfWorkMock.Setup(u => u.Exercises.Get(exerciseId))
            .ReturnsAsync(exercise);

        // Act
        var result = await _sut.GetNextCounter(exerciseId, "workorder");

        // Assert
        result.Should().Be(101);
        exercise.CurrentCounter.Should().Be(101);
    }
}
```

4. **Add to CI pipeline**:

```yaml
- name: Run Unit Tests
  run: dotnet test --configuration Release --no-restore --verbosity normal --logger "trx" --collect:"XPlat Code Coverage"

- name: Upload Test Results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-results
    path: "**/TestResults/*.trx"

- name: Upload Coverage Reports
  uses: codecov/codecov-action@v3
  with:
    files: "**/coverage.cobertura.xml"
    flags: backend
```

**Coverage Goals**:

- Initial target: 60% overall coverage
- Critical services: 80% coverage
- Controllers: 70% coverage

---

### 7. Add Integration Tests

**Effort**: 2 weeks (setup) + ongoing  
**Impact**: 🧪 Test full request/response cycles

**Implementation**:

1. **Setup WebApplicationFactory**:

```csharp
public class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DB with test container
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Port=5433;Database=lilith_test");
            });
        });
    }
}
```

2. **Example integration test**:

```csharp
public class ExerciseApiTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly ApiTestFactory _factory;

    public ExerciseApiTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExercise_WithValidId_ReturnsOk()
    {
        // Arrange
        var exerciseId = await SeedTestExercise();

        // Act
        var response = await _client.GetAsync($"/api/exercise/{exerciseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exercise = await response.Content.ReadFromJsonAsync<Exercise>();
        exercise.Should().NotBeNull();
    }
}
```

3. **Use Testcontainers for PostgreSQL**:

```bash
dotnet add package Testcontainers.PostgreSql
```

---

## 🟡 Phase 3: Advanced CI/CD (1-2 weeks)

### 8. Add Post-Deployment Health Validation

**Effort**: 2 hours  
**Impact**: 🎯 Auto-rollback on failures

**Implementation**:

Update `.github/workflows/docker-image-ci.yml`:

```yaml
- name: Deploy to server via SSH
  if: github.ref_name == 'dev'
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.HOST }}
    port: ${{ secrets.PORT }}
    username: ${{ secrets.USER }}
    password: ${{ secrets.PASSWORD }}
    script: |
      cd ~/lilith

      # Backup current state
      docker tag marcgurt/lilith-backend:dev marcgurt/lilith-backend:backup-$(date +%s)

      # Pull and deploy new version
      docker pull marcgurt/lilith-backend:dev
      docker compose up -d --no-deps --force-recreate lilith-backend

      # Wait for service to be ready
      echo "Waiting for service to start..."
      sleep 15

      # Health check with retries
      max_attempts=6
      attempt=0
      while [ $attempt -lt $max_attempts ]; do
        if curl -f http://localhost:5000/health; then
          echo "Health check passed"
          exit 0
        fi
        attempt=$((attempt + 1))
        echo "Health check failed, attempt $attempt/$max_attempts"
        sleep 10
      done

      # Rollback on failure
      echo "Health check failed after $max_attempts attempts. Rolling back..."
      docker compose up -d --no-deps lilith-backend
      exit 1
```

---

### 9. Add Code Quality Gates (SonarCloud)

**Effort**: 3 hours (setup) + 30 min/week review  
**Impact**: 📊 Track technical debt, enforce standards

**Implementation**:

1. **Sign up for SonarCloud** at https://sonarcloud.io

2. **Add sonar-project.properties**:

```properties
sonar.projectKey=marcgurt_lilith-backend
sonar.organization=marcgurt
sonar.sources=Api/,Application/,Domain/,Infrastructure/
sonar.tests=Api.Tests/
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
sonar.coverage.exclusions=**/Migrations/**,**/Program.cs
sonar.exclusions=**/bin/**,**/obj/**,**/Migrations/**
```

3. **Add to workflow**:

```yaml
- name: SonarCloud Scan
  uses: SonarSource/sonarcloud-github-action@master
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  with:
    args: >
      -Dsonar.projectKey=marcgurt_lilith-backend
      -Dsonar.organization=marcgurt
```

**Quality Gates**:

- Code coverage: > 60%
- Security rating: A
- Maintainability rating: A
- No critical/blocker issues

---

### 10. Version-Control Docker Compose

**Effort**: 1 hour  
**Impact**: 🗂️ Infrastructure as Code, reproducible environments

**Implementation**:

Create `docker-compose.yml`:

```yaml
version: "3.8"

services:
  lilith-backend:
    image: marcgurt/lilith-backend:${TAG:-dev}
    container_name: lilith-backend
    restart: unless-stopped
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=${ENVIRONMENT:-Development}
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - JWT__SecretKey=${JWT_SECRET}
      - JWT__Issuer=${JWT_ISSUER}
      - JWT__Audience=${JWT_AUDIENCE}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - lilith-network
    volumes:
      - ./files:/app/files

  postgres:
    image: postgres:16-alpine
    container_name: lilith-postgres
    restart: unless-stopped
    environment:
      - POSTGRES_DB=${POSTGRES_DB:-lilith}
      - POSTGRES_USER=${POSTGRES_USER:-lilith}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-lilith}"]
      interval: 10s
      timeout: 5s
      retries: 5
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - lilith-network

networks:
  lilith-network:
    driver: bridge

volumes:
  postgres-data:
```

Create `.env.example`:

```env
# Environment
ENVIRONMENT=Development
TAG=dev

# Database
DB_CONNECTION_STRING=Host=postgres;Database=lilith;Username=lilith;Password=your_password
POSTGRES_DB=lilith
POSTGRES_USER=lilith
POSTGRES_PASSWORD=your_strong_password

# JWT
JWT_SECRET=your_jwt_secret_key_min_32_chars
JWT_ISSUER=LilithBackend
JWT_AUDIENCE=LilithFrontend
```

---

## 📋 Phase 4: Polish & Optimization (Ongoing)

### 11. Add Semantic Versioning

**Effort**: 2 hours  
**Impact**: 🏷️ Better release tracking

**Implementation**:

Add to workflow:

```yaml
- name: Generate Semantic Version
  id: version
  run: |
    VERSION=$(date +'%Y.%m.%d')-${GITHUB_SHA::8}
    echo "version=$VERSION" >> $GITHUB_OUTPUT

- name: Build and Push
  uses: docker/build-push-action@v5
  with:
    tags: |
      marcgurt/lilith-backend:${{ github.ref_name }}
      marcgurt/lilith-backend:${{ steps.version.outputs.version }}
```

---

### 12. Add Performance Testing

**Effort**: 1 week  
**Impact**: 🏎️ Detect performance regressions

**Tools**: NBomber, BenchmarkDotNet, K6

---

### 13. Add Deployment Notifications

**Effort**: 30 minutes  
**Impact**: 📢 Team awareness

**Implementation**:

```yaml
- name: Notify Deployment Success
  if: success()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK_URL }}
    payload: |
      {
        "text": "✅ Lilith Backend deployed to ${{ github.ref_name }}",
        "blocks": [
          {
            "type": "section",
            "text": {
              "type": "mrkdwn",
              "text": "*Deployment Successful* :rocket:\n\n*Environment:* `${{ github.ref_name }}`\n*Commit:* <${{ github.event.head_commit.url }}|${{ github.event.head_commit.message }}>\n*Author:* ${{ github.actor }}"
            }
          }
        ]
      }
```

---

## 🎯 Success Metrics

Track these KPIs monthly:

| Metric                   | Baseline | Target   | Current |
| ------------------------ | -------- | -------- | ------- |
| Build Time               | ~8 min   | < 4 min  | -       |
| Deployment Frequency     | Manual   | Daily    | -       |
| Test Coverage            | 0%       | 60%      | -       |
| Mean Time to Recovery    | Unknown  | < 15 min | -       |
| Failed Deployments       | Unknown  | < 5%     | -       |
| Critical Vulnerabilities | Unknown  | 0        | -       |
| Code Quality Rating      | Unknown  | A        | -       |

---

## 📅 Recommended Timeline

### Week 1-2: Foundation

- ✅ Add .dockerignore
- Add health endpoints
- Add Trivy scanning
- Add build caching
- Add Dependabot

### Week 3-4: Testing Setup

- Create test projects
- Write first 20 unit tests
- Setup CI test execution
- Add code coverage reporting

### Week 5-6: Integration Tests

- Setup Testcontainers
- Write API integration tests
- Add smoke tests to deployment

### Week 7-8: Quality & Optimization

- Setup SonarCloud
- Add health check validation
- Version-control docker-compose
- Add deployment notifications

### Ongoing: Maintain & Improve

- Write tests for new features
- Review Dependabot PRs weekly
- Monitor metrics dashboard
- Iterate on CI/CD optimizations

---

## 🔗 Related Documentation

- [How to Create Endpoints](./how-to-create-endpoints.md)
- [How to Refactor Controllers to Services](./how-to-refactor-controllers-to-services.md)
- [Copilot Instructions](../.github/copilot-instructions.md)

---

## 📞 Need Help?

For questions or issues implementing these improvements:

1. Create an issue in the repository
2. Tag `@backend-team` for review
3. Reference this roadmap in discussions

---

**Remember**: Perfect is the enemy of good. Implement incrementally, test thoroughly, and iterate based on feedback.
