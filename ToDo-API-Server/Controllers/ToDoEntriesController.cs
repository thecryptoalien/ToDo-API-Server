using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
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
        /// ILogger<ToDoEntriesController> - ILogger for sending messages to stdio
        /// </summary>
        private readonly ILogger<ToDoEntriesController> _logger;

        /// <summary>
        /// ToDoEntriesController constructor that accepts ApplicationDbContext param
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        public ToDoEntriesController(ApplicationDbContext context, ILogger<ToDoEntriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: ToDoEntries
        /// </summary>
        /// <returns>List<ToDoEntry></returns>
        [HttpGet]
        [SwaggerResponse(200, "OK - The list of ToDoEntries has been retrieved")]
        [SwaggerOperation(Description = "Gets list of ToDo Entries - Requires Authorization", OperationId = "/ToDoEntries - List")]
        public async Task<ActionResult<IEnumerable<ToDoEntry>>> GetToDoEntries()
        {
            // Return list of ToDo Entries
            _logger.LogInformation("/ToDoEntries - GET - Request received from " + GetUserId());
            return await _context.ToDoEntries.ToListAsync();
        }

        /// <summary>
        /// GET: ToDoEntries/Guid
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <returns>ToDoEntry</returns>
        [HttpGet("{id}")]
        [SwaggerResponse(200, "OK - The ToDoEntry has been retrieved")]
        [SwaggerOperation(Description = "Gets ToDo Entry with Id - Requires Authorization", OperationId = "/ToDoEntries/id - Get")]
        public async Task<ActionResult<ToDoEntry>> GetToDoEntry(Guid id)
        {
            // Find ToDoEntry by Guid id param
            _logger.LogInformation("/ToDoEntries/id - GET - Request received from " + GetUserId());
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
        /// PUT: ToDoEntries/Guid,bool 
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <param name="confirm">Boolean true/false to Confirm</param>
        /// <returns>Empty Response</returns>
        [HttpPut("{id},{confirm}")]
        [Authorize(Roles = "Admin")]
        [SwaggerResponse(204, "OK - The ToDoEntry completion has been confirmed true/false")]
        [SwaggerOperation(Description = "Confirms ToDo Entry completion true/false - Requires Authorization", OperationId = "/ToDoEntries/id - Confirm")]
        public async Task<IActionResult> PutToDoEntryConfirm(Guid id, bool confirm)
        {
            // Find ToDoEntry by Guid id param
            _logger.LogInformation("/ToDoEntries/id,confirm - PUT - Request received from " + GetUserId());
            var toDoEntry = await _context.ToDoEntries.FindAsync(id);

            // Check if Valid Entry Exists
            if (toDoEntry == null)
            {
                return NotFound();
            }

            // Check If confirm is true and set approval property values
            if (confirm) 
            {
                toDoEntry.ApprovedTime = DateTime.UtcNow;
                toDoEntry.ApprovedBy = GetUserId();
                toDoEntry.Status = ToDoStatus.Done;
            }

            // Set pendingApproval false and updatedBy
            toDoEntry.PendingApproval = false;
            toDoEntry.UpdatedBy = GetUserId();

#if DEBUG
            // Set Update Time for inMemory database
            toDoEntry.UpdateTime = DateTime.UtcNow;
#endif

            // Change State of db entity but ignore createdBy and createdTime
            _context.Entry(toDoEntry).State = EntityState.Modified;

            // Try to save changes to database 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) 
            {
                throw;
            }

            // Return Empty Response
            return NoContent();
        }

        /// <summary>
        /// PUT: ToDoEntries/Guid 
        /// </summary>
        /// <param name="id">Id of ToDoEntry</param>
        /// <param name="toDoEntry">ToDoEntry from body of request</param>
        /// <returns>Empty Response</returns>
        [HttpPut("{id}")]
        [SwaggerResponse(204, "OK - The ToDoEntry has been updated")]
        [SwaggerOperation(Description = "Updates ToDo Entry - Requires Authorization", OperationId = "/ToDoEntries/id - Update")]
        public async Task<IActionResult> PutToDoEntry(Guid id, ToDoEntry toDoEntry)
        {
            // Find ToDoEntry by Guid id param and check for null
            _logger.LogInformation("/ToDoEntries/id - PUT - Request received from " + GetUserId());
            var dbToDoEntry = await _context.ToDoEntries.FindAsync(id);
            if (dbToDoEntry == null)
            { 
                return NotFound(); 
            }

            // Check for Status Update from Request body and if already approved
            if (toDoEntry.Status == ToDoStatus.Done && dbToDoEntry.ApprovedTime == null) 
            {
                toDoEntry.PendingApproval = true;
                toDoEntry.Status = ToDoStatus.Doing;
            }

            // Set UpdatedBy 
            toDoEntry.UpdatedBy = GetUserId();
#if DEBUG
            // Set Update Time for inMemory database
            toDoEntry.UpdateTime = DateTime.UtcNow;
#endif

            // Update only values given
            Type t = typeof(ToDoEntry);
            PropertyInfo[] propInfo = t.GetProperties();
            foreach (PropertyInfo pi in propInfo) 
            {
                // Get value from object in body of request
                var value = pi.GetValue(toDoEntry);
                if (value != null)
                {
                    // Check for null/new Guids and DateTimes to not overwrite those properties
                    if (!(value.GetType() == typeof(Guid) && (Guid)value == new Guid()) && !(value.GetType() == typeof(DateTime) && (DateTime)value == new DateTime()))
                    {
                        // Update value given
                        pi.SetValue(dbToDoEntry, value);
                    }                
                }
            }

            // Change State of db entity 
            _context.Entry(dbToDoEntry).State = EntityState.Modified;
            
            // Try to save changes to database
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Check if Entry still exists and return not found or throw exception
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
        [SwaggerResponse(201, "OK - The ToDoEntry has been created", typeof(ToDoEntry))]
        [SwaggerOperation(Description = "Creates new ToDo Entry - Requires Authorization", OperationId = "/ToDoEntries - Create")]
        public async Task<ActionResult<ToDoEntry>> PostToDoEntry(ToDoEntry toDoEntry)
        {
            // Check for initial Done status and prepare for approval
            _logger.LogInformation("/ToDoEntries - POST - Request received from " + GetUserId());
            if (toDoEntry.Status == ToDoStatus.Done)
            {
                toDoEntry.PendingApproval = true;
                toDoEntry.Status = ToDoStatus.Doing;
            }

            // Set CreatedBy 
            toDoEntry.CreatedBy = GetUserId();
#if DEBUG
            // Set Create Time for inMemory database
            toDoEntry.CreateTime = DateTime.UtcNow;
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
        [SwaggerResponse(204, "OK - The ToDoEntry has been deleted")]
        [SwaggerOperation(Description = "Deletes ToDo Entry with Id - Requires Authorization", OperationId = "/ToDoEntries/id - Delete")]
        public async Task<IActionResult> DeleteToDoEntry(Guid id)
        {
            // Find ToDoEntry with id and check if valid entry
            _logger.LogInformation("/ToDoEntries/id - DELETE - Request received from " + GetUserId());
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
