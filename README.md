# IceDog.CodeEngine

代码引擎测试项目

## JavaScript 引擎

### Jint

[sebastienros/jint: Javascript Interpreter for .NET(https://github.com/sebastienros/jint)](https://github.com/sebastienros/jint)

封装了 Jint Module 调用模块形式执行代码，这样就能用 esm 模块形式了

测试代码在`IceDog.CodeEngine.Javascript.Jint.Cmd`：

```csharp
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
```
由于模块的特性是执行一次加载后重复执行就不会重新加载执行，因此封装后改为在执行代码之前初始化Jint引擎,
这样同一份模块代码就能反复执行了。

输出结果：
```bash
-------------------
[info]:hello world
[info]:input:{"name":"Tim","age":25,"list":[3,4,5,6,7,8,9,10,11],"dict":{"key1":[5,5,5,5,5],"key2":"vvvvvvvv"}}
[info]:input:Tim
[info]:input:25
[info]:input:list:3,4,5,6,7,8,9,10,11
[info]:input:dict:5,5,5,5,5
[info]:context:{}
-------------------
执行结果：{"name":"Tim","age":25,"list":[3,4,5,6,7,8,9,10,11],"dict":{"key1":[5,5,5,5,5],"key2":"vvvvvvvv"},"num1":51,"num2":123,"num3":51.2551}
-------------------
[info]:hello world
[info]:input:{"name":"Tim","age":25,"list":[3,4,5,6,7,8,9,10,11],"dict":{"key1":[5,5,5,5,5],"key2":"vvvvvvvv"}}
[info]:input:Tim
[info]:input:25
[info]:input:list:3,4,5,6,7,8,9,10,11
[info]:input:dict:5,5,5,5,5
[info]:context:{}
-------------------
执行结果：{"name":"Tim","age":25,"list":[3,4,5,6,7,8,9,10,11],"dict":{"key1":[5,5,5,5,5],"key2":"vvvvvvvv"},"num1":51,"num2":123,"num3":51.2551}
```
