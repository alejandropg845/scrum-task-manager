import { ChangeDetectorRef, Component, ElementRef, EventEmitter, Inject, OnDestroy, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { finalize, Subject, takeUntil } from 'rxjs';
import { TaskItem, UserTask } from '../../interfaces/user-tasks.interface';
import { PopupService } from '../../services/popup.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { UsersHubService } from '../../services/hub.service';
import { DOCUMENT } from '@angular/common';
import { GroupMember } from '../../interfaces/group-member.interface';
import { animate, style, transition, trigger } from '@angular/animations';
import { TaskItemService } from '../../services/task-items.service';
import { SprintService } from '../../services/sprint.service';
import { AssistantService } from '../../services/assistant.service';
import { GroupRoleService } from '../../services/group-role.service';
import { UserService } from '../../services/users.service';
import { AuthService } from '../../services/auth.service';
import { ChatService } from '../../services/chat.service';
import { InitialUserInfo } from '../../interfaces/responses/user-info-response.interface';
import { RetrospectivesService } from '../../services/retrospectives.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-my-tasks',
  templateUrl: './tasks.component.html',
  styles: ``,
  animations: [
    trigger("fadeInOut", [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0)' }),
        animate('200ms ease', style({ opacity: 1, transform: 'scale(1.05)' })),
        animate('200ms ease', style({ transform: 'scale(1)' })),
      ]),
      transition(':leave', [
        animate('200ms ease', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class MyTasksComponent implements OnInit, OnDestroy {

  destroy$ = new Subject<void>();
  tasks: UserTask[] = [];
  isAssignedTasksOpen: boolean = false;
  isPeopleConnectedOpen: boolean = false;
  usersInGroup: GroupMember[] = []
  userTasksLocalStorage: UserTask[] = [];
  isSpanish:boolean = false;

  @ViewChildren('assignedTo') taskItemAssignedToSelects!: QueryList<ElementRef>;
  @ViewChildren('taskItemContent') taskItemContentInputs!: QueryList<ElementRef>;
  @ViewChildren('taskItemLi') taskItemsLi!:QueryList<ElementRef>
  
  deleteTaskRequestList = new Set<string>();
  addTaskItemRequestList = new Set<string>();
  deleteTaskItemRequestList = new Set();
  markAsCompletedRequestList = new Set();
  focusPopup: boolean = false;
  isProductOwner:boolean = true;


  getTasks() {

    this.tasksService.needsToGetTasks_Subject.asObservable()
    .subscribe(_ => {
      if (this.tasksService.getToken) {
        /* Obtener la info del user*/
        this.tasksService.getUserInitialInfo()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: res => {
            
            this.setInitialUserInfo(res);

            /* Abrir retrospective si ha finalizado */
            if (res.finishedSprintName && res.finishedSprintId) 
              this.retrosService.showRetrospective_BSubject.next({
                groupName: res.groupName,
                sprintId: res.finishedSprintId,
                sprintName: res.finishedSprintName,
                username: res.username
              });
            

            // Si el usuario tiene grupo, entonces agregamos automáticamente su connectionId al grupo
            if (res.groupName) {

              this.startHubConfiguration(res.groupName, res.groupRole);

              this.setInitialAppListeners(res.groupName);

              if (res.isScrum) {
                this.sprintService.getGroupSprints_BSubject.next(true);
                this.retrosService.getRetros(res.sprintNumber);
              }
              
            }

            

            this.getAndSetTasksToBSubject(res.groupName, res.isScrum);


          },
          error: err => HandleBackendError(err, this.popupService)
        });
      } else {
        this.getUserTasksLS();
        this.authService.initiateGoogleConfig.next(true);
      }
    });

  }

  getAndSetTasksToBSubject(groupName:string, isScrum:boolean){
    
    this.tasksService.getTasks(groupName)
    .subscribe({
      next: (res:UserTask[]) => {

        this.setTasksListener();

        this.orderTasksByPriority(res);

        this.tasks.forEach(task => this.orderTaskItemsByPriority(task.id));

        this.filterTasks(res, isScrum);

      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }
  
  @ViewChildren('selectvalue') selects!:QueryList<ElementRef>;

  openEditTaskItemInterface(taskItem:TaskItem, task:UserTask) {
    task.taskItemEdit = taskItem;
  }

  closeEditTaskItemInterface(taskId:string) {
    const task = this.tasks.find(t => t.id === taskId);
    if (task) task.taskItemEdit = null;
  }

  updateTaskItem(newContent:string, taskId:string, taskItemId:string){
  
    const task = this.tasks.find(t => t.id === taskId);

    if (this.userService.groupName){

      /* Obtener el valor del select por medio de su id. El valor de su Id es TaskId */
      const select = this.selects.find(e => (e.nativeElement as HTMLSelectElement).id === taskId);
      const assignTo = (select?.nativeElement as HTMLSelectElement).value;

      if (task && assignTo) {
        
        const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

        if (taskItem) {

          if (taskItem.content === newContent.trim() && taskItem.assignToUsername === assignTo) {
            
            if (!this.isSpanish)
              this.popupService.showPopup('i', 'No changes detected');
            else
              this.popupService.showPopup('i', 'No se detectaron cambios');

            return;
          }

          this.updateTaskItem_Request(newContent, assignTo, taskItemId, taskId);
        }

      }

    } else {

      if (task) {
        
        if (this.userService.username) {

          const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

          if (taskItem) {

            if (taskItem.content === newContent.trim()) {
              if (!this.isSpanish)
                this.popupService.showPopup('i', 'No changes detected');
              else
                this.popupService.showPopup('i', 'No se detectaron cambios');

              return;
            }

            this.updateTaskItem_Request(newContent, null, taskItemId, taskId);

          }

        } else this.updateTaskItemLS(taskId, taskItemId, newContent);

      }
    }
  }

  updateTaskItem_Request(newContent:string, assignTo:string | null, taskItemId:string, taskId:string){
    this.taskItemService.updateTaskItem(newContent, assignTo, taskItemId, taskId)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {

        this.popupService.showPopup('s', res.message);

        this.closeEditTaskItemInterface(taskId);

        if (this.userService.groupName) 

          this.usersHub.onInvokeSendUpdatedTaskItem(
            taskId, 
            taskItemId, 
            newContent, 
            assignTo!, 
            this.userService.groupName
          )

        else {  
          
          const task = this.tasks.find(t => t.id === taskId);

          if (task) {

            const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

            if (taskItem) taskItem.content = newContent;
            
          }
        }

      },
      error: err => HandleBackendError(err, this.popupService)
    })
  }

  setTasksListener(){
    this.tasksService.tasks_BSubject.asObservable()
    .subscribe(tasks => this.tasks = tasks);
  }

  filterTasks(tasks:UserTask[], isScrum:boolean){

    if (isScrum) {
      const sprintsTasks = tasks.filter(t => t.sprintStatus === "finished");
      this.sprintService.sprintsTasks_BSubject.next(sprintsTasks);
    }

    const noFinishedTasks = tasks.filter(t => t.sprintStatus !== "finished");

    
    this.tasksService.tasks_BSubject.next(noFinishedTasks); 

  }

  setInitialUserInfo(res:InitialUserInfo){
    this.userService.groupName = res.groupName; //GroupName en donde usuario se encuentra unido
    this.userService.username = res.username;
    //OwnerGroupName donde obtenemos directamente el groupName del microservicio Group del username
    this.userService.isGroupOwner = res.isGroupOwner;
    this.userService.isScrum = res.isScrum;
    this.userService.isAllowed = res.isAddingTasksAllowed;
    this.userService.groupRole = res.groupRole;
    this.userService.expirationTime = res.expirationTime;
    this.userService.status = res.status;
    this.userService.avatarBgColor = res.avatarBgColor;
    this.sprintService.sprintNumber = res.sprintNumber;
    this.sprintService.sprintName = res.sprintName;
    if (res.remainingTime) {
      const remainingTime = this.parseRemainingTime(res.remainingTime);
      this.remainingTimeCounter(remainingTime);
    }
  }

  private setInitialAppListeners(groupName:string){

    this.getUsersFromBSubject();

    this.userService.getGroupInfo_subject.next(true);

    // this.chatService.setChatReceivers_Subject.next(true);

    this.tasksService.getUserAssignedTasks(groupName);

    this.userService.getAndSetUsersBSubject(groupName);

    this.retrosService.setRetrosHubReceiver();
  }

  startHubConfiguration(groupName:string, groupRole:string){
    this.usersHub.onConnectedUser()
    .then(_ => {
      this.usersHub.onInvokeJoinedGroup(groupName, groupRole);
      this.usersHub.onReceiveTaskItemPriority(this.onReceiveTaskItemPriority.bind(this));
      this.usersHub.onReceiveGroupTask(this.onReceiveGroupTask.bind(this));
      this.usersHub.onReceiveGroupTaskItem(this.onReceiveGroupTaskItem.bind(this));
      this.usersHub.onReceiveRemovedTaskItem(this.onReceiveRemovedTaskItem.bind(this));
      this.usersHub.onReceiveCompletedTaskItem(this.onReceiveCompletedTaskItem.bind(this));
      this.usersHub.onReceiveSprintToTasks(this.onReceiveSprintToTasks.bind(this));
      this.usersHub.onReceiveCompletedTask(this.onReceiveCompletedTask.bind(this));
      this.usersHub.onReceiveDeletedGroupTask(this.onReceiveDeletedGroupTask.bind(this));
      this.usersHub.onReceiveTaskPriority(this.onReceiveTaskPriority.bind(this));
      this.usersHub.onReceiveUpdatedTaskItem(this.onReceiveUpdatedTaskItem.bind(this));
    })
    .catch(() => this.popupService.showPopup('e', "Error while joining to group server"))
  }

  getUsersFromBSubject(){
    this.userService.usersInGroup_BSubject.asObservable()
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: users => this.usersInGroup = users,
      error: err => HandleBackendError(err, this.popupService)
    })
  }

  canDeleteTask(isRemovable:boolean){

    const isScrum = this.userService.isScrum;
    const isGroupOwner = this.userService.isGroupOwner;
    const isProductOwner = (this.userService.groupRole === "product owner");
    
    if (!isScrum && (isRemovable || isGroupOwner))
      return true;
    else {
      if (isScrum)
        if (isProductOwner) return true;
      else 
        if (isGroupOwner) return true;

      return false;
    }
  }

  private orderTasksByPriority(tasks:UserTask[]){

    this.tasks = tasks;

      this.tasks.sort((a, b) => {
      
      const aSprintId = a.sprintId ? 1 : 0;
      const bSprintId = b.sprintId ? 1 : 0;

      // Si retorna negativo, a se ubicará antes
      // Si retorna positivo, b se ubicará antes
      // Si ambos son iguales, ordena por prioridad más abajo
      const sprintIdSort = bSprintId - aSprintId;
      if (sprintIdSort !== 0) {
        return sprintIdSort;
      }

      return b.priority - a.priority;
    });



  }

  deleteTask(taskId: string) {

    if (this.deleteTaskRequestList.has(taskId)) return;

    this.deleteTaskRequestList.add(taskId);

    this.tasksService.deleteTask(taskId, this.userService.groupName)
      .pipe(
        takeUntil(this.destroy$), 
        finalize(() => this.deleteTaskRequestList.delete(taskId))
      )
      .subscribe({
        next: _ => {

          //informar a los demás sobre el cambio y hacer su respectivo cambio
          if (this.userService.groupName) 
            this.usersHub.onInvokeDeletedGroupTask(taskId, this.userService.groupName);
          else {

            const userTasks = this.tasksService.tasks_BSubject.getValue();

            const taskIndex = userTasks.findIndex(t => t.id === taskId)!;

            userTasks.splice(taskIndex, 1);

            this.tasksService.tasks_BSubject.next(userTasks);

          }


        },
        error: err => HandleBackendError(err, this.popupService)
      });
  }

  weeks!:number;
  
  selectedTasks:UserTask[] = [];

  addTaskToSprint(task:UserTask, element:HTMLInputElement) {

    if (element.checked) 
      this.selectedTasks.push(task);
    else {

      const taskIndex = this.selectedTasks.findIndex(st => st.id === task.id)!;

      this.selectedTasks.splice(taskIndex, 1);

    }
    this.tasksService.updateSelectedTasksForSprintInBSubject(this.selectedTasks);
    this.updateSelectedTasksPriority(task.id);
  }

  addTaskItem(taskId: string, taskTitle:string, sprintId:string, ul: HTMLUListElement) {
    let input!: HTMLInputElement;

    this.taskItemContentInputs.forEach(inputElementRef => {
      const inputElement = inputElementRef.nativeElement as HTMLInputElement;
      if (inputElement.id === taskId) {
        input = inputElement;
      };
    });

    if (!input.value || !taskId) {
      return;
    }

    let assignedTo;


    if (this.userService.groupName) {
      this.taskItemAssignedToSelects.forEach(assignedToSelect => {
        const select = assignedToSelect.nativeElement as HTMLSelectElement;
        if (select.id === taskId) assignedTo = select.value;
      });
    }
    else
      assignedTo = "own task";

    if (!assignedTo && this.userService.groupName) {

      if (!this.isSpanish)
        this.popupService.showPopup('e', "You must provide an user to assign this task");
      else
        this.popupService.showPopup('e', "Debes especificar un usuario para asignar esta tarea");

      return;
    }

    if (input.value.length > 300) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', 'Your task description must be lower than 300 characters');
      else
        this.popupService.showPopup('e', 'La descripción de tu tarea debe ser menor a 300 caracteres');

      return;
    }

    if (this.addTaskItemRequestList.has(taskId)) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', 'Your previous request is being processed. Please, wait')
      else
        this.popupService.showPopup('i', 'Tu petición anterior está siendo procesada, espera')

      return;
    }

    this.addTaskItemRequestList.add(taskId);

    const groupName = this.userService.groupName;

    this.taskItemService.addTaskItem(
      taskId,  input.value, 
      assignedTo!, 
      taskTitle, sprintId, groupName
    )
    .pipe(takeUntil(this.destroy$), finalize(() => this.addTaskItemRequestList.delete(taskId)))
    .subscribe({
      next: addedTaskItem => {
        if (!this.userService.groupName) {

          this.addNoGroupTaskItem(addedTaskItem, input, ul);
          input.value = '';

        } else {
          this.usersHub.onInvokeSendGroupItemTask(addedTaskItem);
          input.value = '';
        }

      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  addNoGroupTaskItem(addedTaskItem: TaskItem, content: HTMLInputElement, ul: HTMLUListElement) {

    addedTaskItem.isCompletable = true;
    addedTaskItem.isRemovable = true;

    const userTaskIndex = this.tasks.findIndex(ut => ut.id === addedTaskItem.taskId);

    this.tasks[userTaskIndex].taskItems.push(addedTaskItem);
    content.value = '';
    this.scrollToBottom(ul);
  }

  scrollToBottom(ulContainer: HTMLUListElement) {
    setTimeout(() => {
      ulContainer.scrollTo({
        top: ulContainer.scrollHeight,
        behavior: 'smooth'
      });
    }, 100);
  }

  deleteTaskItem(taskItem: TaskItem) {

    if (this.deleteTaskItemRequestList.has(taskItem.id)) return;

    this.deleteTaskItemRequestList.add(taskItem.id);


    this.taskItemService.deleteTaskItem(taskItem.id, taskItem.taskId, this.userService.groupName)
      .pipe(takeUntil(this.destroy$), finalize(() => this.deleteTaskItemRequestList.delete(taskItem.id)))
      .subscribe({
        next: _ => {

          //Hay token, es decir, hay cuenta de usuario
          if (this.tasksService.getToken) {

            //Hay cuenta de usuario y se encuentra unido a un grupo
            if (this.userService.groupName) {
              this.usersHub.onInvokeSendRemovedTaskItem(taskItem);
            } else {
              //Hay cuenta de usuario pero no tiene grupo
              this.applyLocalChanges(taskItem.id, taskItem.taskId);
            }

            //No hay token (cuenta de usuario), por lo tanto se está usando localStorage
          } else this.applyLocalChanges(taskItem.id, taskItem.taskId);

        },
        error: err => HandleBackendError(err, this.popupService)
      });
  }

  applyLocalChanges(taskItemId: string, taskId: string) {
    const task = this.tasks.find(ut => ut.id === taskId);

    if (task) {
      const taskItemIndex = task.taskItems.findIndex(ti => ti.id === taskItemId);
      if (taskItemIndex !== -1) task.taskItems.splice(taskItemIndex, 1);
    }

  }

  showActionButtons(sele_buttons: HTMLDivElement){
    sele_buttons.classList.add('show-buttons');
  }

  hideActionButtons(sele_buttons: HTMLDivElement){
    sele_buttons.classList.remove('show-buttons');
  }

  count:number = 0;
  togglePhoneActionButtons(taskItem:TaskItem, task:UserTask){

    this.count++;
    setTimeout(() => {
      if(this.count > 1)
        this.openEditTaskItemInterface(taskItem, task);
      
      this.count = 0;

    }, 500);
  }

  isMobilePhone(){
    return /Android|iPhone/i.test(navigator.userAgent);
  }

  isTruncated(p: HTMLParagraphElement): boolean {
    return p.scrollHeight > p.clientHeight;
  }

  openTaskItemContent(element: HTMLParagraphElement) {

    if (element.classList.contains("truncated-text")) {
      element.classList.remove("truncated-text");
      return;
    }

    element.classList.add("truncated-text");

  }

  markTaskItemAsCompleted(taskItem: TaskItem, taskOwnerName: string, sprintId:string) {

    if (this.userService.isScrum && this.userService.status !== 'begun') {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', 'You can set as completed only when sprint has begun');
      else
        this.popupService.showPopup('i', 'Puedes marcar como completado cuando el Sprint haya comenzado');

      return;
    }

    if (this.markAsCompletedRequestList.has(taskItem.id)) return;

    this.markAsCompletedRequestList.add(taskItem.id);

      const groupName = this.userService.groupName;

      this.taskItemService.markTaskItemAsCompleted(taskItem.id, taskItem.taskId, sprintId, groupName)
      .pipe(takeUntil(this.destroy$), finalize(() => this.markAsCompletedRequestList.delete(taskItem.id)))
      .subscribe({
        next: res => {
          
          //El usuario tiene una cuenta
          if (this.tasksService.getToken) {

            //El usuario tiene un grupo
            if (this.userService.groupName) {
              
              //Enviar a todos el taskItem completado
              this.usersHub.onInvokeSendCompletedTaskItem(taskItem, taskOwnerName, this.userService.username!, res.taskIsCompleted);
              
            } else {
              //El usuario no tiene grupo
              taskItem.isCompleted = true;

              if (res.taskIsCompleted) {

                const tasks = this.tasksService.tasks_BSubject.getValue();

                const task = tasks.find(t => t.id === taskItem.taskId)!;

                task.status = "completed";

                this.tasksService.tasks_BSubject.next(tasks);

              }

            }

          }

        },
        error: err => HandleBackendError(err, this.popupService)
      });
  }

  showAnimation:boolean = false;

  onClickListUsers() {
    if (this.isPeopleConnectedOpen) {
      this.focusPopup = false;
      this.isPeopleConnectedOpen = false;
      this.showAnimation = false;
    } else {
      this.focusPopup = true;
      this.isPeopleConnectedOpen = true;
      this.showAnimation = true;
    }
  }


  onSetTaskItemPriority(taskId:string, taskItemId:string, priority:number){


    if(this.userService.isScrum) {
      
      let canPrioritize = this.userService.groupRole === 'product owner' || this.userService.groupRole === 'scrum master';

      if (!canPrioritize) {
        if (!this.isSpanish)
          this.popupService.showPopup('i', 'You cannot set priorities');
        else
          this.popupService.showPopup('i', 'No puedes priorizar tareas');

        return;
      }
      
    }

    const groupName = this.userService.groupName;

    this.taskItemService.setPriorityToTaskItem(taskId, taskItemId, priority, groupName)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {

        if(!this.userService.groupName) {

          // El usuario no contiene ningún grupo (es own task), por lo que actualizamos sólo para él
          const tasks = this.tasksService.tasks_BSubject.getValue();

          const task = tasks.find(t => t.id === taskId)!;

          const taskItem = task.taskItems.find(ti => ti.id === res.taskItemId)!;

          taskItem.priority = res.priority;

          this.tasksService.tasks_BSubject.next(tasks);

          this.orderTaskItemsByPriority(taskId);


          //El usuario se encuentra en un grupo, por lo que actualicemos para todos (incluyendolo)
        } else this.usersHub.onInvokeSetTaskItemPriority(taskItemId, taskId, priority, res.groupName);
        
      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  private orderTaskItemsByPriority(taskId:string){

    const task = this.tasks.find(t => t.id === taskId);
    if (task) {
      const taskItems = task.taskItems;

      if (taskItems) {
        task.taskItems.sort((b, a) => a.priority - b.priority);
      }
    }
  }

  private updateSelectedTasksPriority(taskId:string){

    const task = this.selectedTasks.find(t => t.id === taskId);

    /* Verificar que el task al que se le cambió ya sea su prioridad o a sus items 
    se encuentra en los tasks seleccionados para el sprint */
    if (task) {
      
      this.orderSelectedTaskByPriority(this.selectedTasks);

      this.tasksService.updateSelectedTasksForSprintInBSubject(this.selectedTasks);
    }

  }

  private orderSelectedTaskByPriority(tasks:UserTask[]){

    tasks.sort((lower, greater) => greater.priority - lower.priority);

    tasks.forEach(task => task.taskItems.sort((lower, greater) => greater.priority - lower.priority));

  }

  onSetTaskItemPriorityLS(taskId:string, taskItemId:string, priority:number){

    const userTasks = this.tasksService.userTasksLocalStorage_BSubject.getValue();

    const task = userTasks.find(t => t.id === taskId);

    if (task) {

      const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

      if (taskItem) taskItem.priority = priority;

    }

    this.orderTaskItemsByPriority(taskId);

    this.saveChangesLocalStorage(userTasks);

  }

  @Output() openAssistantEventEmitter = new EventEmitter();

  openAssistant(taskItemContent:string){
    this.openAssistantEventEmitter.emit(taskItemContent);
  }

  private parseRemainingTime(remainingTime:string){

    const [remainingDays, time, miliseconds] = remainingTime.split('.');

    let days = "";

    if (remainingDays[0] === '-') 
    {
      days = remainingDays.replace('-', '');
    } else 
      days = remainingDays;

    const [hours, minutes, seconds] = time.split(':');

    const value = `${days}-${hours}-${minutes}-${seconds}`;


    return value;

  }

  private remainingTimeCounter(remainingTime:string){ 

    const [days, hours, minutes, seconds] = remainingTime.split('-');

    let daysNumber = Number(days);
    let hoursNumber = Number(hours);
    let minutesNumber = Number(minutes);
    let secondsNumber = Number(seconds);

    const interval = setInterval(() => {
      
      if (secondsNumber > 0) {
        secondsNumber--; // Quitar un segundo
        
        if (secondsNumber === 0) { //Ya no hay segundos restantes

          if (minutesNumber > 0) { 

            /*Ya no hay segundos restantes pero faltan minutos
            por lo que simplemente restamos un minuto*/
            minutesNumber--;

          } else {

            /* Si entra aquí es porque seconds y minutes son 0 */

            if (hoursNumber > 0) { // Verificar que hayan horas

              hoursNumber--; //Eliminamos una hora
              minutesNumber = 59; // Agregamos 60 minutes

            } else {

              /* No hay horas restantes */

              if (daysNumber > 0) { // Verificar que hayan dias restantes

                daysNumber--; // Eliminar un dia
                hoursNumber = 23; // Resetear hours
                minutesNumber = 59; // Resetear minutes
                
              } else {
                clearInterval(interval);
                secondsNumber = 0;
                this.tasksService.needsToGetTasks_Subject.next(true);
              }

            }

            

          }
         
        }

      } else secondsNumber = 59;

      this.sprintService.remainingTime = `${daysNumber}d - ${hoursNumber}h - ${minutesNumber}m - ${secondsNumber}s`;

    }, 1000);
      
    

  }

  setTaskPriority(taskId:string, priority:number){

    this.tasksService.setTaskPriority(taskId, priority, this.userService.groupName!)
    .subscribe({
      next: res => this.updateTaskPriority(res.taskId, res.priority),
      error: err => HandleBackendError(err, this.popupService)
    });
    
  }

  private updateTaskPriority(taskId:string, priority:number){
    
    if (!this.userService.groupName){

      this.tasks.find(t => t.id === taskId)!.priority = priority;
      
      this.orderTasksByPriority(this.tasks);

    } else this.usersHub.onInvokeSetTaskPriority(taskId, priority, this.userService.groupName);


  }

  //LOCALSTORAGE INTERACTION

  getUserTasksLS() {

    
    if (localStorage.getItem('userTasks') === null)
      localStorage.setItem('userTasks', JSON.stringify([]));

    
    const userTasksLC: UserTask[] = JSON.parse(localStorage.getItem('userTasks')!);

    this.tasksService.userTasksLocalStorage_BSubject.next(userTasksLC || []);

    this.tasksService.userTasksLocalStorage_BSubject.asObservable()
    .subscribe(userTasks => this.userTasksLocalStorage = userTasks);

    this.tasks = this.userTasksLocalStorage;

  }

  addTaskItemLocalStorage(taskId: string) {
  
    let input!: HTMLInputElement;

    this.taskItemContentInputs.forEach(inputElementRef => {
      const inputElement = inputElementRef.nativeElement as HTMLInputElement;
      if (inputElement.id === taskId) input = inputElement;
    });


    if (!input.value) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', "No task description was given");
      else
        this.popupService.showPopup('e', "No se especificó una descripción");

      return;
    }

    if (input.value.length > 300) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', "Your task description cannot exceed 300 characters");
      else
        this.popupService.showPopup('e', "La descripción de la tarea no puede superar los 300 caracteres");

      return;
    }

    const taskItem: TaskItem = {
      assignToUsername: '',
      content: input.value,
      id: Math.random().toString(),
      isCompletable: true,
      isRemovable: true,
      isCompleted: false,
      taskId,
      priority: 0,
      taskTitle: ""
    }

    const userTasks = this.tasksService.userTasksLocalStorage_BSubject.getValue();

    const task = userTasks.find(t => t.id === taskId);

    if (task) {
      task.taskItems.push(taskItem);
      this.saveChangesLocalStorage(userTasks);
    }

    input.value = "";

  }

  private updateTaskItemLS(taskId:string, taskItemId:string, newContent:string){

    const task = this.tasks.find(t => t.id === taskId)!;

    const taskItem = task.taskItems.find(ti => ti.id === taskItemId)!;

    taskItem.content = newContent;
    
    task.taskItemEdit = null;
    
    this.saveChangesLocalStorage(this.tasks);


  }

  deleteTaskLocalStorage(taskId: string) {

    const userTasks = this.tasksService.userTasksLocalStorage_BSubject.getValue();

    const userTaskIndex = userTasks.findIndex(ut => ut.id === taskId)!;

    userTasks.splice(userTaskIndex, 1);

    this.saveChangesLocalStorage(userTasks);

  }

  deleteTaskItemLocalStorage(taskId: string, taskItemId: string) {

    const userTasks = this.tasksService.userTasksLocalStorage_BSubject.getValue();
 
    const task = userTasks.find(t => t.id === taskId);

    if (task) {

      const taskItemIndex = task.taskItems.findIndex(ti => ti.id === taskItemId);

      if (taskItemIndex !== -1) task.taskItems.splice(taskItemIndex, 1);

      this.saveChangesLocalStorage(userTasks);

    }

  }

  setAsCompletedTaskItemLocalStorage(taskItem: TaskItem) {

    taskItem.isCompleted = true;

    const userTasks = this.tasksService.userTasksLocalStorage_BSubject.getValue();

    this.saveChangesLocalStorage(userTasks);

  }

  saveChangesLocalStorage(userTasks:UserTask[]) {

    this.tasksService.userTasksLocalStorage_BSubject.next(userTasks);
    localStorage.setItem('userTasks', JSON.stringify(this.tasks));
  }

  getLanguage() {
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => {
      this.isSpanish = isSpanish;
      this.cdr.detectChanges();
    })
  }

  constructor(public tasksService: TasksService,
    public taskItemService:TaskItemService,
    public sprintService:SprintService,
    public assistantService:AssistantService,
    public RoleService:GroupRoleService,
    public userService:UserService,
    private popupService: PopupService,
    private usersHub: UsersHubService,
    private cdr: ChangeDetectorRef,
    private authService:AuthService,
    private chatService:ChatService,
    private retrosService:RetrospectivesService,
    @Inject(DOCUMENT) private document: Document,
    private languageService:LanguageService) {

    const storage = this.document.defaultView?.localStorage;

    if (storage) {
      const userTasks = localStorage.getItem('userTasks');
      this.userTasksLocalStorage = userTasks ? JSON.parse(userTasks) : [];
    }

  }

  ngOnInit(): void {
    this.getTasks();
    this.tasksService.needsToGetTasks_Subject.next(true);
    this.getLanguage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  
  //?HUB STUFF

  onReceiveGroupTask(userTask: UserTask) {

    if (userTask.username === this.userService.username)
      userTask.isRemovable = true;

    
    const currentTasks = this.tasksService.tasks_BSubject.getValue();
    const updatedTasks = [...currentTasks, userTask];
    this.tasksService.tasks_BSubject.next(updatedTasks);

    this.cdr.detectChanges();
  }

  onReceiveDeletedGroupTask(taskId: string) {

    this.deleteAssignedTasksFromDeletedTask(taskId);

    const currentTasks = this.tasksService.tasks_BSubject.getValue();

    const deletedTaskIndex = currentTasks.findIndex(t => t.id === taskId)!;

    currentTasks.splice(deletedTaskIndex, 1);

    this.tasksService.tasks_BSubject.next(currentTasks);

    this.updateSelectedTasksForSprint(taskId);

    this.cdr.detectChanges();

  }

  private deleteAssignedTasksFromDeletedTask(taskId:string){

    
    const userAssignedTasks = this.tasksService.userPendingTasks_BSubject.getValue();
    
    const updatedAssignedTasks = userAssignedTasks.filter(ti => ti.taskId !== taskId);

    this.tasksService.userPendingTasks_BSubject.next(updatedAssignedTasks);

  }

  private updateSelectedTasksForSprint(taskId:string){

    const deletedTaskIndex = this.selectedTasks.findIndex(t => t.id === taskId);

    if (deletedTaskIndex !== -1) {

      this.selectedTasks.splice(deletedTaskIndex, 1);

      this.tasksService.updateSelectedTasksForSprintInBSubject(this.selectedTasks);
    }

  }

  onReceiveGroupTaskItem(taskItem: TaskItem) {

    

    const task = this.tasks.find(t => t.id === taskItem.taskId)!;

    //Asignar completable si al que se lo asignaron es igual al username
    if (taskItem.assignToUsername === this.userService.username)
      taskItem.isCompletable = true;

    //Asignar isRemovable si soy el dueño del task
    if (task.username === this.userService.username)
      taskItem.isRemovable = true;

    task.taskItems.push(taskItem);

    


    this.tasksService.updateSelectedTasksForSprintInBSubject(this.selectedTasks);


    //Asignar taskItem y notificar al assignedToUsername que se le ha asignado un taskItem
    this.assignTaskItemToUser(taskItem.id, taskItem.taskId);

    this.cdr.detectChanges();

    this.orderTaskItemsByPriority(taskItem.taskId);

  }

  private assignTaskItemToUser(taskItemId:string, taskId:string) {

    const task = this.tasks.find(t => t.id === taskId)!;

    const taskItem = task.taskItems.find(ti => ti.id === taskItemId)!;

    //Si el usuario al que se le asignó es el mismo al username, se notifica su asignación, 
    // pero si el owner del userTask al que pertenece el taskItem es el mismo al asignado, quiere decir que el usuario
    // se auto-asignó un taskItem, por lo que no notificamos.
    if (taskItem.assignToUsername === this.userService.username)
    {
      const userAssignedTasks = this.tasksService.userPendingTasks_BSubject.getValue();
      const updatedAssignedTasks = [...userAssignedTasks, taskItem];
      this.tasksService.userPendingTasks_BSubject.next(updatedAssignedTasks);

      /* Agregamos este condicional simplemente para que cuando el usuario 
        mismo se asigne una tarea de su propia tarea, no le notifique */
      if (taskItem.assignToUsername !== task.username) {
        
        if (!this.isSpanish)
          this.popupService.showPopup('i', 'A new task was assigned to you');
        else
          this.popupService.showPopup('i', 'Se te ha asignado una nueva tarea');
      }
    }

  }

  onReceiveRemovedTaskItem(taskItem: TaskItem) {

    const taskIndex = this.tasks.findIndex(ut => ut.id === taskItem.taskId);

    if (taskIndex !== -1) {
      const taskItemIndex = this.tasks[taskIndex].taskItems.findIndex(ti => ti.id === taskItem.id);

      if (taskItemIndex !== -1)
      {
        this.tasks[taskIndex].taskItems.splice(taskItemIndex, 1);

        this.deleteAssignedTaskItem(taskItem.id);

      }
    }

    this.cdr.detectChanges();

  }

  deleteAssignedTaskItem(taskItemId:string) {

    const userAssignedTaskItems = this.tasksService.userPendingTasks_BSubject.getValue();

    const deletedTaskItemIndex = userAssignedTaskItems.findIndex(ti => ti.id === taskItemId);

    if (deletedTaskItemIndex !== -1) {

      userAssignedTaskItems.splice(deletedTaskItemIndex, 1);
      this.tasksService.userPendingTasks_BSubject.next(userAssignedTaskItems);
    }
  }

  onReceiveCompletedTaskItem(completedTaskItem: TaskItem, taskOwnerName: string, username: string, taskIsCompleted:boolean) {

    //Procedimientos para activar respectivos buttons actions dependiendo del usuario
    completedTaskItem.isCompleted = true;

    const task = this.tasks.find(ut => ut.id === completedTaskItem.taskId);

    if (task) {
      const taskItemIndex = task.taskItems.findIndex(ti => ti.id === completedTaskItem.id);
      task.taskItems[taskItemIndex].isCompleted = true;

      if (task.username === this.userService.username)
        task.taskItems[taskItemIndex].isRemovable = true;

      if (taskIsCompleted) {
        task.status = "completed";
      }

    }

    //Procedimientos para enviar notificación de completedTask al taskOwnerName
    if (taskOwnerName === this.userService.username && this.userService.username !== username) {

      if (!this.isSpanish)
        this.popupService.showPopup('i', "The user " + username + " has completed a task");
      else 
        this.popupService.showPopup('i', "El usuario " + username + " ha completado una tarea");

    }
      

    this.cdr.detectChanges();

  }

  onReceiveCompletedAssignedTask(taskItemId: string) {
    const task = this.tasks.find(ut => ut.id === taskItemId);

    if (task) {
      const taskItemIndex = task.taskItems.findIndex(ti => ti.id === taskItemId);
      task.taskItems[taskItemIndex].isCompleted = true;

      if (task.username === this.userService.username)
        task.taskItems[taskItemIndex].isRemovable = true;

    }

    this.cdr.detectChanges();

  }


  onReceiveTaskItemPriority(taskItemId:string, taskId:string, priority:number){

    const task = this.tasks.find(t => t.id === taskId);

    if(task){

      const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

      if (taskItem) {
        taskItem.priority = priority;
        this.updateTaskItemsBehaviorSubject(taskItemId, priority);
        
        this.updateSelectedTasksPriority(taskId);
      }

    }

    this.orderTaskItemsByPriority(taskId);

    this.cdr.detectChanges();

  }

  updateTaskItemsBehaviorSubject(taskItemId:string, newPriority:number){

    const taskItems = this.tasksService.userPendingTasks_BSubject.getValue();

    const updatedTaskItem = taskItems.find(ti => ti.id === taskItemId);

    if (updatedTaskItem) {
      updatedTaskItem.priority = newPriority;
      this.tasksService.userPendingTasks_BSubject.next(taskItems);
    }

  }
  
  onReceiveSprintToTasks(tasksIds:string[], expirationTime:Date, sprintId:string, sprintName:string, remainingTime:string) {

    this.sprintService.sprintName = sprintName;

    const clearedDate = this.parseRemainingTime(remainingTime);

    this.remainingTimeCounter(clearedDate);

    tasksIds.forEach(taskId => {
      const task = this.tasks.find(t => t.id === taskId)!;
      task.status = "in progress";
      task.sprintId = sprintId;
    });

    this.userService.expirationTime = expirationTime;
    this.userService.status = 'begun';

    if (!this.isSpanish)
      this.popupService.showPopup('s', 'Sprint has begun!');
    else
      this.popupService.showPopup('s', 'El Sprint ha comenzado!');

    this.cdr.detectChanges();

  }

  onReceiveCompletedTask(taskId:string){

    this.tasks.find(t => t.id === taskId)!.status = "completed"; 

  }

  onReceiveTaskPriority(taskId:string, priority:number) {

    this.tasks.find(t => t.id === taskId)!.priority = priority;
    this.cdr.detectChanges();
    
    this.orderTasksByPriority(this.tasks);
    this.updateSelectedTasksPriority(taskId);

  }

  onReceiveUpdatedTaskItem(taskId:string, taskItemId:string, newContent:string, assignTo:string) {

    const task = this.tasks.find(t => t.id === taskId);

    if (task) {

      const taskItem = task.taskItems.find(ti => ti.id === taskItemId);

      if (taskItem) {
        taskItem.content = newContent;
        taskItem.assignToUsername = assignTo;
        this.deleteAssignedTaskItem(taskItemId);
        this.assignTaskItemToUser(taskItemId, taskId);
      }
    }
  }
  
}
