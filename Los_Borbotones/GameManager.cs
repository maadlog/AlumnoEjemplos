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
        float SPAWN_TIME = 5f;
        float SPAWN_TIME_COUNTER = 0f;

        TgcScene Vegetation;
        TgcSimpleTerrain terrain;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ = 100f;
        float currentScaleY = 8f;
        private List<TgcMesh> vegetation;

        TgcText2d scoreText;
        float killCount = 0;

        TgcArrow arrow = new TgcArrow();

        internal void Init()
        {
            player1.Init();

            currentHeightmap = GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\" + "map1c.jpg";
            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\" + "splatting1.png";
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            terrain.loadTexture(currentTexture);

            this.vegetation = new List<TgcMesh>();
            TgcSceneLoader loader = new TgcSceneLoader();
            Vegetation = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\100%-veg-map1c-TgcScene.xml");
            vegetation = Vegetation.Meshes;

            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;

            //Crear texto 1, básico
            scoreText = new TgcText2d();
            scoreText.Text = "Score: " + killCount;

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
            foreach (Enemy enemigo in enemies)
            {
                enemigo.Update(elapsedTime);
            }

            scoreText.Text = "Score: " + killCount;
        }

        internal void Render(float elapsedTime)
        {
            terrain.render();
            //foreach (TgcMesh v in vegetation)
            int i;
            for (i = 1; i < 48; i++)
            {
                vegetation[i].render();
                //if (RenderBoundingBoxes) v.BoundingBox.render();
            }

            foreach(Enemy enemigo in enemies ){
                enemigo.Render(elapsedTime);
            }
            scoreText.render();

            player1.Render(elapsedTime);
        }

        internal void close()
        {
            Vegetation.disposeAll();
            terrain.dispose();
            player1.dispose();
        }

        public void fireWeapon()
        {
            TgcRay ray = new TgcRay(CustomFpsCamera.Instance.Position, CustomFpsCamera.Instance.LookAt - CustomFpsCamera.Instance.Position);
            Vector3 newPosition = new Vector3(0, 0, 0);

            for (int i = enemies.Count - 1; i >= 0; i--)
            { 
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies[i].mesh.BoundingBox, out newPosition))
                {
                    killCount++;

                    if (enemies.Count == 0)
                    {
                        Enemy enemigo = new Enemy_lvl_1();
                        enemies.Add(enemigo);
                        enemigo.Init();
                    }

                    enemies[i].dispose();
                    enemies.Remove(enemies[i]);
                }
            }
        }
    }
}
