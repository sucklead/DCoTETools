using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace banal
{
    public enum NewFunctionTypeA :short
    {
        Wait = 0,
        Random = 1,
        RandomInt = 2,
        RunGameScript = 95,
        RunGameScriptEx = 96,
        D_Message = 206,
        D_SetGlobalInt = 208,
        D_GetGlobalInt = 211,
        D_GetCurrentLevel = 280,
        D_FadeScreen = 284,
        D_AppendToString = 477,
        D_RainEnable = 487,
        D_CannonGetNumberOfSorcerers = 645,
        D_SetGlobalFloat = 207
    }
}
