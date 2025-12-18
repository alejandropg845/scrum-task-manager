import { DOCUMENT } from "@angular/common";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Inject, Injectable, signal } from "@angular/core";
import { environment } from "../../environments/environment";
import { TaskItem, UserTask } from "../interfaces/user-tasks.interface";
import { BehaviorSubject, Subject } from "rxjs";
import { HandleBackendError } from "../interfaces/error-handler";
import { PopupService } from "./popup.service";
import { UserService } from "./users.service";
import { ApiGatewayService } from "./api-gateway.service";
import { InitialUserInfo } from "../interfaces/responses/user-info-response.interface";
import { SelectedTask } from "../interfaces/selected-task.interface";

@Injectable({
    providedIn: 'root'
})
export class TasksService {

    accessToken!:string | null;
    refreshToken!:string | null;

    destroy$ = new Subject<void>();
    userPendingTasks_BSubject = new BehaviorSubject<TaskItem[]>([]);
    userTasksLocalStorage_BSubject = new BehaviorSubject<UserTask[]>([]);
    needsToGetTasks_Subject = new Subject<boolean>();
    tasks_BSubject = new BehaviorSubject<UserTask[]>([]);
    getUserTasks$ = this.tasks_BSubject.asObservable();

    private selectedTasksForSprint_BSubject = new BehaviorSubject<SelectedTask[]>([]);
    
    show_assigned_tasks_Signal = signal<boolean>(false);
    showLogin:boolean = false;

    get getToken() {
        return localStorage.getItem('tmat');
    }

    getUserInitialInfo(){
        return this.apiGateway.sendRequest<InitialUserInfo>("get","users",`GetUserInfo`);
    }

    getTasks(groupName:string){
        return this.apiGateway.sendRequest<UserTask[]>("get", "tasks", `GetUserTasks/${groupName}`);
    }

    addTask(title:string, groupName:string | null){

        const body = {
            isShared: groupName !== null,
            title,
            groupName
        }

        return this.apiGateway.sendRequest<UserTask>("post", "tasks", "AddUserTask", body);
    }

    deleteTask(taskId:string, groupName:string | null){
        return this.apiGateway.sendRequest("delete", "tasks", `DeleteUserTask/${taskId}?groupName=${groupName ?? ""}`); // <= El query en el backend lo interpreta como null al asignar ""
    }

    getUserAssignedTasks(groupName:string){
        this.apiGateway.sendRequest<TaskItem[]>(
            "get", 
            "task-items",
            `GetUserPendingTaskItems/${groupName}`
        )
        .subscribe(taskItems => this.userPendingTasks_BSubject.next(taskItems))
    }

    getUserAssignedTasks$(){
        return this.userPendingTasks_BSubject.asObservable();
    }

    showAssignedTasks(){
        this.show_assigned_tasks_Signal.set(true);
    }

    hideAssignedTasks(){
        this.show_assigned_tasks_Signal.set(false);
    }

    updateSelectedTasksForSprintInBSubject(tasks:UserTask[]){

        let selectedTasks:SelectedTask[] = []

        tasks.forEach(task => {

            let selectedTask:SelectedTask = {
                id: "",
                priority: 0,
                taskItems: [],
                title: ""
            };

            selectedTask.title = task.title;
            selectedTask.priority = task.priority;
            selectedTask.id = task.id;

            selectedTask.taskItems = task.taskItems.map(taskItem => {
                const priority = taskItem.priority;
                const content = taskItem.content;
                const id = taskItem.id
                return { priority, content, id };
            });
            
            selectedTasks.push(selectedTask);
            
        });

        this.selectedTasksForSprint_BSubject.next(selectedTasks);
    }

    getSelectedTasksForSprint(){
        return this.selectedTasksForSprint_BSubject.asObservable();
    }

    getSelectedTasksForSprintValue(){
        return this.selectedTasksForSprint_BSubject.getValue();
    }

    setTaskPriority(taskId:string, priority:number, groupName: string){

        const body = { groupName, taskId, priority };

        return this.apiGateway.sendRequest<any>("put", "tasks", `SetTaskPriority`, body);
    }
    
    constructor(
        @Inject(DOCUMENT) private document:Document,
        private apiGateway:ApiGatewayService){

        const storage = this.document.defaultView?.localStorage;
        if(storage) {
            this.accessToken = localStorage.getItem('tmat');
            this.refreshToken = localStorage.getItem('tmrt');
        }

    }
}