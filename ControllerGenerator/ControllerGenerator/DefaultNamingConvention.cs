namespace ControllerGenerator
{
    internal class DefaultNamingConvention : INamingConvention
    {
        public string GetControllerName<TService>()
        {
            return $"{typeof(TService)}Controller";
        }

        public string GetDTOName(string input)
        {
            return $"{input}Parameters";
        }

        public string GetMethodName(string originalMethodName)
        {
            return originalMethodName;
        }
    }
}
