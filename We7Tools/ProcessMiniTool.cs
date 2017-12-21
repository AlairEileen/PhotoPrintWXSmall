using ConfigData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace We7Tools
{
    public class ProcessMiniTool
    {
        public static void CreateProcessMiniZip(SiteInfo siteInfo, out string zipFilePath)
        {
            var siteInfoJs = @"var siteinfo={
  'title': '" + siteInfo.title + @"',
  'uniacid': '" + siteInfo.uniacid + @"',
  'acid': '" + siteInfo.acid + @"',
  'multiid': '" + siteInfo.multiid + @"',
  'version': '" + siteInfo.version + @"',
  'siteroot': '" + siteInfo.siteroot + @"',
  'design_method': '" + siteInfo.design_method + @"',
  'redirect_module': '" + siteInfo.redirect_module + @"',
  'template': '" + siteInfo.template + @"'
};
//模块暴露
module.exports = siteinfo;";
            using (var sw = new StreamWriter(We7Config.ProcessMiniFolderPath + "/siteinfo.js", false, Encoding.UTF8))
            {
                sw.Write(siteInfoJs);
            }
            zipFilePath = (We7Config.ProcessMiniFolderPath.LastIndexOf("/") ==
                (We7Config.ProcessMiniFolderPath.Length - 1) ?
                We7Config.ProcessMiniFolderPath.Substring(0, We7Config.ProcessMiniFolderPath.LastIndexOf("/")) :
                We7Config.ProcessMiniFolderPath) + ".zip";
            System.IO.Compression.ZipFile.CreateFromDirectory(We7Config.ProcessMiniFolderPath, zipFilePath);
        }
    }

    public class SiteInfo
    {
        public string title { get; set; }
        public string uniacid { get; set; }
        public string acid { get; set; }
        public string multiid { get; set; }
        public string version { get; set; }
        public string siteroot { get; set; }
        public string design_method { get; set; }
        public string redirect_module { get; set; }
        public string template { get; set; }
    }
}
