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
        List<Enemy> enemies = new List<Enemy>();
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
        float score = 0;
        TgcText2d specialKillText;
        float TEXT_DELAY = 0;
        float TEXT_DELAY_MAX = 2f;
        int killMultiTracker = 0;
        float KILL_DELAY = 0;
        float KILL_DELAY_MAX = 5;        

        TgcStaticSound sound = new TgcStaticSound();
        string headshotSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/headshot.wav";
        string headhunterSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/headhunter.wav";
        string doubleSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/doublekill.wav";
        string multiSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/multikill.wav";
        string ultraSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/ultrakill.wav";
        string megaSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/megakill.wav";
        string monsterSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/monsterkill.wav";
        string massacreSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/massacre.wav";
        string deniedSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Anunciador/denied.wav";

        public bool drawBoundingBoxes;

        bool zoomEnabled = false;
        float ZOOM_CONST = 0.8f; //TODO Hacer dependiente del arma
        TgcTexture normalScope;
        TgcTexture zoomedScope;

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
            int i;
            for (i = 1; i < 48; i++)
            {
                Vector3 center = vegetation[i].BoundingBox.calculateBoxCenter();
                float y;
                interpoledHeight(center.X, center.Z, out y);
                center.Y = y;

                Matrix trans = Matrix.Translation(center + new Vector3(-4f, 0, 0));
                Matrix scale = Matrix.Scaling(new Vector3(0.06f, 0.4f, 0.06f));

                vegetation[i].BoundingBox.transform(scale * trans);
            }

            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;


            //-------------User Interface------------
            //Crear texto 1, básico
            specialKillText = new TgcText2d();
            specialKillText.Color = Color.Crimson;
            specialKillText.Align = TgcText2d.TextAlign.CENTER;
            specialKillText.Position = new Point(0, 100);
            specialKillText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            scoreText = new TgcText2d();
            scoreText.Text = "Score: " + score;

         
            cross = new TgcSprite();
            normalScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Sprites\\normalScope.png");
            zoomedScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Sprites\\zoomedScope.png");
            cross.Texture = normalScope;

            refreshScopeTexture();

        }

        internal void Update(float elapsedTime)
        {
            drawBoundingBoxes = (bool)GuiController.Instance.Modifiers["DrawBoundingBoxes"];

            SPAWN_TIME_COUNTER = SPAWN_TIME_COUNTER + elapsedTime;
            player1.Update(elapsedTime);
            if (SPAWN_TIME_COUNTER > SPAWN_TIME) {
                rand = random.Next(1, 3);
                if (rand == 1){
                Enemy enemigo = new Enemy_lvl_1();
                enemies.Add(enemigo);
                enemigo.Init();
                }
                if (rand == 2)
                {
                    Enemy enemigo = new Enemy_lvl_2();
                    enemies.Add(enemigo);
                    enemigo.Init();
                }
                SPAWN_TIME_COUNTER = 0;
            }
            foreach (Enemy enemigo in enemies)
            {
                enemigo.Update(elapsedTime);
            }

            scoreText.Text = "Score: " + score;
            if (TEXT_DELAY > 0) { TEXT_DELAY -= elapsedTime; }
            if (KILL_DELAY > 0) { KILL_DELAY -= elapsedTime; }
            if (KILL_DELAY <= 0 && killMultiTracker >= 0) {                
                if (killMultiTracker >= 2) { playSound(deniedSoundDir); }
                killMultiTracker = 0;
            }
        }

        internal void Render(float elapsedTime)
        {
            terrain.render();
            //foreach (TgcMesh v in vegetation)
            int i;
            for (i = 1; i < 48; i++)
            {
                vegetation[i].render();

                if (drawBoundingBoxes) { vegetation[i].BoundingBox.render(); }
            }

            if (drawBoundingBoxes) { CustomFpsCamera.Instance.boundingBox.render(); }

            foreach(Enemy enemigo in enemies){
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
            int killHeadTracker = 0;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies[i].HEADSHOT_BOUNDINGBOX, out newPosition))
                {
                    score += 1;
                    killHeadTracker++;
                    specialKillText.Text = "HEADSHOT!!";
                    TEXT_DELAY = TEXT_DELAY_MAX;
                    playSound(headshotSoundDir);
                    enemies[i].health = 0;
                }           

                if (TgcCollisionUtils.intersectRayAABB(ray, enemies[i].mesh.BoundingBox, out newPosition))
                {
                    enemies[i].health -= 50;
                    if (enemies[i].health <= 0)
                    {
                        score += enemies[i].score;
                        eliminarEnemigo(i);
                        killMultiTracker++;
                        awardKill();
                        KILL_DELAY = KILL_DELAY_MAX;
                    }
                }                
            }
            
            if (killHeadTracker > 1)
            {
                specialKillText.Text = "HEAD HUNTER!!";
                TEXT_DELAY = TEXT_DELAY_MAX;
                playSound(headhunterSoundDir);
                score += killHeadTracker;
            }
        }

        public void eliminarEnemigo(int i)
        {
            if (enemies.Count == 0)
            {
                Enemy enemigo = new Enemy_lvl_1();
                enemies.Add(enemigo);
                enemigo.Init();
            }

            enemies[i].dispose();
            enemies.Remove(enemies[i]);
        }

        private void awardKill()
        {
            if (killMultiTracker >= 2)
            {
                score += 2;
                switch (killMultiTracker)
                {
                    case 2:
                        specialKillText.Text = "DOUBLE KILL";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(doubleSoundDir);
                        break;
                    case 3:
                        specialKillText.Text = "MULTI KILL";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(multiSoundDir);
                        break;
                    case 4:
                        specialKillText.Text = "MEGA KILL";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(megaSoundDir);
                        break;
                    case 5:
                        specialKillText.Text = "ULTRA KILL";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(ultraSoundDir);
                        break;
                    case 6:
                        specialKillText.Text = "MONSTER KILL";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(monsterSoundDir);
                        break;
                    case 10:
                        specialKillText.Text = "MASSACRE";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(massacreSoundDir);
                        break;
                    default:
                        break;
                }
            }
        }

        private void playSound(string dir)
        {
            sound.dispose();
            sound.loadSound(dir);
            sound.play();
        }

        /// <summary>
        /// Retorna la altura del terreno en ese punto utilizando interpolacion bilineal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool interpoledHeight(float x, float z, out float y)
        {
            Vector2 coords;
            float i;
            y = 0;
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 traslation;

            traslation.X = center.X - (400 / 2);
            traslation.Y = center.Y;
            //this.center.Y = traslation.Y;
            traslation.Z = center.Z - (400 / 2);

            if (!xzToHeightmapCoords(x, z, traslation, out coords)) return false;
            interpoledIntensity(coords.X, coords.Y, out i);

            y = (i + traslation.Y) * currentScaleY;
            return true;
        }

        /// <summary>
        /// Retorna la intensidad del heightmap en ese punto utilizando interpolacion bilineal.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool interpoledIntensity(float u, float v, out float i)
        {
            i = 0;

            float maxX = terrain.HeightmapData.GetLength(0);
            float maxZ = terrain.HeightmapData.GetLength(1);
            if (u >= maxX || v >= maxZ || v < 0 || u < 0) return false;

            int x1, x2, z1, z2;
            float s, t;

            x1 = (int)FastMath.Floor(u);
            x2 = x1 + 1;
            s = u - x1;

            z1 = (int)FastMath.Floor(v);
            z2 = z1 + 1;
            t = v - z1;

            if (z2 >= maxZ) z2--;
            if (x2 >= maxX) x2--;

            float i1 = terrain.HeightmapData[x1, z1] + s * (terrain.HeightmapData[x2, z1] - terrain.HeightmapData[x1, z1]);
            float i2 = terrain.HeightmapData[x1, z2] + s * (terrain.HeightmapData[x2, z2] - terrain.HeightmapData[x1, z2]);

            i = i1 + t * (i2 - i1);
            return true;


        }

        /// <summary>
        /// Transforma coordenadas del mundo en coordenadas del heightmap.
        /// </summary>
        public bool xzToHeightmapCoords(float x, float z, Vector3 traslation, out Vector2 coords)
        {
            float i, j;

            i = x / currentScaleXZ - traslation.X;
            j = z / currentScaleXZ - traslation.Z;


            coords = new Vector2(i, j);

            if (coords.X >= terrain.HeightmapData.GetLength(0) || coords.Y >= terrain.HeightmapData.GetLength(1) || coords.Y < 0 || coords.X < 0) return false;

            return true;
        }

        public void refreshScopeTexture()
        {
            Size tamaño = cross.Texture.Size;
            Size screen = GuiController.Instance.Panel3d.Size;
            cross.Scaling = new Vector2((float)screen.Width / (12 * (float)tamaño.Width), (float)screen.Width / (12 * (float)tamaño.Height));
            Vector2 size = new Vector2(tamaño.Width * cross.Scaling.X, tamaño.Height * cross.Scaling.Y);
            cross.Position = new Vector2((screen.Width - size.X) / 2, (screen.Height - size.Y) / 2);
        }

        public void zoomCamera()
        {
            if (zoomEnabled)
            {
                cross.Texture = normalScope;
                CustomFpsCamera.Instance.Zoom = 0;
                zoomEnabled = false;
            }
            else
            {
                cross.Texture = zoomedScope;
                CustomFpsCamera.Instance.Zoom = ZOOM_CONST;
                zoomEnabled = true;
            }

            refreshScopeTexture();

        }
    }
}
