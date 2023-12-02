using patter_pal.domain.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.domain.Util
{
    public static class AuthHelper
    {
        public static bool IsValidSpecialCode(AppConfig appConfig, string code)
        {
            return appConfig.ValidSpecialCodes.Split(";").Any(c => c == code);
        }
    }
}
