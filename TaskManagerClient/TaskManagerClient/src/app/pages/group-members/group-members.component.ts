import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { UserService } from '../../services/users.service';
import { GroupAction } from '../../interfaces/simple objects/group-action.interface';
import { PopupService } from '../../services/popup.service';
import { GroupMember } from '../../interfaces/group-member.interface';
import { SprintService } from '../../services/sprint.service';
import { GroupService } from '../../services/group.service';
import { Subject, takeUntil } from 'rxjs';
import { TasksService } from '../../services/tasks.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { GroupRoleService } from '../../services/group-role.service';
import { UsersHubService } from '../../services/hub.service';
import { SelectedTask } from '../../interfaces/selected-task.interface';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-group-members',
  templateUrl: './group-members.component.html',
  styles: ``
})
export class GroupMembersComponent implements OnInit, OnDestroy{

  createGroup:boolean = false;

  isScrum:boolean = false;

  @ViewChild('joinGroupName') joinGroupName!:ElementRef;
  @ViewChild('createGroupName') createGroupName!:ElementRef;
  destroy$ = new Subject<void>();
  @Output() groupActionEventEmitter = new EventEmitter<any>();
  @Input() groupMembers:GroupMember[] = [];
  selectedTasksForSprint:SelectedTask[] = [];
  isSpanish: boolean = false;

  onGroupAction() {
    
    let groupName = "";

    if (this.createGroup) {
      groupName = (this.createGroupName.nativeElement as HTMLInputElement).value;
    }
    else {
      groupName = (this.joinGroupName.nativeElement as HTMLInputElement).value;
    }

    if (!groupName){
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "Group name is required");
      else
        this.popupService.showPopup('i', "Nombre de grupo es requerido");

      return;
    }

    
    if (!/^[a-zA-Z0-9]*$/.test(groupName)) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('e', 'Please, avoid using special characters');
      else
        this.popupService.showPopup('e', 'Por favor, evita usar caracteres especiales');

      return;
    }



    let event:GroupAction = {
      actionName: this.createGroup?'c':'j',
      groupName: groupName.trim(),
      isScrum: this.isScrum
    };

    this.groupActionEventEmitter.emit(event);

  }

  isAllowedPreviousValue!:boolean;
  
  onSetAddingTasksAllowed(isAllowed:HTMLInputElement) {


    this.isAllowedPreviousValue = isAllowed.checked;


    this.groupService.onSetAddingTasksAllowed(this.userService.groupName!, isAllowed.checked)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.userService.isAllowed = res.result;
      },
      error: err => {
        HandleBackendError(err, this.popupService);
        isAllowed.checked = this.isAllowedPreviousValue;
      }
    });

  }

  previousRoleValue!:string;

  onChooseRole(selectReference: HTMLSelectElement, selectedUsername: string) {
    
    if(this.userService.groupRole !== "product owner") {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "You are not the product owner");
      else
        this.popupService.showPopup('i', "No eres Product Owner");

      selectReference.value = this.previousRoleValue;
      return;
    }

    if (selectedUsername === this.userService.username){
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "You cannot change your own role");
      else
        this.popupService.showPopup('i', "No puedes cambiar tu propio rol");

      selectReference.value = this.previousRoleValue;
      return;
    }

    const groupName = this.userService.groupName!;

    this.groupRolesService.setUserGroupRole(
      groupName, 
      selectReference.value,
      selectedUsername,
      this.userService.isScrum
    )
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.hub.onInvokeReceiveUserGroupRole(
          res.groupRole.groupName, 
          res.groupRole.userName, 
          selectReference.value,
          res.userThatAssignedProductOwner,
          res.isSwitchingScrumMaster,
          res.userThatIsScrumMaster,
          res.userThatWasScrumMaster
        );

      },

      error: err => {
        HandleBackendError(err, this.popupService);
        selectReference.value = this.previousRoleValue;
      }

    });

    
  }
  
  weeks:string = "";

  @ViewChild('sprintName') sprintName!:ElementRef;
 
  beginSprint() {

    if(this.selectedTasksForSprint.length === 0) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "You haven't checked any tasks for this Sprint");
      else
        this.popupService.showPopup('i', "No has agregado tareas para este Sprint");

      return;
    }

    if(!this.weeks) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "You haven't selected any week duration");
      else
        this.popupService.showPopup('i', "No has seleccionado una duración");

      return;
    }

    let error:boolean = false;

    this.selectedTasksForSprint.forEach(selectedTask => {

      if (selectedTask.taskItems.length === 0) {
        error = true;
        return;
      }

    });

    if (error) {
      this.popupService.showPopup('e', "Task cannot be empty");
      return;
    }

    error = false;

    const weeks = Number(this.weeks);

    const tasksIds = this.selectedTasksForSprint.map(t => t.id);

    const sprintName = (this.sprintName.nativeElement as HTMLInputElement).value;

    this.sprintService.beginSprint(
      this.userService.groupName!,
      weeks,
      tasksIds,
      sprintName
    )
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
  
        this.hub.onInvokeSetSprintToTasks
        (
          res.tasksIds, 
          this.userService.groupName!,
          res.expirationTime,
          res.sprintId,
          res.sprintName,
          res.remainingTime
        );
      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  getSelectedTasksForSprint(){
    this.taskService.getSelectedTasksForSprint()
    .pipe(takeUntil(this.destroy$))
    .subscribe(tasks => this.selectedTasksForSprint = tasks);
  }

  getToken(){
    return localStorage.getItem('tmat');
  }


  @Output() leaveGroupEventEmitter = new EventEmitter<any>();
  
  onLeaveGroup(){
    this.leaveGroupEventEmitter.emit();
  }

  @Output() removeGroupEventEmitter = new EventEmitter<any>();

  onRemoveGroup(){
    this.removeGroupEventEmitter.emit();
  }

  getLanguage() {
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  downloadSprintsSummary(){

    

    this.sprintService.downloadSprintsSummary(this.userService.groupName!)
    .subscribe({
      next: blob => {

        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        
        a.download = `${this.userService.groupName}.pdf`;
        
        document.body.appendChild(a);
        a.click();
        
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  constructor(public userService:UserService, 
    private popupService:PopupService, 
    public sprintService:SprintService,
    private taskService:TasksService,
    private groupService:GroupService,
    private groupRolesService:GroupRoleService,
    private hub:UsersHubService,
    private languageService:LanguageService){}


  ngOnInit(): void {
    this.getSelectedTasksForSprint();
    this.getLanguage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
