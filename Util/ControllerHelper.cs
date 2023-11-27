using Microsoft.AspNetCore.Mvc;

namespace patter_pal.Util
{
    public static class ControllerHelper
    {
        /// <summary>
        /// Gets the name of the controller to be used in routing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetControllerName<T>() where T : Controller
        {
            string controllerName = typeof(T).Name;
            return controllerName.EndsWith("Controller") ? controllerName[0..^"Controller".Length] : controllerName;
        }
    }
}
