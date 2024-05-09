using System.Collections.Generic;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Core.Utils
{
    public static class EasingUtils
    {
        // Rename!!!!
        // A map that contains probably all the possible values inside the "ease" field of the new json files on FNF
        // Most of the names are like "expoOut" (type|direction) and in osu framework they are like "OutExpo" (direction|type) also including capital letters
        public static Dictionary<string, Easing> FLX_CONVERSION { get; private set; } = new Dictionary<string, Easing>()
        {
            { "linear", Easing.None },

            { "sineIn", Easing.InSine },
            { "sineOut", Easing.OutSine },
            { "sineInOut", Easing.InOutSine },

            { "quadIn", Easing.InQuad },
            { "quadOut", Easing.OutQuad },
            { "quadInOut", Easing.InOutQuad },

            { "cubeIn", Easing.InCubic },
            { "cubeOut", Easing.OutCubic },
            { "cubeInOut", Easing.InOutCubic },

            { "quartIn", Easing.InQuart },
            { "quartOut", Easing.OutQuart },
            { "quartInOut", Easing.InOutQuart },

            { "quintIn", Easing.InQuint },
            { "quintOut", Easing.OutQuint },
            { "quintInOut", Easing.InOutQuint },

            { "expoIn", Easing.InExpo },
            { "expoOut", Easing.OutExpo },
            { "expoInOut", Easing.InOutExpo },

            // Missing smooths
            { "smoothStepIn", Easing.None },
            { "smoothStepOut", Easing.None },
            { "smoothStepInOut", Easing.None },


            { "elasticIn", Easing.InElastic },
            { "elasticOut", Easing.OutElastic },
            { "elasticInOut", Easing.InOutElastic },
        };
    }
}
