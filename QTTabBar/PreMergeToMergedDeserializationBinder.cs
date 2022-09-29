using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace QTTabBarLib
{
    internal class PreMergeToMergedDeserializationBinder : SerializationBinder
    {

        public override Type BindToType(string assemblyName, string typeName)
        {

            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to
            // a different type, set typeToDeserialize to the desired type.
            String exeAssembly = Assembly.GetExecutingAssembly().FullName;

            QTUtility2.log("PreMergeToMergedDeserializationBinder exeAssembly:" + exeAssembly + " typeName: " + typeName );
            if (typeName.Contains("Entities."))
            {
                typeName = typeName.Replace("Entities", "FMDService");
                typeToDeserialize = Type.GetType("FMDServiceProxy." + typeName + ", FMDServiceProxy, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null");
            }
            else
            {
                // The following line of code returns the type.
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, exeAssembly));
            }

            return typeToDeserialize;
        }
    }

}
