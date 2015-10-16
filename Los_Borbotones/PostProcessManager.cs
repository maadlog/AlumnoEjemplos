using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.Fog;
using Microsoft.DirectX.Direct3D;
using System.Collections;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Shaders;
using AlumnoEjemplos.Los_Borbotones;

namespace AlumnoEjemplo.Los_Borbotones
{
    public class PostProcessManager
    {
        #region Singleton
        private static volatile PostProcessManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static PostProcessManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new PostProcessManager();
                }
                return instance;
            }
        }
        #endregion
        
        internal void Init()
        {
           
        }

        internal void Update(float elapsedTime)
        {
            
        }

        internal void Render(float elapsedTime)
        {
        }

        internal void close()
        {
        }

    }
}
