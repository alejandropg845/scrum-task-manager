import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { SetGroupRoleResponse } from "../interfaces/responses/set-group-role.interface";
import { UserService } from "./users.service";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class GroupRoleService {

    setUserGroupRole(groupName:string, roleName:string, username:string, isScrum:boolean){

        const body = { groupName, roleName }

        return this.apiGateway.sendRequest<SetGroupRoleResponse>("put", "groups-roles", `SetUserGroupRole/${username}?isScrum=${isScrum}`, body)
    }
    
    constructor(private apiGateway:ApiGatewayService){}

}