using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Common.Mappers
{
    public static class TaskItemMapper
    {
        public static TaskItemDto ToTaskItemDto(this TaskItem taskItem, bool isRemovable, bool isCompletable)
        {
            return new TaskItemDto
            {
                AssignToUsername = taskItem.AssignToUsername,
                Content = taskItem.Content,
                Id = taskItem.Id,
                IsCompleted = taskItem.IsCompleted,
                TaskId = taskItem.TaskId,
                IsRemovable = isRemovable,
                GroupName = taskItem.GroupName,
                IsCompletable = isCompletable,
                Priority = taskItem.Priority,
                TaskTitle = taskItem.TaskTitle
            };
        }
    }
}
