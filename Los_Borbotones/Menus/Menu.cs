using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones.Menus
{
    public class Menu : Pantalla
    {
        internal int selectedText;
        internal TgcText2d[] texts;
        internal bool select;
        internal TgcSprite logo;

        internal override void Init()
        {
            select = false;
            selectedText = 0;
            GuiController.Instance.BackgroundColor = Color.White;

            logo = new TgcSprite();
            TgcTexture texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Sprites\\logo.png");
            logo.Texture = texture;

            float ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            float ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;
            Size tamaño = logo.Texture.Size;
            float scale = ScreenWidth * (1) / tamaño.Width;
            logo.Scaling = new Vector2(scale, scale);
            logo.Position = new Vector2((ScreenWidth - (tamaño.Width * scale)), (ScreenHeight - (tamaño.Height * scale)) / 10);
        }

        internal override void Update(float elapsedTime)
        {
            //Size size = texts[selectedText].Size;
            //size.Height -= 10;
            //size.Width -= 10;
            //texts[selectedText].Size = size;
            texts[selectedText].Color = Color.Crimson;

            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (input.keyPressed(Key.Up) || input.keyPressed(Key.W))
            {
                selectedText--;
            }
            else if (input.keyPressed(Key.Down) || input.keyPressed(Key.S))
            {
                selectedText++;
            }
            if (selectedText == texts.Length -1)
            {
                selectedText = 0;
            }
            else if(selectedText < 0)
            {
                selectedText = texts.Length -2;
            }

            //size = texts[selectedText].Size;
            //size.Height += 10;
            //size.Width += 10;
            //texts[selectedText].Size = size;
            texts[selectedText].Color = Color.Red;
        }

        internal override void Render(float elapsedTime)
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            logo.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            if (!select)
            {
                foreach (TgcText2d text in texts)
                {
                    text.render();
                }
            }
        }

        internal override void close()
        {           
            for (int i = texts.Length-1; i < 0; i--)
            {
                texts[i].dispose();
            }
        }
    }
}
