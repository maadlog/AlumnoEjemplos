
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public abstract class GameObject
    {
        public TgcMesh mesh;

        public abstract void Init();
        public abstract void Update(float elapsedTime);
        public abstract void Render(float elapsedTime);
        
        public void dispose()
        {
            mesh.dispose();
        }

        public Vector3 getPosition()
        {
            Vector3 vector = new Vector3();
            mesh.getPosition(vector);
            return vector;
        }
    }
}
