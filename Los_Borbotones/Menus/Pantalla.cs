using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlumnoEjemplos.Los_Borbotones.Menus
{
    public abstract class Pantalla
    {
        internal abstract void Init();

        internal abstract void Update(float elapsedTime);

        internal abstract void Render(float elapsedTime);

        internal abstract void Dispose();
    }
}
