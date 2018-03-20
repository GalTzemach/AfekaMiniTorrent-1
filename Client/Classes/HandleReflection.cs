using System;
using System.Reflection;
using System.Text;

namespace Client
{
    class HandleReflection
    {
        public static string GetAuthors(string downloadPath)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Assembly assembly = null;

            try
            {
                // Loading assembly from dll file.
                assembly = Assembly.LoadFrom(downloadPath + "\\" + UserWindow.REFLECTION_DLL_FILE_NAME);

                // Check if loading failed.
                if (assembly == null)
                    return UserWindow.REFLECTION_DLL_FILE_NAME + " file not found";

                // Getting Author type for use below.
                Type op = assembly.GetType("MyReflection.Author");

                // Getting all types from assembly.
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    stringBuilder.Append("The author/s of " + type.Name + ":\n");

                    // Getting all attributes for each type .
                    object[] objects = type.GetCustomAttributes(false);
                    foreach (object obj in objects)
                    {
                        Attribute attribute = obj as Attribute;
                        if (attribute != null)
                        {
                            Type typeOfAttribute = attribute.GetType();
                            if (typeOfAttribute == op)
                            {
                                PropertyInfo propertyInfo = typeOfAttribute.GetProperty("Name");
                                string authorName = (string)propertyInfo.GetValue(attribute, null);
                                stringBuilder.Append("     " + authorName + "\n");
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {
                return e.ToString();
            }

            return stringBuilder.ToString();
        }
    }
}
