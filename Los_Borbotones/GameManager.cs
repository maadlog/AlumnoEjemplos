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
        List<Enemy> enemies_lvl_1 = new List<Enemy>();
        List<Enemy> enemies_lvl_2 = new List<Enemy>();
        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public int ScreenHeight, ScreenWidth;        
        float SPAWN_TIME = 5f;
        float SPAWN_TIME_COUNTER = 0f;
        public Random random = new Random();
        int rand;

        TgcScene Vegetation;
        TgcSimpleTerrain terrain;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ = 100f;
        float currentScaleY = 8f;
        private List<TgcMesh> vegetation;
        TgcSprite cross;

        TgcText2d scoreText;
        float killCount = 0;
        TgcText2d specialKillText;
        float TEXT_DELAY = 0;
        float TEXT_DELAY_MAX = 2f;
        float killMultiTracker;
        float KILL_DELAY;
        float KILL_DELAY_MAX;
        float killColateralTracker;

        TgcStaticSound sound = new TgcStaticSound();
        string headshotSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/headshot.wav";

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


            //-------------User Interface------------
            //Crear texto 1, básico
            scoreText = new TgcText2d();
            scoreText.Text = "Score: " + killCount;

            cross = new TgcSprite();
            cross.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Sprites\\cross.png");

            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size tamaño = cross.Texture.Size;
            cross.Scaling = new Vector2(0.1f, 0.1f);
            Vector2 size = new Vector2( tamaño.Width * cross.Scaling.X, tamaño.Height*cross.Scaling.Y);
            cross.Position = new Vector2((screenSize.Width - size.X) / 2, (screenSize.Height - size.Y) /2);

        }

        internal void Update(float elapsedTime)
        {
            SPAWN_TIME_COUNTER = SPAWN_TIME_COUNTER + elapsedTime;
            player1.Update(elapsedTime);
            if (SPAWN_TIME_COUNTER > SPAWN_TIME) {
                rand = random.Next(1, 3);
                if (rand == 1){
                Enemy enemigo = new Enemy_lvl_1();
                enemies_lvl_1.Add(enemigo);
                enemigo.Init();
                }
                if (rand == 2)
                {
                    Enemy enemigo = new Enemy_lvl_2();
                    enemies_lvl_2.Add(enemigo);
                    enemigo.Init();
                }
                SPAWN_TIME_COUNTER = 0;
            }
            foreach (Enemy enemigo in enemies_lvl_1)
            {
                enemigo.Update(elapsedTime);
            }

            foreach (Enemy enemigo in enemies_lvl_2)
            {
                enemigo.Update(elapsedTime);
            }

            scoreText.Text = "Score: " + killCount;
            if (TEXT_DELAY > 0) { TEXT_DELAY -= elapsedTime; }
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

            foreach(Enemy enemigo in enemies_lvl_1 ){
                enemigo.Render(elapsedTime);
            }
            foreach (Enemy enemigo in enemies_lvl_2)
            {
                enemigo.Render(elapsedTime);
            }
            scoreText.render();
            if (TEXT_DELAY > 0) { specialKillText.render(); }

            player1.Render(elapsedTime);

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            cross.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();


        }

        internal void close()
        {
            Vegetation.disposeAll();
            terrain.dispose();
            player1.dispose();
            specialKillText.dispose();
        }

        public void fireWeapon()
        {
            TgcRay ray = new TgcRay(CustomFpsCamera.Instance.Position, CustomFpsCamera.Instance.LookAt - CustomFpsCamera.Instance.Position);
            Vector3 newPosition = new Vector3(0, 0, 0);
            killColateralTracker = 0;

            for (int i = enemies_lvl_1.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies_lvl_1[i].HEADSHOT_BOUNDINGBOX, out newPosition))
                {                    
                    specialKillText = new TgcText2d();
                    specialKillText.Text = "HEADSHOT!!";
                    specialKillText.Color = Color.Crimson;
                    specialKillText.Align = TgcText2d.TextAlign.CENTER;
                    specialKillText.Position = new Point(0, 100);
                    specialKillText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

                    killCount++;
                    TEXT_DELAY = TEXT_DELAY_MAX;
                    playSound(headshotSoundDir);
                }

                if (TgcCollisionUtils.intersectRayAABB(ray, enemies_lvl_1[i].mesh.BoundingBox, out newPosition))
                {
                    killCount++;
                    killColateralTracker++;
                    killMultiTracker++;
                    eliminarEnemigo_lvl_1(i);
                }                
            }

            for (int i = enemies_lvl_2.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies_lvl_2[i].mesh.BoundingBox, out newPosition))
                {
                    killCount += 3;
                    killMultiTracker++;
                    killColateralTracker++;
                    eliminarEnemigo_lvl_2(i);
                }
            }

            if (killMultiTracker == 2) {
                
            };
        }

        public void eliminarEnemigo_lvl_1(int i)
        {
            if (enemies_lvl_1.Count == 0)
            {
                Enemy enemigo = new Enemy_lvl_1();
                enemies_lvl_1.Add(enemigo);
                enemigo.Init();
            }

            enemies_lvl_1[i].dispose();
            enemies_lvl_1.Remove(enemies_lvl_1[i]);
        }

        public void eliminarEnemigo_lvl_2(int i)
        {
            if (enemies_lvl_2.Count == 0)
            {
                Enemy enemigo = new Enemy_lvl_2();
                enemies_lvl_2.Add(enemigo);
                enemigo.Init();
            }

            enemies_lvl_2[i].dispose();
            enemies_lvl_2.Remove(enemies_lvl_2[i]);
        }

        private void playSound(string dir)
        {
            sound.dispose();
            sound.loadSound(dir);
            sound.play();
        }
    }
}
