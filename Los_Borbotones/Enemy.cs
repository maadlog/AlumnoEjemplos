using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;


namespace AlumnoEjemplos.Los_Borbotones
{
    abstract class Enemy : GameObject
    {
        public float MOVEMENT_SPEED;
        public float SPAWN_RADIUS;
        public float ANGLE;
        public Matrix posicionActual;
        

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void Update(float elapsedTime)
        {
       Device d3dDevice = GuiController.Instance.D3dDevice;
            Vector3 vectorDireccion;
            Vector3 inicio = new Vector3(0, 0, 0);
            vectorDireccion = ( CustomFpsCamera.Instance.Position );
            vectorDireccion.Normalize();
            Vector3 normal = new Vector3(0,1,0);
            Matrix MatOrientarObjeto = Matrix.RotationAxis(Vector3.Cross(normal, vectorDireccion), (float)Math.Acos(Vector3.Dot(normal, vectorDireccion)));
            this.mesh.Transform = MatOrientarObjeto * posicionActual;
            
        }

        public override void Render(float elapsedTime)
        {
            this.mesh.render();
        }
    }
}
