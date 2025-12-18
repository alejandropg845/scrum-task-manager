import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { PopupService } from './popup.service';
import { TaskItem, UserTask } from '../interfaces/user-tasks.interface';
import { TasksService } from './tasks.service';
import { Message, MessagesDate } from '../interfaces/group-message.interface';
import { Retrospective } from '../interfaces/retrospective.interface';

@Injectable({providedIn: 'root'})

export class UsersHubService {

    private hubConnection!:HubConnection;

    async onConnectedUser(){
        this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.usersUrl}/commonHub`,{
            accessTokenFactory: () => localStorage.getItem('tmat') || ""
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.None)
        .build();

        this.hubConnection.onreconnected(_ => this.pagesService.needsToGetTasks_Subject.next(true));

        try {
            await this.hubConnection.start();
        } catch (err) {
            throw err;
        }
    }

    onLeaveGroup(groupName:string){
        return this.hubConnection.invoke("OnLeaveGroup", groupName);
    }

    async stopConnection(){
        //El hubConnection puede ser null porque el usuario
        //hizo logOut pero no se encontraba en ningun grupo
        return this.hubConnection?.stop();
    }

    onInvokeJoinedGroup(groupName:string, roleName:string | null){
        this.hubConnection.invoke("OnJoinedGroup", groupName, roleName);
    }

    onReceiveUserLeftGroup(callback:(username:string) => void){
        this.hubConnection.on("onReceiveUserLeftGroup", callback);
    }

    onReceiveUserJoinedGroup(callback:(username:string, roleName:string) => void){
        this.hubConnection.on("onReceiveUserJoinedGroup", callback);
    }

    onInvokeDeleteGroup(groupName:string){
        this.hubConnection.invoke("OnRemoveGroup", groupName);
    }

    onJoinedGroupReceiver(callback:(message:string) => void){
        this.hubConnection.on("OnJoinedGroup", callback);
    }

    onReceiveGroupTask(callback: (userTask:UserTask) => void){
        this.hubConnection.on("onReceiveGroupTask", callback);
    }

    onInvokeSendTaskToEveryone(userTask:UserTask){
        this.hubConnection.invoke("SendTaskToEveryone", userTask);
    }

    onInvokeDeletedGroupTask(taskId:string, groupName:string){
        this.hubConnection.invoke("OnDeletedGroupTask", taskId, groupName);
    }

    onReceiveDeletedGroupTask(callback:(taskId:string) => void){
        this.hubConnection.on("onReceiveDeletedGroupTask", callback);
    }

    onInvokeRemoveGroup(groupName:string){
        this.hubConnection.invoke("OnRemoveGroup", groupName);
    }

    onReceiveRemovedGroup(callback:() => void){
        this.hubConnection.on("onReceiveRemovedGroup", callback);
    }

    onInvokeSendGroupItemTask(taskItem:TaskItem){
        this.hubConnection.invoke("OnSendTaskItemToGroup", taskItem);
    }

    onReceiveGroupTaskItem(callback: (taskItem:TaskItem) => void){
        this.hubConnection.on("onReceiveGroupTaskItem", callback);
    }

    onInvokeSendRemovedTaskItem(taskItem:TaskItem){
        this.hubConnection.invoke("OnSendRemovedTaskItem", taskItem);
    }

    onReceiveRemovedTaskItem(callback:(taskItem:TaskItem) => void){
        this.hubConnection.on("onReceiveRemovedTaskItem", callback);
    }

    onInvokeSendCompletedTaskItem(taskItem:TaskItem, taskOwnerName:string, username:string, taskIsCompleted:boolean){
        this.hubConnection.invoke("OnSendCompletedTaskItem", taskItem, taskOwnerName, username, taskIsCompleted);
    }

    onReceiveCompletedTaskItem(callback:(taskItem:TaskItem, taskOwnerName:string, username:string, taskIsCompleted:boolean) => void) {
        this.hubConnection.on("OnReceiveCompletedTaskItem", callback);
    }

    onInvokeReceiveUserGroupRole(groupName:string, 
        username:string,
        groupRole:string, 
        userThatAssignedProductOwner:string,
        isSwitchingScrumMaster:boolean,
        userThatIsScrumMaster:string,
        userThatWasScrumMaster:string){

        const obj = {
            groupName,
            username,
            groupRole,
            userThatAssignedProductOwner,
            isSwitchingScrumMaster,
            userThatIsScrumMaster,
            userThatWasScrumMaster
        }   
           
        this.hubConnection.invoke("OnUserGroupRoleSet", obj);
    }

    onReceiveUserGroupRole(callback: (username:string, 
        groupRole:string, 
        userThatAssignedProductOwner:string,
        isSwitchingScrumMaster:boolean,
        userThatIsScrumMaster:string,
        userThatWasScrumMaster:string) => void) {
        this.hubConnection.on("onReceiveUserGroupRole", callback);
    }

    onInvokeSetTaskItemPriority(taskItemId:string, taskId:string, priority:number, groupName:string){
        this.hubConnection.invoke("OnSetTaskItemPriority", taskItemId, taskId, priority, groupName);
    }

    onReceiveTaskItemPriority(callback:(taskItemId:string, taskId:string, priority:number) => void){
        this.hubConnection.on("onReceiveTaskItemPriority", callback);
    }

    onReceiveSprintToTasks(callback:(tasksIds:string[], expirationTime:Date, sprintId:string, sprintName:string, remainingTime:string) => void){
        this.hubConnection.on("onReceiveSprintTasks", callback);
    }

    onInvokeSetSprintToTasks(tasksIds:string[], groupName:string, expirationTime:Date, sprintId:string, sprintName:string, remainingTime:string){
        this.hubConnection.invoke("OnSetSprintToTasks", tasksIds, groupName, expirationTime, sprintId, sprintName, remainingTime);
    }

    onReceiveCompletedTask(callback:(taskId:string) => void){
        this.hubConnection.on("onReceiveCompletedTask", callback);
    }

    onInvokeSendCompletedTask(taskId:string, groupName:string){
        this.hubConnection.invoke("OnSendCompletedTask", taskId, groupName);
    }

    onInvokeSendGroupMessage(groupName:string, date:MessagesDate, message:Message){
        this.hubConnection.invoke("OnSendGroupMessage", groupName, date, message);
    }

    onReceiveGroupMessage(callback:(date:MessagesDate, message:Message) => void){
        this.hubConnection.on("onReceiveGroupMessage", callback);
    }

    onInvokeSetTaskPriority(taskId:string, priority:number, groupName:string){
        this.hubConnection.invoke("OnSendTaskPriority", taskId, priority, groupName);
    }

    onReceiveTaskPriority(callback:(taskId:string, priority:number) => void){
        this.hubConnection.on("onReceiveTaskPriority", callback);
    }

    onInvokeSendRetro(retro:Retrospective, groupName:string){
        this.hubConnection.invoke('OnSendRetro', retro, groupName);
    }

    onReceiveRetro(callback:(retro:Retrospective) => void){
        this.hubConnection.on('onReceiveRetro', callback);
    }

    onInvokeSendUpdatedTaskItem(taskId:string, taskItemId:string, newContent:string, assignTo:string, groupName:string){
        this.hubConnection.invoke("OnSendUpdatedTaskItem", taskId, taskItemId, newContent, assignTo, groupName);
    }

    onReceiveUpdatedTaskItem(callback:(taskId:string, taskItemId:string, newContent:string, assignTo:string) => void) {
        this.hubConnection.on("onReceiveUpdatedTaskItem", callback);
    }

    /* DELETERS */
    
    deleteGroupMessageReceiver(){
        this.hubConnection.off("onReceiveGroupMessage");
    }

    deleteRetrosReceiver(){
        this.hubConnection.off('onReceiveRetro');
    }

    constructor(private pagesService:TasksService){}


}