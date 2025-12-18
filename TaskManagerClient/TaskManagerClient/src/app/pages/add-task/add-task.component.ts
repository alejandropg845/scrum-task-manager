import { Component, OnDestroy, OnInit } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { PopupService } from '../../services/popup.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { UsersHubService } from '../../services/hub.service';
import { Subject, takeUntil } from 'rxjs';
import { UserTask } from '../../interfaces/user-tasks.interface';
import { UserService } from '../../services/users.service';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-add-task',
  templateUrl: './add-task.component.html',
  styles: ``
})
export class AddTaskComponent implements OnDestroy, OnInit{

  destroy$ = new Subject<void>();
  isSpanish:boolean = false;
  
  addTask(element:HTMLInputElement){


    if (!element.value) {
      return;
    }

    if (element.value.length > 90) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "Content must be less or equal to 90 characters");
      else
        this.popupService.showPopup('i', "El contenido debe ser menor o igual a 90 caracteres");

      return;
    }


    this.taskService.addTask(element.value, this.userService.groupName)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: task => {
        
        element.value = "";

        if (this.userService.groupName) 
          this.hub.onInvokeSendTaskToEveryone(task);
        else {

          task.isRemovable = true;
          const currentTasks = this.taskService.tasks_BSubject.getValue();
          const updatedTasks = [...currentTasks, task];
          this.taskService.tasks_BSubject.next(updatedTasks);

        }
      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  addTaskLS(element:HTMLInputElement){
    
    const title = element.value;

    if (!title) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', "No title for task was given");
      else
        this.popupService.showPopup('e', "No se ha proporcionado un título para la tarea");

      return;
    }

    if (title.length > 50) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', "Title cannot be greater than 50 characters");
      else
        this.popupService.showPopup('e', "El título no puede ser mayor a 50 caracteres");

      return;
    }

    const newTask: UserTask = {
      createdOn: new Date(),
      groupName: "no group",
      id: Math.random().toString(),
      isRemovable: true,
      taskItems: [],
      title: title,
      username: "",
      status: "",
      sprintId: "",
      sprintStatus: "",
      priority: 0,
      taskItemEdit: null
    }

    const userTasks = this.taskService.userTasksLocalStorage_BSubject.getValue();

    userTasks.push(newTask);

    this.taskService.userTasksLocalStorage_BSubject.next(userTasks);

    localStorage.setItem('userTasks', JSON.stringify(userTasks));

    element.value = '';

  }

  private getLanguage() {

    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);

  }

  constructor(private taskService:TasksService, 
    private popupService:PopupService, 
    private hub:UsersHubService, 
    public userService:UserService,
    private languageService:LanguageService){}

  ngOnInit(): void {
    this.getLanguage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


}
