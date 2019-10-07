using System.Collections.Generic;

namespace Aliyun.ACM.Sdk.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ACMConfig
    {
        /// <summary>
        /// 总配置数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 分页页号
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// 可用分页数
        /// </summary>
        public int PagesAvailable { get; set; }
        /// <summary>
        /// 配置信息
        /// </summary>
        public List<ACMConfigItem> PageItems { get; set; }
    }
}
