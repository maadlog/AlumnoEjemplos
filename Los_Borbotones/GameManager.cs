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

        public Player1 player1 = new Player1();
        public List<Enemy> enemies;
        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public int ScreenHeight, ScreenWidth;        
        float SPAWN_TIME = 5f;
        float SPAWN_TIME_COUNTER;
        public Random random = new Random();
        int rand;

        public TgcScene Vegetation;
        TgcSimpleTerrain terrain;
        int heightmapResolution;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ = 100f;
        float currentScaleY = 8f;
        private List<TgcMesh> vegetation;
        public int vegetacionVisible = 0;
        TgcSprite cross;
        Quadtree quadTree;
        TgcSkyBox skyBox;

        TgcText2d scoreText;
        float score;
        TgcText2d specialKillText;
        float TEXT_DELAY;
        float TEXT_DELAY_MAX = 2f;
        int killMultiTracker;
        float KILL_DELAY;
        float KILL_DELAY_MAX = 5;
        public bool GAME_OVER;
        public TgcText2d healthText;

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
        public bool invincibility;

        bool zoomEnabled = false;
        float ZOOM_CONST = 0.8f; //TODO Hacer dependiente del arma
        TgcTexture normalScope;
        TgcTexture zoomedScope;
        float screenCovered = 0.12f;

        internal void Init()
        {
            GAME_OVER = false;
            score = 0;
            TEXT_DELAY = 0;
            killMultiTracker = 0;
            KILL_DELAY = 0;
            SPAWN_TIME_COUNTER = 0f;
            
            //Creo skybox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(11500, 11500, 11500);

            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox1\\";

            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.updateValues();

            currentHeightmap = GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\" + "experimento-editando3.jpg";
            //Seteo de la resolucion del jpg de heightmap para la interpolacion de altura, como es cuadrado se usa una sola variable
            heightmapResolution = 800;
            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\" + "splatting1.png";
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, 0));
            terrain.loadTexture(currentTexture);

            this.vegetation = new List<TgcMesh>();
            TgcSceneLoader loader = new TgcSceneLoader();
            Vegetation = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Mapas\\100x8-TgcScene.xml");
            
            vegetation = Vegetation.Meshes;
            int i;
            Matrix scale = Matrix.Scaling(new Vector3(0.06f, 0.4f, 0.06f));
            for (i = 1; i < vegetation.Count; i++)
            {
                Vector3 center = vegetation[i].BoundingBox.calculateBoxCenter();
                float y;
                interpoledHeight(center.X, center.Z, out y);
                center.Y = y;
                Matrix trans = Matrix.Translation(center + new Vector3(-4f, 0, 0));
                vegetation[i].BoundingBox.transform(scale * trans);
            }
            Matrix sceneScale = Matrix.Scaling(new Vector3(1f, 0.4f, 1f));
            Vegetation.BoundingBox.transform(sceneScale);

            player1.Init();

            enemies = new List<Enemy>();

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
            scoreText.Text = "SCORE: " + score;
                scoreText.Color = Color.LightBlue;
                scoreText.changeFont(new System.Drawing.Font("Arial", 10, FontStyle.Bold));

                healthText = new TgcText2d();
                healthText.Text = "HEALTH: " + player1.vida;
                healthText.Color = Color.Green;
                healthText.changeFont(new System.Drawing.Font("Arial", 10, FontStyle.Bold));
            healthText.Position = new Point(0, 250);
            healthText.Align = TgcText2d.TextAlign.LEFT;
            
              
            cross = new TgcSprite();
            normalScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Sprites\\normalScope.png");
            zoomedScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Sprites\\zoomedScope.png");
            cross.Texture = normalScope;

            refreshScopeTexture();

            quadTree = new Quadtree();
            quadTree.create(vegetation, Vegetation.BoundingBox);
            quadTree.createDebugQuadtreeMeshes();

            Device d3dDevice = GuiController.Instance.D3dDevice;
            d3dDevice.RenderState.FogTableMode = FogMode.Linear;
            d3dDevice.RenderState.FogVertexMode = FogMode.None;
            d3dDevice.RenderState.FogColor = Color.LightBlue;
            d3dDevice.RenderState.FogStart = 3000f;
            d3dDevice.RenderState.FogEnd = 9000f;
            d3dDevice.RenderState.FogEnable = true;
        }

        internal void Update(float elapsedTime)
        {
            drawBoundingBoxes = (bool)GuiController.Instance.Modifiers["DrawBoundingBoxes"];
            invincibility = (bool)GuiController.Instance.Modifiers["Invincibility"];

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

            enemies.ForEach(enemy => enemy.Update(elapsedTime));

            scoreText.Text = "Score: " + score;
            ChangeTextColor();
            if (TEXT_DELAY > 0) { TEXT_DELAY -= elapsedTime; }
            if (KILL_DELAY > 0) { KILL_DELAY -= elapsedTime; }
            if (KILL_DELAY <= 0 && killMultiTracker >= 0) {                
                if (killMultiTracker >= 2) { playSound(deniedSoundDir); }
                killMultiTracker = 0;
            }

            if (TEXT_DELAY <= 0 && GAME_OVER)
            {
                close();
                Init();
            }
            skyBox.Center = CustomFpsCamera.Instance.Position;
            skyBox.updateValues();
        }

        private void ChangeTextColor()
        {
            if (score >= 0)
            {
                scoreText.Color = Color.White;
            }
            if (score > 10)
            {
                scoreText.Color = Color.Orange;
            }
            if (score > 20)
            {
                scoreText.Color = Color.Silver;
            }
            if (score > 30)
            {
                scoreText.Color = Color.Gold;
            }
            if(score > 50)
            {
                scoreText.Color = Color.LightCyan;
            }
        }

        internal void Render(float elapsedTime)
        {
            terrain.render();

            skyBox.render();
            quadTree.render(GuiController.Instance.Frustum, drawBoundingBoxes);

            if (drawBoundingBoxes) { CustomFpsCamera.Instance.boundingBox.render(); }

            foreach(Enemy enemigo in enemies){
                enemigo.Render(elapsedTime);
            }

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            cross.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            scoreText.render();
            healthText.render();
            if (TEXT_DELAY > 0) { specialKillText.render(); }

            player1.Render(elapsedTime);

            //Obtener valor de UserVar (hay que castear)
            GuiController.Instance.UserVars.setValue("N Vegetacion Visible", vegetacionVisible);
            int valor = (int)GuiController.Instance.UserVars.getValue("N Vegetacion Visible");
            vegetacionVisible = 0;

        }

        internal void close()
        {
            Vegetation.disposeAll();
            terrain.dispose();
            player1.dispose();
            specialKillText.dispose();
            scoreText.dispose();
            healthText.dispose();
            normalScope.dispose();
            zoomedScope.dispose();
            skyBox.dispose();
            foreach (Enemy enemy in enemies)
            {
                enemy.dispose();
            }
        }

        public void gameOver(float elapsedTime)
        {
            if (GAME_OVER || invincibility) { return; }
            specialKillText.Text = "GAME OVER";
            TEXT_DELAY = TEXT_DELAY_MAX;
            GAME_OVER = true;
        }

        public void fireWeapon()
        {
            TgcRay ray = new TgcRay(CustomFpsCamera.Instance.Position, CustomFpsCamera.Instance.LookAt - CustomFpsCamera.Instance.Position);
            Vector3 newPosition = new Vector3(0, 0, 0);
            List<Vector3> posicionObstaculos = new List<Vector3>();
            bool vegetacionFrenoDisparo = false;
            foreach (TgcMesh obstaculo in vegetation)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, obstaculo.BoundingBox, out newPosition))
                posicionObstaculos.Add(newPosition);
            }

            int killHeadTracker = 0;
            bool hit = false;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies[i].HEADSHOT_BOUNDINGBOX, out newPosition))
                {
                    foreach(Vector3 posicion in posicionObstaculos){
                        if (Vector3.Length(posicion - ray.Origin) < Vector3.Length(newPosition - ray.Origin))
                        {
                            vegetacionFrenoDisparo = true;
                            break;
                        }
                    }
                    if (!vegetacionFrenoDisparo)
                    {
                        hit = true;
                        score += 1;
                        killHeadTracker++;
                        specialKillText.Text = "HEADSHOT!!";
                        TEXT_DELAY = TEXT_DELAY_MAX;
                        playSound(headshotSoundDir);
                        enemies[i].health = 0;
                        eliminarEnemigo(enemies[i]);
                        killMultiTracker++;
                        awardKill();
                        KILL_DELAY = KILL_DELAY_MAX;
                    }
                    vegetacionFrenoDisparo = false;
                }
                if (!hit && TgcCollisionUtils.intersectRayAABB(ray, enemies[i].LEGS_BOUNDINGBOX, out newPosition))
                {
                    foreach (Vector3 posicion in posicionObstaculos)
                    {
                        if (Vector3.Length(posicion - ray.Origin) < Vector3.Length(newPosition - ray.Origin))
                        {
                            vegetacionFrenoDisparo = true;
                            break;
                        }
                    }
                    if (!vegetacionFrenoDisparo)
                    {
                        enemies[i].health -= 25;
                        hit = true;
                        if (enemies[i].health <= 0)
                        {
                            score += enemies[i].score;
                            eliminarEnemigo(enemies[i]);
                            killMultiTracker++;
                            awardKill();
                            KILL_DELAY = KILL_DELAY_MAX;
                        }
                    }
                    vegetacionFrenoDisparo = false;
                }
                if (!hit && TgcCollisionUtils.intersectRayAABB(ray, enemies[i].CHEST_BOUNDINGBOX, out newPosition))
                {
                    foreach(Vector3 posicion in posicionObstaculos){
                        if (Vector3.Length(posicion - ray.Origin) < Vector3.Length(newPosition - ray.Origin))
                        {
                            vegetacionFrenoDisparo = true;
                            break;
                        }
                    }
                    if (!vegetacionFrenoDisparo)
                    {
                        enemies[i].health -= 50;
                        if (enemies[i].health <= 0)
                        {
                            score += enemies[i].score;
                            eliminarEnemigo(enemies[i]);
                            killMultiTracker++;
                            awardKill();
                            KILL_DELAY = KILL_DELAY_MAX;
                        }
                    }
                    vegetacionFrenoDisparo = false;
                }
            

                hit = false;
            }
            
            if (killHeadTracker > 1)
            {
                specialKillText.Text = "HEAD HUNTER!!";
                TEXT_DELAY = TEXT_DELAY_MAX;
                playSound(headhunterSoundDir);
                score += killHeadTracker;
            }
        }

        public void eliminarEnemigo(Enemy enemy)
        {
            if (enemies.Count == 0)
            {
                Enemy enemigo = new Enemy_lvl_1();
                enemies.Add(enemigo);
                enemigo.Init();
            }

            enemy.dispose();
            enemies.Remove(enemy);
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
            traslation.X = center.X - (heightmapResolution / 2);
            traslation.Y = center.Y;
            //this.center.Y = traslation.Y;
            traslation.Z = center.Z - (heightmapResolution / 2);

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
            cross.Scaling = new Vector2((float)screen.Width * screenCovered / (float)tamaño.Width, (float)screen.Width * screenCovered / (float)tamaño.Height);
            Vector2 size = new Vector2(tamaño.Width * cross.Scaling.X, tamaño.Height * cross.Scaling.Y);
            cross.Position = new Vector2((screen.Width - size.X) / 2, (screen.Height - size.Y) / 2);
        }

        public void zoomCamera()
        {
            if (zoomEnabled)
            {
                cross.Texture = normalScope;
                CustomFpsCamera.Instance.Zoom = 0;
                screenCovered = 0.12f; // 1/12 of screen covered by scope
                zoomEnabled = false;
            }
            else
            {
                cross.Texture = zoomedScope;
                CustomFpsCamera.Instance.Zoom = ZOOM_CONST;
                screenCovered = 2f; // scope scaled to twice the screen wdth
                zoomEnabled = true;
            }

            refreshScopeTexture();

        }

        public void ChangeColorHealth()
        {
            if (player1.vida >=51)
            {
                healthText.Color = Color.Green;
            }
            if (player1.vida < 51)
            {
                healthText.Color = Color.Yellow;
            }
            if (player1.vida < 26)
            {
                healthText.Color = Color.Red;
            }
           
        }
    }
}
