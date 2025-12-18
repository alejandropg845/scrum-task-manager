
import { Message, MessagesDate } from "../group-message.interface";

export interface GetChatMessages {
    noMoreMessages:boolean,
    messagesDate:MessagesDate,
    messages:Message[]
    dateId:string
}