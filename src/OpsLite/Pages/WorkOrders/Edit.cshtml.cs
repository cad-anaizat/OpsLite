using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpsLite.Data;
using OpsLite.Models;

namespace OpsLite.Pages.WorkOrders;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    public EditModel(AppDbContext db) { _db = db; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public WorkOrderCreateDto Input { get; set; } = new("", "", WorkOrderPriority.Medium, "General", "anaizat", null, null);

    [BindProperty]
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

    public async Task<IActionResult> OnGetAsync()
    {
        var wo = await _db.WorkOrders.FindAsync(Id);
        if (wo is null) return RedirectToPage("Index");

        Input = new WorkOrderCreateDto(wo.Title, wo.Description, wo.Priority, wo.Category, wo.Requester, wo.Assignee, wo.DueDateUtc);
        Status = wo.Status;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var wo = await _db.WorkOrders.FirstOrDefaultAsync(x => x.Id == Id);
        if (wo is null) return RedirectToPage("Index");

        wo.Title = Input.Title;
        wo.Description = Input.Description;
        wo.Priority = Input.Priority;
        wo.Category = Input.Category;
        wo.Assignee = Input.Assignee;
        if (wo.Status != Status)
        {
            wo.Status = Status;
            _db.WorkOrderEvents.Add(new WorkOrderEvent
            {
                WorkOrderId = wo.Id, Type = "StatusChanged", Data = $"{{\"to\":\"{Status}\"}}", CreatedAtUtc = DateTime.UtcNow
            });
        }
        wo.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}