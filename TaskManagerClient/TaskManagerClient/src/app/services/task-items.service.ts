import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { TasksService } from "./tasks.service";
import { TaskItem } from "../interfaces/user-tasks.interface";
import { UserService } from "./users.service";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class TaskItemService {

    addTaskItem(taskId:string, content:string, assignToUsername:string, taskTitle:string, sprintId:string, groupName:string | null){
    
        const body = {
            taskId, content, 
            assignToUsername,
            groupName: groupName, taskTitle,
            sprintId
        }

        return this.apiGateway.sendRequest<TaskItem>("post", "task-items", `CreateTaskItem`, body);
    }

    markTaskItemAsCompleted(taskItemId:string, taskId:string, sprintId:string, groupName:string | null){
        
        const body = { 
            taskItemId, 
            taskId, 
            groupName: groupName ?? "none", 
            sprintId 
        };

        return this.apiGateway.sendRequest<any>("put", "task-items", "SetTaskItemAsCompleted", body);
    }
    
    deleteTaskItem(taskItemId:string, taskId:string, groupName:string | null){
        return this.apiGateway.sendRequest("delete", "task-items", `DeleteSingleTaskItem/${taskItemId}?taskId=${taskId}&groupName=${groupName}`);
    }

    

    setPriorityToTaskItem(taskId:string, taskItemId:string, priority:number, groupName:string | null){

        const body = {
            taskId, taskItemId,
            priority, groupName: groupName ?? "no group"
        }

        return this.apiGateway.sendRequest<any>("put", "task-items", "SetPriorityToTaskItem", body);

    }
    
    updateTaskItem(newContent:string, assignTo:string | null, taskItemId:string, taskId:string){

        const body = { content:newContent , assignTo, taskItemId, taskId};

        return this.apiGateway.sendRequest<any>("put", "task-items", "UpdateTaskItem", body);
    }

    constructor(private apiGateway:ApiGatewayService){}

}