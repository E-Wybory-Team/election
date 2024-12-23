﻿using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    [ElectionDateRange]
    public class ElectionViewModel
    {
        [Required(ErrorMessage ="ID wyborów jest obowiązkowe")] public int IdElection { get; set; } = 0;
        [Required(ErrorMessage = "Data rozpoczęcia wyborów jest obowiązkowa")] public DateTime ElectionStartDate { get; set; } = DateTime.MinValue;
        [Required(ErrorMessage = "Data zakończenia wyborów jest obowiązkowa")] public DateTime ElectionEndDate { get; set; } = DateTime.MinValue;
        [Range(1,2, ErrorMessage = "Tura wyborów musi być 1 lub 2")]
        public int ElectionTour { get; set; } = 1;
        [Required(ErrorMessage = "Rodzaj wyborów jest obowiązkowy")] public int IdElectionType { get; set; } = 0;
        [Required(ErrorMessage = "Data rozpoczęcia wyborów jest obowiązkowa")]
        public string DateOfStartString
        {
            get => ElectionStartDate != DateTime.MinValue
                ? ElectionStartDate.ToString("yyyy-MM-dd HH:mm")
                : string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var parsedDate))
                {
                    ElectionStartDate = parsedDate;
                }
            }
        }
        [Required(ErrorMessage = "Data zakończenia wyborów jest obowiązkowa")]
        public string DateOfEndString
        {
            get => ElectionEndDate != DateTime.MinValue
                ? ElectionEndDate.ToString("yyyy-MM-dd HH:mm")
                : string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out var parsedDate))
                {
                    ElectionEndDate = parsedDate;
                }
            }
        }

    }

    public class ElectionViewModelShort
    {
        [Required] public int IdElectionType { get; set; } = 0;
        [Required] public string ElectionType { get; set; } = string.Empty;
    }
}
