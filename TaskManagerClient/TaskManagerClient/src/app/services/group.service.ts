import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { Subject } from "rxjs";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class GroupService {

    isScrum:boolean = false;

    onCreateGroup(groupName:string, isScrum:boolean){
        return this.apiGateway.sendRequest<{groupName:string}>("post", "groups", `CreateGroup/${groupName}?isScrum=${isScrum}`);
    }

    onJoinGroup(groupName:string){
        return this.apiGateway.sendRequest<{groupName:string}>("post", "groups", `JoinGroup/${groupName}`);
    }

    onDeleteGroup(groupName:string){
        return this.apiGateway.sendRequest<any>("delete", "groups",`DeleteGroup/${groupName}`);
    }
    
    onSetAddingTasksAllowed(groupName:string, isAllowed:boolean){
        return this.apiGateway.sendRequest<any>("post", "groups", `SetAllowMembersToAddTask/${groupName}?isAllowed=${isAllowed}`);
    }

    onLeaveGroup(groupName: string){
        return this.apiGateway.sendRequest<any>("put", "users", `LeaveGroup/${groupName}`);
    }

    constructor(private apiGateway:ApiGatewayService){}
}