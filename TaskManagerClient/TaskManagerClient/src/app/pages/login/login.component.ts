import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: ``
})
export class LoginComponent implements OnInit{

  showPassword:boolean = false;
  forgotPassword:boolean = false;
  @Input() isLogging:boolean = false;
  form!:FormGroup;
  isSpanish: boolean = false;
  destroy$ = new Subject<void>();

  @Output() eventEmitter = new EventEmitter<any>();

  onSubmit() {

    if(!this.form.valid) return;

    this.eventEmitter.emit(this.form.value);
  }

  backToLogin(){
    this.forgotPassword = false;
  }

  getLanguage(){

    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);

  }

  constructor(public taskService:TasksService, private fb:FormBuilder, private languageService:LanguageService){

    this.form = this.fb.group({
      username: [null, Validators.required],
      password: [null, Validators.required]
    });

  }

  ngOnInit(): void {
    this.getLanguage();
  }

}
