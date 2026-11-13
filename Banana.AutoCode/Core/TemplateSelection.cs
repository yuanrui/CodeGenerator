using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Banana.AutoCode.Core
{
    /// <summary>
    /// 模板勾选状态的读写。
    /// 状态保存在独立的配置节点 templateSelection 中（Config\template-selection.config），
    /// 只记录未勾选的模板相对路径；配置缺失或损坏时按默认全选处理。
    /// </summary>
    public static class TemplateSelection
    {
        public const String SECTION_NAME = "templateSelection";

        public const String CONFIG_FILE = @"Config\template-selection.config";

        /// <summary>旧版本使用 appSettings 的键前缀，仅用于迁移和清理。</summary>
        public const String LEGACY_KEY_PREFIX = "template.enabled:";

        public const String TEMPLATE_EXTENSION = ".tt";

        private const String EMPTY_SECTION_XML =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<templateSelection>\r\n  <disabled />\r\n</templateSelection>\r\n";

        private static readonly Object SyncRoot = new Object();
        private static HashSet<String> _disabled;
        private static Boolean _loaded;

        /// <summary>
        /// 只有 .tt 才是可勾选、参与生成过滤的模板；.ttinclude 等仅作为被引用文件。
        /// </summary>
        public static Boolean IsTemplateFile(String path)
        {
            return String.Equals(Path.GetExtension(path), TEMPLATE_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 计算相对模板根目录的路径，作为持久化记录的一部分。
        /// </summary>
        public static String GetRelativePath(String rootPath, String path)
        {
            if (String.IsNullOrEmpty(rootPath) || String.IsNullOrEmpty(path))
            {
                return path;
            }

            var root = rootPath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(root.Length);
            }

            return path;
        }

        private static String Normalize(String relativePath)
        {
            return (relativePath ?? String.Empty).Replace('\\', '/').TrimStart('/');
        }

        /// <summary>
        /// 读取勾选状态（进程内只加载一次）。配置节点缺失/损坏时回退为默认全选。
        /// </summary>
        public static void Initialize()
        {
            lock (SyncRoot)
            {
                if (_loaded)
                {
                    return;
                }

                var disabled = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    EnsureSectionFile();

                    var section = ConfigurationManager.GetSection(SECTION_NAME) as TemplateSelectionSection;
                    if (section != null && section.Disabled != null)
                    {
                        foreach (DisabledTemplateElement element in section.Disabled)
                        {
                            if (!String.IsNullOrEmpty(element.Path))
                            {
                                disabled.Add(Normalize(element.Path));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 节点缺失或损坏：不抛出，按默认全选。
                    Trace.WriteLine("读取模板勾选配置失败，按默认全选处理：" + ex.Message);
                }

                _disabled = disabled;
                _loaded = true;

                MigrateLegacySettings(disabled);
            }
        }

        /// <summary>
        /// 没有记录时返回 true（默认勾选）；只有显式记录在 disabled 节点中才算未勾选。
        /// </summary>
        public static Boolean IsEnabled(String relativePath)
        {
            Initialize();
            return !_disabled.Contains(Normalize(relativePath));
        }

        /// <summary>
        /// 写入当前全部模板的勾选状态：未勾选项写入 disabled 节点，
        /// 因此已删除模板的旧记录会随本次重写被清除。
        /// </summary>
        public static void Save(IEnumerable<KeyValuePair<String, Boolean>> states)
        {
            var disabled = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
            foreach (var state in states)
            {
                if (!state.Value)
                {
                    disabled.Add(Normalize(state.Key));
                }
            }

            lock (SyncRoot)
            {
                // 状态没有变化时不重写配置，避免无意义的磁盘写入。
                if (_loaded && _disabled != null && disabled.SetEquals(_disabled) && File.Exists(GetSectionFilePath()))
                {
                    return;
                }

                try
                {
                    EnsureSectionFile();
                    WriteSection(disabled);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("保存模板勾选配置失败：" + ex.Message);

                    // 配置节点文件损坏时重建后再试一次。
                    try
                    {
                        WriteEmptySectionFile();
                        ConfigurationManager.RefreshSection(SECTION_NAME);
                        WriteSection(disabled);
                    }
                    catch (Exception retry)
                    {
                        Trace.WriteLine("重建模板勾选配置后仍保存失败：" + retry.Message);
                        return;
                    }
                }

                _disabled = disabled;
                _loaded = true;
            }
        }

        private static void WriteSection(HashSet<String> disabled)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = config.GetSection(SECTION_NAME) as TemplateSelectionSection;
            if (section == null)
            {
                throw new ConfigurationErrorsException("缺少配置节点 " + SECTION_NAME);
            }

            section.Disabled.ResetAll();
            foreach (var path in disabled.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                section.Disabled.Add(path);
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(SECTION_NAME);
        }

        /// <summary>
        /// 旧版本把状态写在 appSettings 的 template.enabled:* 键中：
        /// 新节点为空且存在旧键时先迁移（只迁移 false），随后删除全部旧键，避免两套存储并存。
        /// </summary>
        private static void MigrateLegacySettings(HashSet<String> disabled)
        {
            try
            {
                var legacyKeys = ConfigurationManager.AppSettings.AllKeys
                    .Where(k => k.StartsWith(LEGACY_KEY_PREFIX, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (legacyKeys.Count == 0)
                {
                    return;
                }

                var migrated = false;
                if (disabled.Count == 0)
                {
                    foreach (var key in legacyKeys)
                    {
                        if (String.Equals(ConfigurationManager.AppSettings[key], "false", StringComparison.OrdinalIgnoreCase))
                        {
                            disabled.Add(Normalize(key.Substring(LEGACY_KEY_PREFIX.Length)));
                        }
                    }
                    migrated = true;
                }

                if (migrated)
                {
                    WriteSection(disabled);
                }

                RemoveLegacyKeys();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("迁移模板勾选配置失败：" + ex.Message);
            }
        }

        private static void RemoveLegacyKeys()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;
            var keys = settings.AllKeys
                .Where(k => k.StartsWith(LEGACY_KEY_PREFIX, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keys.Count == 0)
            {
                return;
            }

            foreach (var key in keys)
            {
                settings.Remove(key);
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private static String GetSectionFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILE);
        }

        private static void EnsureSectionFile()
        {
            if (File.Exists(GetSectionFilePath()))
            {
                return;
            }

            WriteEmptySectionFile();
            ConfigurationManager.RefreshSection(SECTION_NAME);
        }

        private static void WriteEmptySectionFile()
        {
            var path = GetSectionFilePath();
            var directory = Path.GetDirectoryName(path);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, EMPTY_SECTION_XML, new UTF8Encoding(false));
        }
    }

    /// <summary>
    /// 模板勾选的独立配置节点，只保存未勾选的模板相对路径。
    /// </summary>
    public sealed class TemplateSelectionSection : ConfigurationSection
    {
        [ConfigurationProperty("disabled", IsRequired = false)]
        public DisabledTemplateCollection Disabled
        {
            get { return (DisabledTemplateCollection)this["disabled"]; }
        }
    }

    public sealed class DisabledTemplateElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true, IsKey = true)]
        public String Path
        {
            get { return (String)this["path"]; }
            set { this["path"] = value; }
        }
    }

    public sealed class DisabledTemplateCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DisabledTemplateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DisabledTemplateElement)element).Path;
        }

        public void Add(String path)
        {
            if (String.IsNullOrEmpty(path) || BaseGet(path) != null)
            {
                return;
            }

            BaseAdd(new DisabledTemplateElement { Path = path });
        }

        public void ResetAll()
        {
            BaseClear();
        }
    }
}
