using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class GameManager
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
        List<Enemy> enemies = new List<Enemy>();
        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public int ScreenHeight, ScreenWidth;
        TgcScene scene;
        float SPAWN_TIME = 1f;
        float SPAWN_TIME_COUNTER = 0f;


        internal void Init()
        {
            player1.Init();

            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;
        }

        internal void Update(float elapsedTime)
        {
            SPAWN_TIME_COUNTER = SPAWN_TIME_COUNTER + elapsedTime;
            player1.Update(elapsedTime);
            if (SPAWN_TIME_COUNTER > SPAWN_TIME) { 
                Enemy enemigo = new Enemy_lvl_1();
                enemies.Add(enemigo);
                enemigo.Init();
                SPAWN_TIME_COUNTER = 0;
            }

        }

        internal void Render(float elapsedTime)
        {
            player1.Render(elapsedTime);
            scene.renderAll();
            foreach(Enemy enemigo in enemies ){
                enemigo.Render(elapsedTime);
            }
        }

        internal void close()
        {
            scene.disposeAll();
            player1.dispose();
        }

        public void fireWeapon()
        {
            int sh = GameManager.Instance.ScreenHeight;
            int sw = GameManager.Instance.ScreenWidth;

            Vector3 halfScreen = new Vector3(sw / 2, sh / 2, 0);
            TgcRay ray = new TgcRay(GuiController.Instance.FpsCamera.Position, GuiController.Instance.FpsCamera.LookAt);
            Vector3 newPosition = new Vector3(0, 0, 0);
            foreach(Enemy enemy in enemies){
                if (TgcCollisionUtils.intersectRayAABB(ray, enemy.mesh.BoundingBox, out newPosition))
                {
                    //enemy.die();
                }
            }
        }
    }
}
