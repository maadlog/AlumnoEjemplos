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
    public class MainMenu : Menu
    {
        TgcText2d playText;
        TgcText2d helpText;
        TgcText2d creditsText;

        internal override void Init()
        {
            playText = new TgcText2d();
            playText.Text = "Play";
            playText.Color = Color.Crimson;
            playText.Align = TgcText2d.TextAlign.LEFT;
            playText.Position = new Point(100, 300);
            playText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            helpText = new TgcText2d();
            helpText.Text = "Help";
            helpText.Color = Color.Crimson;
            helpText.Align = TgcText2d.TextAlign.LEFT;
            helpText.Position = new Point(100, 350);
            helpText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            creditsText = new TgcText2d();
            creditsText.Text = "Credits";
            creditsText.Color = Color.Crimson;
            creditsText.Align = TgcText2d.TextAlign.LEFT;
            creditsText.Position = new Point(100, 400);
            creditsText.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            texts = new TgcText2d[3] { playText, helpText, creditsText };
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
                    case "Play": MenuManager.Instance.cargarPantalla(PostProcessManager.Instance);
                        break;

                    case "Help": MenuManager.Instance.cargarPantalla(new HelpMenu());
                        break;

                    case "Credits": MenuManager.Instance.cargarPantalla(new CreditsMenu());
                        break;
                }
            }
        }
    }
}
