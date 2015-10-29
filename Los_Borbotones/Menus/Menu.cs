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
    public class Menu : Pantalla
    {
        internal int selectedText;
        internal TgcText2d[] texts;
        internal bool select;

        internal override void Init()
        {
            select = false;
            selectedText = 0;
            GuiController.Instance.BackgroundColor = Color.Black;
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
            if (selectedText == texts.Length)
            {
                selectedText = 0;
            }
            else if(selectedText < 0)
            {
                selectedText = texts.Length -1;
            }

            //size = texts[selectedText].Size;
            //size.Height += 10;
            //size.Width += 10;
            //texts[selectedText].Size = size;
            texts[selectedText].Color = Color.Red;
        }

        internal override void Render(float elapsedTime)
        {
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
