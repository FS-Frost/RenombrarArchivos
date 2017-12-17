using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using WPFFolderBrowser;
using RenombrarArchivosWPF.Clases;
using RenombrarArchivosWPF.Properties;
using System;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Media;

namespace RenombrarArchivosWPF {
    public partial class MainWindow : Window {
        /// <summary>
        /// Lista de archivos encontrados en el directorio.
        /// </summary>
        List<string> listaArchivos = new List<string>();
        
        /// <summary>
        /// Lista de todos los archivos listos para el grid.
        /// </summary>
        List<GridValue> listaGrid = new List<GridValue>();

        /// <summary>
        /// Lista de archivos en el grid.
        /// </summary>
        List<GridValue> listaGridFiltrada = new List<GridValue>();

        /// <summary>
        /// Lista para deshacer el cambio de nombre.
        /// </summary>
        List<GridValue> listaGridDeshacer = new List<GridValue>();

        public MainWindow() {
            InitializeComponent();
            
            // Ingresando consejos.
            txt_Filtro.ToolTip = "Ingrese expresión regular para C#.";
            txt_Nombre.ToolTip = "Ingrese el nombre tipo para los archivos. Coloque un \"|n|\" donde quiera que aparezca la numeración.\nEjemplo: Video |n| -> Video 01, Video 02, etc.";
            btn_SeleccionarCarpeta.ToolTip = "Presione para seleccionar la carpeta con los archivos a renombrar.";
            btn_CambiarNombres.ToolTip = "Presione para renombrar los archivos seleccionados.";
            btn_Deshacer.ToolTip = "Presione para restaurar los nombres de los archivos, sólo en caso de que no se modificaran externamente.";

            // Ingresando datos iniciales.
            txt_Nombre.Text = "Nombre |n|";


            // Cargando último directorio de trabajo.
            lbl_Directorio.Content = Settings.Default.UltimoDirectorio;

            // Verificando que el directorio exista para proseguir.
            if (Directory.Exists(lbl_Directorio.Content.ToString())) {
                listaArchivos = Directory.GetFiles(lbl_Directorio.Content.ToString()).ToList();
                listaGrid.AddRange(from x in listaArchivos
                                   select new GridValue(x));
                ActualizarGrid();
                ActivarNotificacion("Último directorio cargado automágicamente.", Colores.Negro);
            }
        }

        /// <summary>
        /// Invoca diálogo para establecer carpeta de trabajo.
        /// </summary>
        public void SeleccionarCarpeta() {
            DesactivarNotificacion();

            // Generando ventana.
            var dialogo = new WPFFolderBrowserDialog();

            // Invocando diálogo para interacción con usuario.
            var resultado = dialogo.ShowDialog();

            try {
                // Se actúa si el resultado no es nulo.
                if (resultado.HasValue) {
                    // Colocando directorio en el label.
                    // "FileName" corresponde al directorio.
                    // Es un error de implementación de WPFFolderBrowser.
                    lbl_Directorio.Content = dialogo.FileName;

                    // Colocando como último directorio en la configuración.
                    Settings.Default.UltimoDirectorio = dialogo.FileName;
                    
                    // Obteniendo lista string de archivos en el directorio.
                    listaArchivos = Directory.GetFiles(lbl_Directorio.Content.ToString()).ToList();
                    
                    // Generando lista grid con todos los archivos.
                    listaGrid = (from x in listaArchivos
                                       select new GridValue(x)).ToList();
                }
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// Indica si un archivo es válido según el filtro.
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public bool EsArchivoValido(string nombre) {
            DesactivarNotificacion();

            // Verificando que la palabra coincida en el filtro.
            try {
                // Regex del filtro.
                var regexFiltro = new Regex(txt_Filtro.Text);
                var matchFiltro = regexFiltro.Match(nombre);

                // Regex de numeración.
                var regexNumeracion = new Regex(@"(\d+)");
                var matchNumeracion = regexNumeracion.Match(nombre);

                if (matchFiltro.Success && matchNumeracion.Success) {
                    return true;
                }
            }
            catch (Exception) {
                // Si no coincide, se avisa error de sintaxis.
                ActivarNotificacion("Expresión regular de filtro incorrecta. Revisar sintaxis.", Colores.Rojo);
            }
            return false;
        }

        /// <summary>
        /// Actualiza los elementos en el grid.
        /// </summary>
        private void ActualizarGrid() {
            // Inicializando lista grid filtrada.
            listaGridFiltrada = new List<GridValue>();

            // Agregando archivos válidos según filtro en la lista grid filtrada.
            listaGridFiltrada.AddRange(from x in listaGrid
                                       where EsArchivoValido(x.NombreActual) == true
                                       select x);
            
            // Estableciendo las líneas como contenido del grid.
            dataGrid_Archivos.ItemsSource = null;
            dataGrid_Archivos.ItemsSource = listaGridFiltrada;

            // Actualizando barra de estado.
            ActualizarBarraEstado();
        }

        /// <summary>
        /// Actualiza los valores en la barra de estado.
        /// </summary>
        private void ActualizarBarraEstado() {
            txtb_NumArchivos.Text = "Archivos: " + listaGrid.Count;
            txtb_NumFiltrados.Text = "Filtrados: " + listaGridFiltrada.Count;
        }

        private void btn_SeleccionarCarpeta_Click(object sender, RoutedEventArgs e) {
            SeleccionarCarpeta();
            ActualizarGrid();

            ActivarNotificacion("Archivos del directorio cargados con éxito.", Colores.Negro);
        }
        
        private void Window_Closed(object sender, EventArgs e) {
            // Guardando configuración de la aplicación.
            Settings.Default.Save();
        }

        /// <summary>
        /// Genera un diálogo de advertencia con el texto ingresado.
        /// </summary>
        /// <param name="texto">Texto a mostrar.</param>
        /// <param name="args">Argumentos para string.Format().</param>
        static public void Mensaje(string texto, params object[] args) {
            MessageBox.Show(string.Format(texto, args), "Advertencia", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void txt_Filtro_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            DesactivarNotificacion();

            // Se filtra sólo al presionar la tecla Enter.
            if (e.Key == System.Windows.Input.Key.Enter) {
                ActivarNotificacion("Filtro aplicado con éxito.", Colores.Negro);
                ActualizarGrid();
            } else {
                ActivarNotificacion("Presione Enter dentro del filtro para aplicar.", Colores.Negro);
            }
        }
        
        private void txt_Nombre_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            DesactivarNotificacion();

            // Verificando la expresión contenga "|n|". Si no está, se anula el flujo.
            if (!txt_Nombre.Text.Contains("|n|")) {
                ActivarNotificacion("El filtro debe contener \"|n|\", que indica la numeración.", Colores.Rojo);
                return;
            }

            // Verificando que se presione Enter para actuar.
            if (e.Key == System.Windows.Input.Key.Enter) {
                // Agregando nombre nuevo a cada elemento en el grid, donde se asocia su captura con la expresión.
                listaGridFiltrada.ForEach(x => x.NombreNuevo = string.Format(txt_Nombre.Text.Replace("|n|", "{0}"), x.Captura) + x.Extension);

                // Actualizando lista del grid, que ahora cuenta con los nombres nuevos ingresados.
                dataGrid_Archivos.ItemsSource = null;
                dataGrid_Archivos.ItemsSource = listaGridFiltrada;

                ActivarNotificacion("Nombres aplicados con éxito.", Colores.Negro);
            }
        }

        /// <summary>
        /// Actualiza la notificación en la barra de estado.
        /// </summary>
        /// <param name="texto">Texto a mostrar.</param>
        /// <param name="color">Color del texto.</param>
        private void ActivarNotificacion(string texto, Colores color) {
            // Obteniendo color.
            var c = Colors.Black;
            switch (color) {
                case Colores.Rojo:
                    c = Colors.DarkRed;
                    break;
                case Colores.Verde:
                    c = Colors.ForestGreen;
                    break;
                default:
                    break;
            }

            // Aplicando color.
            txtb_Notificación.Foreground = new SolidColorBrush(c);

            // Ingresando texto.
            txtb_Notificación.Text = texto;
        }

        /// <summary>
        /// Anula el texto en la notificación de la barra de estado.
        /// </summary>
        private void DesactivarNotificacion() {
            txtb_Notificación.Text = string.Empty;
        }

        private void btn_CambiarNombres_Click(object sender, RoutedEventArgs e) {
            DesactivarNotificacion();

            try {
                // Estableciendo elementos del grid que se modificarán.
                var listaModificar = new List<GridValue>();
                listaModificar.AddRange(from x in listaGridFiltrada
                                        where x.Modificar == true
                                        where ArchivoExiste(x.NombreActual) == true
                                        where !ArchivoExiste(x.NombreNuevo) == true
                                        select x);

                // Se copia la lista a modificar para tener la opción de deshacer.
                listaGridDeshacer = listaModificar;

                // Renombrando archivos.
                listaModificar.ForEach(x =>
                FileSystem.RenameFile(lbl_Directorio.Content.ToString() +
                    "\\" +
                    x.NombreActual,
                    x.NombreNuevo)
                );

                ActivarNotificacion("¡Operación realizada con éxito!", Colores.Verde);
            }
            catch (Exception) {
                Mensaje("¡Los archivos en el directorio difieren de los cargados!");
            }
        }

        private void btn_Deshacer_Click(object sender, RoutedEventArgs e) {
            DesactivarNotificacion();

            // Se usa la misma lógica de btn_CambiarNombres_Click, pero con los valores inversos.
            try {
                var listaModificar = new List<GridValue>();
                listaModificar.AddRange(from x in listaGridDeshacer
                                        where ArchivoExiste(x.NombreNuevo) == true
                                        where !ArchivoExiste(x.NombreActual) == true
                                        select x);

                listaModificar.ForEach(x =>
                FileSystem.RenameFile(lbl_Directorio.Content.ToString() +
                        "\\" +
                        x.NombreNuevo,
                        x.NombreActual)
                );

                ActivarNotificacion("Operación deshecha con éxito.", Colores.Verde);
            }
            catch (Exception) {
                Mensaje("¡Los archivos en el directorio difieren de los cargados!");
                throw;
            }
        }

        /// <summary>
        /// Indica si el archivo existe en el sistema según el directorio de trabajo.
        /// </summary>
        /// <param name="nombre">Nombre del archivo incluyendo extensión.</param>
        /// <returns></returns>
        private bool ArchivoExiste(string nombre) {
            var ruta = lbl_Directorio.Content.ToString() + "\\" + nombre;
            return File.Exists(ruta);
        }
    }
}
