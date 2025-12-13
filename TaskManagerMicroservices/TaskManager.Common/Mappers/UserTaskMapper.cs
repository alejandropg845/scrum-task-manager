using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Common.Mappers
{
    public static class UserTaskMapper
    {
        public static UserTaskDto ToUserTaskDto(this UserTask userTask, string username)
        {
            bool isRemovable = username == userTask.Username;

            return new UserTaskDto
            {
                CreatedOn = userTask.CreatedOn,
                GroupName = userTask.GroupName,
                Id = userTask.Id,
                TaskItems = userTask.TaskItems
                .Select(ti => 
                ti.ToTaskItemDto
                (
                    username == userTask.Username, 
                    username == ti.AssignToUsername)
                ).ToList(),
                Title = userTask.Title,
                SprintId = userTask.SprintId,
                Username = userTask.Username,
                IsRemovable = isRemovable,
                Status = userTask.Status,
                SprintStatus = userTask.SprintStatus,
                Priority = userTask.Priority
                
            };
        }
    }
}
