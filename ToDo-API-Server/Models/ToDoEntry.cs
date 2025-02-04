using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDo_API_Server.Models
{
    /// <summary>
    /// ToDoEntry Class Model - Model of a ToDo Entry 
    /// </summary>
    [Index(nameof(Id), nameof(Status), nameof(CreatedBy))]
    public class ToDoEntry
    {
        /// <summary>
        /// Id of ToDo Entry - Guid - Database Generated Identity
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("The ToDo Enrty Identifier", ReadOnly = true)]
        public Guid Id { get; set; }
        /// <summary>
        /// Title of ToDo Entry - String(128) - Required
        /// </summary>
        [Required]
        [StringLength(128)]
        [DefaultValue("Title of ToDo Entry - Limit of 128 characters")]
        [SwaggerSchema("Title of ToDo Enrty")]
        public string? Title { get; set; }
        /// <summary>
        /// Description of ToDo Entry - String(512) - Required
        /// </summary>
        [Required]
        [StringLength(512)]
        [DefaultValue("Description of ToDo Entry - Limit of 512 characters")]
        [SwaggerSchema("Description of ToDo Enrty")]
        public string? Description { get; set; }
        /// <summary>
        /// Status of ToDo Entry - Enum<ToDoStatus> - Required
        /// </summary>
        [DefaultValue("ToDo")]
        [SwaggerSchema("Status of ToDo Enrty")]
        public ToDoStatus Status { get; set; }
        /// <summary>
        /// Is ToDo Entry Pending Aproval - Boolean - Nullable 
        /// </summary>
        [SwaggerSchema("Is ToDo Entry Pending Aproval", ReadOnly = true)]
        public bool? PendingApproval { get; set; }
        /// <summary>
        /// Create Time of ToDo Entry - DateTime - Required 
        /// </summary>
        [SwaggerSchema("Creation Time of ToDo Enrty" , ReadOnly = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Uptate Time of ToDo Entry - DateTime - Nullable 
        /// </summary>
        [SwaggerSchema("Update Time of ToDo Enrty", ReadOnly = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// Create Time of ToDo Entry - DateTime - Nullable 
        /// </summary>
        [SwaggerSchema("Approval Time of ToDo Enrty", ReadOnly = true)]
        public DateTime? ApprovedTime { get; set; }
        /// <summary>
        /// Id of User that created ToDo Entry - Guid - Required
        /// </summary>
        [SwaggerSchema("Creator Id of ToDo Enrty", ReadOnly = true)]
        public Guid CreatedBy { get; set; }
        /// <summary>
        /// Id of User that updated ToDo Entry - Guid - Nullable
        /// </summary>
        [SwaggerSchema("Updater Id of ToDo Enrty", ReadOnly = true)]
        public Guid? UpdatedBy { get; set; }
        /// <summary>
        /// Id of User that approved ToDo Entry - Guid - Nullable
        /// </summary>
        [SwaggerSchema("Approver Id of ToDo Enrty", ReadOnly = true)]
        public Guid? ApprovedBy { get; set; }
    }

    /// <summary>
    /// ToDoStatus Enum - One of the following [ 'ToDo', 'Doing', 'Done' ]
    /// </summary>
    [SwaggerSchema("[ 0 = ToDo, 1 = Doing, 2 = Done ]")]
    public enum ToDoStatus
    {
        /// <summary>
        /// ToDo - The ToDo Entry has been Created and is Pending commencement 
        /// </summary>
        ToDo,
        /// <summary>
        /// Doing - The ToDo Entry is in Progress or Pending Approval
        /// </summary>
        Doing,
        /// <summary>
        /// Done - The ToDo Entry has been Approved and is Completed
        /// </summary>
        Done
    }
}
