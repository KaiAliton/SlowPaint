using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Objects;

namespace WindowsFormsApp1
{
    //класс, сохраняющий все данные
    static class Serializer
    {
        //сериализовать фигуры в json
        public static void Serialize(string path, List<ObjectShape> shapes)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.TypeNameHandling = TypeNameHandling.All;

            foreach (var s in shapes)
            {
                s.PenWidth = s.pen.Width;
                s.PenBrush = s.pen.Color;
                s.PenDashStyle = s.pen.DashStyle;
            }
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, shapes);
            }
        }
        //десериализовать из json в переменные
        public static List<ObjectShape> Deserialize(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string jsonStr = sr.ReadToEnd();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                List<ObjectShape> shapes = JsonConvert.DeserializeObject<List<ObjectShape>>(jsonStr, settings);
                foreach (ObjectShape sh in shapes)
                    sh.pen = new System.Drawing.Pen(sh.PenBrush, sh.PenWidth) { DashStyle = sh.PenDashStyle };
                return shapes;
            }
        }
    }
}
