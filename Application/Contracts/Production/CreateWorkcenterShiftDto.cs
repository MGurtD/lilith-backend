﻿using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Production;

public class CreateWorkcenterShiftDto
{
    [Required]
    public Guid WorkcenterId { get; set; }
    [Required]
    public Guid ShiftDetailId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
}
