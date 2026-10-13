
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIntegradorFinal.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdOrden { get; set; }

        [ForeignKey("IdOrden")]
        public virtual Orden Orden { get; set; }

        public DateTime FechaPago { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public string MetodoPago { get; set; }

        [Required]
        public string EstadoPago { get; set; }
    }
}
