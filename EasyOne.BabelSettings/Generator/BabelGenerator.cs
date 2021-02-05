using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using EasyOne.BabelSettings;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Emit;
using EasyOne.BabelSettings.Models;
using Newtonsoft.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp;
namespace Generator
{
    public class BabelGenerator
    {
        private readonly BabelFileManager babelFileManager;
        private readonly BabelDatabaseManger babelDatabaseManger;
        private readonly ApplicationSettings _applicationSettings;

        public BabelGenerator(ApplicationSettings applicationSettings)
        {
            babelFileManager = new BabelFileManager();
            babelDatabaseManger = new BabelDatabaseManger();
            _applicationSettings = applicationSettings;
        }
        public void Compile(Arguments arguments)
        {
            string sb = ExtractSyntaxFromFile();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sb);

            // define other necessary objects for compilation
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            // analyse and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Console.WriteLine("Compiling your input.cs file. Please wait.");
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    Type type = assembly.GetType("EasyOne.Compile.BabelSettings");
                    object obj = Activator.CreateInstance(type);
                    Console.WriteLine("Your code has been successfully compiled.");
                    GenerateProperties(obj, arguments);
                }
            }
        }
        private string ExtractSyntaxFromFile()
        {
            string syntax = babelFileManager.ReadFile();

            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
                namespace EasyOne.Compile{");
            sb.Append(syntax);
            sb.AppendLine();
            sb.AppendLine("}");
            return sb.ToString();
        }
        private string GenerateProperties(object source, Arguments arguments)
        {
            if (source == null)
            {
                throw new Exception("No properties found.");
            }
            PropertyInfo[] properties = source.GetType().GetProperties();
            return GeneratePropertiesBasedJson(properties.GetNestedProperties(new Dictionary<string, Type>(), string.Empty), arguments);
        }
        private string GeneratePropertiesBasedJson(Dictionary<string, Type> keyValuePairs, Arguments arguments)
        {
            List<DefaultLayout> defaultLayout = new List<DefaultLayout>();
            List<DefaultSettings> defaultSettings = new List<DefaultSettings>();

            int count = 1;

            Console.WriteLine("Generating json file. Please wait.");

            foreach (KeyValuePair<string, Type> kvp in keyValuePairs)
            {
                string format = GetFormat(kvp.Value);
                int decimals = kvp.Value == typeof(double) || kvp.Value == typeof(double?) ||
                                            kvp.Value == typeof(float) || kvp.Value == typeof(float?) ? 2 : 0;
                defaultSettings.Add(new DefaultSettings()
                {
                    DataField = kvp.Key,
                    Alignment = format.Equals("number") ? "right" : "left",
                    Format = format,
                    DataSource = string.Empty,
                    Decimals = decimals,
                    Mandatory = false,
                    ReadOnly = false
                });

                defaultLayout.Add(new DefaultLayout()
                {
                    DataField = kvp.Key,
                    IsVisible = false,
                    Caption = kvp.Key,
                    SortOrder = count * 10,
                    GroupOrder = -1,
                    Width = 0,
                });
                count++;
            }

            LanguageLayout languageLayout = new LanguageLayout();

            switch (arguments.Language)
            {
                case LanguageEnum.en: languageLayout.English = defaultLayout; break;
                case LanguageEnum.it: languageLayout.Italian = defaultLayout; break;
                case LanguageEnum.dt: languageLayout.Deutsch = defaultLayout; break;
                case LanguageEnum.fr: languageLayout.Francaise = defaultLayout; break;
                case LanguageEnum.pt: languageLayout.Portoguese = defaultLayout; break;
            }

            BabelSettingsModel settingsModel = new BabelSettingsModel
            {
                SettingCode = arguments.SettingsCode,
                DefaultLayout = languageLayout,
                DefaultSettings = defaultSettings,
            };

            string json = JsonConvert.SerializeObject(settingsModel, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            babelFileManager.WriteFile(json, arguments);
            babelFileManager.OpenFile(arguments);

            if (_applicationSettings.ConnectionString != null &&
                !string.IsNullOrEmpty(_applicationSettings.ConnectionString.Database) &&
                !string.IsNullOrEmpty(_applicationSettings.ConnectionString.MongoConnection))
            {
                Console.Write("Connection string detected, would you like to update database?(y/n): ");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.KeyChar.ToString().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    babelDatabaseManger.AddDefaultSettingsInMongodb(json, _applicationSettings, arguments);
                }
                else
                {
                    Console.WriteLine("\nIgnore update database. Program complete.");
                }
            }

            return json;
        }
        private string GetFormat(Type value)
        {
            if (value == typeof(string) || value == typeof(Guid) || value == typeof(Guid?))
                return "string";
            if (value == typeof(int) || value == typeof(short) || value == typeof(float) || value == typeof(double) || value == typeof(long) ||
                value == typeof(int?) || value == typeof(short?) || value == typeof(float?) || value == typeof(double?) || value == typeof(long?))
                return "number";
            if (value == typeof(DateTime) || value == typeof(DateTimeOffset) ||
                value == typeof(DateTime?) || value == typeof(DateTimeOffset?))
                return "date";
            if (value == typeof(bool) || value == typeof(bool?)) return "boolean";
            return "string";
        }
    }

}