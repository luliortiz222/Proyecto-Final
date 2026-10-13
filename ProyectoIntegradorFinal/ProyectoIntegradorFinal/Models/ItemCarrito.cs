using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ProyectoIntegradorFinal.Models
{
    public class ItemCarrito
    {
        [Key]
        public int IdItem { get; set; }

        public int cantidad { get; set; }

        public int IdCarrito { get; set; }
        [ForeignKey("IdCarrito")]
        public virtual Carrito Carrito { get; set; }
        
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }


    }
}
