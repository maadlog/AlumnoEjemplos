using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Enemy_lvl_2:Enemy
    {
        override
           public void Init()
        {
            //seteamos atributos particulares de las naves
            health = 50;
            score = 2;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MESH_SCALE = 0.5f;
            attackDamage = 50;
            MOVEMENT_SPEED = 170f;
            //cargamos el mesh
            //las naves no tienen skeletalMesh
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\StarWars-Speeder\\StarWars-Speeder-TgcScene.xml");
            this.mesh = scene.Meshes[0];
            SPAWN_HEIGHT = 1000f;
            giroInicial = Matrix.RotationY(0);

            //realizamos el init() comun a todos los enemigos
            base.Init();

            setBaseEffect();

            //creamos las boundingbox
            //a pesar de que las naves no tienen legs ni head, le seteamos boxes "vacias" para no tener problemas con Excepciones de null
            HEADSHOT_BOUNDINGBOX = new TgcBoundingBox();
            CHEST_BOUNDINGBOX = this.mesh.BoundingBox.clone();
            LEGS_BOUNDINGBOX = new TgcBoundingBox();
            //carga de sonido
            SonidoMovimiento = new Tgc3dSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Audio\\Robot\\ufoHum.wav", getPosicionActual());
            SonidoMovimiento.MinDistance = 75f;
            SonidoMovimiento.play(true);
            
        }

        override public void updateMovementMatrix(float elapsedTime, Vector3 Direccion)
        {
            //redefinimos esta funcion para las naves porque no queremos que hagan el nivelado con el piso
            //salvo por eso funciona igual que el updatemovementmatrix de Enemy

            Vector3 vectorPosActual = new Vector3(posicionActual.M41, posicionActual.M42, posicionActual.M43);

            //vectorDireccionRotacion = new Vector3(vectorDireccion.X, vectorDireccion.Y, vectorDireccion.Z);
            //vectorDireccionRotacion.Normalize();

            float y;
            GameManager.Instance.interpoledHeight(vectorPosActual.X, vectorPosActual.Z, out y);
            //float headOffsetY = posicionActualHeadshot.M42 - posicionActual.M42;

            
            if (vectorPosActual.Y < y)
            {
                posicionActual.M42 = y;
            }
            
            //posicionActualHeadshot.M42 = headOffsetY + y;

            MatOrientarObjeto = calcularMatrizOrientacion(vectorDireccionRotacion);

            Traslacion = Matrix.Translation(Direccion * MOVEMENT_SPEED * elapsedTime);

            Matrix transform = MatOrientarObjeto * posicionActual * Traslacion;
            this.mesh.Transform = transform;

            this.mesh.BoundingBox.transform(transform);
            this.CHEST_BOUNDINGBOX.transform(transform);

            posicionActual = posicionActual * Traslacion;
            //this.HEADSHOT_BOUNDINGBOX.transform(MatOrientarObjeto * posicionActualHeadshot * Traslacion);
            //posicionActualHeadshot = posicionActualHeadshot * Traslacion;
        }
    }
}
