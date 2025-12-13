import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { finalize, Subject, takeUntil } from 'rxjs';
import { HandleBackendError } from '../../interfaces/error-handler';
import { PopupService } from '../../services/popup.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styles: ``
})
export class ForgotPasswordComponent implements OnInit{

  email:string = "";
  destroy$ = new Subject<void>();
  waitingRecoveryCode:boolean = false;
  isSendingRecoverEmail:boolean = false;
  isSendingRecoveryCode:boolean = false;
  isSpanish: boolean = false;

  @Output() backToLoginEventEmitter = new EventEmitter();

  onSubmit(element:HTMLInputElement){

    if (!element.value) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', 'No email provided');
      else 
        this.popupService.showPopup('i', 'Por favor ingresa un correo');

      return;
    }

    this.isSendingRecoverEmail = true;

    const email = element.value;

    this.authService.forgotPassword(email)
    .pipe(takeUntil(this.destroy$), finalize(() => this.isSendingRecoverEmail = false))
    .subscribe({
      next: _ => {
        this.email = element.value;
        this.waitingRecoveryCode = true;
      },
      error: err => HandleBackendError(err, this.popupService)
    });

  }

  onReceiveRecoveryCode(recoveryCode:string, password1:string, password2:string){

    if (!recoveryCode || !password1 || !password2) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', 'Fields missing');
      else
        this.popupService.showPopup('i', 'Campos faltantes');

      return;
    }

    if (password1 !== password2) {
      
      if (!this.isSpanish)
        this.popupService.showPopup('i', "Passwords don't match");
      else
        this.popupService.showPopup('i', "Las contraseñas no coinciden");

      return;
    }

    this.isSendingRecoveryCode = true;

    this.authService.receiveRecoveryCode(recoveryCode, password1, password2, this.email)
    .pipe(takeUntil(this.destroy$), finalize(() => this.isSendingRecoveryCode = false))
    .subscribe({
      next: res => {

        this.popupService.showPopup('s', res.message);
        this.backToLogin();
      },
      error: err => {
        HandleBackendError(err, this.popupService);
        if (err.restart) this.backToLogin();
      }
    });

  }

  backToLogin(){
    this.email = "";
    this.waitingRecoveryCode = false;
    this.backToLoginEventEmitter.emit();
  }

  getLanguage(){
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish);
  }

  constructor(private authService:AuthService, private popupService:PopupService, private languageService:LanguageService){}
  
  ngOnInit(): void {
    this.getLanguage();
  }

}
