using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProyectoIntegradorFinal.Models
{
    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }

        [Required]
        public string Nombre { get; set;}

        [JsonIgnore]
        public virtual ICollection<Producto>? Productos { get; set; }
    }
}
