export interface CurrentUserInfo{
    isGroupOwner:boolean,
    groupName:string,
    username:string,
    groupRole:string,
    isScrum:boolean,
    isAddingTasksAllowed:boolean,
    expirationTime:Date,
    status:string,
    sprintNumber:number
}