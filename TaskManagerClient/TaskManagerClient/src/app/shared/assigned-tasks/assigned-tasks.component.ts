import { Component, OnDestroy, OnInit } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { Subject, takeUntil } from 'rxjs';
import { PopupService } from '../../services/popup.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { UsersHubService } from '../../services/hub.service';
import { TaskItem } from '../../interfaces/user-tasks.interface';
import { UserService } from '../../services/users.service';
import { TaskItemService } from '../../services/task-items.service';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-assigned-tasks',
  templateUrl: './assigned-tasks.component.html',
  styles: ``
})
export class AssignedTasksComponent implements OnInit, OnDestroy{

  assignedTasks:TaskItem[] = [];
  destroy$ = new Subject<void>();
  isSpanish: boolean = false;

  hideAssignedTasks(){
    this.taskService.hideAssignedTasks();
  }

  getUserPendingTasks(){
    this.taskService.getUserAssignedTasks$()
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: taskItems => this.assignedTasks = taskItems,
      error: err => HandleBackendError(err, this.popupService)
    });
    
  }

  markTaskItemAsCompleted(taskItem:TaskItem){

    const sprintId = this.taskService.tasks_BSubject.getValue().find(t => t.id === taskItem.taskId)!.sprintId;

    if (this.userService.isScrum)

      if (!sprintId) {
        this.popupService.showPopup('i', "You can complete this task once the sprint has begun");
        return;
      }

    const body = { 
      taskItemId: taskItem.id,
      taskId: taskItem.taskId, 
      groupName:this.userService.groupName ?? "none", 
      sprintId 
    };

    this.taskItemService.markTaskItemAsCompleted(taskItem.id, taskItem.taskId, sprintId, this.userService.groupName)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.popupService.showPopup('s', res.message);
        this.applyChanges(taskItem, res.taskIsCompleted);
      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  applyChanges(assignedTask:TaskItem, taskIsCompleted:boolean){

    if (this.userService.groupName) {

      const taskOwnerName = this.taskService.tasks_BSubject.getValue()
      .find(t => t.id === assignedTask.taskId)!.username

      this.usersHub.onInvokeSendCompletedTaskItem
      (
        assignedTask, 
        taskOwnerName, 
        this.userService.username!,
        taskIsCompleted
      );
      

    } else assignedTask.isCompleted = true;

  }

  getLanguage(){
    this.languageService.isSpanish$.pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  constructor(
    public taskService:TasksService, 
    private popupService:PopupService, 
    private usersHub:UsersHubService,
    private taskItemService:TaskItemService,
    private userService:UserService,
    private languageService:LanguageService
  ){}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  ngOnInit(): void {
    this.getUserPendingTasks();
    this.getLanguage();
  }

}
