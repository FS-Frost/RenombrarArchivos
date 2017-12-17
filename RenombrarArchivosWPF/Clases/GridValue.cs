using System.IO;
using System.Text.RegularExpressions;

namespace RenombrarArchivosWPF.Clases {
    public class GridValue {
        public string NombreActual { get; set; }
        public string NombreNuevo { get; set; }
        public bool Modificar { get; set; }
        public string Captura { get; set; }
        public string Extension { get; set; }

        public GridValue(string ubicacion) {
            NombreActual = Path.GetFileName(ubicacion);
            NombreNuevo = "";
            Modificar = true;
            Captura = CapturarNumeracion(NombreActual);
            Extension = Path.GetExtension(ubicacion);
        }

        private string CapturarNumeracion(string nombre) {
            var regex = new Regex(@"(\d+)");
            var match = regex.Match(nombre);
            if (match.Success) {
                return match.Value;
            }
            return "";
        }
    }
}
