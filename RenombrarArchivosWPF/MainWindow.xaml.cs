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
            txt_Nombre.ToolTip = "Ingrese expresión regular para C#.\nColoque un \"|n|\" donde quiera que aparezca la numeración.\nEjemplo: Video |n| -> Video 01, Video 02, etc.";
            btn_SeleccionarCarpeta.ToolTip = "Presione para seleccionar la carpeta con los archivos a renombrar.";
            btn_CambiarNombres.ToolTip = "Presione para renombrar los archivos seleccionados.";
            btn_Deshacer.ToolTip = "Presione para restaurar los nombres de los archivos, sólo en caso de que no se modificaran externamente.";

            // Ingresando datos iniciales.
            txt_Nombre.Text = "Nombre |n|";


            // Cargando último directorio de trabajo.
            lbl_Directorio.Content = Settings.Default.UltimoDirectorio;
            listaArchivos = Directory.GetFiles(lbl_Directorio.Content.ToString()).ToList();
            listaGrid.AddRange(from x in listaArchivos
                               select new GridValue(x));
            ActualizarGrid();

            ActivarNotificacion("Último directorio cargado automágicamente.", Colores.Negro);
        }

        public void SeleccionarCarpeta() {
            DesactivarNotificacion();

            // Generando ventana.
            var dialogo = new WPFFolderBrowserDialog();

            // Invocando diálogo para interacción con usuario.
            var resultado = dialogo.ShowDialog();

            try {
                if (resultado.HasValue) {
                    lbl_Directorio.Content = dialogo.FileName;
                    Settings.Default.UltimoDirectorio = dialogo.FileName;
                    
                    listaArchivos = Directory.GetFiles(lbl_Directorio.Content.ToString()).ToList();
                    listaGrid = (from x in listaArchivos
                                       select new GridValue(x)).ToList();
                }
            }
            catch (Exception) {
            }
        }
        
        public bool EsArchivoValido(string nombre) {
            DesactivarNotificacion();

            // Verificando que la palabra coincida en el filtro.
            try {
                var regex = new Regex(txt_Filtro.Text);
                var match = regex.Match(nombre);
                if (match.Success) {
                    return true;
                }
            }
            catch (Exception) {
                // Si no coincide, se avisa error de sintaxis.
                ActivarNotificacion("Expresión regular de filtro incorrecta. Revisar sintaxis.", Colores.Rojo);
            }
            return false;
        }

        private void ActualizarGrid() {
            listaGridFiltrada = new List<GridValue>();
            listaGridFiltrada.AddRange(from x in listaGrid
                                       where EsArchivoValido(x.NombreActual) == true
                                       select x);
            // Estableciendo las líneas como contenido del grid.
            dataGrid_Archivos.ItemsSource = null;
            dataGrid_Archivos.ItemsSource = listaGridFiltrada;

            ActualizarBarraEstado();
        }

        private void ActualizarBarraEstado() {
            // Actualizando barra de estado.
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

            if (!txt_Nombre.Text.Contains("|n|")) {
                ActivarNotificacion("El filtro debe contener \"|n|\", que indica la numeración.", Colores.Rojo);
                return;
            }

            if (e.Key == System.Windows.Input.Key.Enter) {
                listaGridFiltrada.ForEach(x => x.NombreNuevo = string.Format(txt_Nombre.Text.Replace("|n|", "{0}"), x.Captura) + x.Extension);
                dataGrid_Archivos.ItemsSource = null;
                dataGrid_Archivos.ItemsSource = listaGridFiltrada;

                ActivarNotificacion("Nombres aplicados con éxito.", Colores.Negro);
            }
        }

        private void ActivarNotificacion(string texto, Colores color) {
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
            txtb_Notificación.Foreground = new SolidColorBrush(c);
            txtb_Notificación.Text = texto;
        }

        private void DesactivarNotificacion() {
            txtb_Notificación.Text = string.Empty;
        }

        private void btn_CambiarNombres_Click(object sender, RoutedEventArgs e) {
            DesactivarNotificacion();

            try {
                var listaModificar = new List<GridValue>();
                listaModificar.AddRange(from x in listaGridFiltrada
                                        where x.Modificar == true
                                        where ArchivoExiste(x.NombreActual) == true
                                        where ArchivoNoExiste(x.NombreNuevo) == true
                                        select x);

                listaGridDeshacer = listaModificar;

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

            try {
                var listaModificar = new List<GridValue>();
                listaModificar.AddRange(from x in listaGridDeshacer
                                        where ArchivoExiste(x.NombreNuevo) == true
                                        where ArchivoNoExiste(x.NombreActual) == true
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

        private bool ArchivoExiste(string nombre) {
            var ruta = lbl_Directorio.Content.ToString() + "\\" + nombre;
            return File.Exists(ruta);
        }

        private bool ArchivoNoExiste(string nombre) {
            var ruta = lbl_Directorio.Content.ToString() + "\\" + nombre;
            return !File.Exists(ruta);
        }
    }
}
