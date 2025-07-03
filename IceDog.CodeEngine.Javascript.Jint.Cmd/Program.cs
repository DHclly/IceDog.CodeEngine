using IceDog.CodeEngine.Javascript.JintEngine;
using System.Text.Json;

namespace IceDog.CodeEngine.Javascript.Jint.Cmd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Main1(args);
        }

        static void Main1(string[] args)
        {
            var engine = CodeEngineJintProvider.CreateInstance();
            var code = """
                import logger from 'logger';
                export async function handler({input,context}){
                    logger.log("info","hello world");
                    logger.log("info",`input:${JSON.stringify(input)}`);
                    logger.log("info",`input:${input.name}`);
                    logger.log("info",`input:${input.age}`);
                    logger.log("info",`input:list:${input.list}`);
                    logger.log("info",`input:dict:${input.dict.key1}`);
                    logger.log("info",`context:${JSON.stringify(context)}`);

                    input.num1=51;
                    input.num2=123;
                    input.num3=51.2551;

                    return input;
                }
                """;
            var codeParams = new Dictionary<string, object>()
            {
                ["name"] = "Tim",
                ["age"] = 25,
                ["list"] = Enumerable.Range(3, 9).ToList(),
                ["dict"] = new Dictionary<string, object>()
                {
                    ["key1"] = Enumerable.Repeat(5, 5),
                    ["key2"] = "vvvvvvvv"
                }
            };
            Console.WriteLine("-------------------");
            var result = engine.ExecuteCode(code, codeParams);
            Console.WriteLine("-------------------");
            if (result is not null)
            {
                Console.WriteLine("执行结果："+JsonSerializer.Serialize(result));
            }
            Console.WriteLine("-------------------");
            result = engine.ExecuteCode(code, codeParams);
            Console.WriteLine("-------------------");
            if (result is not null)
            {
                Console.WriteLine("执行结果：" + JsonSerializer.Serialize(result));
            }
        }
    }
}
