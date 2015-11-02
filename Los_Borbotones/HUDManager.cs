using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class HUDManager
    {
        #region Singleton
        private static volatile HUDManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static HUDManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new HUDManager();
                }
                return instance;
            }
        }
        #endregion

        public void Init()
        {
            
        }

        public void Update(float elapsedTime)
        {
            
        }

        public void Render(float elapsedTime)
        {
            
        }

        public void close()
        {
            
        }
    }
}
