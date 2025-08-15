using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpsLite.Data;
using OpsLite.Models;

namespace OpsLite.Pages.WorkOrders;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) { _db = db; }

    public List<WorkOrder> Items { get; set; } = new();

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public string? Assignee { get; set; }

    public async Task OnGetAsync()
    {
        var q = _db.WorkOrders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Status) && Enum.TryParse<WorkOrderStatus>(Status, true, out var st))
            q = q.Where(w => w.Status == st);
        if (!string.IsNullOrWhiteSpace(Assignee))
            q = q.Where(w => w.Assignee!.Contains(Assignee));

        Items = await q.OrderByDescending(w => w.CreatedAtUtc).ToListAsync();
    }
}