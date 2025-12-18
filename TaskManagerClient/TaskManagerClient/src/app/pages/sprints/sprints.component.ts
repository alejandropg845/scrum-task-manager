import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { UserTask } from '../../interfaces/user-tasks.interface';
import { Sprint } from '../../interfaces/sprint.interface';
import { UserService } from '../../services/users.service';
import { filter, Subject, takeUntil } from 'rxjs';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { HandleBackendError } from '../../interfaces/error-handler';
import { PopupService } from '../../services/popup.service';
import { SprintService } from '../../services/sprint.service';
import { animate, style, transition, trigger } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RetrospectivesService } from '../../services/retrospectives.service';
import { UsersHubService } from '../../services/hub.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-sprints',
  templateUrl: './sprints.component.html',
  styles: ``,
  animations: 
    [
      trigger('slideUp', [
        transition(':enter', [
          style({ opacity: 0, transform: 'translateX(100%)' }),
          animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
        ]),
        transition(':leave', [
          animate('300ms ease-in', style({ opacity: 0, transform: 'translateX(100%)' }))
        ])
      ]),
      trigger('fadeInOut',[
        transition(':enter', [
          style({ opacity: 0 }),
          animate('500ms ease-in', style({ opacity: 1 }))
        ]),
        transition(':leave',[
          animate('500ms ease-out', style({ opacity: 0 }))
        ])
      ])
    ]
})
export class SprintsComponent implements OnInit, OnDestroy{

  @Input() groupSprints:Sprint[] = [];
  
  retrospectiveForm!:FormGroup;
  showSprints:boolean = false;
  destroy$ = new Subject<void>();
  ratingHover:number = 0;
  showForm:boolean = false;
  isSpanish: boolean = false;

  isSprintNotCompleted(sprintTasks:UserTask[]){
    return sprintTasks.some(s => s.status !== 'completed')
  }

  
  //Señal o listener para cuando se deban obtener los sprints
  getGroupSprints(){
    this.sprintService.getGroupSprints_BSubject.asObservable()
    .pipe(filter(((value) => value !== false)))
    .subscribe(_ => this.getSprints())
  }

  getSprints(){
    this.sprintService.getGroupSprints(this.userService.groupName!)
    .subscribe({
      next: res => {
        this.groupSprints = res;
        
        this.sprintService.sprintsTasks_BSubject.asObservable()
        .subscribe(sprintsTasks => 
          this.setTasksToSprints(sprintsTasks)
        );

      },
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  setTasksToSprints(tasks:UserTask[]){

    this.groupSprints.forEach(sprint => {
      
      sprint.tasks = [];

      sprint.tasks = tasks.filter(t => t.sprintId === sprint.id);

      this.sprintService.showSprints = true;
    });


  }

  hoverRating(value:number){
    this.ratingHover = value;
  }

  deleteRating(){
    this.retrospectiveForm.get('rating')?.setValue(0);
  }

  getRating(){
    return this.retrospectiveForm.get('rating')?.value;
  }

  onSubmit(){

    if (!this.retrospectiveForm.valid) return;

    this.retrosService.createSprintRetrospective(this.retrospectiveForm)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.popupService.showPopup('s', res.message);
        this.retrospectiveForm.reset();
        this.showForm = false;
        this.hub.onInvokeSendRetro(res.createdRetro, this.groupName);
      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  private setValuesToForm(groupName:string, sprintId:string, sprintName:string){
    this.retrospectiveForm.get('groupName')!.setValue(groupName);
    this.retrospectiveForm.get('name')!.setValue(sprintName);
    this.retrospectiveForm.get('sprintId')!.setValue(sprintId);
  }

  setRating(value:number){
    this.retrospectiveForm.get('rating')?.setValue(value);
  }

  onCloseForm(){
    this.retrospectiveForm.reset();
    this.showForm = false;
    this.markFeedbackAsSubmited();
  }

  sprintId!:string;
  groupName!:string;
  username!:string;

  markFeedbackAsSubmited(){
    this.retrosService.markFeedbackAsSubmited(this.username, this.groupName, this.sprintId)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => this.popupService.showPopup('s', res.message),
      error: err => HandleBackendError(err, this.popupService)
    });
  }

  setShowRetrosFormListener(){
    this.retrosService.showRetrospective_BSubject.asObservable()
    .pipe(takeUntil(this.destroy$), filter((value) => value !== null))
    .subscribe(res => {

      this.groupName = res!.groupName;
      this.sprintId = res!.sprintId;
      this.username = res!.username

      this.setValuesToForm(res!.groupName, res!.sprintId, res!.sprintName);
      this.showForm = true;
    });
  }

  getLanguage(){
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  constructor(public userService:UserService,
    private popupService:PopupService,
    private sprintService:SprintService,
    private retrosService:RetrospectivesService,
    private formBuilder:FormBuilder,
    private hub:UsersHubService,
    private languageService:LanguageService
  ){
    this.retrospectiveForm = this.formBuilder.group({
      sprintId: [null, Validators.required],
      groupName: [null, Validators.required],
      feedBack: [null, [Validators.required, Validators.maxLength(200)]],
      rating: [0, [Validators.min(0), Validators.max(5)]],
      name: [null, Validators.required]
    });
  }
  
  ngOnInit(): void {
    this.getGroupSprints();
    this.setShowRetrosFormListener();
    this.getLanguage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
