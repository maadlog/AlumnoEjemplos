using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using AlumnoEjemplo.Los_Borbotones;
using TgcViewer.Utils.Input;
using AlumnoEjemplos.Los_Borbotones.Menus;

namespace AlumnoEjemplos.Los_Borbotones
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        GameManager gameManager;
        MenuManager menuManager;
        HUDManager hudManager;

        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Los Borbotones";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return @"Sniper - El objetivo del juego es sobrevivir la mayor cantidad de tiempo posible y sumar puntos, el juego termina cuando el jugador es alcanzado por los enemigos y pierde toda su vida.
Presionar L para capturar el mouse. WASD para moverse. L-Shift Para correr. Click izqierdo para disparar, derecho para hacer zoom. N para activar NightVision, Q para cambiar de arma. F6 para dibujar AABB, F7 para activar GodMode. P para aumentar velocidad de Vuelo, O para decrementar";
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("N Vegetacion Visible");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("N Vegetacion Visible", 0);

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("N Sub-terrenos Visibles", 0);

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("N Sub-terrenos Visibles", 0);

            GuiController.Instance.UserVars.addVar("High Score", 0f);

            //GuiController.Instance.UserVars.setValue("High Score", 0);

            ///////////////MODIFIERS//////////////////
            GuiController.Instance.Modifiers.addInterval("RenderFlux", new string[] {
                "RenderAll",
                "NightVision"
            }, 0);

            GuiController.Instance.Modifiers.addInterval("Arma", new string[] {
                "Sniper",
                "Rocket Launcher"
            }, 1);

            GuiController.Instance.Modifiers.addBoolean("DrawBoundingBoxes", "Renderizar BoundingBoxes", false);

            GuiController.Instance.Modifiers.addBoolean("Invincibility", "Activar invencibilidad", false);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("FlySpeed", 0, 1000, 0);
            GuiController.Instance.Modifiers.addFloat("weaponRotation", 0, 2f * (float)Math.PI, 0.1f);

            //Crear un modifier para un ComboBox con opciones
            //string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            //GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("weaponOffset", new Vector3(-10, -20, -10), new Vector3(10, 10, 10), new Vector3(5f, -10.2f, 0.8f));

            //Creacion del Game, Menu, y HUD Managers
            gameManager = GameManager.Instance;
            menuManager = MenuManager.Instance;
            hudManager = HUDManager.Instance;

            menuManager.Init();
            gameManager.Init();
            hudManager.Init();
        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            menuManager.Update(elapsedTime);
            menuManager.Render(elapsedTime);
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            gameManager.close();
            menuManager.close();
            hudManager.close();
        }

    }
}
