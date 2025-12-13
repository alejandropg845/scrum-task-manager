import { UserTask } from "./user-tasks.interface";

export interface Sprint {
    id:string,
    expirationTime:Date,
    status:number,
    sprintNumber:string,
    sprintName:string
    tasks: UserTask[]
}