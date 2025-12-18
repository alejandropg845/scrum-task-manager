import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { finalize, Subject, takeUntil, tap } from 'rxjs';
import { PopupService } from '../../services/popup.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { UsersHubService } from '../../services/hub.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/users.service';
import { SprintService } from '../../services/sprint.service';
import { GroupService } from '../../services/group.service';
import { animate, style, transition, trigger } from '@angular/animations';
import { GroupAction } from '../../interfaces/simple objects/group-action.interface';
import { GroupMember } from '../../interfaces/group-member.interface';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { ChatService } from '../../services/chat.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styles: ``,
  animations: [
    trigger("foldAnimation", [
      transition(":enter", [
        style({ transform: 'translateX(-100%)' }),
        animate(".2s ease-in-out", style({ transform: 'translateX(0)' }))
      ]),
      transition(":leave", [
        animate(".2s ease-in-out", style({ transform: 'translateX(-100%)' }))
      ])
    ]),
    trigger("fadeInOut", [
      transition(":enter", [
        style({ opacity: 0 }),
        animate(".2s ease-in-out", style({ opacity: 1 }))
      ]),
      transition(":leave", [
        animate(".2s ease-in-out", style({ opacity: 0 }))
      ])
    ])
  ]
})
export class SidebarComponent implements OnDestroy, OnInit{


  isNewTaskPressed:boolean = false;
  destroy$ = new Subject<void>();
  num:number = 0
  isLogin:boolean = false;

  isDoingGroupAction:boolean = false;
  isLogging:boolean = false;
  isRegistering:boolean = false;
  isLoggingOut:boolean = false;
  isLeavingGroup:boolean = false;
  isRemovingGroup:boolean = false;

  showAssignedTasks:boolean = false;
  showGroupMembers:boolean = false;
  showUser:boolean = false;
  showAuth:boolean = false;
  showLogin:boolean = false;
  showAddTask:boolean = false;
  showChat:boolean = false;
  showRetros:boolean = false;

  isUnfolded:boolean = false;
  isSpanish: boolean = false;

  groupMembers:GroupMember[] = [];



  @ViewChild('joingroup') joinGroup!:ElementRef | null;
  @ViewChild('creategroup') createGroup!:ElementRef | null;


  previousValue:string = '';
  showOption(option:string){

    if (this.previousValue === option) return;
    
    this.previousValue = option;

    this.showAssignedTasks = false;
    this.showGroupMembers = false;
    this.showUser = false;
    this.showAuth = false;
    this.showAddTask = false;
    this.showChat = false;
    this.showRetros = false;

    switch (option) {

      case 'user':
        this.showUser = true;
        break;
      case 'assigned':
        this.showAssignedTasks = true;
        break;
      case 'group':
        this.showGroupMembers = true;
        break;
      case 'auth':
        this.showAuth = true;
        break;
      case 'chat':
      this.showChat = true;
        break;
      case 'add':
        this.showAddTask = true;
        break;
      case 'retros':
        this.showRetros = true;
        break;
    }

  }

  isNoOptionSelected(){
    return !this.showAssignedTasks
    && !this.showGroupMembers
    && !this.showUser
    && !this.showAuth
    && !this.showAddTask
    && !this.showChat
    && !this.showRetros
  }
  
  changeType(){
    this.isLogin = !this.isLogin;
  }

  
  login(event:{username:string, password:string}){

    if(this.isDoingAnyProcess()) return;

    this.isLogging = true;

   
    this.authService.loginUser(event.username, event.password).pipe
    (
      takeUntil(this.destroy$),
      tap((res:any) => {
        if(res.ok){
          localStorage.setItem('tmat', res.accessToken);
          localStorage.setItem('tmrt', res.refreshToken);
          this.taskService.accessToken = res.accessToken;
          this.taskService.refreshToken = res.refreshToken;
          this.showAuth = false;
          this.taskService.needsToGetTasks_Subject.next(true);
        }
      }),
      finalize(() => this.isLogging = false)
    )
    .subscribe({
      next: res => {
        this.popupService.showPopup('s', res.message);
      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  register(event:{username:string, password:string, email:string}) {
    
    if(this.isDoingAnyProcess()) return;

    this.isRegistering = true;

    this.authService.registerUser(event.username, event.password, event.email).pipe
    (
      takeUntil(this.destroy$),
      tap(res => {
        if(res.ok){
          localStorage.setItem('tmat', res.accessToken);
          localStorage.setItem('tmrt', res.refreshToken);
          this.taskService.accessToken = res.accessToken;
          this.taskService.refreshToken = res.refreshToken;
          this.showAuth = false;
          this.taskService.needsToGetTasks_Subject.next(true);
        }
      }),
      finalize(() => this.isRegistering = false)
    )
    .subscribe({
      next: res => {
        this.popupService.showPopup('s', res.message);
      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  continueWithGoogle(){
    this.authService.continueWithOAuthGoogle();
  }

  onSendGoogleInfo(){

    this.authService.sendGoogleInfo_Subject.asObservable()
    .pipe(takeUntil(this.destroy$))
    .subscribe(tokenId => {

      this.authService.sendGoogleInfoForLogin(tokenId)
      .pipe(
        takeUntil(this.destroy$),
        tap(res => {
          if (res.ok) {
          localStorage.setItem('tmat', res.accessToken);
          localStorage.setItem('tmrt', res.refreshToken);
          this.taskService.accessToken = res.accessToken;
          this.taskService.refreshToken = res.refreshToken;
          this.showAuth = false;
          this.taskService.needsToGetTasks_Subject.next(true);
          }
        })
      )
      .subscribe({
        next: res => this.popupService.showPopup('s', res.message),
        error: err => HandleBackendError(err, this.popupService)
      });

    });

  }

  logOut(){
    
    if(this.isDoingAnyProcess()) return;
    

    this.showOption('auth');

    this.isLoggingOut = true;

    this.taskService.accessToken = null;
    localStorage.removeItem('tmat');
    this.userService.groupName = null;
    this.userService.username = null;
    this.userService.isGroupOwner = false;
    this.userService.usersInGroup_BSubject.next([]);
    this.hub.stopConnection();
    this.userService.isScrum = false;
    this.userService.isAllowed = false;
    this.userService.groupRole = null;
    this.userService.expirationTime = null;
    this.userService.status = "";
    this.sprintService.sprintNumber = 0;
    this.sprintService.showSprints = false;

    this.taskService.needsToGetTasks_Subject.next(true);

    this.isLoggingOut = false;

  }

  onGroupSubmit(event:GroupAction){

    if(this.isDoingAnyProcess()) return;

    this.isDoingGroupAction = true;

    let action$;

    if (event.actionName === 'j') 
      action$ = this.groupService.onJoinGroup(event.groupName)
    else 
      action$ = this.groupService.onCreateGroup(event.groupName, event.isScrum);
    
    action$.pipe(takeUntil(this.destroy$), finalize(() => this.isDoingGroupAction = false))
    .subscribe({
      next: res => {
        this.userService.groupName = res.groupName;
        this.hub.onConnectedUser()
        .then(_ => this.taskService.needsToGetTasks_Subject.next(true))
        .catch(() => console.log("Failed to initialize hub"))
      },
      error: err => HandleBackendError(err, this.popupService)
    });
    

  }

  counter:number = 0;

  onLeaveGroup(){

    this.counter++;

    if(this.isDoingAnyProcess()) return;
    
    this.isLeavingGroup = true;

    this.groupService.onLeaveGroup(this.userService.groupName!)
    .pipe(takeUntil(this.destroy$), finalize(() => this.isLeavingGroup = false))
    .subscribe({
      next: _ => {

        const groupNameValueBeforeSetNull = this.userService.groupName;
        this.hub.onLeaveGroup(groupNameValueBeforeSetNull!).then();
        this.hub.stopConnection().then();
        this.ResetLeaveGroup();
        
      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  private ResetLeaveGroup() {
    this.userService.groupName = null;
    this.userService.isGroupOwner = false;
    this.userService.isScrum = false;
    this.userService.isAllowed = false;
    this.userService.groupRole = null;
    this.userService.expirationTime = null;
    this.userService.status = "";
    this.sprintService.sprintNumber = 0
    this.sprintService.showSprints = false;
    this.sprintService.sprintsTasks_BSubject.next([]);
    this.taskService.tasks_BSubject.next([]);
    this.userService.userPendingTasks.next([]);
    this.taskService.updateSelectedTasksForSprintInBSubject([]);
    this.userService.usersInGroup_BSubject.next([]);

  }

  onRemoveGroup(){

    if(this.isDoingAnyProcess()) return;

    this.isRemovingGroup = true;

    if(this.userService.groupName){
      this.groupService.onDeleteGroup( this.userService.groupName!)
      .pipe(takeUntil(this.destroy$), finalize(() => this.isRemovingGroup = false))
      .subscribe({
        next: res => { 
          this.popupService.showPopup('s', res.message);
          this.hub.onInvokeDeleteGroup(res.deletedGroup);
        },
        error: err => HandleBackendError(err, this.popupService)
      });
    }
  }

  getUserAssignedTasks(){

    const groupName = this.userService.groupName;
    
    this.taskService.getUserAssignedTasks(groupName!);
    this.taskService.showAssignedTasks();
  }

  isDoingAnyProcess(){
    return this.isLogging || this.isDoingGroupAction || this.isLeavingGroup
    || this.isLoggingOut || this.isRemovingGroup
  }


  @ViewChild('sidebar') sidebar!:ElementRef;
  @ViewChild('sidebar_arrow') sidebarArrow!:ElementRef;

  toggleSidebar(){

    this.isUnfolded = !this.isUnfolded;
    const sidebar = (this.sidebar.nativeElement as HTMLDivElement);
    const sidebarArrow = (this.sidebarArrow.nativeElement as HTMLElement);

    if (this.isUnfolded){
      sidebar.classList.add('unfold-sidebar');
      sidebarArrow.classList.add('rotate-arrow');
    }
    else {
      sidebar.classList.remove('unfold-sidebar');
      sidebarArrow.classList.remove('rotate-arrow');
    }

  }

  setInfoForGroup(){

    const anySignal = this.userService.getGroupInfo_subject.asObservable();

    anySignal.subscribe(_ => {
      this.hub.onReceiveUserLeftGroup(this.onReceiveUserLeftGroup.bind(this));
      this.hub.onReceiveUserJoinedGroup(this.onReceiveUserJoinedGroup.bind(this));
      this.hub.onReceiveUserGroupRole(this.onReceiveUserGroupRole.bind(this));
      this.hub.onReceiveRemovedGroup(this.onReceiveRemovedGroup.bind(this));


      this.userService.usersInGroup_BSubject.asObservable()
      .subscribe(groupMembers => {
        this.groupMembers = groupMembers;
      });

    });
    
  }

  /* HUB METHODS */

  onReceiveRemovedGroup(){

    this.userService.groupName = null;
    this.userService.expirationTime = null;
    this.userService.isScrum = false;
    this.userService.isGroupOwner = false;
    this.userService.status = "";
    this.userService.groupRole = null;
    this.userService.isAllowed = false;
    this.userService.groupRole = null;
    this.userService.avatarBgColor = "";
    this.sprintService.sprintNumber = 0;
    this.sprintService.sprintName = "";
    
    this.userService.usersInGroup_BSubject.next([]);
    this.taskService.userPendingTasks_BSubject.next([]);
    this.taskService.tasks_BSubject.next([]);
    this.sprintService.sprintsTasks_BSubject.next([]);

    if (!this.userService.isGroupOwner) {

      if (!this.isSpanish)
        this.popupService.showPopup('i', "This group has been deleted");
      else 
        this.popupService.showPopup('i', "Este grupo ha sido eliminado");

    }
      

    this.showOption('user');

    this.userService.isGroupOwner = false;

    this.cdr.detectChanges();

  }

  onReceiveUserLeftGroup(username: string) {
  
    const index = this.groupMembers.findIndex(u => u.username === username);

    if (index !== -1) this.groupMembers.splice(index, 1);

    if (!this.isSpanish)
      this.popupService.showPopup('i', "User " + username + " has left the group");
    else 
      this.popupService.showPopup('i', "Usuario " + username + " ha dejado el grupo");

    this.userService.usersInGroup_BSubject.next(this.groupMembers);

    this.taskService.userPendingTasks_BSubject.next([]);

    this.cdr.detectChanges();
  
  }
  
  onReceiveUserJoinedGroup(username: string, roleName: string) {
    const user: GroupMember = {
      groupName: this.userService.groupName!,
      groupRole: roleName,
      username
    }

    const userIsInGroup = this.groupMembers.find(u => u.username === username);

    if (!userIsInGroup) 
    {
      this.groupMembers.push(user);
      this.userService.usersInGroup_BSubject.next(this.groupMembers); 

      // Avisar al usuario actual que entró al grupo tal
      if (username === this.userService.username) {

        if (!this.isSpanish)
          this.popupService.showPopup('i', "Joined to group " + this.userService.groupName);
        else 
          this.popupService.showPopup('i', "Entraste al grupo " + this.userService.groupName);
      } else {

        if (!this.isSpanish)
          this.popupService.showPopup('i', `User ${username} has joined the group`);
        else 
          this.popupService.showPopup('i', `Usuario ${username} ha entrado al grupo`);

      }
        

    } else userIsInGroup.groupRole = roleName;
    
    this.cdr.detectChanges();

  }

  onReceiveUserGroupRole(username: string, 
    groupRole: string, 
    userThatAssignedProductOwner:string,
    isSwitchingScrumMaster:boolean,
    userThatWasScrumMaster:string,
    userThatIsScrumMaster:string) {

    const userInGroup = this.groupMembers.find(u => u.username === username)!;
    userInGroup.groupRole = groupRole;

    if (!isSwitchingScrumMaster && !userThatAssignedProductOwner){
      
      if (username === this.userService.username)
        this.userService.groupRole = groupRole;
      return;
    }

    if (isSwitchingScrumMaster) {

      const oldUser = this.groupMembers.find(u => u.username === userThatWasScrumMaster);
      const newUser = this.groupMembers.find(u => u.username === userThatIsScrumMaster);

      if (oldUser && newUser) {
        oldUser.groupRole = "none";
        newUser.groupRole = "scrum master";
        if (newUser.username === this.userService.username) 
          this.userService.username = "scrum master";
        if (oldUser.username === this.userService.username) 
          this.userService.username = "none";
      }
      return;
    }

    if (userThatAssignedProductOwner) {
      const userThatAssigned = this.groupMembers.find(m => m.username === userThatAssignedProductOwner)!;
      userThatAssigned.groupRole = "none";

      if (username === this.userService.username) this.userService.groupRole = "product owner";
      if (userThatAssignedProductOwner === this.userService.username) this.userService.groupRole = "none";
    }

  }

  getLanguage(){
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish)
  }

  constructor(
    public taskService:TasksService, 
    private popupService:PopupService, 
    private hub:UsersHubService, 
    private cdr:ChangeDetectorRef,
    public authService:AuthService,
    public userService:UserService,
    public sprintService:SprintService,
    public groupService:GroupService,
    private languageService:LanguageService
  ){}
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnInit(): void {
    this.setInfoForGroup();
    this.onSendGoogleInfo();

    if (!this.taskService.getToken) {
      this.showLogin = true;
      this.showAuth = true;
    }

    this.getLanguage();
  }

}
