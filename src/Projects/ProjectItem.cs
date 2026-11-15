#nullable enable
using System;
using System.Collections.Generic;

namespace UnityMegaHub
{
    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime Modified { get; set; }
        public string Platform { get; set; } = "Win64";

        // Путь к текущей выбранной (дефолтной) картинке-обложке
        public string? ImagePath { get; set; }

        // ИСПРАВЛЕНО: Список всех картинок, найденных в папке проекта, для переключения
        public List<string> AvailableImages { get; set; } = new List<string>();

        public string ModifiedDisplay => Modified.ToString("g");
    }
}
