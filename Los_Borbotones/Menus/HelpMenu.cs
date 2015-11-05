using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.Los_Borbotones.Menus
{
    public class HelpMenu : Menu
    {
        TgcText2d instrucciones;

        internal override void Init()
        {
            int ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            int ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;
            int widthRatio = ScreenWidth * 100 / 1126;
            int heightRatio = ScreenHeight * 100 / 617;

            instrucciones = new TgcText2d();
            instrucciones.Text = @"    El objetivo del juego es sobrevivir la mayor cantidad de tiempo posible y sumar puntos, el juego termina cuando el jugador es alcanzado por los enemigos y pierde toda su vida.
    Presionar L para capturar el mouse. WASD para moverse. L-Shift Para correr. Click izqierdo para disparar, derecho para hacer zoom. N para activar NightVision, Q para cambiar de arma. 
    Para Debug: F6 para dibujar AABB, F7 para activar GodMode. P para aumentar velocidad de Vuelo, O para decrementar.";
            instrucciones.Color = Color.Crimson;
            instrucciones.Align = TgcText2d.TextAlign.LEFT;
            instrucciones.Position = new Point(0 * widthRatio / 100, 300 * heightRatio / 100);
            instrucciones.changeFont(new System.Drawing.Font("TimesNewRoman", 15, FontStyle.Bold));

            TgcText2d backText = new TgcText2d();
            backText.Text = "Back";
            backText.Color = Color.Crimson;
            backText.Align = TgcText2d.TextAlign.LEFT;
            backText.Position = new Point(100 * widthRatio / 100, 500 * heightRatio / 100);
            backText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            texts = new TgcText2d[1] { backText };
            base.Init();
        }

        internal override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (input.keyPressed(Key.Space) || input.keyPressed(Key.Return))
            {
                select = true;
                switch (texts[selectedText].Text)
                {
                    case "Back": MenuManager.Instance.cargarPantalla(new MainMenu());
                        break;
                }
            }
        }

        internal override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);
            instrucciones.render();
        }

        internal override void close()
        {
            instrucciones.dispose();
            base.close();
        }
    }
}
