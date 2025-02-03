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
        [SwaggerIgnore]
        public Guid Id { get; set; }
        /// <summary>
        /// Title of ToDo Entry - String(128) - Required
        /// </summary>
        [Required]
        [StringLength(128)]
        [DefaultValue("Title of ToDo Entry - Limit of 128 characters")]
        public string? Title { get; set; }
        /// <summary>
        /// Description of ToDo Entry - String(512) - Required
        /// </summary>
        [Required]
        [StringLength(512)]
        [DefaultValue("Description of ToDo Entry - Limit of 512 characters")]
        public string? Description { get; set; }
        /// <summary>
        /// Status of ToDo Entry - Enum<ToDoStatus> - Required
        /// </summary>
        [DefaultValue("ToDo")]
        public ToDoStatus Status { get; set; }
        /// <summary>
        /// Is ToDo Entry Pending Aproval - Boolean - Nullable 
        /// </summary>
        [SwaggerIgnore]
        public bool? PendingApproval { get; set; }
        /// <summary>
        /// Create Time of ToDo Entry - DateTime - Required 
        /// </summary>
        [SwaggerIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Uptate Time of ToDo Entry - DateTime - Nullable 
        /// </summary>
        [SwaggerIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// Create Time of ToDo Entry - DateTime - Nullable 
        /// </summary>
        [SwaggerIgnore]
        public DateTime? ApprovedTime { get; set; }
        /// <summary>
        /// Id of User that created ToDo Entry - Guid - Required
        /// </summary>
        public Guid CreatedBy { get; set; }
        /// <summary>
        /// Id of User that updated ToDo Entry - Guid - Nullable
        /// </summary>
        [SwaggerIgnore]
        public Guid? UpdatedBy { get; set; }
        /// <summary>
        /// Id of User that approved ToDo Entry - Guid - Nullable
        /// </summary>
        [SwaggerIgnore]
        public Guid? ApprovedBy { get; set; }
    }

    /// <summary>
    /// ToDoStatus Enum - One of the following [ 'ToDo', 'Doing', 'Done' ]
    /// </summary>
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
