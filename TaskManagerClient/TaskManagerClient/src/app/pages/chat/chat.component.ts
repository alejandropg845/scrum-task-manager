import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ApiGatewayService } from '../../services/api-gateway.service';
import { Message, MessagesDate } from '../../interfaces/group-message.interface';
import { UserService } from '../../services/users.service';
import { EMPTY, of, Subject, switchMap, takeUntil } from 'rxjs';
import { HandleBackendError } from '../../interfaces/error-handler';
import { PopupService } from '../../services/popup.service';
import { UsersHubService } from '../../services/hub.service';
import { ChatService } from '../../services/chat.service';
import { animate, style, transition, trigger } from '@angular/animations';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styles: ``,
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('500ms ease-out', style({ opacity: 1 }))
      ])
    ])
  ]
})
export class ChatComponent implements OnInit, OnDestroy{

  chatMessages:MessagesDate[] = [];
  destroy$ = new Subject<void>();
  @ViewChild('messagesContent') messagesContainer!:ElementRef;

  isFirstTime:boolean = false;

  dateId!:string | null;
  datePage:number = 0;
  messagesPage:number = 0;

  /* Esta variable es importante, ya que si el usuario antes de realizar una paginación
  al actual Date, envía un mensaje, debemos agregar al descarte de rows de la base de datos este mensaje
  y lo hacemos incrementando el número de sentMessages para sumarlo al descarte */
  sentMessages:number = 0;
  isSpanish: boolean = false;

  getMessages(){

    /* Agregamos false por primera vez para entrar al método. Si dentro del método es true, ya no entra más. */

    if (!this.isFirstTime) {
      
      this.chatService.getChatMessages(
        this.userService.groupName!,
        this.datePage,
        this.messagesPage,
        this.dateId,
        this.sentMessages
      )
      .pipe(

        takeUntil(this.destroy$),
        
        switchMap(value => {

          /* Cuando no hay más mensajes para el actual datePage, lo que hacemos es aumentar el valor de datePage para traer el siguiente
          datePage, también reiniciamos la paginación de messagesPage a 0 para tomar los messages del nuevo datePage */
          if (value.noMoreMessages) {

            this.datePage++; /* <-- Incrementar datePage para descartar el actual y obtener 
                                    el otro date más reciente después de este con paginación */

            this.messagesPage = 0; // <-- Reiniciamos la paginación de mensajes a 0, para empezar nuevamente y obtener los mensajes del otro date
            this.dateId = null;

            /* Reenviamos la misma petición con los nuevos valores */
            return this.chatService.getChatMessages(
              this.userService.groupName!, 
              this.datePage, 
              this.messagesPage, 
              this.dateId,
              this.sentMessages
            );
          }

          /* Continuar flujo noraml si sí hay más mensajes en el date actual*/
          return of(value);
        })
      )
      .subscribe({
        next: res => {

          /* Sabemos que al hacer la primera petición, los pages en general por default son 0.
            En dado caso que la respuesta no traiga nada por primera vez, quiere decir que no se han envaido mensajes aún.
            Al activar isFirstTime como true, no permitimos ejecutar el método para obtener nuevos mensajes, ya que no los hay. */
          if (this.datePage === 0 && this.messagesPage === 0 && !res.messagesDate){
            this.isFirstTime = true;
            return;
          }
            
          /* Agregamos el valor de dateId para "settear" por default de manera temporal el date actual y obtener sus mensajes */
          this.dateId = res.dateId;

          /* Si el array messages contiene elementos, queire decir que hay mensajes disponibles para agregar al date actual */
          if (res.messages.length > 0) 

            this.addMessagesToDate(res.messages); // <-- Agregamos los messages
            
          
          else if (res.messagesDate) // <-- Obtuvimos en vez de mensajes un nuevo date junto con sus mensajes paginados.

            this.chatMessages.unshift(res.messagesDate); // <-- Agregar date completo

          this.cdr.detectChanges(); // <-- Actualizar el height del contenedor para el scrollToTop()
          this.scrollToTop();

          this.messagesPage++; /* <-- Incrementos el paginador de messages messagesPage, para que cuando se soliciten más mensajes
                                        agregar paginación */

        },
        error: err => HandleBackendError(err, this.popupService)
      });

    }

  }

  onScrollTop(){

    const scroll = (this.messagesContainer.nativeElement as HTMLDivElement).scrollTop;

    if (scroll === 0) this.getMessages();

  }

  scrollToTop(){
    const scroll = (this.messagesContainer.nativeElement as HTMLDivElement);

    scroll.scrollTo({
      top: 300,
      behavior: 'smooth'
    });
  }

  private addMessagesToDate(messages:Message[]){

    const date = this.chatMessages.find(d => d.id === messages[0].dateId)!;

    date.messages.unshift(...messages);

  }

  onSendMessage(textArea:HTMLTextAreaElement){

    if (!textArea.value) return;

    const message = textArea.value;
    const avatarBgColor = this.userService.avatarBgColor;

    this.chatService.sendMessage(this.userService.groupName!, message, avatarBgColor)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {

        textArea.value = '';

        this.usersHub.onInvokeSendGroupMessage(
          this.userService.groupName!,
          res.messagesDate,
          res.message
        );

      },
      error: err => HandleBackendError(err, this.popupService)

    });

  }
  
  setHubReceivers(){
    this.chatService.setChatReceivers_Subject.asObservable()
    .subscribe(_ => {

      this.usersHub.onReceiveGroupMessage(this.onReceiveGroupMessage.bind(this));

    });

  }
  private scrollToBottom(){

    const container = this.messagesContainer.nativeElement as HTMLDivElement;
    container.scrollTo({
      top: container.scrollHeight,
      behavior: 'smooth'
    });

  }

  isUser(sender:string){
    return sender === this.userService.username;
  }

  /* HUB */

  onReceiveGroupMessage(messagesDate:MessagesDate, message:Message){

    this.sentMessages++;

    if (!messagesDate && message) {

      const lastMessagesDateIndex = this.chatMessages.length - 1;
      
      this.chatMessages[lastMessagesDateIndex].messages.push(message);

    } else if (messagesDate && !message) {

      this.chatMessages.push(messagesDate);
      
    }

    this.cdr.detectChanges();

    this.scrollToBottom();
  }

  getLanguage(){
    this.languageService.isSpanish$
    .pipe(takeUntil(this.destroy$))
    .subscribe(isSpanish => this.isSpanish = isSpanish)
  }

  constructor(
    private userService:UserService,
    private popupService:PopupService,
    private usersHub:UsersHubService,
    private chatService:ChatService,
    private cdr:ChangeDetectorRef,
    private languageService:LanguageService
  ){}

  ngOnInit(): void {
    this.setHubReceivers();
    this.getMessages();
    this.getLanguage();
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.usersHub.deleteGroupMessageReceiver();
  }

}
