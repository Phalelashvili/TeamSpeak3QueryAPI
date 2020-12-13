using System;
using System.Collections.Generic;
using System.Reflection;

namespace TeamSpeak3QueryApi.Net.Specialized.Responses
{
    public abstract class Response : ITeamSpeakSerializable
    {
        public List<Parameter> ToQueryParameters()
        {
            var queryParameters = new List<Parameter>();

            foreach (var field in GetType().GetFields())
            {
                var value = field.GetValue(this);
                if (value == null)
                    continue;

                // find attribute that determines what key is used in TS query
                var attribute = field.GetCustomAttribute<QuerySerializeAttribute>();
                if (attribute == null)
                    throw new ArgumentException($"Attribute '{nameof(QuerySerializeAttribute)}' not set on field '{field.Name}'");

                // find constructor of ParameterValue that can deserialize value
                ConstructorInfo initializerConstructor = null;
                foreach (var constructorInfo in typeof(ParameterValue).GetConstructors())
                {
                    var constructorParameters = constructorInfo.GetParameters();
                    if (constructorParameters.Length == 1 && constructorParameters[0].ParameterType == value.GetType())
                        initializerConstructor = constructorInfo;
                }
                if (initializerConstructor == null)
                    throw new ArgumentException($"Field '{field.Name}' is not serializable by {nameof(ParameterValue)}");

                var parameterValue = (ParameterValue) initializerConstructor.Invoke(new []{value});

                queryParameters.Add(new Parameter(attribute.Name, parameterValue));
            }

            return queryParameters;
        }
    }
}
