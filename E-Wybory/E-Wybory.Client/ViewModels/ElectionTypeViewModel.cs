﻿using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ElectionTypeViewModel
    {
        [Required] public int IdElectionType { get; set; } = 0;
        [Required] public string ElectionTypeName { get; set; } = String.Empty;
        public string? ElectionTypeInfo { get; set; } = null;

    }
}