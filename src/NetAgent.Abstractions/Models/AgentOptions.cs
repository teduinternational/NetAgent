using System;
using System.Collections.Generic;

namespace NetAgent.Abstractions.Models
{
    /// <summary>
    /// Định nghĩa cấu hình cho agent
    /// </summary>
    public class AgentOptions
    {
        /// <summary>
        /// Tên của agent
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Vai trò của agent
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Các mục tiêu của agent
        /// </summary>
        public string[] Goals { get; set; } = Array.Empty<string>();

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
        /// Cấu hình bộ nhớ cho agent
        /// </summary>
        public MemoryOptions Memory { get; set; } = new MemoryOptions();

        /// <summary>
        /// Danh sách công cụ được phép sử dụng
        /// </summary>
        public List<string> EnabledTools { get; set; } = new();

        /// <summary>
        /// Ngôn ngữ giao tiếp của agent
        /// </summary>
        public AgentLanguage Language { get; set; } = AgentLanguage.English;
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
    }
}
