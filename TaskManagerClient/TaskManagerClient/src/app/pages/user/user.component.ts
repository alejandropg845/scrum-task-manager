import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { UserService } from '../../services/users.service';
import { LanguageService } from '../../services/language.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styles: ``
})
export class UserComponent implements OnInit{

  @Output() logOutEventEmitter = new EventEmitter<any>();
  destroy$ = new Subject<void>();
  isSpanish: boolean = false;

  logOut(){
    this.logOutEventEmitter.emit();
  }

  getLanguage(){
    this.languageService.isSpanish$.pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  constructor(public userService:UserService, private languageService:LanguageService){}

  ngOnInit(): void {
    this.getLanguage();
  }

}
