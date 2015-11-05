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
using AlumnoEjemplos.Los_Borbotones.Menus;
using TgcViewer.Utils.Input;
using AlumnoEjemplo.Los_Borbotones;

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
        public List<Proyectil> proyectiles;
        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public int ScreenHeight, ScreenWidth;        
        float SPAWN_TIME = 5f;
        float SPAWN_TIME_COUNTER;
        public Random random = new Random();
        int rand;

        public TgcScene Vegetation;
        Effect windShader;
        CustomTerrain terrain;
        int heightmapResolution;
        int textureResolution;
        int cantidadFilasColumnas = 8;
        string currentHeightmap;
        string currentTexture;
        float currentScaleXZ = 100f;
        float currentScaleY = 8f;
        private List<TgcMesh> vegetation;
        List<TgcMesh> obstaculos;
        public int vegetacionVisible = 0;
        public int terrenosVisibles = 0;

        public Quadtree quadTree;
        CustomSkyBox skyBox;

        public float score;

        int killMultiTracker;
        float KILL_DELAY;
        float KILL_DELAY_MAX = 5;
        public bool GAME_OVER;
 
        public int MAX_ENEMIES = 10;
        public TgcMesh ModeloRobot;
        public TgcMesh ModeloNave;
        public TgcMesh ModeloProyectil;

        
        public bool drawBoundingBoxes = false;
        public bool invincibility = false;


        public List<Barril> barriles = new List<Barril>();
        private List<TgcMesh> meshesBarril;
        public TgcScene Barriles;

        float time;
        public List<Pasto> pastos = new List<Pasto>();
        public List<Vector3> pastosCoords = new List<Vector3>();
        bool positiveMove0 = true;
        bool positiveMove1 = true;
        bool positiveMove2 = true;
        float tLeftMoved0 = 0;
        float tRightMoved0 = 0;
        float tLeftMoved1 = 0;
        float tRightMoved1 = 0;
        float tLeftMoved2 = 0;
        float tRightMoved2 = 0;
        float maxMoved = 5;
        float minMoved = -5;

        internal void Init()
        {
            
            score = 0; //lleva el score del jugador
            
            killMultiTracker = 0;
            KILL_DELAY = 0;
            SPAWN_TIME_COUNTER = 0f;
            time = 0;

            // creo meshes de modelo para clonar y asi optimizar
            TgcSceneLoader loader2 = new TgcSceneLoader();
            TgcScene scene = loader2.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Robot\\Robot-TgcScene.xml");
            this.ModeloRobot = scene.Meshes[0];            
            TgcScene scene2 = loader2.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-Speeder\\StarWars-Speeder-TgcScene.xml");
            this.ModeloNave = scene2.Meshes[0];
            TgcScene scene3 = loader2.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\proyectiles\\EnergyBall-TgcScene.xml");
            this.ModeloProyectil = scene3.Meshes[0];

            //Creo skybox
            skyBox = new CustomSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            float farplane = CustomFpsCamera.FAR_PLANE;
            skyBox.Size = new Vector3(farplane, farplane, farplane);

            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox1\\";

            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");
            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(CustomSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.updateValues();

            //Creacion del terreno
            currentHeightmap = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Mapas\\" + "experimento-editando4_3.jpg";
            //Seteo de la resolucion del jpg de heightmap para la interpolacion de altura, como es cuadrado se usa una sola variable
            heightmapResolution = 800;
            textureResolution = 1600;

            Vector3 posInicial = new Vector3(0, 0, 0);
            currentTexture = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Mapas\\" + "grunge.jpg";
            terrain = new CustomTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, posInicial, cantidadFilasColumnas);
            terrain.loadTexture(currentTexture);
            terrain.Effect = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Shaders\\RenderTerrain.fx");
            terrain.Technique = "RenderTerrain";

            //Creacion del shader de viento
            windShader = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Shaders\\WindTree.fx");

            //Creacion de la Vegetacion
            this.vegetation = new List<TgcMesh>();
            TgcSceneLoader loader = new TgcSceneLoader();
            Vegetation = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Mapas\\100x8 v4-TgcScene.xml");
            
            vegetation = Vegetation.Meshes;
            int i;
            Matrix scale = Matrix.Scaling(new Vector3(0.06f, 0.4f, 0.06f));
            for (i = 1; i < vegetation.Count; i++)
            {
                vegetation[i].setColor(Color.Purple);
                vegetation[i].Effect = windShader;
                vegetation[i].Technique = "WindTree";
                Vector3 center = vegetation[i].BoundingBox.calculateBoxCenter();
                float y;
                interpoledHeight(center.X, center.Z, out y);
                center.Y = y;
                Matrix trans = Matrix.Translation(center + new Vector3(-4f, 0, 0));
                vegetation[i].BoundingBox.transform(scale * trans);
            }
            //Creacion de barriles 
            this.meshesBarril = new List<TgcMesh>();
            TgcSceneLoader loader4 = new TgcSceneLoader();
            Barriles = loader4.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Mapas\\Barriles2-TgcScene.xml");
            Barriles.setMeshesEnabled(true);
            meshesBarril = Barriles.Meshes;
            int j;
            //Matrix scale = Matrix.Scaling(new Vector3(0.06f, 0.4f, 0.06f));
            for (j = 1; j < meshesBarril.Count; j++)
            {
                Barril barril = new Barril();
                
                meshesBarril[j].Scale = new Vector3(0.3f, 0.3f, 0.3f);
                barril.mesh = meshesBarril[j];
                barril.Init();
                barriles.Add(barril);
                //vegetation[i].setColor(Color.SkyBlue);
                //Vector3 center = vegetation[i].BoundingBox.calculateBoxCenter();
                //float y;
                //interpoledHeight(center.X, center.Z, out y);
                //center.Y = y;
                //Matrix trans = Matrix.Translation(center + new Vector3(-4f, 0, 0));
                //vegetation[i].BoundingBox.transform(scale * trans);
            }

            //inicializamos al player
            player1.Init();

            enemies = new List<Enemy>();
            proyectiles = new List<Proyectil>();

            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;


            
            

            obstaculos = new List<TgcMesh>();
            obstaculos.AddRange(vegetation);
            obstaculos.AddRange(meshesBarril);

            quadTree = new Quadtree();
            quadTree.create(obstaculos, Vegetation.BoundingBox);
            quadTree.createDebugQuadtreeMeshes();

            //seteamos niebla
            Device d3dDevice = GuiController.Instance.D3dDevice;
            //d3dDevice.RenderState.FogTableMode = FogMode.Linear;
            d3dDevice.RenderState.FogTableMode = FogMode.Exp2;
            d3dDevice.RenderState.FogVertexMode = FogMode.None;
            d3dDevice.RenderState.FogColor = Color.MediumPurple;
            //d3dDevice.RenderState.FogStart = 3000f;
            //d3dDevice.RenderState.FogEnd = farplane;
            d3dDevice.RenderState.FogDensity = 0.00006f;
            d3dDevice.RenderState.FogEnable = true;

            initPastos();

            int secuense = 0;
            int part = 0;
            Pasto pasto = new Pasto();
            foreach (Vector3 pastoCoord in pastosCoords)
            {                
                pasto.crearPasto(d3dDevice, secuense, pastoCoord);
                if (part == 5)
                {
                    pastos.Add(pasto);
                    pasto = new Pasto();
                }
                secuense++;
                part++;
                if (secuense > 2) secuense = 0;
                if (part > 5) part = 0;
            }
        }

        internal void Update(float elapsedTime)
        {
            time += elapsedTime;
            windShader.SetValue("time", time);

            
           

            SPAWN_TIME_COUNTER = SPAWN_TIME_COUNTER + elapsedTime;//contamos el tiempo que paso desde el ultimo spawn de enemigos

            player1.Update(elapsedTime);
            if (SPAWN_TIME_COUNTER > SPAWN_TIME && enemies.Count < MAX_ENEMIES)
            {
                //si paso un tiempo = SPAWN_TIME agregamos un nuevo enemigo seleccionado al azar
                rand = random.Next(1, 4);
                if (rand == 1)
                {
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
                if (rand == 3)
                {
                    Enemy enemigo = new Enemy_lvl_3();
                    enemies.Add(enemigo);
                    enemigo.Init();
                }
                SPAWN_TIME_COUNTER = 0;
            }

            //update de los enemigos
            enemies.ForEach(enemy => updateYEliminarMuertos(elapsedTime, enemy));
            proyectiles.ForEach(proyectil => proyectil.Update(elapsedTime));


            if (KILL_DELAY > 0) { KILL_DELAY -= elapsedTime; }
            if (KILL_DELAY <= 0 && killMultiTracker >= 0)
            {
                if (killMultiTracker >= 2) {
                    HUDManager.Instance.denied(); }
                killMultiTracker = 0;
            }

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.N))
            {
                if (PostProcessManager.Instance.renderFlux == "RenderAll")
                {
                    PostProcessManager.Instance.renderFlux = "NightVision";
                   }
                else
                {
                    PostProcessManager.Instance.renderFlux = "RenderAll";
                }
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Q))
            {
                if (player1.weapon.Equals( player1.sniper))
                {
                    player1.weapon = player1.launcher;
                }
                else
                {
                    player1.weapon = player1.sniper;
                }
                player1.weapon.muzzle.scale = player1.weapon.scaleMuzzle;
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F6))
            {
                if (drawBoundingBoxes)
                {
                    drawBoundingBoxes = false;
                }
                else
                {
                    drawBoundingBoxes = true;
                }
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F7))
            {
                if (invincibility)
                {
                    invincibility = false;
                }
                else
                {
                    invincibility = true;
                }
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.P))
            {
                CustomFpsCamera.Instance.JumpSpeed += 100;
            }

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.O))
            {
                CustomFpsCamera.Instance.JumpSpeed -= 100;
            }

            //hacemos que el skybox siga al player para no tener problemas con el farplane
            Matrix translate = Matrix.Translation(CustomFpsCamera.Instance.Position);
            skyBox.transform(translate);

            foreach (Barril barril in barriles)
            {
                barril.Update(elapsedTime);
            }

            if (positiveMove0)
            {
                tLeftMoved0 += 0.02f;
                tRightMoved0 += 0.02f;
                if (tLeftMoved0 >= maxMoved) positiveMove0 = false;
            }
            else
            {
                tLeftMoved0 -= 0.02f;
                tRightMoved0 -= 0.02f;
                if (tLeftMoved0 <= minMoved) positiveMove0 = true;
            }

            if (positiveMove1)
            {
                tLeftMoved1 += 0.015f;
                tRightMoved1 += 0.015f;
                if (tLeftMoved1 >= maxMoved) positiveMove1 = false;
            }
            else
            {
                tLeftMoved1 -= 0.015f;
                tRightMoved1 -= 0.015f;
                if (tLeftMoved1 <= minMoved) positiveMove1 = true;
            }

            if (positiveMove2)
            {
                tLeftMoved2 += 0.01f;
                tRightMoved2 += 0.01f;
                if (tLeftMoved2 >= maxMoved) positiveMove2 = false;
            }
            else
            {
                tLeftMoved2 -= 0.01f;
                tRightMoved2 -= 0.01f;
                if (tLeftMoved2 <= minMoved) positiveMove2 = true;
            }
        }

        public void updateYEliminarMuertos(float elapsedTime, Enemy enemy)
        {
            if (enemy.tiempoDesdeMuerto > enemy.tiempoMuerte)
            {
                eliminarEnemigo(enemy);
            }
            else
            {
                enemy.Update(elapsedTime);
            };
        }

        internal void RenderAll(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            terrain.Technique = "RenderTerrain";
            terrain.render();

            TgcFrustum frustum = GuiController.Instance.Frustum;
            if (drawBoundingBoxes)


            {

                foreach (Barril barril in barriles)
                {
                    //barril.explosion.render();
                }

            }

            skyBox.render();
            quadTree.render(frustum, drawBoundingBoxes, "ArbolBosque");

            if (drawBoundingBoxes) { CustomFpsCamera.Instance.boundingBox.render(); }

            //dibujamos todos los enemigos
            foreach(Enemy enemigo in enemies){
                TgcCollisionUtils.FrustumResult result = TgcCollisionUtils.classifyFrustumAABB(frustum, enemigo.mesh.BoundingBox);
                if (result == TgcCollisionUtils.FrustumResult.INSIDE || result ==  TgcCollisionUtils.FrustumResult.INTERSECT){
                enemigo.Render(elapsedTime);
                }
            }

            foreach(Proyectil proyectil in proyectiles){
                TgcCollisionUtils.FrustumResult result = TgcCollisionUtils.classifyFrustumAABB(frustum, proyectil.mesh.BoundingBox);
                if (result == TgcCollisionUtils.FrustumResult.INSIDE || result ==  TgcCollisionUtils.FrustumResult.INTERSECT){
                proyectil.Render(elapsedTime);
                }
            }

            player1.Render(elapsedTime);

            //Obtener valor de UserVar (hay que castear)
            GuiController.Instance.UserVars.setValue("N Vegetacion Visible", vegetacionVisible);
            int valor = (int)GuiController.Instance.UserVars.getValue("N Vegetacion Visible");
            vegetacionVisible = 0;
            GuiController.Instance.UserVars.setValue("N Sub-terrenos Visibles", terrenosVisibles);
            int valor2 = (int)GuiController.Instance.UserVars.getValue("N Sub-terrenos Visibles");
            terrenosVisibles = 0;

            foreach (Barril barril in barriles)
            {

                barril.Render(elapsedTime);

            }

            int parte, texture = 0;
            foreach (Pasto pasto in pastos)
            {
                for (parte = 0; parte < 6; parte++)
                {
                    switch (texture)
                    {
                        case 0:
                            pasto.renderPasto(tLeftMoved0, tRightMoved0, parte);
                            break;
                        case 1:
                            pasto.renderPasto(tLeftMoved1, tRightMoved1, parte);
                            break;
                        case 2:
                            pasto.renderPasto(tLeftMoved2, tRightMoved2, parte);
                            break;
                    }
                    texture++;
                    if (texture > 2) texture = 0;
                }
            }
        }

        internal void RenderBrigth(float elapsedTime)
        {

            TgcFrustum frustum = GuiController.Instance.Frustum;

            foreach (Enemy enemigo in enemies)
            {
                enemigo.Render(elapsedTime);
            }

            quadTree.render(frustum, drawBoundingBoxes, "Oil");

        }

        internal void RenderDull(float elapsedTime)
        {
           
            terrain.Technique = "DullRender";

            Device d3dDevice = GuiController.Instance.D3dDevice;
            terrain.render();

            TgcFrustum frustum = GuiController.Instance.Frustum;

            proyectiles.ForEach(proyectil => proyectil.Render(elapsedTime));
            
        }


        internal void close()
        {
            Vegetation.disposeAll();
            terrain.dispose();
            player1.dispose();

            skyBox.dispose();

            foreach (Enemy enemy in enemies)
            {
                enemy.dispose();
            }
            enemies.Clear();
            foreach (Pasto pasto in pastos)
            {
                pasto.dispose();
            }
        }

        void compareAssign(float n, float max)
        {
            if (n > max)
            {
                n = max;
            }
            else if (n < -max){
                n=-max;
            }
        }

        public void gameOver()
        {
            if (GAME_OVER || invincibility) { return; }
            HUDManager.Instance.gameOver();
            foreach (Enemy enemy in enemies)
            {
                enemy.SonidoMovimiento.stop();
            }
            GAME_OVER = true;
        }

        public Vector3 fireLauncher()
        {
            //Disparamos el arma, nos fijamos si colisiona con un enemigo, y si hay obstaculos en el medio
            TgcRay ray = new TgcRay(CustomFpsCamera.Instance.Position, CustomFpsCamera.Instance.LookAt - CustomFpsCamera.Instance.Position);
            Vector3 newPosition = new Vector3(0, 0, 0);
            List<Vector3> posicionObstaculos = new List<Vector3>();
            foreach (TgcMesh obstaculo in vegetation)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, obstaculo.BoundingBox, out newPosition))
                    posicionObstaculos.Add(newPosition);
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, enemies[i].mesh.BoundingBox, out newPosition))
                    posicionObstaculos.Add(newPosition);       
            }
            //////////////////////////disparo a barriles////////////////////////////////////////
            for (int i = barriles.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, barriles[i].mesh.BoundingBox, out newPosition))
                    posicionObstaculos.Add(newPosition); 
            }

            posicionObstaculos.Add(intersectRayTerrain(ray));

            posicionObstaculos.Sort(delegate(Vector3 x, Vector3 y)
            {
                return distanciaACamara(x).CompareTo(distanciaACamara(y));
            });

            Vector3 min = posicionObstaculos[0];
            return min;
        }

        public float distanciaACamara(Vector3 vector)
        {
            Vector3 camara = CustomFpsCamera.Instance.eye;
            Vector3 dist = camara - vector;
            return dist.Length();
        }

        public void fireSniper()
        {
            //Disparamos el arma, nos fijamos si colisiona con un enemigo, y si hay obstaculos en el medio
            Vector3 dir = CustomFpsCamera.Instance.LookAt - CustomFpsCamera.Instance.Position;
            TgcRay ray = new TgcRay(CustomFpsCamera.Instance.Position, dir);
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
                        HUDManager.Instance.headShot();
                        enemies[i].health = 0;
                        enemies[i].sangrar(-dir, newPosition.Y - enemies[i].getPosicionActual().Y);

                        //eliminarEnemigo(enemies[i]);
                        enemies[i].morirse();
                        sumarScore(enemies[i]);

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
                        enemies[i].sangrar(-dir, newPosition.Y - enemies[i].getPosicionActual().Y);
                        hit = true;
                        if (enemies[i].health <= 0)
                        {
                           // eliminarEnemigo(enemies[i]);
                            enemies[i].morirse();
                            sumarScore(enemies[i]);

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
                        enemies[i].sangrar(-dir, newPosition.Y - enemies[i].getPosicionActual().Y);
                        if (enemies[i].health <= 0)
                        {
                            //eliminarEnemigo(enemies[i]);
                            enemies[i].morirse();
                            sumarScore(enemies[i]);
                        }
                    }
                    vegetacionFrenoDisparo = false;
                }
            

                hit = false;
            }
            //////////////////////////disparo a barriles////////////////////////////////////////
            for (int i = barriles.Count - 1; i >= 0; i--)
            {
                if (TgcCollisionUtils.intersectRayAABB(ray, barriles[i].mesh.BoundingBox, out newPosition))
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

                        // playSound(explosionSoundDir); TODO


                        barriles[i].explotar();

                    }
                    vegetacionFrenoDisparo = false;
                }
            }
                /////////////////////////////////////////////////////////////
            
            if (killHeadTracker > 1)
            {
                HUDManager.Instance.headHunter();//Constante que reproduce el efecto de headhunter.
                score += killHeadTracker;
                HUDManager.Instance.refreshScore();
            }
        }

        public void dispararProyectil(Matrix posicionActual, Vector3 vectorDireccion){
            Proyectil proyectil = new Proyectil(posicionActual, vectorDireccion);
            proyectil.Init();
            proyectiles.Add(proyectil);
        }

        public void sumarScore(Enemy enemy)
        {
            score += enemy.score;
            killMultiTracker++;
            awardKill();
            KILL_DELAY = KILL_DELAY_MAX;
            //Hacemos refresh del score
            HUDManager.Instance.refreshScore();
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

        public void eliminarProyectil(Proyectil proy)
        {
            proy.dispose();
            proyectiles.Remove(proy);
        }

        private void awardKill()
        {
            //chequeamos los combos de kills
            if (killMultiTracker >= 2)
            {
                score += 2;
                HUDManager.Instance.awardKill(killMultiTracker);
            }
        }

        public Vector3 intersectRayTerrain(TgcRay ray)
        {
            int iteraciones = heightmapResolution/cantidadFilasColumnas * (int)currentScaleXZ;
            Vector3 dir = ray.Direction;
            dir.Normalize();
            Vector3 origin = ray.Origin;
            Vector3 pos = origin;
            float y = 0;
            for (int i = 0; i < iteraciones; i++)
            {
                interpoledHeight(pos.X, pos.Z, out y);
                if (FastMath.Abs(pos.Y - y) < 1f)
                {
                    return pos;
                }
                pos += dir;
            }
            return pos;
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
            //te devuelve la altura del terreno en el punto
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

        public void eliminarBarril(Barril barril)
        {
            obstaculos.Remove(barril.mesh);
        }

        public void agregarBarril(Barril barril)
        {
            obstaculos.Add(barril.mesh);
        }

        private void initPastos()
        {
            addPastos(0, 0);
            addPastos(-17364,65);
            addPastos(-30420,-15584);
            addPastos(-29653,16619);
            addPastos(-9857,28731);
            addPastos(12670,28548);
            addPastos(19032,4878);
            addPastos(28848,-9831);
            addPastos(-2758,-18265);
            addPastos(15552,-28198);
            addPastos(-16219,-31620);
            /*
            addArbusto(0, 0);
            addArbusto(130, 0);
            addArbusto(90, -66);
            addArbusto(68,75);
            addArbusto(12,126);
            addArbusto(153, 92);
            addArbusto(175, -20);
            addArbusto(10, 60);
            addArbusto(-76,-7);
            addArbusto(-31,-45);
            addArbusto(33,-91);
            addArbusto(129,109);
            addArbusto(-29,31);
            addArbusto(100, -29);
            addArbusto(-13, 98);
            addArbusto(-33, 18);
            addArbusto(86, -49);
            addArbusto(-65, 29);
            addArbusto(-25, 93);
            addArbusto(60, -18);
            
            pastosCoords.Add(new Vector3(22, 880, 22));
            pastosCoords.Add(new Vector3(35, 880, 23));
            pastosCoords.Add(new Vector3(0, 880, 23));
            pastosCoords.Add(new Vector3(5, 880, 24));
            pastosCoords.Add(new Vector3(22, 880, 24));
            pastosCoords.Add(new Vector3(27, 880, 21));
            */
        }

        private void addPastos(float x, float z)
        {
            float d = 15;

            addArbusto(x, z);
            addArbusto(x + 130 * d, z);
            addArbusto(x + 90 * d, z - 66 * d);
            addArbusto(x + 68 * d, z + 75 * d);
            addArbusto(x + 12 * d, z + 126 * d);
            addArbusto(x + 153 * d, z + 92 * d);
            addArbusto(x + 175 * d, z - 20 * d);
            addArbusto(x + 10 * d, z + 60 * d);
            addArbusto(x - 76 * d, z - 7 * d);
            addArbusto(x - 31 * d, z - 45 * d);
            addArbusto(x + 33 * d, z - 91 * d);
            addArbusto(x + 129 * d, z + 109 * d);
            addArbusto(x - 29 * d, z + 31 * d);
            addArbusto(x + 100 * d, z - 29 * d);
            addArbusto(x - 13 * d, z + 98 * d);
            addArbusto(x - 33 * d, z + 18 * d);
            addArbusto(x + 86 * d, z - 49 * d);
            addArbusto(x - 65 * d, z + 29 * d);
            addArbusto(x - 25 * d, z + 93 * d);
            addArbusto(x + 60 * d, z - 18 * d);
        }

        private void addArbusto(float x, float z)
        {
            float y;
            interpoledHeight(x, z, out y);

            pastosCoords.Add(new Vector3(x + 22, y, z + 4));
            pastosCoords.Add(new Vector3(x + 35, y, z + 8));
            pastosCoords.Add(new Vector3(x, y, z + 8));
            pastosCoords.Add(new Vector3(x + 5, y, z + 12));
            pastosCoords.Add(new Vector3(x + 22, y, z + 12));
            pastosCoords.Add(new Vector3(x + 27, y, z));
        }
    }
}