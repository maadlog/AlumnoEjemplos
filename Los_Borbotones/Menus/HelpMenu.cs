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
            instrucciones = new TgcText2d();
            instrucciones.Text = @"Sniper - El objetivo del juego es sobrevivir la mayor cantidad de tiempo posible y sumar puntos, el juego termina cuando el jugador es alcanzado por los enemigos y pierde toda su vida.
Presionar L para capturar el mouse. WASD para moverse. L-Shift Para correr. Click izqierdo para disparar, derecho para hacer zoom";
            instrucciones.Color = Color.Crimson;
            instrucciones.Align = TgcText2d.TextAlign.LEFT;
            instrucciones.Position = new Point(0, 200);
            instrucciones.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            TgcText2d backText = new TgcText2d();
            backText.Text = "Back";
            backText.Color = Color.Crimson;
            backText.Align = TgcText2d.TextAlign.LEFT;
            backText.Position = new Point(100, 450);
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
            instrucciones.render();
            base.Render(elapsedTime);
        }

        internal override void close()
        {
            instrucciones.dispose();
            base.close();
        }
    }
}
