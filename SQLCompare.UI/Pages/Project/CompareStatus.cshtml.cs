﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the CompareStatus page
    /// </summary>
    public class CompareStatus : PageModel
    {
        private readonly ITaskService taskService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareStatus"/> class.
        /// </summary>
        /// <param name="taskService">The injected task service</param>
        public CompareStatus(ITaskService taskService)
        {
            this.taskService = taskService;
        }

        /// <summary>
        /// Gets or sets the current TaskInfo
        /// </summary>
        public TaskInfo Task { get; set; }

        /// <summary>
        /// Get the CompareStatus page for the current Task
        /// </summary>
        public void OnGet()
        {
            this.Task = this.taskService.CurrentTaskInfo;
        }
    }
}