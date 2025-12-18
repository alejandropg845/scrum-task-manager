import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../../services/users.service';
import { Message } from '../../interfaces/message.interface';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { finalize, Observable, Subject, takeUntil } from 'rxjs';
import { HandleBackendError } from '../../interfaces/error-handler';
import { PopupService } from '../../services/popup.service';
import { animate, keyframes, style, transition, trigger } from '@angular/animations';
import { SprintService } from '../../services/sprint.service';
import { AssistantService } from '../../services/assistant.service';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styles: ``,
  animations: [
    trigger('jump', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translate(-50%,-50%) scale(0.9)' }),
        animate("100ms ease-in", style({ transform: 'translate(-50%,-50%) scale(1.1)', opacity: 1 })),
        animate("100ms ease-in", style({ transform: 'translate(-50%,-50%) scale(1)' }))
      ]),
      transition(':leave', [
        animate('100ms ease-in', style({ transform: 'translate(-50%,-50%) scale(0.8)', opacity: 0 }))
      ])
    ])
  ]
})
export class MainComponent implements OnDestroy {

  chatMessages: Message[] = [];
  isAssistantOpen: boolean = false;
  content!: string | null;
  isProccessingMessage: boolean = false;
  remainingTime:string = this.sprintService.remainingTime;
  isSpanish: boolean = false;
  destroy$ = new Subject<void>();

  openAssistant(taskItemContent: string) {

    if (this.isProccessingMessage) return;

    this.isAssistantOpen = true;
    this.isProccessingMessage = true;
    this.content = taskItemContent;

    /* Agregar un mensaje en el chat temporal para el Assistant */
    const tempMessage: Message = {
      content: "",
      isUserMessage: false
    };

    this.chatMessages.push(tempMessage);

    const tempMessageInChat = this.chatMessages.find(m => !m.isUserMessage)!;

    this.assistantService.openAssistant(taskItemContent)
    .pipe(
      takeUntil(this.destroy$),
      finalize(() => this.isProccessingMessage = false)
    )
    .subscribe({
      next: res => {
        const text = res.candidates[0].content.parts[0].text;
        this.writeMessage(text, tempMessageInChat, .3);
      },
      error: err => {
        HandleBackendError(err, this.popupService);
        if (!this.isSpanish)
          tempMessageInChat.content = "Failed trying to complete the request";
        else
          tempMessageInChat.content = "Hubo un error al completar la petición";
      }
    });
  }

  private writeMessage(text: string, message: Message, speed: number) {
    let index = 0;
    const interval = setInterval(() => {
      if (index < text.length) {
        message.content += text.charAt(index);
        index++;
      } else {
        clearInterval(interval);
      }
    }, speed);
  }

  sendMessage(input: HTMLInputElement) {

    if (this.isProccessingMessage) return;

    if (!input.value) return;

    const geminiMessages = this.chatMessages.filter(m => !m.isUserMessage);

    const previousGeminiResponse: string = geminiMessages[geminiMessages.length - 1].content;

    const userMessage: Message = { content: input.value, isUserMessage: true };

    const geminiTemporaryMessage: Message = { content: "", isUserMessage: false };

    const prompt = input.value;

    this.chatMessages.push(userMessage);
    this.chatMessages.push(geminiTemporaryMessage);

    input.value = "";

    this.assistantService.sendMessage(this.content!, prompt, previousGeminiResponse)
    .pipe(takeUntil(this.destroy$), finalize(() => this.isProccessingMessage = false))
    .subscribe({
      next: res => {
        const text = res.candidates[0].content.parts[0].text;

        this.writeMessage(text, geminiTemporaryMessage, .3);
      },
      error: err => {
        HandleBackendError(err, this.popupService);
        
        if (!this.isSpanish)
          geminiTemporaryMessage.content = "Failed trying to complete the request";
        else
          geminiTemporaryMessage.content = "Hubo un error al completar la petición";

      }
    });
  }

  closeAssistantWindow() {
    this.isAssistantOpen = false;
    this.isProccessingMessage = false;
    this.content = null;
    this.chatMessages = [];

    // TODO Hacer cancellationToken para la api
  }

  switchLanguage(){
    this.languageService.switchLanguage();
  }

  constructor(
    public userService: UserService,
    private assistantService:AssistantService,
    private popupService: PopupService,
    public sprintService:SprintService,
    private languageService:LanguageService
  ) {}


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
