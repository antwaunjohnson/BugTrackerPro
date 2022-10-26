﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BugTrackerPro.Models;

public class Project
{
    public int Id { get; set; }

    [DisplayName("Company")]
    public int CompanyId { get; set; }

    public int ProjectPriorityId { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Project Name")]
    public string? Name { get; set; }

    [DisplayName("Description")]
    public string? Description { get; set; }

    [DisplayName("Start Date")]
    [DataType(DataType.Date)]
    public DateTimeOffset? StartDate { get; set; }

    [DisplayName("End Date")]
    [DataType(DataType.Date)]
    public DateTimeOffset? EndDate { get; set; }

    [NotMapped]
    [DataType(DataType.Upload)]
    public IFormFile? ImageFormFile { get; set; }

    [DisplayName("File Name")]
    public string? ImageFileName { get; set; }

    public byte[]? ImageFileData { get; set; }

    [DisplayName("File Extension")]
    public string? ImageFileContentType { get; set; }

    [DisplayName("Archived")]
    public bool Archived { get; set; }

    public virtual Company? Company { get; set; }
    public virtual ProjectPriority? ProjectPriority { get; set; }

    public virtual ICollection<BTProUser>? Members { get; set; } = new HashSet<BTProUser>();

    public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();
}
