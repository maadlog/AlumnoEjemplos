using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class GameManager
    {
        #region Singleton
        private static volatile GameManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new GameManager();
                }
                return instance;
            }
        }
        #endregion

        Player1 player1 = new Player1();
        List<Enemy> enemies;
        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public static int ScreenHeight, ScreenWidth;
        TgcScene scene;

        internal void Init()
        {
            player1.Init();

            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

        }

        internal void Update(float elapsedTime)
        {
            player1.Update(elapsedTime);
        }

        internal void Render(float elapsedTime)
        {
            player1.Render(elapsedTime);
            scene.renderAll();
        }
    }
}
