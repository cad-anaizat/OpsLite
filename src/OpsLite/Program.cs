using Microsoft.EntityFrameworkCore;
using OpsLite.Data;
using OpsLite.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

// DB: use env var ConnectionStrings__DefaultConnection if provided, else appsettings (SQLite)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // If it looks like a SQL Server conn string, use SQL Server, else SQLite
    if (!string.IsNullOrWhiteSpace(conn) && conn.Contains("Initial Catalog=") || conn.Contains("Server=") || conn.Contains("Database="))
    {
        options.UseSqlServer(conn);
    }
    else
    {
        options.UseSqlite(conn ?? "Data Source=opslite.sqlite");
    }
});

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

// Minimal API (v1)
var api = app.MapGroup("/api");

api.MapGet("/workorders", async (AppDbContext db, int page = 1, int pageSize = 20, string? status = null, string? assignee = null) =>
{
    var query = db.WorkOrders.AsQueryable();

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkOrderStatus>(status, true, out var st))
        query = query.Where(w => w.Status == st);

    if (!string.IsNullOrWhiteSpace(assignee))
        query = query.Where(w => w.Assignee == assignee);

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(w => w.CreatedAtUtc)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new { total, items });
})
.WithName("ListWorkOrders");

api.MapGet("/workorders/{id:int}", async (int id, AppDbContext db) =>
{
    var wo = await db.WorkOrders
        .Include(w => w.Notes)
        .Include(w => w.Events)
        .FirstOrDefaultAsync(w => w.Id == id);
    return wo is not null ? Results.Ok(wo) : Results.NotFound();
})
.WithName("GetWorkOrder");

api.MapPost("/workorders", async (WorkOrderCreateDto dto, AppDbContext db) =>
{
    var now = DateTime.UtcNow;
    var wo = new WorkOrder
    {
        Title = dto.Title,
        Description = dto.Description,
        Priority = dto.Priority,
        Category = dto.Category,
        Status = WorkOrderStatus.Open,
        Requester = dto.Requester,
        Assignee = dto.Assignee,
        DueDateUtc = dto.DueDateUtc,
        CreatedAtUtc = now,
        UpdatedAtUtc = now
    };
    db.WorkOrders.Add(wo);
    db.WorkOrderEvents.Add(new WorkOrderEvent
    {
        WorkOrder = wo,
        Type = "Created",
        Data = $"{{\"by\":\"{dto.Requester}\"}}",
        CreatedAtUtc = now
    });
    await db.SaveChangesAsync();
    return Results.Created($"/api/workorders/{wo.Id}", wo);
})
.WithName("CreateWorkOrder");

api.MapPut("/workorders/{id:int}/status", async (int id, StatusUpdateDto dto, AppDbContext db) =>
{
    var wo = await db.WorkOrders.FindAsync(id);
    if (wo is null) return Results.NotFound();

    wo.Status = dto.Status;
    wo.UpdatedAtUtc = DateTime.UtcNow;
    db.WorkOrderEvents.Add(new WorkOrderEvent
    {
        WorkOrderId = wo.Id,
        Type = "StatusChanged",
        Data = $"{{\"to\":\"{dto.Status}\"}}",
        CreatedAtUtc = DateTime.UtcNow
    });
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateWorkOrderStatus");

api.MapPost("/workorders/{id:int}/notes", async (int id, NoteCreateDto dto, AppDbContext db) =>
{
    var wo = await db.WorkOrders.FindAsync(id);
    if (wo is null) return Results.NotFound();

    var note = new WorkOrderNote
    {
        WorkOrderId = id,
        Author = dto.Author,
        Body = dto.Body,
        CreatedAtUtc = DateTime.UtcNow
    };
    db.WorkOrderNotes.Add(note);
    db.WorkOrderEvents.Add(new WorkOrderEvent
    {
        WorkOrderId = id,
        Type = "NoteAdded",
        Data = $"{{\"by\":\"{dto.Author}\"}}",
        CreatedAtUtc = DateTime.UtcNow
    });
    await db.SaveChangesAsync();
    return Results.Created($"/api/workorders/{id}/notes/{note.Id}", note);
})
.WithName("AddWorkOrderNote");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

namespace OpsLite.Models
{
    public enum WorkOrderPriority { Low, Medium, High, Critical }
    public enum WorkOrderStatus { Open, InProgress, Resolved, Closed }

    public class WorkOrder
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
        public string Category { get; set; } = "General";
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;
        public string Requester { get; set; } = "system";
        public string? Assignee { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }

        public List<WorkOrderNote> Notes { get; set; } = new();
        public List<WorkOrderEvent> Events { get; set; } = new();
    }

    public class WorkOrderNote
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string Author { get; set; } = "system";
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        public WorkOrder? WorkOrder { get; set; }
    }

    public class WorkOrderEvent
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Data { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public WorkOrder? WorkOrder { get; set; }
    }

    public record WorkOrderCreateDto(
        string Title,
        string? Description,
        WorkOrderPriority Priority,
        string Category,
        string Requester,
        string? Assignee,
        DateTime? DueDateUtc
    );

    public record StatusUpdateDto(WorkOrderStatus Status);

    public record NoteCreateDto(string Author, string Body);
}

namespace OpsLite.Data
{
    using Microsoft.EntityFrameworkCore;
    using OpsLite.Models;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderNote> WorkOrderNotes => Set<WorkOrderNote>();
        public DbSet<WorkOrderEvent> WorkOrderEvents => Set<WorkOrderEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkOrder>()
                .HasMany(w => w.Notes)
                .WithOne(n => n.WorkOrder!)
                .HasForeignKey(n => n.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(w => w.Events)
                .WithOne(e => e.WorkOrder!)
                .HasForeignKey(e => e.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed minimal data
            modelBuilder.Entity<WorkOrder>().HasData(
                new WorkOrder { Id = 1, Title = "Sample: Replace filter", Description = "Air handling unit filter replacement", Priority = WorkOrderPriority.Medium, Category="Maintenance", Status = WorkOrderStatus.Open, Requester="anaizat", CreatedAtUtc=DateTime.UtcNow, UpdatedAtUtc=DateTime.UtcNow }
            );
        }
    }
}