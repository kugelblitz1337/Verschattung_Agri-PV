
using Autofac.Core;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ALKISService
{
    /// <summary>
    /// Kontext für die Containerverwaltung
    /// </summary>
    public class UtilitiesContext
    {
        private UtilitiesContext() { }

        /// <summary>
        /// Eine Instanz des Containers erzeugen, sobald der erste Zugriff passiert wird dieser registriert
        /// </summary>
        public static readonly Lazy<IContainer> _containerBuilder = new Lazy<IContainer>(() =>
        {
            var builder = new ContainerBuilder();
            RegisterComponents(builder);
            return builder.Build();
        });

        public static IContainer Container => _containerBuilder.Value;

        /// <summary>
        /// Registrierung aller vorhandenen Komponenten im Verzeichnis
        /// </summary>
        /// <param name="builder">IoC Containerbuilder für die Regsitrierung der Komponenten</param>
        private static void RegisterComponents(ContainerBuilder builder)
        {
            // Sucht nach allen Assemblys mit dem Kürzel API vorne
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dllFiles = Directory.GetFiles(directory, "ALKISService.*.dll");
            foreach (var dllFile in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dllFile);
                //Sucht nach Module die Interface von Autofac erben
                var moduleTypes = assembly.GetTypes().Where(t => typeof(Autofac.Module).IsAssignableFrom(t));
                foreach (var moduleType in moduleTypes)
                {
                    //Erstellen der Instanz
                    var instance = Activator.CreateInstance(moduleType);
                    if (instance != null)
                    {
                        var convertedModule = (Autofac.Module)instance;
                        //Registrieren der Module
                        builder.RegisterModule(convertedModule);
                    }
                }
            }
            ;
        }
    }
}