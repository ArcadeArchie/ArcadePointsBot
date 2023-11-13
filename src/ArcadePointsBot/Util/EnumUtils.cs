using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Util;

public static class EnumUtils
{
    public static IEnumerable GetValues<T>() where T : Enum
    => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(typeof(T)));
}
