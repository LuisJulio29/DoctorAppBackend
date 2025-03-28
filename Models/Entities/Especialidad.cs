using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    public class Especialidad
    {
        public int Id { get; set; }
        [StringLength(60, MinimumLength =2, ErrorMessage ="El nombre no cumple los parametros")]
        public required string Nombre { get; set; }
        [StringLength(200, MinimumLength = 2, ErrorMessage = "La descripcion no cumple los parametros")]
        public required string Descripcion { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
