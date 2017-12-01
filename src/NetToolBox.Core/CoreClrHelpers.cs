using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NetToolBox.Core
{
    public static class CoreClrHelpers

    {
        static string coreCLRVersion = "NOT_YET_ASSESSED";
        public static string GetCoreClrVersion()
        {
            if (coreCLRVersion == "NOT_YET_ASSESSED") //the following code might take some time to run, but we only need to do the heavy lifting once.  Not sure if this is the best way to determine CLr version, but it works
            {
                var appDomainType = typeof(object).GetTypeInfo().Assembly?.GetType("System.AppDomain");
                var currentDomain = appDomainType?.GetProperty("CurrentDomain")?.GetValue(null);
                var deps = appDomainType?.GetMethod("GetData")?.Invoke(currentDomain, new[] { "FX_DEPS_FILE" });
                if (deps == null)
                {
                    coreCLRVersion = "";
                    return coreCLRVersion;
                }
                coreCLRVersion = GetCoreClrVersionImpl(deps.ToString());
            }
            return coreCLRVersion;
        }

        internal static string GetCoreClrVersionImpl(string deps)
        {
            var result = Regex.Match(deps, "(?:(\\d+)\\.)?(?:(\\d+)\\.)?(?:(\\d+)\\.\\d+)").Value;
            return result;
        }
    }

}
