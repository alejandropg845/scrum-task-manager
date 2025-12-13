export interface SelectedTask {
    id:string,
    title:string,
    priority:number,
    taskItems:SelectedTaskItem[]
}

interface SelectedTaskItem {
    id:string,
    content:string,
    priority:number
}