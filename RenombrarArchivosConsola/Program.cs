using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RenombrarArchivosConsola {
    class Program {
        static void Main(string[] args) {
            const string directorio = @"C:\Users\Ed\Desktop\Mis cosas\Anime\Fullmetal Alchemist Brotherhood";
            Console.WriteLine("Directorio de trabajo:\n{0}", directorio);

            var rutas = Directory.GetFiles(directorio, "*Fullmetal Alchemist Brotherhood*", System.IO.SearchOption.TopDirectoryOnly);
            Console.WriteLine("Archivos filtrados: {0}", rutas.Length);

            var regex = new Regex(@"Fullmetal Alchemist Brotherhood (?<numero>\d{2}) \[(?<hash>\w+)\]");

            var nombresNuevos = new List<string>();

            foreach (var ruta in rutas) {
                var nombre = Path.GetFileName(ruta);
                var nombreNuevo = "";
                var match = regex.Match(nombre);

                if (match.Success) {
                    var numero = match.Groups["numero"].Value;
                    var hash = match.Groups["hash"].Value;
                    nombreNuevo = string.Format("[BB] Fullmetal Alchemist Brotherhood {0} [BDrip 720p] [{1}].mkv", numero, hash);
                }

                nombresNuevos.Add(nombreNuevo);

                FileSystem.RenameFile(ruta, nombreNuevo);
            }
            
            Console.WriteLine("Presione una tecla para finalizar...");
            Console.ReadKey();
        }
    }
}
