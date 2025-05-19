using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NotizApi.Data;
using NotizApi.Models;

namespace NotizApi.Controllers
{
    [ApiController, Authorize, Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly NotizContext _db;
        public NotesController(NotizContext db) => _db = db;

        [HttpGet]
        public async Task<IEnumerable<Note>> Get()
        {
            // Null-Forgiving, weil Authorize garantiert, dass der Claim existiert
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _db.Notes
                            .Where(n => n.UserId == userId)
                            .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await _db.Notes.FindAsync(id);
            if (note == null) 
                return NotFound();

            if (note.UserId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                return Forbid();

            return Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Note note)
        {
            note.UserId    = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            note.CreatedAt = DateTime.UtcNow;

            _db.Notes.Add(note);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Note updated)
        {
            var note = await _db.Notes.FindAsync(id);
            if (note == null) 
                return NotFound();

            if (note.UserId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                return Forbid();

            note.Title     = updated.Title;
            note.Content   = updated.Content;
            note.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _db.Notes.FindAsync(id);
            if (note == null) 
                return NotFound();

            if (note.UserId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                return Forbid();

            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
