using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using EasyOne.BabelSettings.Models;
using MongoDB.Bson.Serialization.Conventions;
using System.Collections.Generic;
using System.Linq;
using EasyOne.BabelSettings;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace Generator
{
    public class BabelDatabaseManger
    {
        internal void AddDefaultSettingsInMongodb(string json, ApplicationSettings applicationSettings, Arguments arguments)
        {
            Console.WriteLine("\nAdding your json to database, please wait.");

            try
            {
                ConventionRegistry.Register("IgnoreIfDefault",
                            new ConventionPack { new IgnoreIfDefaultConvention(true) },
                            t => true);
                MongoContext context = new MongoContext(applicationSettings);

                InsertOrUpdate(json, context, arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nSomething went wrong {ex.Message}");
                throw;
            }
        }
        private void InsertOrUpdate(string json, MongoContext context, Arguments arguments)
        {
            BabelSettingsModel currentObject = JsonConvert.DeserializeObject<BabelSettingsModel>(json);

            IMongoCollection<BabelSettingsModel> collection = context.Database.GetCollection<BabelSettingsModel>("defaultsettings");
            BabelSettingsModel dbObject = collection.Find(
                Builders<BabelSettingsModel>.Filter.Eq(x => x.SettingCode, currentObject.SettingCode))
                .ToList()
                .FirstOrDefault();
            if (dbObject == null)
            {
                collection.InsertOne(currentObject);
                Console.WriteLine("Successfully inserted your document. Program complete.");
            }
            else
            {
                Console.WriteLine("File detected, trying to update.");
                List<List<DefaultLayout>> languages = new List<List<DefaultLayout>>()
                {
                    currentObject.DefaultLayout.Italian,
                    currentObject.DefaultLayout.English,
                    currentObject.DefaultLayout.Deutsch,
                    currentObject.DefaultLayout.Francaise,
                    currentObject.DefaultLayout.Portoguese,
                };
                foreach (List<DefaultLayout> lang in languages)
                {
                    if (arguments.Language == LanguageEnum.it && (dbObject.DefaultLayout.Italian == null || arguments.Override))
                    {
                        dbObject.DefaultLayout.Italian = lang;
                    }
                    else if (arguments.Language == LanguageEnum.en && (dbObject.DefaultLayout.English == null || arguments.Override))
                    {
                        dbObject.DefaultLayout.English = lang;
                    }
                    else if (arguments.Language == LanguageEnum.dt && (dbObject.DefaultLayout.Deutsch == null || arguments.Override))
                    {
                        dbObject.DefaultLayout.Deutsch = lang;
                    }
                    else if (arguments.Language == LanguageEnum.fr && (dbObject.DefaultLayout.Francaise == null || arguments.Override))
                    {
                        dbObject.DefaultLayout.Francaise = lang;
                    }
                    else if (arguments.Language == LanguageEnum.pt && (dbObject.DefaultLayout.Portoguese == null || arguments.Override))
                    {
                        dbObject.DefaultLayout.Portoguese = lang;
                    }
                }
                collection.ReplaceOne(Builders<BabelSettingsModel>.Filter.Eq(x => x.SettingCode, currentObject.SettingCode), dbObject);
                Console.WriteLine($"Your {currentObject.SettingCode}.json was updated for language -> {arguments.Language}");
            }
        }
    }

    public class MongoContext
    {
        public readonly IMongoDatabase Database = null;

        public MongoContext(ApplicationSettings options)
        {
            MongoClient client = new MongoClient(options.ConnectionString.MongoConnection);
            if (client != null)
            {
                Database = client.GetDatabase(options.ConnectionString.Database);
            }
        }
    }


}
