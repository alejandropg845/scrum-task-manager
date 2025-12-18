import { Component, OnDestroy, OnInit } from '@angular/core';
import { RetrospectivesService } from '../../services/retrospectives.service';
import { UserService } from '../../services/users.service';
import { Subject, takeUntil } from 'rxjs';
import { Retrospective } from '../../interfaces/retrospective.interface';
import { FormBuilder } from '@angular/forms';
import { UsersHubService } from '../../services/hub.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-retrospectives',
  templateUrl: './retrospectives.component.html',
  styles: ``
})
export class RetrospectivesComponent implements OnDestroy, OnInit{

  retrospectives:Retrospective[] = [];
  retrosCopy:Retrospective[] = [];
  sprintsNumbers:string[] = [];

  isDescending:boolean = true;
  destroy$ = new Subject<void>();
  isSpanish: boolean = false;

  setRetrosListener(){
    this.retrosService.setRetrosSubjectListener()
    .pipe(takeUntil(this.destroy$))
    .subscribe((sprintNumber:number) => {
      this.retrosService.getRetrospectives(this.userService.groupName!)
      .pipe(takeUntil(this.destroy$))
      .subscribe(retros => {
        this.retrospectives = retros;
        this.retrosCopy = [...retros];
        this.orderBy('desc');
        this.fillSprintsNumbers(sprintNumber);
      });
    });
  }

  private fillSprintsNumbers(sprintsNumber:number){
    if (sprintsNumber > 1){

      let i = 1;

      while(i < sprintsNumber) {
        this.sprintsNumbers.push(i.toString());
        i++;
      }

    }

  }

  orderBy(order:string) {

    if (order === 'desc') 
      this.retrosCopy.sort((lower, greater) => greater.rating - lower.rating);
    else 
      this.retrosCopy.sort((lower, greater) => lower.rating - greater.rating);

    this.isDescending = !this.isDescending;
  }

  filterBy(sprintNumber:string) {

    if (sprintNumber === "0"){
      this.retrosCopy = [...this.retrospectives];
      return;
    }

    const filteredRetros = this.retrospectives.filter(r => r.name.includes(sprintNumber));
    this.retrosCopy = filteredRetros;
  }


  getRetrosHubReceiver(){
    this.retrosService.getRetrosHubReceiver()
    .pipe(takeUntil(this.destroy$))
    .subscribe(() => this.hub.onReceiveRetro(this.onReceiveRetro.bind(this)));
  }

  getLanguage(){
    this.languageService.isSpanish$.pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  /*HUB */
  onReceiveRetro(retro:Retrospective){
    this.retrospectives.push(retro);
    this.retrosCopy.push(retro);
  }

  constructor(
    private retrosService:RetrospectivesService, 
    private userService:UserService,
    private hub:UsersHubService,
    private languageService:LanguageService){}
  
  ngOnInit(): void {
    this.setRetrosListener();
    this.getRetrosHubReceiver();
    this.getLanguage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.hub.deleteRetrosReceiver();
  }
  
}
