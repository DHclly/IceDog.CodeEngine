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
                    logger.log("info",`context:${JSON.stringify(context)}`);
                    return input;
                }
                """;
            var codeParams = new Dictionary<string, object>()
            {
                ["name"] = "Tim",
                ["age"] = 25,
            };

            var result = engine.ExecuteCode(code, codeParams);
            Console.WriteLine(JsonSerializer.Serialize(result));
        }
    }
}
