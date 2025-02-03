using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_API_Server.Data;
using ToDo_API_Server.Models;

namespace ToDo_API_Server.Controllers
{
    /// <summary>
    /// ToDoEntriesController - Api Controller for ToDoEntries
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ToDoEntriesController : ControllerBase
    {
        /// <summary>
        /// ApplicationDbContext - Main Database context for ToDo-API-Server 
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// ToDoEntriesController constructor that accepts ApplicationDbContext param
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        public ToDoEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: ToDoEntries
        /// </summary>
        /// <returns>List<ToDoEntry></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoEntry>>> GetToDoEntries()
        {
            // Return list of ToDo Entries
            return await _context.ToDoEntries.ToListAsync();
        }

        /// <summary>
        /// GET: ToDoEntries/Guid
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <returns>ToDoEntry</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoEntry>> GetToDoEntry(Guid id)
        {
            // Find ToDoEntry by Guid id param
            var toDoEntry = await _context.ToDoEntries.FindAsync(id);

            // Check if Valid Entry Exists
            if (toDoEntry == null)
            {
                return NotFound();
            }

            // Return ToDo Entry
            return toDoEntry;
        }

        /// <summary>
        /// PUT: ToDoEntries/Guid 
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <param name="toDoEntry">ToDoEntry from body of request</param>
        /// <returns>Empty Response</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToDoEntry(Guid id, ToDoEntry toDoEntry)
        {
            // Check param match id must equal toDoEntry.Id
            if (id != toDoEntry.Id)
            {
                return BadRequest();
            }

            // Set UpdatedBy 

            toDoEntry.UpdatedBy = GetUserId();
#if DEBUG
            // Set Update Time for inMemory database
            toDoEntry.UpdateTime = DateTime.Now;
#endif

            // Change State of db entity but ignore createdBy and createdTime
            _context.Entry(toDoEntry).State = EntityState.Modified;
            _context.Entry(toDoEntry).Property(p => p.CreatedBy).IsModified = false;
            _context.Entry(toDoEntry).Property(p => p.CreateTime).IsModified = false;

            // Try to save changes to database
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if Entry exists and return not found or throw exception
                if (!ToDoEntryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Return Empty Response
            return NoContent();
        }

        /// <summary>
        /// POST: ToDoEntries
        /// </summary>
        /// <param name="toDoEntry">ToDoEntry from body of request</param>
        /// <returns>CreatedAtAction GetToDoEntry</returns>
        [HttpPost]
        public async Task<ActionResult<ToDoEntry>> PostToDoEntry(ToDoEntry toDoEntry)
        {
            // Set CreatedBy 
            toDoEntry.CreatedBy = GetUserId();
#if DEBUG
            // Set Create Time for inMemory database
            toDoEntry.CreateTime = DateTime.Now;
#endif
            // Add ToDoEntry to DbSet and Save Changes
            _context.ToDoEntries.Add(toDoEntry);
            await _context.SaveChangesAsync();

            // Return CreatedAtAction
            return CreatedAtAction("GetToDoEntry", new { id = toDoEntry.Id }, toDoEntry);
        }

        /// <summary>
        /// DELETE: ToDoEntries/Guid
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <returns>Empty Response</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDoEntry(Guid id)
        {
            // Find ToDoEntry with id and check if valid entry
            var toDoEntry = await _context.ToDoEntries.FindAsync(id);
            if (toDoEntry == null)
            {
                return NotFound();
            }

            // Remove ToDoEntry from DbSet and Save Changes
            _context.ToDoEntries.Remove(toDoEntry);
            await _context.SaveChangesAsync();

            // Return Empty Response
            return NoContent();
        }

        /// <summary>
        /// ToDoEntryExists - Private Method to check if ToDoEntry exists in database context
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <returns>True or False</returns>
        private bool ToDoEntryExists(Guid id)
        {
            // Return True or False with .Any()
            return _context.ToDoEntries.Any(e => e.Id == id);
        }

        /// <summary>
        /// GetUserId - Private Method to retrieve User Id from user claims 
        /// </summary>
        /// <returns>UserId currently logged in</returns>
        private Guid GetUserId()
        {
            // init local userId variable, try parsing id from User Claim then return
            Guid userId;
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
            return userId;
        }
    }
}
