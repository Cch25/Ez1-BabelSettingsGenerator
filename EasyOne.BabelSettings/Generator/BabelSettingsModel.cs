using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Generator
{
    public class BabelSettingsModel
    {
        [JsonIgnore][BsonElement("_id")] public ObjectId Id{ get; set; }
        [BsonElement("settingCode")] public string SettingCode { get; set; }
        [BsonElement("defaultLayout")] public LanguageLayout DefaultLayout { get; set; }
        [BsonElement("defaultSettings")] public List<DefaultSettings> DefaultSettings { get; set; }

    }

    public class DefaultSettings
    {
        [BsonElement("dataField")] public string DataField { get; set; }
        [BsonElement("alignment")] public string Alignment { get; set; }
        [BsonElement("format")] public string Format { get; set; }
        [BsonElement("decimals")] public int Decimals { get; set; }
        [BsonElement("dataSource")] public string DataSource { get; set; }
        [BsonElement("mandatory")] public bool Mandatory { get; set; }
        [BsonElement("readOnly")] public bool ReadOnly { get; set; }
    }
    public class LanguageLayout
    {
        [BsonElement("english")] public List<DefaultLayout> English { get; set; }
        [BsonElement("italian")] public List<DefaultLayout> Italian { get; set; }
        [BsonElement("deutsch")] public List<DefaultLayout> Deutsch { get; set; }
        [BsonElement("francaise")] public List<DefaultLayout> Francaise { get; set; }
        [BsonElement("portoguese")] public List<DefaultLayout> Portoguese { get; set; }
    }
    public class DefaultLayout
    {
        [BsonElement("dataField")] public string DataField { get; set; }
        [BsonElement("isVisible")] public bool IsVisible { get; set; }
        [BsonElement("caption")] public string Caption { get; set; }
        [BsonElement("sortOrder")] public int? SortOrder { get; set; }
        [BsonElement("width")] public int Width { get; set; }
        [BsonElement("groupOrder")] public int? GroupOrder { get; set; }
    }
}
