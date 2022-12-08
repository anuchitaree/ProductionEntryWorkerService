using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionEntryWorkerService.Models
{
    public class ProdRecordReq
    {
        [Required, MaxLength(20)]
        public string ChildNumber { get; set; } = null!;

        [Required, MaxLength(25)]
        public string ProductId { get; set; } = null!;


        [Required, MaxLength(20)]
        public string MachineAssetNo { get; set; } = null!;

        [Required]
        public DateTime CurrentDateTime { get; set; }

        [Required, MaxLength(20)]
        public string PartNumber { get; set; } = null!;

        [Required, MaxLength(2)]
        public string Judgement { get; set; } = null!;

        public double CycleTime { get; set; }

    }
}
