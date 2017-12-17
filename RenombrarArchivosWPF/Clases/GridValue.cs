using System.IO;
using System.Text.RegularExpressions;

namespace RenombrarArchivosWPF.Clases {
    /// <summary>
    /// Clase que describe objetos a mostrar en el DataGrid.
    /// </summary>
    public class GridValue {
        /// <summary>
        /// Nombre actual del archivo.
        /// </summary>

        public string NombreActual { get; set; }
        /// <summary>
        /// Nombre que tendrá el archivo.
        /// </summary>

        public string NombreNuevo { get; set; }
        /// <summary>
        /// Indica si el archivo será modificado.
        /// </summary>
        public bool Modificar { get; set; }

        /// <summary>
        /// Indica el subconjunto del nombre actual que se ingresará en el nombre nuevo.
        /// </summary>
        public string Captura { get; set; }

        /// <summary>
        /// Extensión del archivo.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// En base a la ruta, genera un nuevo objeto con el nombre actual, la captura (que corresponde a el primer número) y la extensión. Modificar se asume verdadero, y nombre nuevo queda por determinar.
        /// </summary>
        /// <param name="ruta"></param>
        public GridValue(string ruta) {
            NombreActual = Path.GetFileName(ruta);
            NombreNuevo = "";
            Modificar = true;
            Captura = CapturarNumeracion(NombreActual);
            Extension = Path.GetExtension(ruta);
        }

        /// <summary>
        /// Captura la numeración del archivo.
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
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
