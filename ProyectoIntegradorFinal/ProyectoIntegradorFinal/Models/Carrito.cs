using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIntegradorFinal.Models
{
    public class Carrito
    {
        [Key]
        public int IdCarrito { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<ItemCarrito> Items { get; set; }
    }
}
