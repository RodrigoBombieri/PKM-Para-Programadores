using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PKM_Project.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }
        [Required]
        [Phone]
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
        public string ImagenUrlPerfil { get; set; }
        //public List<Proyecto> Proyectos { get; set; }
    }
}
