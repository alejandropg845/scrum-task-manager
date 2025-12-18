export interface MessagesDate {
    id:string,
    groupName:string,
    messagesFullDateInfo: Date,
    messages: Message[]
};

export interface Message {
    id:string,
    dateId:string,
    content:string,
    sender:string,
    messageTime: Date,
    avatarBgColor: string
}