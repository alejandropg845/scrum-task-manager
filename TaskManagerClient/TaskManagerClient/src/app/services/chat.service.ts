import { Injectable } from "@angular/core";
import { BehaviorSubject, takeUntil } from "rxjs";
import { Message, MessagesDate } from "../interfaces/group-message.interface";
import { ApiGatewayService } from "./api-gateway.service";
import { HandleBackendError } from "../interfaces/error-handler";
import { PopupService } from "./popup.service";
import { GetChatMessages } from "../interfaces/responses/get-group-messages.interface";

@Injectable({
    providedIn: 'root'
})
export class ChatService {

    setChatReceivers_Subject = new BehaviorSubject<boolean>(false);

    sendMessage(groupName:string, message:string, avatarBgColor:string){

        const body = { 
            groupName,
            message,
            avatarBgColor
        };

        return this.apiGateway.sendRequest<{message:Message, messagesDate:MessagesDate}>("post", "chats", "SendMessage", body);
    }

    getChatMessages(groupName:string, datePage:number, messagesPage:number, dateId:string | null, sentMessages:number){

        const body = { groupName, datePage, messagesPage, dateId, sentMessages };

        return this.apiGateway.sendRequest<GetChatMessages>("get", "chats", `GetGroupChatMessages`, body);
    }

    constructor(private apiGateway:ApiGatewayService){}

}