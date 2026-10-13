using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIntegradorFinal.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Fundamental para que MySQL guarde precios con decimales
        public decimal Precio { get; set; }

        public int Stock { get; set; }
        public int VolumenMl { get; set; }
        public string Concentracion { get; set; }
        public string ImagenUrl { get; set; }

        // Clave foránea y navegación hacia Marca
        public int IdMarca { get; set; }
        [ForeignKey("IdMarca")]
        public virtual Marca Marca { get; set; }

        // Clave foránea y navegación hacia Categoría
        public int IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public virtual Categoria Categoria { get; set; }
    }
}
