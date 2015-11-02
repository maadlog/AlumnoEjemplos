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
        float time;
        TgcSprite cross;

        TgcText2d scoreText;
        public TgcText2d healthText;
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


        float SMALL_SCOPE = 0.10f; // 20% of screen covered by scope (X-Axis)
        float BIGASS_SCOPE = 2f;  // 200%
        float ZOOM_CONST = 0.8f; //TODO Hacer dependiente del arma
        TgcTexture normalScope;
        TgcTexture zoomedScope;
        float screenCovered;


        public void Init()
        {
            GAME_OVER = false;
            TEXT_DELAY = 0;
            time = 0;
            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;

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
            scoreText.Text = "SCORE: " + GameManager.Instance.score;
            scoreText.Color = Color.LightBlue;
            scoreText.changeFont(new System.Drawing.Font("Arial", 10, FontStyle.Bold));

            //texto para la vida
            //tambien cambia de color segun la vida
            healthText = new TgcText2d();
            healthText.Text = "HEALTH: "  + GameManager.Instance.player1.vida;
            healthText.Color = Color.Green;
            healthText.changeFont(new System.Drawing.Font("Arial", 10, FontStyle.Bold));
            healthText.Position = new Point(0, 250);
            healthText.Align = TgcText2d.TextAlign.LEFT;

            // cargamos la mira
            screenCovered = SMALL_SCOPE;
            cross = new TgcSprite();
            normalScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\normalScope.png");
            zoomedScope = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\zoomedScope.png");
            cross.Texture = normalScope;


            refreshScopeTexture();

            //inicializo audio
            sound = new TgcStaticSound();
            ambient = new TgcStaticSound();
            string dir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Ambiente/Deep_space.wav";
            ambient.loadSound(dir, -1500);
            ambient.play(true);
        }

        public void Update(float elapsedTime)
        {
            if (TEXT_DELAY > 0) { TEXT_DELAY -= elapsedTime; }

            if (TEXT_DELAY <= 0 && GAME_OVER)
            {
                MenuManager.Instance.cargarPantalla(new AlumnoEjemplos.Los_Borbotones.Menus.MainMenu());
                close();
                Init();
            }
        }

        public void Render(float elapsedTime)
        {
            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            cross.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            scoreText.render();
            healthText.render();
            if (TEXT_DELAY > 0) { specialKillText.render(); }
        }

        //SPRITES

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

        //HEALTH

        public void ChangeColorHealth()
        {
            ;
            //cambiar color de la vida segun el atributo vida
            if (GameManager.Instance.player1.vida >= 51)
            {
                healthText.Color = Color.Green;
            }
            if (GameManager.Instance.player1.vida < 51)
            {
                healthText.Color = Color.Yellow;
            }
            if (GameManager.Instance.player1.vida < 26)
            {
                healthText.Color = Color.Red;
            }

        }

        //SCORE
        public void refreshScore()
        {
            scoreText.Text = "SCORE: " + GameManager.Instance.score;
            ChangeTextColor();
        }

        public void refreshHealth()
        {
            healthText.Text = "HEALTH: " + GameManager.Instance.player1.vida;
            ChangeColorHealth();
        }

        public void headShot()
        {
            specialKillText.Text = "HEADSHOT!!";
            TEXT_DELAY = TEXT_DELAY_MAX;
            playSound(headshotSoundDir);
        }

        public void headHunter()
        {
            specialKillText.Text = "HEAD HUNTER!!";
            TEXT_DELAY = TEXT_DELAY_MAX;
            playSound(headhunterSoundDir);
        }

        public void awardKill(int killMultiTracker)
        {
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

        public void gameOver()
        {
            specialKillText.Text = "GAME OVER";
            TEXT_DELAY = TEXT_DELAY_MAX;
            GAME_OVER = true;
        }

        public void denied()
        {
            playSound(deniedSoundDir);
        }

        private void ChangeTextColor()
        {
            //cambiamos el color del score segun el puntaje
            if (GameManager.Instance.score >= 0)
            {
                scoreText.Color = Color.White;
            }
            if (GameManager.Instance.score > 10)
            {
                scoreText.Color = Color.Orange;
            }
            if (GameManager.Instance.score > 20)
            {
                scoreText.Color = Color.Silver;
            }
            if (GameManager.Instance.score > 30)
            {
                scoreText.Color = Color.Gold;
            }
            if (GameManager.Instance.score > 50)
            {
                scoreText.Color = Color.LightCyan;
            }
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
        public void close()
        {
            specialKillText.dispose();
            scoreText.dispose();
            healthText.dispose();
            normalScope.dispose();
            zoomedScope.dispose();
            ambient.dispose();
        }
    }
}
