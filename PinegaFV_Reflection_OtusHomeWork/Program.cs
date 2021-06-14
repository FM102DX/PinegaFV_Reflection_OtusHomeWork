using System;
using System.Collections.Generic;
using System.Text;
using static PinegaFV_Reflection_OtusHomeWork.ObjectParameters;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace PinegaFV_Reflection_OtusHomeWork
{

    // 11.06.2021
    // в рамках ДЗ ОТУС по reflection
    // CsvSerializer - это сериализатор - десериализатор простых, "плоских" классов, т.е. таких, где все поля  String, DateTime, Boolean, Int32, Double
    // используется ObjectParametersEngine.cs - это набор reflection-based методов, написанных мной ранее как раз для целей, подобных этому заданию

    class Program
    {
        //количество генерируемых объектов в сериализуемой оллекции
        public static int objectsCol = 10;
        public static bool dumpsOn = false;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Starting program");

            Stopwatch timer1 = new Stopwatch();
            Stopwatch timer2 = new Stopwatch();

            //готовим массив объектов
            List<F> objectArray= new List<F>();
            for (int i = 0; i< objectsCol; i++) {objectArray.Add(F.GetRandomInstance());}
            Console.WriteLine($"Working with object array contains {objectArray.Count()} items");
            if (dumpsOn) Fn.MakeObjectListDump(objectArray.Cast<object>().ToList());

            Console.WriteLine("");
            Console.WriteLine("*******************Serialization into CSV*******************");
            Console.WriteLine("");

            //готовим сериализатор
            CsvSerializer csvSerializer = CsvSerializer.GetInstance(';');

            //сериализуем
            //счиатем файл локальным и сущетсвующим, т.е. не морочимся с его созданием, папками, проверкой существования и т.п. - он просто есть в проекте
            timer1.Start();
            string rez = csvSerializer.SerializeToScv(objectArray.Cast<object>().ToList());
            timer1.Stop();
            File.WriteAllText(@"..\..\..\result.csv", rez);
            Console.WriteLine($"Serialization time {timer1.ElapsedMilliseconds} ms");
            timer1.Reset();
            
            //десериализация
            var lines = File.ReadAllLines(@"..\..\..\result.csv");
            timer1.Start();
            List<F> objectArrayDeserializedCsv = csvSerializer.DeSerializeFromCsv(typeof(F), lines).Cast<F>().ToList();
            timer1.Stop();

            Console.WriteLine($"Deserialization time {timer1.ElapsedMilliseconds} ms");
            timer1.Reset();
            if (dumpsOn) Fn.MakeObjectListDump(objectArrayDeserializedCsv.Cast<object>().ToList());

            Console.WriteLine("");
            Console.WriteLine("*******************Serialization into JSON via newtonsot *******************");
            Console.WriteLine("");

            //сериализация
            timer1.Start();
            string serialized = JsonConvert.SerializeObject(objectArray);
            timer1.Stop();
            File.WriteAllText(@"..\..\..\result.json", serialized);
            Console.WriteLine($"Serialization time {timer1.ElapsedMilliseconds} ms");

            //десериализация
            string line = string.Join("",  File.ReadAllLines(@"..\..\..\\result.json"));
            timer1.Reset();
            timer1.Start();
            var objectArrayDeserializedJson = JsonConvert.DeserializeObject<List<F>>(line);
            timer1.Stop();

            Console.WriteLine($"Deserialization time {timer1.ElapsedMilliseconds} ms");
            if (dumpsOn) Fn.MakeObjectListDump(objectArrayDeserializedCsv.Cast<object>().ToList());
        }
    }
    public class F 
    {
        public int i1 { get; set; }
        public int i2 { get; set; }
        public int i3 { get; set; }
        public int i4 { get; set; }
        public int i5 { get; set; }

        public F() { }
        public static F GetRandomInstance()
        {
            return new F
            {
                i1 = new Fn.RandomInt(10, 1000).value,
                i2 = Fn.GetRndInt(10, 1000),
                i3 = Fn.GetRndInt(10, 1000),
                i4 = Fn.GetRndInt(10, 1000),
                i5 = Fn.GetRndInt(10, 1000)
            };
        }
        public override string ToString()
        {
            return $"{i1} {i2} {i3} {i3} {i4} {i5}";
        }
    }

    
    public class CsvSerializer
    {

        private CsvSerializer() { }
        public char Separator { get; set; }
        private string StrSeparator {get{ return Separator.ToString(); } }

        public static CsvSerializer GetInstance(char _Separator)
        {
            return new CsvSerializer
            {
                Separator = _Separator
            };
        }

        public string SerializeToScv (List<object> source)
        {
            //сериализация любого плоского объекта (такого, где все поля - это int, string, и т.д., то есть не вложенные объекты) в Json
            //пройти по все полям и пропертям объекта, и просто записать 
            //первая строка - это заголовки

            if (source == null) return "";
            if (source.Count==0) return "";

            string s = "";
            StringBuilder stringBuilder = new StringBuilder();

            //сначала заголовки
            List<string> paramNames = GetObjectParameters(source[0]).Select(y => y.name).ToList();
            s=string.Join(StrSeparator, paramNames);
            stringBuilder.Append(s);
            stringBuilder.Append(Fn.Chr13);

            //далее сами объекты
            foreach (object seralizabeObject in source)
            {
                stringBuilder.Append(
                    string.Join(StrSeparator,
                                GetObjectParameters(seralizabeObject)
                                    .Select(y => y.value)
                                    .ToList()));
                stringBuilder.Append(Fn.Chr13);
            }

            return stringBuilder.ToString();
        }
        public List<object> DeSerializeFromCsv(Type targetType, string[] source)
        {
            List<object> rez = new List<object>();

            object newObject;

            string[] paramNames = source[0].Split(Separator);

            string[] tmp;

            for (int i = 1; i<source.Length; i++)
            {
                newObject = Activator.CreateInstance(targetType);
                
                tmp = source[i].Split(Separator);

                for (int j=0; j< paramNames.Length; j++)
                {
                    SetObjectParameter(newObject, paramNames[j], tmp[j]);
                }
                rez.Add(newObject);
            }
            return rez;
        }
    }

    public static class Fn
    {
        //вспомогательные функции
        public static double GetRndDouble(double min, double max, int digits = 2)
        {
            double d = min + random.NextDouble() * (max - min);

            d = Math.Round(d, digits);

            return d;
        }
        public class RandomInt
        {
            public int value{get; private set;}
            public RandomInt(int min, int max)
            {
                value = random.Next(min, max);
            }

        }

        public static int GetRndInt(int min, int max)
        {
            return random.Next(min, max);
        }

        

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string Chr10 { get { return Convert.ToChar(10).ToString(); } }
        public static string Chr13 { get { return Convert.ToChar(13).ToString(); } }

        public static void MakeObjectListDump(List<object> source) 
        {
            source.ForEach(x => Console.WriteLine(x.ToString()));
        }
    }



}
