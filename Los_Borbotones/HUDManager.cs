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

        string alumnoDir = GuiController.Instance.AlumnoEjemplosDir;
        string exampleDir = GuiController.Instance.ExamplesMediaDir;
        public int ScreenHeight, ScreenWidth;
        Device d3dDevice;
        float time;
        Weapon prevWeap;

        TgcSprite HudFront;
        TgcSprite cross;
        TgcSprite healthSprite;
        TgcSprite hudWeapon;
        TgcSprite scoreSprite;
        TgcSprite mapBaseSprite;
        TgcSprite playerPointerSprite;
        TgcSprite highScoreSprite;

        TgcText2d scoreText;
        TgcText2d captureText;
        
        TgcText2d specialKillText;
        float TEXT_DELAY;
        float TEXT_DELAY_MAX = 2f;
        public bool GAME_OVER;
        bool zoomEnabled = false;

        //seteamos las dir de los sonidos
        public int PLAYER_VOLUME = -1500; //va de -10000 (min) a 0 (max) por alguna razon
        TgcStaticSound sound;
        TgcStaticSound ambient;
        string headshotSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/headshot.wav";
        string headhunterSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/headhunter.wav";
        string doubleSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/doublekill.wav";
        string multiSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/multikill.wav";
        string ultraSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/ultrakill.wav";
        string megaSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/megakill.wav";
        string monsterSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/monsterkill.wav";
        string massacreSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/massacre.wav";
        string deniedSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/denied.wav";
        string highScoreSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Anunciador/highScore.wav"; 

        float SMALL_SCOPE = 0.10f; // 20% of screen covered by scope (X-Axis)
        float BIGASS_SCOPE = 2f;  // 200%
        float ZOOM_CONST = 0.8f; //TODO Hacer dependiente del arma
        TgcTexture normalScope;
        TgcTexture zoomedScope;
        float screenCovered;

        TgcTexture launcherTexture;
        TgcTexture sniperTexture;

        float hudScreenCovered = 0.15f;

        float MAP_RADIUS = 50f;
        Vector2 mapCenter;
        Vector2 initialPos = new Vector2(0f, 1f);

        public bool reachedHighScore = false;

        public void Init()
        {
            GAME_OVER = false;
            TEXT_DELAY = 0;
            time = 0;
            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;
            d3dDevice = GuiController.Instance.D3dDevice;

           
            //-------------User Interface------------
            //Textos para los Kills
            specialKillText = new TgcText2d();
            specialKillText.Color = Color.Crimson;
            specialKillText.Align = TgcText2d.TextAlign.CENTER;
            specialKillText.Position = new Point(0, 100);
            specialKillText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            //texto para el score
            //cambia de color segun el score
            scoreText = new TgcText2d();
            scoreText.Text = GameManager.Instance.score.ToString();
            scoreText.Color = Color.LightBlue;
            scoreText.changeFont(new System.Drawing.Font("Arial", 20, FontStyle.Bold));

            //texto para el capturas
            captureText = new TgcText2d();
            captureText.Text = GameManager.Instance.capturas.ToString() + "/11";
            captureText.Color = Color.FloralWhite;
            captureText.Position = new Point(450,0);
            captureText.changeFont(new System.Drawing.Font("Arial", 20, FontStyle.Bold));

            // cargamos los sprites
            screenCovered = SMALL_SCOPE;
            cross = new TgcSprite();
            normalScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\normalScope.png");
            zoomedScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\zoomedScope.png");
            cross.Texture = normalScope;

            refreshScopeTexture();

            HudFront = new TgcSprite();
            HudFront.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\HudFront.png");

            initHudFront();
            
            healthSprite = new TgcSprite();
            healthSprite.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\HealthStencil.png");

            healthInit();
            
            hudWeapon = new TgcSprite();
            sniperTexture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\sniperHUD.png");
            launcherTexture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\launcherHUD.png");
            refreshMainWeapon();
            hudWeaponInit();

            scoreSprite = new TgcSprite();
            scoreSprite.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\Score.png");

            initScoreSprite();

            mapBaseSprite = new TgcSprite();
            
            mapBaseSprite.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\MapBase.png");

            initMapBase();

            playerPointerSprite = new TgcSprite();
            playerPointerSprite.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\playerPointer.png");

            initPlayerPointer();

            highScoreSprite = new TgcSprite();
            highScoreSprite.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\highScore.png");

            initHighScore();

            //inicializo audio
            sound = new TgcStaticSound();
            ambient = new TgcStaticSound();
            string dir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Ambiente/Deep_space.wav";
            ambient.loadSound(dir, -1500);
            ambient.play(true);

            reachedHighScore = false;

        }

        public void Update(float elapsedTime)
        {
            time += elapsedTime;

            if (TEXT_DELAY > 0) { TEXT_DELAY -= elapsedTime; }

            if (TEXT_DELAY <= 0 && GAME_OVER)
            {
                MenuManager.Instance.cargarPantalla(new AlumnoEjemplos.Los_Borbotones.Menus.MainMenu());
                close();
            }
            refreshMainWeapon();
        }

        public void Render(float elapsedTime)
        {
            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            healthSprite.render();
            HudFront.render();
            scoreSprite.render();
            hudWeapon.render();
            if (reachedHighScore) highScoreSprite.render();

            cross.render();
            

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            // TODO, cuando el refresh funque 
            
            renderMinimap();
            

            scoreText.render();
            captureText.render();
            if (TEXT_DELAY > 0) {
                int alphaLerp = (int)(TEXT_DELAY * 255 / TEXT_DELAY_MAX);
                specialKillText.Color = Color.FromArgb(alphaLerp, specialKillText.Color);
                specialKillText.render(); }
        }

        //SPRITES

        public void specialKillTextInit()
        {
            TEXT_DELAY = TEXT_DELAY_MAX;
            specialKillText.Position = new Point(0, 100);
        }

        public void refreshScopeTexture()
        {
            Size tamaño = cross.Texture.Size;
            float scale = ScreenWidth * screenCovered / tamaño.Width;
            cross.Scaling = new Vector2(scale, scale);
            cross.Position = new Vector2((ScreenWidth - (tamaño.Width * scale)) / 2, (ScreenHeight - (tamaño.Height * scale)) / 2);
        }

        public void zoomCamera()
        {
            //hacer zoom

            if (zoomEnabled)
            {
                cross.Texture = normalScope;
                CustomFpsCamera.Instance.Zoom = 0;
                screenCovered = SMALL_SCOPE; // 1/6 of screen covered by scope
                zoomEnabled = false;
            }
            else
            {
                cross.Texture = zoomedScope;
                CustomFpsCamera.Instance.Zoom = ZOOM_CONST;
                screenCovered = BIGASS_SCOPE; // scope scaled to twice the screen wdth
                zoomEnabled = true;
            }

            refreshScopeTexture();

        }

        public void initHudFront()
        {
            Size tamaño = HudFront.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            HudFront.Scaling = new Vector2(scale, scale);
            HudFront.Position = new Vector2((ScreenWidth * 0.01f) , ScreenHeight - (ScreenHeight * 0.01f) - (tamaño.Height * scale));

        }

        public void initHighScore()
        {
            Size tamaño = highScoreSprite.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered/5f / tamaño.Width;
            highScoreSprite.Scaling = new Vector2(scale, scale);
            highScoreSprite.Position = new Vector2((ScreenWidth * 0.5f) - (tamaño.Width * scale / 2), tamaño.Height * scale); 

        }


        public void initMapBase()
        {
            Size tamaño = mapBaseSprite.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            mapBaseSprite.Scaling = new Vector2(scale, scale);
            mapBaseSprite.Position = new Vector2((ScreenWidth * 0.01f), (ScreenHeight * 0.01f));

            Vector2 tamañoReal = new Vector2(tamaño.Width * scale, tamaño.Height * scale);
            tamañoReal.Multiply(0.5f);
            mapCenter = new Vector2((ScreenWidth * 0.01f) + tamañoReal.X, (ScreenHeight * 0.01f) + tamañoReal.Y);

        }

        public void initPlayerPointer()
        {
            Size tamaño = playerPointerSprite.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            playerPointerSprite.Scaling = new Vector2(scale, scale);
            playerPointerSprite.Position = new Vector2((ScreenWidth * 0.01f), (ScreenHeight * 0.01f));

            playerPointerSprite.RotationCenter = new Vector2(tamaño.Width,tamaño.Height) * scale * 0.5f;
            
        }

        public void updatePlayerPointer()
        {

            Vector3 currentView = CustomFpsCamera.Instance.getLookAt() - CustomFpsCamera.Instance.getPosition();

            Vector2 cur = new Vector2(currentView.X, currentView.Z);
            cur.Normalize();
            float rot = (float)FastMath.Acos(Vector2.Dot(initialPos,cur));
            float cross = Vector2.Ccw(initialPos,cur);

            float arc = cross > 0 ? rot : -rot;

            playerPointerSprite.Rotation = (float)Math.PI / 2 - arc;
            
        }


        public void initScoreSprite()
        {
            Size tamaño = scoreSprite.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            scoreSprite.Scaling = new Vector2(scale, scale);
            scoreSprite.Position = new Vector2((ScreenWidth * 0.5f) - (tamaño.Width * scale/2), 0f);
        }


        public void healthInit()
        {
            Size tamaño = healthSprite.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            healthSprite.Scaling = new Vector2(scale, scale);
            healthSprite.Position = new Vector2((ScreenWidth * 0.01f) , ScreenHeight - (ScreenHeight * 0.01f) - (tamaño.Height * scale));
            healthSprite.Color = Color.LightGreen;
        }

        public void refreshMainWeapon()
        {
            Weapon MainWeapon = GameManager.Instance.player1.weapon;
            if (MainWeapon != prevWeap) {
                if (MainWeapon.Equals(GameManager.Instance.player1.sniper))
                {
                    hudWeapon.Texture = sniperTexture;
                    prevWeap = GameManager.Instance.player1.sniper;
                }
                else
                {
                    hudWeapon.Texture = launcherTexture;
                    prevWeap = GameManager.Instance.player1.launcher;
                }
            }
        }

        public void hudWeaponInit()
        {
            Size tamaño = hudWeapon.Texture.Size;
            float scale = ScreenWidth * hudScreenCovered / tamaño.Width;
            hudWeapon.Scaling = new Vector2(scale, scale);
            hudWeapon.Position = new Vector2((ScreenWidth * 0.01f), ScreenHeight - (ScreenHeight * 0.01f) - (tamaño.Height * scale));
        }
        //HEALTH

        public void refreshHealth()
        {
            //cambiar color de la vida segun el atributo vida
            if (GameManager.Instance.player1.vida >= 51)
            {
                healthSprite.Color = Color.Green;
            }
            if (GameManager.Instance.player1.vida < 51)
            {
                healthSprite.Color = Color.Yellow;
            }
            if (GameManager.Instance.player1.vida < 26)
            {
                healthSprite.Color = Color.Red;
            }
        }

        //SCORE
        public void refreshScore()
        {
            float score = GameManager.Instance.score;
            float highScore = (float)GuiController.Instance.UserVars.getValue("High Score");
            scoreText.Text = score.ToString();
            if(!reachedHighScore && score > highScore)
            {
                playSound(highScoreSoundDir);
                reachedHighScore = true;
            }


        }

        public void refreshCapture()
        {
            captureText.Text = GameManager.Instance.capturas.ToString() + "/11";
        }

        public void headShot()
        {
            specialKillText.Text = "HEADSHOT!!";
            specialKillTextInit();
            playSound(headshotSoundDir);
        }

        public void headHunter()
        {
            specialKillText.Text = "HEAD HUNTER!!";
            specialKillTextInit();
            playSound(headhunterSoundDir);
        }

        public void awardKill(int killMultiTracker)
        {
            switch (killMultiTracker)
            {
                case 2:
                    specialKillText.Text = "DOUBLE KILL";
                    specialKillTextInit();
                    playSound(doubleSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 200, 200));
                    break;
                case 3:
                    specialKillText.Text = "MULTI KILL";
                    specialKillTextInit();
                    playSound(multiSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 170, 170));
                    break;
                case 4:
                    specialKillText.Text = "MEGA KILL";
                    specialKillTextInit();
                    playSound(megaSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 120, 120));
                    break;
                case 5:
                    specialKillText.Text = "ULTRA KILL";
                    specialKillTextInit();
                    playSound(ultraSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 90, 90));
                    break;
                case 6:
                    specialKillText.Text = "MONSTER KILL";
                    specialKillTextInit();
                    playSound(monsterSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 50, 50));
                    break;
                case 10:
                    specialKillText.Text = "MASSACRE";
                    specialKillTextInit();
                    playSound(massacreSoundDir);
                    ChangeTextColor(Color.FromArgb(255, 0, 0));
                    break;
                default:
                    break;
            }
        }

        public void gameOver()
        {
            if (reachedHighScore) GuiController.Instance.UserVars.setValue("High Score", GameManager.Instance.score);
            specialKillText.Text = "GAME OVER";
            specialKillTextInit();
            GAME_OVER = true;
        }

        public void gameWinner()
        {
            if (reachedHighScore) GuiController.Instance.UserVars.setValue("High Score", GameManager.Instance.score);
            specialKillText.Text = "WINNER";
            specialKillTextInit();
            GameManager.Instance.capturas++;
            GameManager.Instance.WINNER = false;
            GAME_OVER = true;
        }

        public void denied()
        {
            playSound(deniedSoundDir);
            ChangeTextColor(Color.LightBlue);
        }

        private void ChangeTextColor(Color aColor)
        {
            scoreText.Color = aColor;
        }

        public void playSound(string dir)
        {
            //reproducir un sonido
            sound.dispose();
            sound.loadSound(dir);
            sound.play();
        }

        public void playSound(TgcStaticSound sound, string dir, bool loop)
        {
            //reproducir un sonido
            sound.dispose();
            sound.loadSound(dir, PLAYER_VOLUME);
            sound.play(loop);
        }

        public void renderMinimap()
        {
            updatePlayerPointer();

            GuiController.Instance.Drawer2D.beginDrawSprite();
            mapBaseSprite.render();
            playerPointerSprite.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
            
            

            foreach(Enemy enemigo in GameManager.Instance.enemies)
            {
                Vector3 distance = Vector3.Subtract(enemigo.getPosicionActual(),CustomFpsCamera.Instance.getPosition());// - enemigo.getPosicionActual();
                Vector2 dist = new Vector2(distance.X, distance.Z);
                if (Vector2.Length(dist) <= 2500 && !enemigo.muerto)
                {
                    enemigo.updatePointer(mapCenter,dist);

                    GuiController.Instance.Drawer2D.beginDrawSprite();
                    enemigo.pointer.render();
                    GuiController.Instance.Drawer2D.endDrawSprite();
                }
            }

            int t;
            for (t = 1; t < GameManager.Instance.tesoros.Count; t++)
            {
                Vector3 distance = Vector3.Subtract(GameManager.Instance.tesoros[t].Position, CustomFpsCamera.Instance.getPosition());
                Vector2 dist = new Vector2(distance.X, distance.Z);
                if (Vector2.Length(dist) <= 2500)
                {
                    GameManager.Instance.updatePointer(mapCenter, dist, t);

                    GuiController.Instance.Drawer2D.beginDrawSprite();
                    GameManager.Instance.pointers[t].render();
                    GuiController.Instance.Drawer2D.endDrawSprite();
                }
            }
        }

        public void close()
        {
            specialKillText.dispose();
            scoreText.dispose();
            captureText.dispose();
            normalScope.dispose();
            zoomedScope.dispose();
            ambient.dispose();


            HudFront.dispose();
            cross.dispose();
            healthSprite.dispose();
            hudWeapon.dispose();
            scoreSprite.dispose();
            mapBaseSprite.dispose();
            playerPointerSprite.dispose();
            highScoreSprite.dispose();
            sound.dispose();
            launcherTexture.dispose();
            sniperTexture.dispose();
            
        }
    }
}
