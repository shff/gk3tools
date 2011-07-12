using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public interface ISceneCustomizer
    {
        void OnLoad();
        void OnCustomFunction(string name);
    }
}
