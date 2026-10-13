using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIntegradorFinal.Models
{
    public class Orden
    {
        [Key]
        public int IdOrden { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [Required]
        public int IdDireccion { get; set; }

        [ForeignKey("IdDireccion")]
        public virtual Direccion Direccion { get; set; }

        public DateTime FechaOrden { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        public string Estado { get; set; }
    }
}
