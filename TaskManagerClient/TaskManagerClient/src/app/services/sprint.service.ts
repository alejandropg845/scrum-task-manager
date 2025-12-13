import { Injectable } from "@angular/core";
import { Sprint } from "../interfaces/sprint.interface";
import { BehaviorSubject } from "rxjs";
import { UserTask } from "../interfaces/user-tasks.interface";
import { ApiGatewayService } from "./api-gateway.service";
import { BeginSprintResponse } from "../interfaces/responses/begin-sprint.interface";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class SprintService {
    
    sprintNumber:number = 0
    showSprints:boolean = false;
    sprintName:string = "";
    remainingTime!:string;
    getGroupSprints_BSubject = new BehaviorSubject<boolean>(false);
    sprintsTasks_BSubject = new BehaviorSubject<UserTask[]>([]);

    beginSprint(groupName:string, weeksNumber:number, tasksIds:string[], sprintName:string){
        
       const body = { 
            groupName, 
            weeksNumber, 
            tasksIds,
            sprintName
        };
       

        return this.apiGateway.sendRequest<BeginSprintResponse>("put", "sprints", "sprints/BeginSprint", body);
    }

    getGroupSprints(groupName:string){
        return this.apiGateway.sendRequest<Sprint[]>("get", "sprints", `sprints/GetGroupSprints/${groupName}`)
    }
        
    downloadSprintsSummary(groupName:string){
        return this.apiGateway.sendRequest<Blob>("get", "sprints", `sprints/GetSummary/${groupName}`, null, true);
    }


    constructor(private apiGateway:ApiGatewayService){}

}