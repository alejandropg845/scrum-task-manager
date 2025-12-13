import { HttpClient } from "@angular/common/http";
import { Injectable, signal } from "@angular/core";
import { environment } from "../../environments/environment";
import { CurrentUserInfo } from "../interfaces/current-user-info.interface";
import { GroupMember } from "../interfaces/group-member.interface";
import { BehaviorSubject, Subject, takeUntil } from "rxjs";
import { HandleBackendError } from "../interfaces/error-handler";
import { TaskItem } from "../interfaces/user-tasks.interface";
import { PopupService } from "./popup.service";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class UserService {

    usersInGroup_BSubject = new BehaviorSubject<GroupMember[]>([]);
    getGroupInfo_subject = new Subject<boolean>();

    userPendingTasks = new BehaviorSubject<TaskItem[]>([]);
    groupName!:string | null;
    username!:string | null;
    isGroupOwner:boolean = false;
    isScrum:boolean = false;
    isAllowed:boolean = false;
    groupRole:string | null = null;
    expirationTime!:Date | null;
    status:string = "";
    avatarBgColor:string = "";

    getUserInfo(){
        return this.apiGateway.sendRequest("get", "users", "GetUserInfo");
    }

    groupNameIsNull(){
        return !this.groupName;
    }

    getAndSetUsersBSubject(groupName:string) {
        this.apiGateway.sendRequest<GroupMember[]>("get", "users", `GetUsers/${groupName}`)
        .subscribe({
            next: users => this.usersInGroup_BSubject.next(users),
            error: err => HandleBackendError(err, this.popupService)
        });
    }
    
    constructor(private popupService:PopupService, private apiGateway:ApiGatewayService){}

}