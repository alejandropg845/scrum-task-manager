import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TasksService } from '../../services/tasks.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LanguageService } from '../../services/language.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styles: ``
})
export class RegisterComponent implements OnInit{

  showPassword:boolean = false;
  @Input() isRegistering:boolean = false;
  form!:FormGroup;
  isSpanish:boolean = false;
  destroy$ = new Subject<void>();

  @Output() eventEmitter = new EventEmitter();

  onSubmit(){

    if (!this.form.valid) return;

    this.eventEmitter.emit(this.form.value);
  }

  getLanguage(){
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  constructor(public taskService:TasksService, private fb:FormBuilder, private languageService:LanguageService){

    this.form = this.fb.group({
      username: [null, Validators.required],
      email: [null],
      password: [null, Validators.required]
    });

  }

  ngOnInit(): void {
    this.getLanguage();
  }

}
