#nullable enable
namespace UnityMegaHub
{
    public class TemplateItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TargetUnityVersion { get; set; } = "2022.3.29f1";
        public string ZipFileName { get; set; } = string.Empty;

        // ИСПРАВЛЕНО: Полный путь к .zip файлу на диске для правильной распаковки
        public string ZipFilePath { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public override string ToString() => Name;
    }
}
