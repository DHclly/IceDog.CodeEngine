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
                    logger.log("info",`input:${input.list}`);
                    logger.log("info",`input:${input.dict.aa}`);
                    logger.log("info",`context:${JSON.stringify(context)}`);
                    input.ff=51;
                    input.f3=123;
                    input.gg=51.2551;
                    return input;
                }
                """;
            var codeParams = new Dictionary<string, object>()
            {
                ["name"] = "Tim",
                ["age"] = 25,
                ["list"] = Enumerable.Range(3, 9).ToArray(),
                ["dict"] = new Dictionary<string, object>()
                {
                    ["aa"] = Enumerable.Repeat(5, 5).ToArray(),
                    ["bb"] = "vvvvvvvv"
                }
            };
            var result = engine.ExecuteCode(code, codeParams);
            if (result is not null)
            {
                Console.WriteLine(JsonSerializer.Serialize(result));
            }
        }
    }
}
