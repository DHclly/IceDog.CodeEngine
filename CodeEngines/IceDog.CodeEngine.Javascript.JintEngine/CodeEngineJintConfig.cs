namespace IceDog.CodeEngine.Javascript.JintEngine
{
    public class CodeEngineJintConfig
    {
        /// <summary>
        /// 执行代码超时时间配置，默认 10 秒
        /// </summary>
        public TimeSpan ExecCodeTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
