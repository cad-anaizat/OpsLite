using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpsLite.Data;
using OpsLite.Models;

namespace OpsLite.Pages.WorkOrders;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailsModel(AppDbContext db) { _db = db; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public WorkOrder? Item { get; set; }

    [BindProperty]
    public string Author { get; set; } = "anaizat";

    [BindProperty]
    public string Body { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        Item = await _db.WorkOrders
            .Include(w => w.Notes)
            .Include(w => w.Events)
            .FirstOrDefaultAsync(w => w.Id == Id);
        if (Item is null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var wo = await _db.WorkOrders.FindAsync(Id);
        if (wo is null) return RedirectToPage("Index");

        var note = new WorkOrderNote { WorkOrderId = Id, Author = Author, Body = Body, CreatedAtUtc = DateTime.UtcNow };
        _db.WorkOrderNotes.Add(note);
        _db.WorkOrderEvents.Add(new WorkOrderEvent { WorkOrderId = Id, Type = "NoteAdded", Data = $"{{\"by\":\"{Author}\"}}", CreatedAtUtc = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }
}