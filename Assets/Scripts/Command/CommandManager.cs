using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;


namespace Commands
{
    public partial class CommandManager : MonoBehaviour
    {
        public static CommandManager Instance { get; private set; }


        [SerializeField] private TMP_InputField textField;
        private Dictionary<string, MethodInfo> _commands = new();
        private Dictionary<Type, object> _instances = new();
        private string _input;

        public void RegisterInstance(object instance)
        {
            _instances.Add(instance.GetType(), instance);
        }
        private void Awake()
        {
            Instance = this;
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies) 
            {
                foreach(MethodInfo methodInfo in assembly.GetTypes().SelectMany(classType => classType.GetMethods()))
                {
                    var attributes = methodInfo.GetCustomAttributes<CommandAttribute>().ToList();
                    if(attributes.Count==0) continue;

                    foreach(var attribute in attributes)
                    {
                        Debug.Log($"{attribute.CommandName} | {methodInfo.Name}");
                        _commands.Add(attribute.CommandName, methodInfo);
                    }
                }
            }
            textField.onSubmit.AddListener(OnSubmit);
        }

        private void OnSubmit(string text)
        {
            _input = text;
            ProcessCommand();
            _input = "";
            textField.text = "";
        }

        private void ProcessCommand()
        {
            Debug.Log($"Processing command");

            string[] tokens = _input.Split(' ');
            string[] parameterTokens = tokens.Skip(1).ToArray();

            if(tokens.Length == 0) return;

            if (!_commands.TryGetValue(tokens[0], out var methodInfo))
            {
                Debug.LogError($"Command \"{tokens[0]}\" doesnt exist");
                return;
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if (parameterInfos.Length != parameterTokens.Length)
            {
                Debug.LogError($"Command \"{tokens[0]}\" requires {parameterInfos.Length} parameters, but {parameterTokens.Length} were provided.");
                return;
            }

            List<object> invocationsParams = new List<object>();
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                invocationsParams.Add(Convert.ChangeType(parameterTokens[i], parameterInfo.ParameterType));
            }

            object instance = this;

            if(methodInfo.DeclaringType != null && _instances.ContainsKey(methodInfo.DeclaringType))
            {
                instance = _instances[methodInfo.DeclaringType];
            }

            methodInfo.Invoke(instance, invocationsParams.ToArray());
        }


        
    }
}

