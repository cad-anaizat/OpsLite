using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpsLite.Data;
using OpsLite.Models;

namespace OpsLite.Pages.WorkOrders;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    public CreateModel(AppDbContext db) { _db = db; }

    [BindProperty]
    public WorkOrderCreateDto Input { get; set; } = new("", "", WorkOrderPriority.Medium, "General", "anaizat", null, null);

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        var now = DateTime.UtcNow;
        var wo = new WorkOrder
        {
            Title = Input.Title,
            Description = Input.Description,
            Priority = Input.Priority,
            Category = Input.Category,
            Status = WorkOrderStatus.Open,
            Requester = Input.Requester,
            Assignee = Input.Assignee,
            DueDateUtc = Input.DueDateUtc,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        _db.WorkOrders.Add(wo);
        _db.WorkOrderEvents.Add(new WorkOrderEvent
        {
            WorkOrder = wo, Type = "Created", Data = $"{{\"by\":\"{Input.Requester}\"}}", CreatedAtUtc = now
        });
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}