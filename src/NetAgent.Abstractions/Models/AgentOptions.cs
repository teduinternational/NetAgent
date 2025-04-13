using System;
using System.Collections.Generic;

namespace NetAgent.Abstractions.Models
{
    /// <summary>
    /// Định nghĩa cấu hình cho agent
    /// </summary>
    public class AgentOptions
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string[] Goals { get; set; }
        /// <summary>
        /// System message cho agent
        /// </summary>
        public string? SystemMessage { get; set; }

        /// <summary>
        /// Model language được sử dụng (gpt-4, gpt-3.5-turbo, ...)
        /// </summary>
        public string Model { get; set; } = "gpt-3.5-turbo";

        /// <summary>
        /// Nhiệt độ cho việc generate text (0.0 to 1.0)
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Số token tối đa cho mỗi response
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Danh sách công cụ được phép sử dụng
        /// </summary>
        public List<string> EnabledTools { get; set; } = new();

        /// <summary>
        /// Ngôn ngữ giao tiếp của agent
        /// </summary>
        public AgentLanguage Language { get; set; } = AgentLanguage.English;

        /// <summary>
        /// Danh sách các nhà cung cấp ưu tiên
        /// </summary>
        public string[] PreferredProviders { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Số bước tối đa trong kế hoạch thực thi
        /// </summary>
        public int MaxPlanSteps { get; set; } = 10;

        /// <summary>
        /// Thời gian timeout cho mỗi operation (giây)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Bật/tắt logging chi tiết
        /// </summary>
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// Cấu hình cho các nhà cung cấp LLM
        /// </summary>
        public Dictionary<string, object> ProviderSettings { get; set; } = new();

        /// <summary>
        /// Cấu hình bộ nhớ cho agent
        /// </summary>
        public MemoryOptions Memory { get; set; } = new();

        /// <summary>
        /// Cấu hình công cụ cho agent
        /// </summary>
        public ToolOptions Tools { get; set; } = new();
    }

    /// <summary>
    /// Cấu hình bộ nhớ cho agent
    /// </summary>
    public class MemoryOptions
    {
        /// <summary>
        /// Số token tối đa cho bộ nhớ
        /// </summary>
        public int MaxTokens { get; set; } = 2000;

        /// <summary>
        /// Ngưỡng liên quan để lưu trữ thông tin (0.0 to 1.0)
        /// </summary>
        public float RelevanceThreshold { get; set; } = 0.7f;

        /// <summary>
        /// Kích thước cửa sổ ngữ cảnh
        /// </summary>
        public int ContextWindowSize { get; set; } = 4096;
    }

    /// <summary>
    /// Cấu hình công cụ cho agent
    /// </summary>
    public class ToolOptions
    {
        /// <summary>
        /// Danh sách công cụ được phép sử dụng
        /// </summary>
        public List<string> EnabledTools { get; set; } = new();

        /// <summary>
        /// Cấu hình cho từng công cụ
        /// </summary>
        public Dictionary<string, object> ToolSettings { get; set; } = new();
    }
}
