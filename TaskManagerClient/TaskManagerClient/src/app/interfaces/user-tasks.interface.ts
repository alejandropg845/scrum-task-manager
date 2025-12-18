export interface UserTask {
    id              : string,
    username        : string,
    title           : string,
    createdOn       : Date,
    taskItems       : TaskItem[],
    groupName       : string,
    status          : string,
    isRemovable     : boolean,
    sprintId        : string,
    sprintStatus    : string,
    priority        : number,
    taskItemEdit    : TaskItem | null
}

export interface TaskItem {
    id              : string,
    taskId          : string,
    assignToUsername  : string,
    content         : string,
    isCompleted     : boolean,
    isRemovable     : boolean,
    isCompletable   : boolean,
    priority        : number,
    taskTitle       : string
}