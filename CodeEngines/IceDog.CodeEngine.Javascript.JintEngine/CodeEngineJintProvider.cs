using Jint;
using System.Text;

namespace IceDog.CodeEngine.Javascript.JintEngine;

/// <summary>
/// 
/// </summary>
public class CodeEngineJintProvider
{
    /// <summary>
    /// 
    /// </summary>
    private HttpClient _httpClient;

    /// <summary>
    /// 
    /// </summary>
    private Engine _codeEngine;

    /// <summary>
    /// 
    /// </summary>
    private CodeEngineJintConfig _codeEngineConfig;

    private object _executeCodeLock = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="codeEngineConfig"></param>
    public CodeEngineJintProvider(CodeEngineJintConfig? codeEngineConfig = null)
    {
        _httpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
        });

        if (codeEngineConfig is null)
        {
            _codeEngineConfig = new CodeEngineJintConfig();
        }
        else
        {
            _codeEngineConfig = codeEngineConfig;
        }
    }

    /// <summary>
    /// 采用模块导入形式，由于模块只能导入一次，所以实例只能用一次
    /// </summary>
    /// <param name="codeEngineConfig"></param>
    /// <returns></returns>
    public static CodeEngineJintProvider CreateInstance(CodeEngineJintConfig? codeEngineConfig = null) => new CodeEngineJintProvider(codeEngineConfig);

    /// <summary>
    /// 
    /// </summary>
    private void InitCodeEngine()
    {
        _codeEngine = new Engine(options =>
        {
            options.LimitMemory(1_000_000_000);
            options.LimitRecursion(2_000);
            options.TimeoutInterval(_codeEngineConfig.ExecCodeTimeout);
            options.RegexTimeoutInterval(TimeSpan.FromSeconds(3));
            options.MaxStatements(100_000_000);
            options.MaxJsonParseDepth(32);
            options.MaxArraySize(10_000_000);
        });
        this.AddDotNetHookMethod();
        this.AddJavascriptModule();
    }

    /// <summary>
    /// 
    /// </summary>
    private void DestoryCodeEngine()
    {
        if (_codeEngine is not null)
        {
            _codeEngine.Dispose();
            _codeEngine = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void AddDotNetHookMethod()
    {
        _codeEngine.SetValue("__dotnet_log", new Action<string, string>((logType, message) =>
        {
            Console.WriteLine($"[{logType}]:{message}");
        }));

        _codeEngine.SetValue("__dotnet_http_get", new Func<string, Task<string>>(async (url) =>
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string data = await response.Content.ReadAsStringAsync();
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }));

        _codeEngine.SetValue("__dotnet_http_post", new Func<string, string, string, object, Task<string>>(async (url, data, contentType, headers) =>
        {
            try
            {
                HttpContent content;
                switch (contentType)
                {
                    case "application/json":
                        content = new StringContent(data, Encoding.UTF8, "application/json");
                        break;
                    case "application/x-www-form-urlencoded":
                        var values = new Dictionary<string, string>();
                        foreach (var pair in data.Split('&'))
                        {
                            string[] keyValue = pair.Split('=');
                            values.Add(keyValue[0], keyValue[1]);
                        }
                        content = new FormUrlEncodedContent(values);
                        break;
                    case "multipart/form-data":
                        MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                        foreach (var pair in data.Split('&'))
                        {
                            string[] keyValue = pair.Split('=');
                            // 假设所有键值对都是简单的文本字段
                            multipartFormDataContent.Add(new StringContent(keyValue[1]), keyValue[0]);
                        }
                        content = multipartFormDataContent;
                        break;

                    default:
                        content = new StringContent(data, Encoding.UTF8, "application/json");
                        break;
                }

                //添加请求头
                if (headers != null)
                {
                    IDictionary<string, object> dict = headers as IDictionary<string, object>;
                    foreach (var entry in dict)
                    {
                        _httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value.ToString());
                    }
                }

                //发起请求
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"POST ${url} {result}");
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }));

    }

    /// <summary>
    /// 
    /// </summary>
    private void AddJavascriptModule()
    {
        _codeEngine.Modules.Add("logger", """
        const log=__dotnet_log;
        const debug=(message)=>__dotnet_log(`debug`,message);
        const info=(message)=>__dotnet_log(`info`,message);
        const warn=(message)=>__dotnet_log(`warn`,message);
        const error=(message)=>__dotnet_log(`error`,message);

        const logger={
            log,
            debug,
            info,
            warn,
            error
        };
        export default logger;
        """);

        _codeEngine.Modules.Add("fetch", """
        const get = async (url) => {
            return await __dotnet_http_get(url);
        };

        const post = async (url, data, contentType = 'application/json', headers = null) => {
            return await  __dotnet_http_post(url, data, contentType, headers);
        };
                
        const fetch = {
            get,
            post
        };
                
        export default fetch;
        """);

        _codeEngine.Modules.Add("context", """      
        const context = {};
        export default context;
        """);

        _codeEngine.Modules.Add("main", """
        import context from 'context';
        import {handler} from 'handler';
        
        let input = __dotnet_input_params;
        export const result = handler({input,context});
        """);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codeParams"></param>
    /// <returns></returns>
    public dynamic ExecuteCode(string code, Dictionary<string, object> codeParams)
    {
        lock (_executeCodeLock)
        {
            try
            {
                this.InitCodeEngine();
                _codeEngine.SetValue("__dotnet_input_params", codeParams);
                _codeEngine.Modules.Add("handler", code);
                var fnMain = _codeEngine.Modules.Import("main");
                var result = fnMain.Get("result").UnwrapIfPromise().ToObject();
                return result;
            }
            finally
            {
                this.DestoryCodeEngine();
            }
        }
    }
}