using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;

namespace AlumnoEjemplos.Los_Borbotones.Menus
{
    public class MenuManager
    {
        #region Singleton
        private static volatile MenuManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static MenuManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new MenuManager();
                }
                return instance;
            }
        }
        #endregion

        public Pantalla pantallaActual;

        public void Init()
        {
            pantallaActual = new MainMenu();
            pantallaActual.Init();
        }

        public void Update(float elapsedTime)
        {
            pantallaActual.Update(elapsedTime);
        }

        public void Render(float elapsedTime)
        {
            pantallaActual.Render(elapsedTime);
        }

        public void close()
        {
            pantallaActual.close();
        }

        public void cargarPantalla(Pantalla pantalla)
        {
            pantallaActual.close();
            pantallaActual = pantalla;
            pantallaActual.Init();
        }
    }
}
