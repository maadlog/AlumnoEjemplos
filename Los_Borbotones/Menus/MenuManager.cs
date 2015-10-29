using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;

namespace AlumnoEjemplos.Los_Borbotones.Menus
{
    internal class MenuManager
    {
        #region Singleton
        private static volatile EjemploAlumno instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static EjemploAlumno Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new EjemploAlumno();
                }
                return instance;
            }
        }
        #endregion

        internal Pantalla pantallaActual;

        internal void cargarPantalla(Pantalla pantalla)
        {
            //pantallaActual.Dispose();
            GuiController.Instance.UserVars.setValue("En menus", false);
            pantallaActual = pantalla;
            pantallaActual.Init();
        }
    }
}
