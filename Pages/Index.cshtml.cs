using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Data;
using MyWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebApp.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<TodoItem> TodoItems { get; set; }

        public async Task OnGetAsync()
        {
            TodoItems = await _context.TodoItems.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var todoItem = new TodoItem
                {
                    Title = title,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                };

                _context.TodoItems.Add(todoItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}